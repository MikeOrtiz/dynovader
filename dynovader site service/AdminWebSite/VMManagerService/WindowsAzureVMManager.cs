using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Xml;
using System.ServiceModel.Syndication;
using System.IO;
using System.Collections.Specialized;
using System.Diagnostics;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure.StorageClient.Protocol;
using System.Runtime.Serialization.Formatters.Soap;
using System.Net;
using System.Xml.Linq;
using System.Threading;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel.Description;
using System.ServiceModel.Activation;
using Microsoft.Web.Administration;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace WindowsAzureCompanion.VMManagerService
{
    public class WindowsAzureVMManagerUsernamePasswordValidator : UserNamePasswordValidator
    {
        // Validate credentials
        public override void Validate(string userName, string password)
        {
            if (!(userName.Equals(RoleEnvironment.GetConfigurationSettingValue("AdminUserName"))
                && password.Equals(RoleEnvironment.GetConfigurationSettingValue("AdminPassword"))))
            {
                throw new SecurityTokenValidationException("Invalid credentials. Access denied.");
            }
        }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class WindowsAzureVMManager : IVMManager
    {
        // Constants
        public static string PHPRuntimeProductID = "PHP_Runtime";
        public static string MySQLBasedDBCategory = "MySQL_BASED_DB";
        public static string MySQLCommunityServerProductID = "MySQL_Community_Server";
        public static string MariaDBProductID = "MariaDB";        
        public static string LibraryFolderForPHP = "includes";
        public static string FrameworkSDKCategoryForPHP = "Frameworks and SDKs";
        public static string CustomExtensionCategoryForPHP = "PHP_Custom_Extension";
        public static string WebApplicationCategoryForPHP = "Web Applications";
        public static string MySQLCommunityServerFolder = "mysql";
        public static string MariaDBServerFolder = "mariadb";
        public static string RuntimeFolderForPHP = "php";
        public static string ExtensionsFolderForPHP = "ext";
        public static string ApplicationsFolder = "applications";
        public static string ApplicationsUnzipUtility = "UnzipUtility.vbs";
        public static string ApplicationsUntarUtility = "ExtractUtility.php";
        public static string SecondaryWebSiteName = "PHPWebSite";
        public static string AdminWebSiteNameInServiceDefinition = "Web";
        
        private string applicationsAndRuntimeResourceFolder = null;
        
        // Storage account and backup container refereences
        private CloudBlobClient blobClient = null;
        private CloudBlobContainer container = null;

        // Mounted Windows Azure drive 
        private CloudDrive drive = null;
        private CloudPageBlob xdrivePageBlob = null;

        // Installation Status Collection and corresponding blob
        private NameValueCollection installationStatusCollection = null;
        private CloudBlob installationStatusBlob = null;

        // Cron job information (ProductID => Thread)
        private Dictionary<string, List<Thread>> cronJobs = new Dictionary<string, List<Thread>>();

        // Progress information
        private CloudBlob progressInformationBlob = null;
        private static object locker = new object();

        // Constructor for the service
        public WindowsAzureVMManager()
        {
            // Create HTTPS storage endpoint
            CloudStorageAccount storageAccount = WindowsAzureVMManager.GetStorageAccount(true);

            // Get backup contain reference            
            blobClient = storageAccount.CreateCloudBlobClient();
            string containerName = RoleEnvironment.GetConfigurationSettingValue("PHPApplicationsBackupContainerName");
            container = blobClient.GetContainerReference(containerName);
            if (container.CreateIfNotExist())
            {
                // TODO: Finally do not provide public access
                BlobContainerPermissions containerPermissions = new BlobContainerPermissions();
                containerPermissions.PublicAccess = BlobContainerPublicAccessType.Container;
                container.SetPermissions(containerPermissions);
            }

            // Initialize installation status information and corresponding blob
            InitializeInstallationStatus();

            // Initialize progress information and corresponding blob
            InitializeProgressInformation();
        }

        // Initialize installation status information and corresponding blob
        private void InitializeInstallationStatus()
        {
            string blobName = RoleEnvironment.GetConfigurationSettingValue("InstallationStatusConfigFileBlob");
            installationStatusBlob = container.GetBlobReference(blobName);
            if (!WindowsAzureVMManager.BlobExists(installationStatusBlob))
            {
                // Create empty NameValueCollection and serialize to blob
                installationStatusCollection = new NameValueCollection();
                SerializeNameValueCollectionToBlob(installationStatusCollection, installationStatusBlob);
            }
            else
            {
                // Deserialize NameValueCollection to blob
                installationStatusCollection = DeserializeNameValueCollectionFromBlob(installationStatusBlob);
            }
        }

        // Initialize progress information and corresponding blob
        private void InitializeProgressInformation()
        {
            string blobName = RoleEnvironment.GetConfigurationSettingValue("ProgressInformationFileBlob");
            progressInformationBlob = container.GetBlobReference(blobName);
            if (!WindowsAzureVMManager.BlobExists(progressInformationBlob))
            {
                // Serialize empty NameValueCollection to blob
                NameValueCollection progressInformation = new NameValueCollection();
                SerializeNameValueCollectionToBlob(progressInformation, 
                    progressInformationBlob);
            }
        }
        
        // Install specified applications
        public bool InstallApplications(IDictionary<string, string> applicationsToInstall)
        {
            try
            {
                // Update progress information
                UpdateProgressInformation("Installing Platform/Applications", false);

                // Create a seperate thread for installing applications
                ThreadStart starter = delegate { InstallApplicationsOnAnotherThread(applicationsToInstall); };
                Thread thread = new Thread(starter);
                thread.Start();
            }
            catch (Exception ex)
            {
                UpdateProgressInformation("Unable to start application installation. Error: " + ex.Message, true);
                return false;
            }
            
            return true;
        }

        // Install specified applications
        private bool InstallApplicationsOnAnotherThread(IDictionary<string, string> applicationsToInstall)
        {
            try
            {
                XmlReader reader = XmlReader.Create(RoleEnvironment.GetConfigurationSettingValue("ProductListXmlFeed"));
                SyndicationFeed feed = SyndicationFeed.Load(reader);

                // Get list of selected products and their download URLs
                var varProducts = from item in feed.Items
                                  where applicationsToInstall.Keys.Contains(
                                  item.ElementExtensions.ReadElementExtensions<string>("productId", "http://www.w3.org/2005/Atom")[0])
                                  select item;

                // Create top level folders for PHP Runtime and Applications
                string phpRunTimeFolder = Path.Combine(applicationsAndRuntimeResourceFolder, WindowsAzureVMManager.RuntimeFolderForPHP);
                if (!Directory.Exists(phpRunTimeFolder))
                {
                    Directory.CreateDirectory(phpRunTimeFolder);
                }
                string phpLibraryFolder = Path.Combine(phpRunTimeFolder, WindowsAzureVMManager.LibraryFolderForPHP);
                if (!Directory.Exists(phpLibraryFolder))
                {
                    Directory.CreateDirectory(phpLibraryFolder);
                }
                if (!Directory.Exists(Path.Combine(applicationsAndRuntimeResourceFolder, WindowsAzureVMManager.ApplicationsFolder)))
                {
                    Directory.CreateDirectory(Path.Combine(applicationsAndRuntimeResourceFolder, WindowsAzureVMManager.ApplicationsFolder));
                }
                string downloadFolder = RoleEnvironment.GetLocalResource("ApplicationsDownloadResource").RootPath;

                // Whether to start Hosted Web Core for PHP
                bool startPHPWebSite = false;

                // Install Applications            
                foreach (var product in varProducts)
                {
                    string productId = product.ElementExtensions.ReadElementExtensions<string>("productId", "http://www.w3.org/2005/Atom")[0];
                    if (installationStatusCollection.AllKeys.Contains(productId))
                    {
                        Trace.TraceWarning("Application {0} already installed.", productId);
                        continue;
                    }

                    string installCategory = product.ElementExtensions.ReadElementExtensions<string>("installCategory", "http://www.w3.org/2005/Atom")[0];
                    string[] applicationsToInstallInfo = applicationsToInstall[productId].ToString().Split(',');
                    string productVersion = applicationsToInstallInfo[0];

                    // Get product properties passed from UI
                    NameValueCollection productProperties = new NameValueCollection();
                    for (int i = 1; i < applicationsToInstallInfo.Length; i++)
                    {
                        string[] property = applicationsToInstallInfo[i].Split('=');
                        productProperties.Add(property[0], property[1]);
                    }
                    string installPath = "/";
                    if (productProperties.AllKeys.Contains("installPath"))
                    {
                        installPath = productProperties["installPath"];
                    }

                    // Set other readonly properties (if any)
                    SyndicationElementExtension productPropertiesElementExtension =
                        product.ElementExtensions.Where<SyndicationElementExtension>
                            (x => x.OuterName == "productProperties").FirstOrDefault();
                    if (productPropertiesElementExtension != null)
                    {
                        foreach (XElement productPropertyExtension in productPropertiesElementExtension.GetObject<XElement>().Elements())
                        {
                            XAttribute captionAttribute = productPropertyExtension.Attribute("caption");
                            if (captionAttribute == null)
                            {
                                XAttribute nameAttribute = productPropertyExtension.Attribute("name");
                                XAttribute valueAttribute = productPropertyExtension.Attribute("value");
                                if ((nameAttribute != null) && (valueAttribute != null))
                                {
                                    productProperties.Add(nameAttribute.Value, valueAttribute.Value);
                                }
                            }
                        }
                    }

                    IInstaller installer = null;
                    if (productId.Equals(WindowsAzureVMManager.PHPRuntimeProductID))
                    {
                        // PHP Runtime
                        installer = new PHPRuntimeInstaller(
                            Path.Combine(applicationsAndRuntimeResourceFolder, WindowsAzureVMManager.RuntimeFolderForPHP),
                            downloadFolder,
                            product,
                            productVersion);
                        startPHPWebSite = true;
                    }
                    else if (installCategory.Equals(WindowsAzureVMManager.MySQLBasedDBCategory))
                    {
                        // Allow only one MySQL based database on the VM
                        if ((installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.MySQLCommunityServerProductID))
                            || (installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.MariaDBProductID)))
                        {
                            Trace.TraceError("Only one MySQL based database can be installed and {0} is alreadyinstalled.",
                                MySQLBasedDBInstaller.GetMySQLBasedDBName());
                        }
                        else
                        {
                            // Root password for MySQL based DB
                            string rootPassword = "";
                            if (productProperties.AllKeys.Contains("rootPassword"))
                            {
                                rootPassword = productProperties["rootPassword"];
                            }

                            if (productId.Equals(WindowsAzureVMManager.MySQLCommunityServerProductID))
                            {
                                // MySQL Community Server
                                installer = new MySQLCommunityServerInstaller(
                                    Path.Combine(applicationsAndRuntimeResourceFolder, WindowsAzureVMManager.MySQLCommunityServerFolder),
                                    downloadFolder,
                                    product,
                                    productVersion,
                                    productProperties);
                            }
                            else if (productId.Equals(WindowsAzureVMManager.MariaDBProductID))
                            {
                                // MariaDB server
                                installer = new MariaDBInstaller(
                                    Path.Combine(applicationsAndRuntimeResourceFolder, WindowsAzureVMManager.MariaDBServerFolder),
                                    downloadFolder,
                                    product,
                                    productVersion,
                                    productProperties);
                            }
                            else
                            {
                                Trace.TraceError("Invalid MySQL based database in applications feed");
                            }
                        }
                    }
                    else if (installCategory.Equals(WindowsAzureVMManager.CustomExtensionCategoryForPHP))
                    {
                        // PHP Custom extension
                        installer = new PHPExtensionInstaller(
                            Path.Combine(applicationsAndRuntimeResourceFolder, WindowsAzureVMManager.RuntimeFolderForPHP),
                            downloadFolder,
                            product,
                            productVersion);
                        startPHPWebSite = true;
                    }
                    else if (installCategory.Equals(WindowsAzureVMManager.FrameworkSDKCategoryForPHP))
                    {
                        // PHP Framework or SDK
                        installer = new PHPFrameworkSDKInstaller(
                            Path.Combine(
                                Path.Combine(applicationsAndRuntimeResourceFolder,
                                WindowsAzureVMManager.RuntimeFolderForPHP),
                                WindowsAzureVMManager.LibraryFolderForPHP),
                            downloadFolder,
                            product,
                            productVersion);
                    }
                    else if (installCategory.Equals(WindowsAzureVMManager.WebApplicationCategoryForPHP))
                    {
                        // PHP Web Application
                        installer = new PHPApplicationInstaller(
                            Path.Combine(applicationsAndRuntimeResourceFolder, WindowsAzureVMManager.ApplicationsFolder),
                            installPath,
                            downloadFolder,
                            product,
                            productVersion,
                            productProperties);
                    }
                    else
                    {
                        Trace.TraceWarning("Invalid installation type.");
                        continue;
                    }

                    // Install the product
                    if (installer != null)
                    {
                        try
                        {
                            installer.Install();
                        
                            // Set product as instaleld into installedProductIds and update status in blob
                            string[] installStatusInfo = null;
                            if (installCategory.Equals(WindowsAzureVMManager.MySQLBasedDBCategory))
                            {
                                installStatusInfo = new string[] {
                                    DateTime.Now.ToString(),
                                    installPath,
                                    productVersion,
                                    productProperties["iniFileName"]
                                };
                            }
                            else
                            {
                                installStatusInfo = new string[] {
                                    DateTime.Now.ToString(),
                                    installPath,
                                    productVersion
                                };
                            }

                            // Setup cron job (if any)
                            if (productProperties.AllKeys.Contains("cronJobs"))
                            {
                                string applicationInstallPath = Path.Combine(applicationsAndRuntimeResourceFolder,
                                    WindowsAzureVMManager.ApplicationsFolder);
                                if (!installPath.Equals("/"))
                                {
                                    applicationInstallPath = Path.Combine(applicationInstallPath,
                                        installPath.Replace("/", "\\").Trim('\\'));
                                }

                                SetupCronJobs(productId, productProperties["cronJobs"], applicationInstallPath);
                            }

                            installationStatusCollection.Add(productId, string.Join(",", installStatusInfo));
                            WindowsAzureVMManager.SerializeNameValueCollectionToBlob(installationStatusCollection, installationStatusBlob);
                        }
                        catch (Exception ex)
                        {
                            UpdateProgressInformation(string.Format("Failed to install {0}. Error: {1}", product.Title.Text , ex.Message), true);
                            return false;
                        }
                    }

                    // Check if explicit web site restart is requested
                    if (productProperties.AllKeys.Contains("restartWebSite"))
                    {
                        if (productProperties["restartWebSite"].ToLower().Equals("true"))
                        {
                            startPHPWebSite = true;
                        }
                    }
                }

                // Start or Restart Hosted Web Core if PHP Runtime or extension is installed
                if (startPHPWebSite)
                {
                    // Re-configure PHP runtime
                    string phpInstallFolder =
                        Path.Combine(applicationsAndRuntimeResourceFolder, WindowsAzureVMManager.RuntimeFolderForPHP);
                    PHPRuntimeInstaller.ReConfigurePHPRuntime(phpInstallFolder);

                    // Restart PHP Web Site
                    // Get latest handle to PHP web site
                    ServerManager serverManager = new ServerManager();
                    Site secondaryWebSite = serverManager.Sites[WindowsAzureVMManager.SecondaryWebSiteName];
                    if (secondaryWebSite != null)
                    {                        
                        if (secondaryWebSite.State == ObjectState.Started)
                        {
                            RestartPHPWebSite();
                        }
                        else
                        {
                            StartPHPWebSite();
                        }
                    }
                    else
                    {
                        StartPHPWebSite();
                    }                                        
                }
            }
            catch (Exception ex)
            {
                UpdateProgressInformation("Installation failed. Error: " + ex.Message, true);
                return false;
            }

            ClearProgressInformation();
            return true;
        }

        // Get cron job property for specified index
        private string GetCronJobProperty(string productId, int cronJobIndex)
        {
            XmlReader reader = XmlReader.Create(RoleEnvironment.GetConfigurationSettingValue("ProductListXmlFeed"));
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            string[] cronJobsInfoArray = GetCronJobsProperty(feed, productId).Split(',');
            if (cronJobIndex < 0 || cronJobIndex >= cronJobsInfoArray.Length)
            {
                return null;
            }
            else
            {
                return cronJobsInfoArray[cronJobIndex];
            }
        }

        // Get cron job properties
        private string GetCronJobsProperty(string productId)
        {
            XmlReader reader = XmlReader.Create(RoleEnvironment.GetConfigurationSettingValue("ProductListXmlFeed"));
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            return GetCronJobsProperty(feed, productId);
        }

        // Get cron job properties
        private string GetCronJobsProperty(SyndicationFeed feed, string productId)
        {
            SyndicationItem product = (from item in feed.Items
                              where item.ElementExtensions.ReadElementExtensions<string>("productId", "http://www.w3.org/2005/Atom")[0].Equals(productId)
                              select item).SingleOrDefault();

            // Check if cronjob is defined for this product
            SyndicationElementExtension productPropertiesElementExtension =
                product.ElementExtensions.Where<SyndicationElementExtension>
                    (x => x.OuterName == "productProperties").FirstOrDefault();
            if (productPropertiesElementExtension != null)
            {
                foreach (XElement productPropertyExtension in productPropertiesElementExtension.GetObject<XElement>().Elements())
                {
                    XAttribute nameAttribute = productPropertyExtension.Attribute("name");
                    if (nameAttribute != null)
                    {
                        if (nameAttribute.Value.Equals("cronJobs"))
                        {
                            XAttribute valueAttribute = productPropertyExtension.Attribute("value");
                            if (valueAttribute != null)
                            {
                                return valueAttribute.Value;
                            }
                        }
                    }
                }
            }

            // Not found
            return null;
        }

        // Setup cron jobs for installed applications (if any)
        private void SetupCronJobsForInstalledApplications()
        {
            try
            {
                XmlReader reader = XmlReader.Create(RoleEnvironment.GetConfigurationSettingValue("ProductListXmlFeed"));
                SyndicationFeed feed = SyndicationFeed.Load(reader);

                // Get list of installed products
                var varProducts = from item in feed.Items
                                  where installationStatusCollection.AllKeys.Contains(
                                  item.ElementExtensions.ReadElementExtensions<string>("productId", "http://www.w3.org/2005/Atom")[0])
                                  select item;
                foreach (var product in varProducts)
                {
                    // Check if cronjob is defined for this product
                    SyndicationElementExtension productPropertiesElementExtension =
                        product.ElementExtensions.Where<SyndicationElementExtension>
                            (x => x.OuterName == "productProperties").FirstOrDefault();
                    if (productPropertiesElementExtension != null)
                    {
                        foreach (XElement productPropertyExtension in productPropertiesElementExtension.GetObject<XElement>().Elements())
                        {
                            XAttribute nameAttribute = productPropertyExtension.Attribute("name");
                            if (nameAttribute != null)
                            {
                                if (nameAttribute.Value.Equals("cronJobs"))
                                {
                                    XAttribute valueAttribute = productPropertyExtension.Attribute("value");
                                    if (valueAttribute != null)
                                    {
                                        string productId = product.ElementExtensions.ReadElementExtensions<string>("productId", "http://www.w3.org/2005/Atom")[0];
                                        string[] installInfo = installationStatusCollection[productId].Split(',');
                                        string installPath = installInfo[1];
                                        string applicationInstallPath = Path.Combine(applicationsAndRuntimeResourceFolder,
                                            WindowsAzureVMManager.ApplicationsFolder);
                                        if (!installPath.Equals("/"))
                                        {
                                            applicationInstallPath = Path.Combine(applicationInstallPath,
                                                installPath.Replace("/", "\\").Trim('\\'));
                                        }

                                        SetupCronJobs(productId, valueAttribute.Value, applicationInstallPath);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error in cron jobs setup. Error: {0}", ex.Message);
            }
        }

        // Setup cron jobs
        private void SetupCronJobs(string productId, string cronJobsInfo, string applicationInstallPath)
        {
            try
            {
                if (!cronJobs.Keys.Contains(productId))
                {
                    string phpInstallFolder =
                        Path.Combine(applicationsAndRuntimeResourceFolder, WindowsAzureVMManager.RuntimeFolderForPHP);
                    string phpExeFileName = Path.Combine(phpInstallFolder, "php.exe");

                    List<Thread> threadList = new List<Thread>();

                    string[] cronJobsInfoArray = cronJobsInfo.Split(',');
                    foreach (string cronJobInfo in cronJobsInfoArray)
                    {
                        string[] cronJobInfoArray = cronJobInfo.Split(';');
                        string cronJobInitialStatus = cronJobInfoArray[4];

                        // Start the cron job if specified in the feed
                        if (cronJobInitialStatus.ToLower().Equals("true"))
                        {
                            string cronJobFileName = Path.Combine(applicationInstallPath,
                                cronJobInfoArray[2].Replace("/", "\\").Trim('\\'));
                            int cronJobFrequencyInSecond = int.Parse(cronJobInfoArray[3]);

                            // Create a thread for cron job
                            ThreadStart starter = delegate { CronJobThreadRoutine(phpExeFileName, cronJobFileName, cronJobFrequencyInSecond); };
                            Thread thread = new Thread(starter);
                            threadList.Add(thread);

                            thread.Start();
                        }
                        else
                        {
                            threadList.Add(null);
                        }
                    }

                    // Add thread list to dictionary
                    cronJobs.Add(productId, threadList);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error in cron job {0} setup. Error: {1}", cronJobsInfo, ex.Message);
            }        
        }
        
        // Cron job thread routine
        private void CronJobThreadRoutine(string processFileName, string processArguments, int cronJobFrequencyInSecond)
        {
            Trace.TraceInformation("Scheduling {0} as cron job with frequency {1} s.", processArguments, cronJobFrequencyInSecond);

            while (true)
            {
                try
                {
                    Trace.TraceInformation("Running {0} as cron job.", processArguments);

                    Process process = new Process();
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.RedirectStandardOutput = true;

                    // Output data received handler external process
                    process.OutputDataReceived += new DataReceivedEventHandler(OutputDataReceivedHandler);

                    // setting the file name and arguments
                    process.StartInfo.FileName = processFileName;
                    process.StartInfo.Arguments = processArguments;
                    process.Start();
                    // Start the asynchronous read of the output stream.
                    process.BeginOutputReadLine();

                    // Wait for cron job to exit
                    process.WaitForExit();

                    // Sleep for specified duration
                    Thread.Sleep(cronJobFrequencyInSecond * 1000);
                }
                catch (ThreadAbortException)
                {
                    Trace.TraceError("Cron job {0} {1} being aborted", processFileName, processArguments);
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Cron job {0} {1} failed: {2}", processFileName, processArguments, ex.Message);
                }
            }
        }

        // Stop all cron jobs
        public void StopAllCronJobs()
        {
            try
            {
                if (cronJobs.Count > 0)
                {
                    Trace.TraceInformation("Stopping all cron jobs...");
                    foreach (KeyValuePair<string, List<Thread>> pair in cronJobs)
                    {
                        List<Thread> threadList = pair.Value;
                        foreach (Thread thread in threadList)
                        {
                            if (thread != null)
                            {
                                try
                                {
                                    thread.Abort();
                                    thread.Join();
                                }
                                catch (ThreadStateException)
                                {
                                    thread.Resume();
                                }
                            }
                        }

                        // Clear List
                        threadList.Clear();
                    }

                    // Clear Dictionary
                    cronJobs.Clear();

                    Trace.TraceInformation("Stopped all cron jobs");
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error stopping all cron jobs. Error: {0}", ex.Message); 
            }
        }

        // Get all cron jobs with their status        
        public List<string> GetCronJobs()
        {
            List<string> allCronJobs = null;
            try
            {                
                if (cronJobs.Count > 0)
                {
                    allCronJobs = new List<string>();

                    XmlReader reader = XmlReader.Create(RoleEnvironment.GetConfigurationSettingValue("ProductListXmlFeed"));
                    SyndicationFeed feed = SyndicationFeed.Load(reader);
                    foreach (KeyValuePair<string, List<Thread>> pair in cronJobs)
                    {
                        string productId = pair.Key;
                        List<Thread> threadList = pair.Value;
                        string cronJobsInfo = GetCronJobsProperty(feed, productId);
                        string[] cronJobsInfoArray = cronJobsInfo.Split(',');
                        List<string> cronJobsInfoStatus = new List<string>();

                        for (int i = 0; i < cronJobsInfoArray.Length; i++)
                        {
                            Thread thread = threadList[i];

                            string isJobStarted = (thread != null).ToString();
                            string[] cronJobInfoArray = cronJobsInfoArray[i].Split(';');

                            // Set actual thread staus in cronJobInfoArray
                            cronJobInfoArray[4] = isJobStarted;
                            cronJobsInfoStatus.Add(string.Join(";", cronJobInfoArray));
                        }

                        allCronJobs.Add(productId + "," + string.Join(",", cronJobsInfoStatus.ToArray()));
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error in listing all cron jobs. Error: {0}", ex.Message);
            }

            return allCronJobs;
        }

        // Is Cron Job started for specified product id and cron job index?
        public bool IsCronJobStarted(string productId, int cronJobIndex)
        {
            if (cronJobs.Keys.Contains(productId))
            {
                List<Thread> threadList = cronJobs[productId];
                if (cronJobIndex >= 0 && cronJobIndex < threadList.Count)
                {
                    Thread thread = threadList[cronJobIndex];
                    if (thread != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // Start cron job for specified product id and cron job index
        public bool StartCronJob(string productId, int cronJobIndex)
        {
            if (cronJobs.Keys.Contains(productId))
            {
                List<Thread> threadList = cronJobs[productId];
                if (cronJobIndex >= 0 && cronJobIndex < threadList.Count)
                {
                    Thread thread = threadList[cronJobIndex];
                    if (thread == null)
                    {
                        string cronJobInfo = GetCronJobProperty(productId, cronJobIndex);
                        if (string.IsNullOrEmpty(cronJobInfo))
                        {
                            return false;
                        }

                        string[] installInfo = installationStatusCollection[productId].Split(',');
                        string installPath = installInfo[1];
                        string applicationInstallPath = Path.Combine(applicationsAndRuntimeResourceFolder,
                            WindowsAzureVMManager.ApplicationsFolder);
                        if (!installPath.Equals("/"))
                        {
                            applicationInstallPath = Path.Combine(applicationInstallPath,
                                installPath.Replace("/", "\\").Trim('\\'));
                        }
                        string[] cronJobInfoArray = cronJobInfo.Split(';');     
                        string cronJobFileName = Path.Combine(applicationInstallPath,
                            cronJobInfoArray[2].Replace("/", "\\").Trim('\\'));
                        int cronJobFrequencyInSecond = int.Parse(cronJobInfoArray[3]);

                        // Create a thread for cron job
                        string phpInstallFolder =
                            Path.Combine(applicationsAndRuntimeResourceFolder, WindowsAzureVMManager.RuntimeFolderForPHP);
                        string phpExeFileName = Path.Combine(phpInstallFolder, "php.exe");
                        ThreadStart starter = delegate { CronJobThreadRoutine(phpExeFileName, cronJobFileName, cronJobFrequencyInSecond); };
                        thread = new Thread(starter);
                        
                        // Replace null with new thread
                        threadList[cronJobIndex] = thread;
                        
                        thread.Start();
                        return true;
                    }
                    else
                    {
                        Trace.TraceError("Cron job already started.");
                        return false;
                    }
                }
            }

            return false;
        }

        // Stop cron job for specified product id and cron job index
        public bool StopCronJob(string productId, int cronJobIndex)
        {
            if (cronJobs.Keys.Contains(productId))
            {
                List<Thread> threadList = cronJobs[productId];
                if (cronJobIndex >= 0 && cronJobIndex < threadList.Count)
                {
                    Thread thread = threadList[cronJobIndex];
                    if (thread != null)
                    {
                        try
                        {
                            thread.Abort();
                            thread.Join();
                        }
                        catch (ThreadStateException)
                        {
                            thread.Resume();
                        }

                        // Set null into threadlist
                        threadList[cronJobIndex] = null;

                        return true;
                    }
                    else
                    {
                        Trace.TraceError("Cron job already stopped.");
                        return false;
                    }
                }
            }

            return false;
        }

        // Restart cron job for specified product id and cron job index
        public bool RestartCronJob(string productId, int cronJobIndex)
        {
            if (cronJobs.Keys.Contains(productId))
            {
                if (StopCronJob(productId, cronJobIndex))
                {
                    return StartCronJob(productId, cronJobIndex);
                }
                else
                {
                    return false;
                }  
            }
            else
            {
                return false;
            }
        }

        // Uninstall specified applications
        public bool UninstallApplications(List<string> applications)
        {
            return false;
        }

        // Set as application in IIS
        public bool SetAsApplicationInIIS(string installPath, string applicationPath)
        {
            try
            {
                // Get latest handle to PHP web site
                ServerManager serverManager = new ServerManager();
                Site secondaryWebSite = serverManager.Sites[WindowsAzureVMManager.SecondaryWebSiteName];
                if (secondaryWebSite != null)
                {
                    Trace.TraceInformation("Trying to set IIS Application at {0}.", applicationPath);
                    Application app = secondaryWebSite.Applications.Add(installPath, applicationPath);
                    if (app != null)
                    {
                        // Set application pool name as that of main web site
                        app.ApplicationPoolName = serverManager.Sites.First().Applications.First().ApplicationPoolName;
                        serverManager.CommitChanges();
                        serverManager.ApplicationPools[app.ApplicationPoolName].Recycle();
                        // Put some delay for IIS
                        Thread.Sleep(5000);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unable to set application in IIS. Error: {0}.", ex.Message);
                return false;
            }
        }

        // Get list of Windows Azure Drive Snapshots
        public List<string> GetWindowsAzureDriveSnapshots()
        {
            List<string> snapshotUris = new List<string>();

            try
            {
                NameValueCollection metadata = xdrivePageBlob.Metadata;
                foreach (string key in metadata.AllKeys)
                {
                    snapshotUris.Add(string.Join(",", new string[] { key, metadata[key] }));
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Could not get list of Windows Azure Drive snapshots. Error: {0}", ex.Message);
            }

            return snapshotUris;
        }

        // Create Windows Azure Drive Snapshot
        public string CreateWindowsAzureDriveSnapshot(string snapshotComment)
        {
            string uristring = null;
            try
            {
                if (IsDriveMounted())
                {
                    if (drive != null)
                    {
                        Uri uri = drive.Snapshot();
                        uristring = uri.ToString();

                        // Get snapshot timestamp
                        string timestamp = uristring.Split('?')[1].Split('=')[1];

                        // Get serialized status.xml
                        byte[] bytesToEncode = Encoding.UTF8.GetBytes(WindowsAzureVMManager.
                            SerializeNameValueCollectionToString(installationStatusCollection));
                        string status = Convert.ToBase64String(bytesToEncode);

                        // Add Uri to metadata of page blob
                        string metadata = string.Join(",", new string[] { snapshotComment, uristring, status });
                        xdrivePageBlob.Metadata.Add(timestamp, metadata);

                        Trace.TraceInformation("Created Windows Azure Drive snapshot {0}", uristring);
                    }
                    else
                    {
                        Trace.TraceError("Windows Azure Drive not mounted.");
                    }
                }
                else
                {
                    Trace.TraceError("Windows Azure Drive not mounted.");
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }

            return uristring;
        }

        // Promote Windows Azure Drive Snapshot
        public bool PromoteWindowsAzureDriveSnapshot(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                Trace.TraceError("Invalid snapshot uri {0}", uri);
                return false;
            }

            try
            {
                CloudBlob snapshotBlob = xdrivePageBlob.ServiceClient.GetBlobReference(uri);
                if (snapshotBlob.SnapshotTime.HasValue == false)
                {
                    Trace.TraceError("Invalid snapshot uri {0}", uri);
                    return false;
                }

                // Update progress information
                UpdateProgressInformation("Promoting Windows Azure Drive snapshot...", false);

                // Create a seperate thread for promoting blob snapshot
                ThreadStart starter = delegate { PromoteWindowsAzureDriveSnapshotOnAnotherThread(uri, snapshotBlob); };
                Thread thread = new Thread(starter);
                thread.Start();
            }
            catch (Exception ex)
            {
                UpdateProgressInformation("Could not promote Windows Azure Drive Snapshot. Error: " + ex.Message, true);
                return false;
            }

            return true;
        }

        // Promote Windows Azure Drive Snapshot on another thread
        private bool PromoteWindowsAzureDriveSnapshotOnAnotherThread(string uri, CloudBlob snapshotBlob)
        {
            try
            {
                // Get snapshot timestamp
                string timestamp = uri.Split('?')[1].Split('=')[1];

                // Get snapshot properties from metadata
                NameValueCollection metadata = new NameValueCollection(xdrivePageBlob.Metadata);
                string snapshotProperties = metadata[timestamp];

                // Stop all runtimes
                if (StopAllRuntimes() == false)
                {
                    UpdateProgressInformation("Unbale to stop all runtime and failed to promote Windows Azure Drive Snapshot.", true);
                    return false;
                }

                // Unmount the Windows Azure Drive
                if (IsDriveMounted())
                {
                    if (drive != null)
                    {
                        UpdateProgressInformation("Unmounting Windows Azure Drive...", false);
                        UnmountXDrive();
                        UpdateProgressInformation("Unmounted Windows Azure Drive.", false);
                        Thread.Sleep(5000);
                    }
                }

                if (WindowsAzureVMManager.BlobExists(xdrivePageBlob))
                {
                    // In cloud, blob snapshot delete works. It does not work in devfabric
                    xdrivePageBlob.CopyFromBlob(snapshotBlob);
                }
                else
                {
                    // In devfabric
                }

                // Restore status.xml
                UpdateProgressInformation("Restoring status.xml file...", false);
                if (string.IsNullOrEmpty(snapshotProperties))
                {
                    Trace.TraceError("Could not find metadata for Windows Azure Drive snapshot with timestamp={0}.", timestamp);
                }
                else
                {
                    string[] snapshotPropertiesArray = snapshotProperties.Split(',');
                    if (snapshotPropertiesArray.Length != 3)
                    {
                        Trace.TraceError("Could not find status information in metadata for Windows Azure Drive snapshot with timestamp={0}.", timestamp);
                    }
                    else
                    {
                        string base64StatusContent = snapshotPropertiesArray[2];
                        byte[] bytesToEncode = Convert.FromBase64String(base64StatusContent);
                        if (bytesToEncode == null)
                        {
                            Trace.TraceError("Could not read status information as bytearray in metadata for Windows Azure Drive snapshot.");
                        }
                        else
                        {
                            string statusContent = Encoding.UTF8.GetString(bytesToEncode);
                            installationStatusCollection = WindowsAzureVMManager.DeserializeNameValueCollectionFromString(statusContent);
                            WindowsAzureVMManager.SerializeNameValueCollectionToBlob(installationStatusCollection, installationStatusBlob);
                            UpdateProgressInformation("Restored status.xml file.", false);                            
                        }
                    }
                }

                // Mount drive again
                Thread.Sleep(5000);
                UpdateProgressInformation("Mounting Windows Azure Drive again...", false);
                if (MountXDrive())
                {
                    UpdateProgressInformation("Mounted Windows Azure Drive again.", false);

                    // Now reset metadata, as copyblob clears existing metadata
                    xdrivePageBlob.Metadata.Clear();
                    foreach (string key in metadata.AllKeys)
                    {
                        xdrivePageBlob.Metadata.Add(key, metadata[key]);
                    }
                    UpdateProgressInformation("Restored Windows Azure Drive snapshot metadata.", false);

                    // Setup Runtime Servers
                    SetupRuntimeServers();

                    ClearProgressInformation();
                    return true;
                }
                else
                {
                    UpdateProgressInformation("Could not promote Windows Azure Drive Snapshot as Widnows Azure Drive could not be remounted.", true);
                    return false;
                }
            }
            catch (Exception ex)
            {
                UpdateProgressInformation("Could not promote Windows Azure Drive Snapshot. Error: " + ex.Message, true);
                return false;
            }            
        }

        // Delete Windows Azure Drive Snapshot
        public bool DeleteWindowsAzureDriveSnapshot(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                return false;
            }
                        
            try
            {
                CloudBlob snapshotBlob = xdrivePageBlob.ServiceClient.GetBlobReference(uri);
                if (snapshotBlob.SnapshotTime.HasValue)
                {
                    // Get snapshot timestamp
                    string timestamp = uri.Split('?')[1].Split('=')[1];

                    // Get snapshot properties from metadata
                    NameValueCollection metadata = new NameValueCollection(xdrivePageBlob.Metadata);
                    string snapshotProperties = xdrivePageBlob.Metadata[timestamp];

                    if (WindowsAzureVMManager.BlobExists(xdrivePageBlob))
                    {
                        // In cloud, blob snapshot delete works. It does not work in devfabric
                        if (!snapshotBlob.DeleteIfExists())
                        {
                            return false;
                        }
                    }
                    else
                    {
                        string snapshotFolder = string.Empty;

                        // This is Windows Azure SDK 1.3 Specific, so might break in future
                        string csrunEnv = Environment.GetEnvironmentVariable("_CSRUN_STATE_DIRECTORY");
                        if (string.IsNullOrEmpty(csrunEnv))
                        {
                            snapshotFolder = Path.Combine(
                                Environment.GetEnvironmentVariable("LOCALAPPDATA"),
                                @"dftmp\wadd\devstoreaccount1\");
                        }
                        else
                        {
                            snapshotFolder = Path.Combine(csrunEnv, @"wadd\devstoreaccount1\");
                        }

                         // Get Windows Azure Drive container and blob names from service configuration file
                        string xdriveContainerName = RoleEnvironment.GetConfigurationSettingValue("PHPApplicationsBackupContainerName");
                        string xdriveBlobName = RoleEnvironment.GetConfigurationSettingValue("XDrivePageBlobName");
                        snapshotFolder = Path.Combine(snapshotFolder, xdriveContainerName);
                        snapshotFolder = Path.Combine(snapshotFolder, xdriveBlobName);
                        snapshotFolder += "!" + timestamp.Replace(":", "_");

                        // Delete the snapshot blob folder in devfabric
                        if (Directory.Exists(snapshotFolder))
                        {
                            Directory.Delete(snapshotFolder, true);
                        }
                    }

                    // Now reset metadata, as delete snapshot clears existing metadata
                    metadata.Remove(timestamp);
                    xdrivePageBlob.Metadata.Clear();
                    foreach (string key in metadata.AllKeys)
                    {
                        xdrivePageBlob.Metadata.Add(key, metadata[key]);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Could not delete blob snapshot. Error: {0}", ex.Message);
                return false;
            }

            return true;
        }

        // Unmount Windows Azure Drive
        public void UnmountXDrive()
        {
            if (IsDriveMounted())
            {
                if (drive != null)
                {
                    string uriString = drive.Uri.ToString();
                    Trace.TraceInformation("Unmounting Windows Azure Drive...");
                    drive.Unmount();
                    Trace.TraceInformation("Successfully unmounted Windows Azure Drive at Uri {0}", uriString);
                }
            }
        }

        // Mount XDrive
        public bool MountXDrive()
        {
            // Create HTTP storage endpoint, needed by Windows Azure Drive
            CloudStorageAccount xdriveStorageAccount = WindowsAzureVMManager.GetStorageAccount(false);

            // Initialize local cache
            LocalResource localCache = RoleEnvironment.GetLocalResource("XDriveLocalCache");
            Char[] backSlash = { '\\' };
            String localCachePath = localCache.RootPath.TrimEnd(backSlash);
            CloudDrive.InitializeCache(localCachePath, localCache.MaximumSizeInMegabytes);

            // Get Windows Azure Drive container and blob names from service configuration file
            string xdriveContainerName = RoleEnvironment.GetConfigurationSettingValue("PHPApplicationsBackupContainerName");
            string xdriveBlobName = RoleEnvironment.GetConfigurationSettingValue("XDrivePageBlobName");

            // Create blob container, if it does not exist
            CloudBlobClient blobClient = xdriveStorageAccount.CreateCloudBlobClient();
            blobClient.GetContainerReference(xdriveContainerName).CreateIfNotExist();

            // Get Windows Azure Drive page blob reference
            xdrivePageBlob = blobClient
                        .GetContainerReference(xdriveContainerName)
                        .GetPageBlobReference(xdriveBlobName);

            // Get a reference to the requested Windows Azure Drive
            drive = xdriveStorageAccount.CreateCloudDrive(
                    xdrivePageBlob.Uri.ToString()
                );

            // Create drive
            try
            {
                drive.Create(int.Parse(RoleEnvironment.GetConfigurationSettingValue("XDriveSizeInMB")));
            }
            catch (CloudDriveException)
            {
                // exception is also thrown if all is well but the drive already exists, hence ignore exception
            }

            try
            {
                // This is 1 VM solution only, so always mount drive in write mode. 
                string xdriveLetter = drive.Mount(
                    int.Parse(RoleEnvironment.GetConfigurationSettingValue("XDriveCacheSizeInMB")),
                    DriveMountOptions.None);
                Trace.TraceInformation("Mounted Windows Azure Drive at uri {0}", drive.Uri);
                Trace.TraceInformation("Applications are durable to Windows Azure Page Blob.");

                // Use different mechanism for devfabric and cloud to determine applicationsAndRuntimeResourceFolder
                if (RoleEnvironment.DeploymentId.StartsWith("deployment"))
                {
                    // This is Windows Azure SDK 1.3 Specific, so might break in future
                    string csrunEnv = Environment.GetEnvironmentVariable("_CSRUN_STATE_DIRECTORY");
                    if (string.IsNullOrEmpty(csrunEnv))
                    {
                        applicationsAndRuntimeResourceFolder = Path.Combine(
                            Environment.GetEnvironmentVariable("LOCALAPPDATA"),
                            @"dftmp\wadd\devstoreaccount1\");
                    }
                    else
                    {
                        applicationsAndRuntimeResourceFolder = Path.Combine(csrunEnv, @"wadd\devstoreaccount1\");
                    }

                    // Get Windows Azure Drive container and blob names from service configuration file
                    applicationsAndRuntimeResourceFolder = Path.Combine(applicationsAndRuntimeResourceFolder, xdriveContainerName);
                    applicationsAndRuntimeResourceFolder = Path.Combine(applicationsAndRuntimeResourceFolder, xdriveBlobName);
                }
                else
                {
                    applicationsAndRuntimeResourceFolder = xdriveLetter;
                }

                return true;
            }
            catch (Exception ex)
            {
                applicationsAndRuntimeResourceFolder = null;
                Trace.TraceError("Unable to Mount Windows Azure Drive. Error: {0}, StackTrace: {1}", ex.Message, ex.StackTrace);                
                return false;
            }
        }

        // Get drive mount status
        public bool IsDriveMounted()
        {
            if (applicationsAndRuntimeResourceFolder != null)
                return true;
            else
                return false;
        }

        // Reset Windows Azure Drive
        public bool ResetWindowsAzureDrive()
        {
            try
            {
                // Update progress information
                UpdateProgressInformation("Resetting Windows Azure Drive", false);

                // Create a seperate thread for installing applications
                ThreadStart starter = delegate { ResetWindowsAzureDriveOnAnotherThread(); };
                Thread thread = new Thread(starter);
                thread.Start();
            }
            catch (Exception ex)
            {
                UpdateProgressInformation("Unable to start resetting Windows Azure Drive. Error: " + ex.Message, true);
                return false;
            }

            return true;
        }

        // Reset Windows Azure Drive On Another Thread
        public bool ResetWindowsAzureDriveOnAnotherThread()
        {
            Trace.TraceInformation("Started Reset for Windows Azure Drive...");

            try
            {
                // Stop all runtimes
                if (StopAllRuntimes() == false)
                {
                    UpdateProgressInformation("Unbale to stop all runtime and failed to reset Windows Azure Drive.", true);                    
                    return false;
                }

                // Delete PHP Web site from IIS (if any)
                ServerManager serverManager = new ServerManager();
                Site secondaryWebSite = serverManager.Sites[WindowsAzureVMManager.SecondaryWebSiteName];
                if (secondaryWebSite != null)
                {
                    UpdateProgressInformation("Deleting PHP Web site...", false);                    
                    DeletePHPWebSite();
                    UpdateProgressInformation("Deleted PHP Web site.", false);
                }
               
                // Unmount and delete Windows Azure Drive
                if (IsDriveMounted())
                {
                    if (drive != null)
                    {
                        UpdateProgressInformation("Unmounting Windows Azure Drive...", false);
                        UnmountXDrive();
                        UpdateProgressInformation("Unmounted Windows Azure Drive.", false);
                        Thread.Sleep(5000);

                        // Delete all snapshots and parent page blob
                        if (WindowsAzureVMManager.BlobExists(xdrivePageBlob))
                        {
                            xdrivePageBlob.Delete(new BlobRequestOptions()
                            {
                                DeleteSnapshotsOption = DeleteSnapshotsOption.IncludeSnapshots
                            });

                            UpdateProgressInformation("Deleted pageblob and associated snapshots for the Windows Azure Drive.", false);
                        }
                        else
                        {
                            // In devfabric, delete the drive
                            drive.Delete();
                            UpdateProgressInformation("Deleted Windows Azure Drive.", false);
                        }

                        // Reset references
                        xdrivePageBlob = null;
                        drive = null;
                    }
                }

                // Reset status.xml file
                UpdateProgressInformation("Resetting status.xml file...", false);
                installationStatusCollection.Clear();
                SerializeNameValueCollectionToBlob(installationStatusCollection, installationStatusBlob);
                UpdateProgressInformation("Reset status.xml file completed.", false);

                // Mount drive again
                UpdateProgressInformation("Mounting Windows Azure Drive again...", false);
                if (MountXDrive())
                {
                    UpdateProgressInformation("Reset Windows Azure Drive completed.", false);
                }
                else
                {
                    UpdateProgressInformation("Unable to reset Windows Azure Drive as Windows Azure Drive could not be remounted", false);
                    return false;
                }
            }
            catch (Exception ex)
            {
                UpdateProgressInformation("Unable to reset Windows Azure Drive. Error: " + ex.Message, true);
                return false;
            }

            ClearProgressInformation();
            return true;
        }

        // Stop all runtimes
        private bool StopAllRuntimes()
        {
            try
            {
                // Stop PHP Web Site
                UpdateProgressInformation("Stopping PHP Web Site.", false);
                StopPHPWebSite();
                UpdateProgressInformation("Stopped PHP Web Site.", false);
                Thread.Sleep(5000);

                // Stop MySQL based database sever
                if ((installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.MySQLCommunityServerProductID))
                        || (installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.MariaDBProductID)))
                {
                    UpdateProgressInformation("Stopping MySQL based database server.", false);
                    if (MySQLCommunityServerInstaller.StopMySQLBasedDB())
                    {
                        UpdateProgressInformation("Stopped MySQL based database server.", false);
                    }
                    else
                    {
                        UpdateProgressInformation("Failed to stop MySQL based database server.", false);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unable to stop all runtimes. Error: {0}", ex.Message);
                return false;
            }

            return true;
        }

        // Clear progress information
        public void ClearProgressInformation()
        {
            // Serialize progress information to the blob
            string blobName = RoleEnvironment.GetConfigurationSettingValue("ProgressInformationFileBlob");
            NameValueCollection progressInformation = new NameValueCollection();            
            CloudBlob progressInformationBlob = GetCloudBlobReference(blobName);
            WindowsAzureVMManager.SerializeNameValueCollectionToBlob(
                progressInformation, 
                progressInformationBlob);
        }

        // Is background task running (installation/reset)
        public bool IsBackgroundTaskRunning()
        {
            IDictionary<string, string> progressInformation = GetProgressInformation();
            if (progressInformation == null || progressInformation.Count == 0)
            {
                return false;
            }
            else
            {
                // Remove title
                string titleKey = progressInformation.Keys.First();
                progressInformation.Remove(titleKey);
                if (progressInformation.Count > 0)
                {
                    // Check if last message is an error
                    string lastKey = progressInformation.Keys.Last();
                    string[] messageInfo = progressInformation[lastKey].Split('|');
                    if (messageInfo[2].ToLower().Equals("true"))
                    {
                        // Error found indicating async operation failed
                        throw new Exception(messageInfo[1]);
                    }
                    else
                    {
                        // No error, background task is still running
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
        }

        // Get progress information of current activity
        public IDictionary<string, string> GetProgressInformation()
        {
            NameValueCollection progressInformation = null;
            lock (WindowsAzureVMManager.locker)
            {
                // Deserialize NameValueCollection from blob
                string blobName = RoleEnvironment.GetConfigurationSettingValue("ProgressInformationFileBlob");
                CloudBlob progressInformationBlob = GetCloudBlobReference(blobName);                
                progressInformation =
                    DeserializeNameValueCollectionFromBlob(progressInformationBlob);                
            }

            // Transform NameValueCollection to Dictionary. 
            // TODO: Need to get rid of all this dirty work
            Dictionary<string, string> dicProgressInformation = new Dictionary<string, string>();
            if (progressInformation != null)
            {
                foreach (string key in progressInformation.AllKeys)
                {
                    dicProgressInformation.Add(key, progressInformation[key]);
                }
            }
            return dicProgressInformation;
        }
   
        // Add progress information
        public void UpdateProgressInformation(string message, bool isError)
        {
            lock (WindowsAzureVMManager.locker)
            {
                // Add to trace logs
                if (isError)
                {
                    Trace.TraceError(message);
                }
                else
                {
                    Trace.TraceInformation(message);
                }

                // Deserialize NameValueCollection from blob
                string blobName = RoleEnvironment.GetConfigurationSettingValue("ProgressInformationFileBlob");
                CloudBlob progressInformationBlob = GetCloudBlobReference(blobName);
                NameValueCollection progressInformation =
                    DeserializeNameValueCollectionFromBlob(progressInformationBlob);

                progressInformation.Add(Guid.NewGuid().ToString(),
                        string.Join("|", new string[] { 
                            DateTime.Now.ToString(),
                            message,
                            isError.ToString().ToLower()
                        })
                    );

                // Serialize progress information to the blob
                WindowsAzureVMManager.SerializeNameValueCollectionToBlob(
                    progressInformation,
                    progressInformationBlob);
            }
        }

        // Get downlolad url from the version
        public static string GetDownloadUrlFromProductVersion(SyndicationItem product, string productVersion)
        {
            SyndicationElementExtension elementExtension =
                product.ElementExtensions.Where<SyndicationElementExtension>
                (x => x.OuterName == "installerFileChoices").FirstOrDefault();
            if (elementExtension != null)
            {
                XElement element = elementExtension.GetObject<XElement>();

                // TODO: Use Linq Query instead of foreach loop
                foreach (XElement extension in element.Elements())
                {
                    if (extension.Attribute("version").Value.Equals(productVersion))
                    {
                        return extension.Attribute("url").Value;
                    }
                }
            }

            // download url not found
            return null;
        }

        // Get attribute value from product version
        public static string GetAttributeValueFromProductVersion(SyndicationItem product, string productVersion, string attributeName)
        {
            SyndicationElementExtension elementExtension =
                product.ElementExtensions.Where<SyndicationElementExtension>
                (x => x.OuterName == "installerFileChoices").FirstOrDefault();
            if (elementExtension != null)
            {
                XElement element = elementExtension.GetObject<XElement>();

                // TODO: Use Linq Query instead of foreach loop
                foreach (XElement extension in element.Elements())
                {
                    if (extension.Attribute("version").Value.Equals(productVersion))
                    {
                        if (extension.Elements().Count() > 0)
                        {
                            foreach (XElement propertyElement in extension.Elements().First().Elements())
                            {
                                if (propertyElement.Attribute("name").Value.Equals(attributeName))
                                {
                                    XAttribute valueAttribute = propertyElement.Attribute("value");
                                    if (valueAttribute != null)
                                    {
                                        return valueAttribute.Value;
                                    }
                                    else
                                    {
                                        // Attribute not found
                                        return null;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Attribute not found
            return null;
        }

        // Whether PHP Web Site is started
        public bool IsPHPWebSiteStarted()
        {
            ServerManager serverManager = new ServerManager();
            Site secondaryWebSite = serverManager.Sites[WindowsAzureVMManager.SecondaryWebSiteName];
            if (secondaryWebSite != null)
            {
                if (secondaryWebSite.State == ObjectState.Started)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
      
        // Register SSL endpoint with http.sys
        private void RegisterSSLWithHTTPSys(string endpointName, string sslCertificateSHA1Thumbprint)
        {
            try
            {
                Trace.TraceInformation(string.Format("Enabling SSL for {0} ...", endpointName));
                IPEndPoint endpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints[endpointName].IPEndpoint;
                string processArguments = string.Format("http add sslcert ipport={0}:{1} certstorename=MY certhash={2} appid={{{3}}}",
                    endpoint.Address.ToString(),
                    endpoint.Port.ToString(),
                    sslCertificateSHA1Thumbprint,
                    Guid.NewGuid()
                );

                // Launch self extracting MSI
                Process process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.FileName = "netsh.exe";
                process.StartInfo.Arguments = processArguments;
                
                // Output data received handler for cscript netsh.exe command
                process.OutputDataReceived += new DataReceivedEventHandler(OutputDataReceivedHandler);

                Trace.TraceInformation(string.Format("Executing command: {0} {1}", 
                    process.StartInfo.FileName, 
                    process.StartInfo.Arguments));

                process.Start();

                // Start the asynchronous read of the output stream.
                process.BeginOutputReadLine();

                // Wait until process is over
                process.WaitForExit();

                Trace.TraceInformation(string.Format("Registerd SSL for input endpoint {0}.", endpointName));
            }
            catch (Exception ex)
            {
                Trace.TraceError(string.Format("Failed to register SSL for input endpoint {0}. Error: {1}", endpointName, ex.Message));
            }
        }

        // Enable SSL for web site
        private void EnableSSLForWebSite(Site webSite, string endpointName, string sslCertificateSHA1Thumbprint)
        {
            try
            {
                //For some reason this created a new store called 'Personal' - we'll have to figure that out
                X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadWrite);

                //Looks like we can create this from a byte array as well
                X509Certificate2Collection col = store.Certificates.Find(
                    X509FindType.FindByThumbprint,
                    sslCertificateSHA1Thumbprint, 
                    false);
                if (col.Count == 1)
                {
                    IPEndPoint endpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints[endpointName].IPEndpoint;
                    ServerManager serverManager = new ServerManager();
                    webSite.Bindings.Add(
                        endpoint.Address.ToString() + ":" + endpoint.Port.ToString() + ":",
                        col[0].GetCertHash(),
                        store.Name);
                    serverManager.CommitChanges();

                    // Put some delay for IIS
                    Thread.Sleep(5000);

                    Trace.TraceInformation("Enabled SSL for input endpoint {0}.", endpointName);
                }
                else
                {
                    Trace.TraceError("SSL sertificate not found for input endpoint {0}.", endpointName);
                }

                store.Close();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to enable SSL for input endpoint {0}. Error: {1}", endpointName, ex.Message);
            }
        }

        // Setup Runtime Servers
        public void SetupRuntimeServers()
        {
            if (StartPHPWebSite())
            {
                Trace.TraceInformation("Started the PHP Web Site.");
            }

            if (StartMySQLServer())
            {
                Trace.TraceInformation("Started the {0}", MySQLBasedDBInstaller.GetMySQLBasedDBName());
            }
        }

        // Has MySQL Server installed?
        public bool IsMySQLServerInstalled()
        {
            if ((installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.MySQLCommunityServerProductID))
                        || (installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.MariaDBProductID)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Has MySQL Server started?
        public bool IsMySQLServerStarted()
        {
            if ((installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.MySQLCommunityServerProductID))
                        || (installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.MariaDBProductID)))
            {
                return MySQLCommunityServerInstaller.IsMySQLServerStarted();
            }
            else
            {
                return false;
            }
        }

        // Get MySQL based database server port number
        public string GetMySQLBasedDBServerPortNumber()
        {
            if ((installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.MySQLCommunityServerProductID))
                        || (installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.MariaDBProductID)))
            {
                return MySQLCommunityServerInstaller.GetMySQLBasedDBServerPortNumber();
            }
            else
            {
                return null;
            }
        }

        // Get MySQL based database name
        public string GetMySQLBasedDBName()
        {
            return MySQLCommunityServerInstaller.GetMySQLBasedDBName();
        }

        // Get MySQL based database ini file name
        public string GetMySQLBasedDBIniFileName()
        {
            string mysqlInstallInfo = null;
            if (installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.MySQLCommunityServerProductID))
            {
                mysqlInstallInfo = installationStatusCollection[WindowsAzureVMManager.MySQLCommunityServerProductID];
                return mysqlInstallInfo.Split(',')[3];
            }
            else if (installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.MariaDBProductID))
            {
                mysqlInstallInfo = installationStatusCollection[WindowsAzureVMManager.MariaDBProductID];
                return mysqlInstallInfo.Split(',')[3];
            }
            else
            {
                Trace.TraceError("Invalid product id for MySQL based database server.");
                return null;
            }
        }

        // Get MySQL based database installation folder
        public string GetMySQLBasedDBInstallationFolder()
        {
            if (installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.MySQLCommunityServerProductID))
            {
                return Path.Combine(applicationsAndRuntimeResourceFolder, WindowsAzureVMManager.MySQLCommunityServerFolder);
            }
            else if (installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.MariaDBProductID))
            {
                return Path.Combine(applicationsAndRuntimeResourceFolder, WindowsAzureVMManager.MariaDBServerFolder);
            }
            else
            {
                Trace.TraceError("Invalid product id for MySQL based database server.");
                return null;
            }
        }

        // Get MySQL based database server IP address
        public string GetMySQLBasedDBServerIPAddress()
        {
            if ((installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.MySQLCommunityServerProductID))
                        || (installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.MariaDBProductID)))
            {
                return MySQLCommunityServerInstaller.GetMySQLBasedDBServerIPAddress();
            }
            else
            {
                return null;
            }            
        }

        // Start MySQL based database server
        public bool StartMySQLServer()
        {
            // Start MySQL based database server (only if installed)
            if ((installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.MySQLCommunityServerProductID))
                        || (installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.MariaDBProductID)))
            {
                try
                {
                    // Set MySQL based database name
                    string installationFolder = null;
                    string mySqlBasedDBProductID = null;
                    string mysqlInstallInfo = null;
                    if (installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.MySQLCommunityServerProductID))
                    {
                        mysqlInstallInfo = installationStatusCollection[WindowsAzureVMManager.MySQLCommunityServerProductID];
                        mySqlBasedDBProductID = WindowsAzureVMManager.MySQLCommunityServerProductID;
                        installationFolder = Path.Combine(applicationsAndRuntimeResourceFolder, WindowsAzureVMManager.MySQLCommunityServerFolder);
                    }
                    else if (installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.MariaDBProductID))
                    {
                        mysqlInstallInfo = installationStatusCollection[WindowsAzureVMManager.MariaDBProductID];
                        mySqlBasedDBProductID = WindowsAzureVMManager.MariaDBProductID;
                        installationFolder = Path.Combine(applicationsAndRuntimeResourceFolder, WindowsAzureVMManager.MariaDBServerFolder);
                    }
                    else
                    {
                        Trace.TraceError("Invalid product id for MySQL based database server.");
                        return false;
                    }

                    // Get MySQL based database name from product id
                    XmlReader reader = XmlReader.Create(RoleEnvironment.GetConfigurationSettingValue("ProductListXmlFeed"));
                    SyndicationFeed feed = SyndicationFeed.Load(reader);
                    var varSqlBasedDBName = from item in feed.Items
                                        where item.ElementExtensions.ReadElementExtensions<string>("productId", "http://www.w3.org/2005/Atom")[0].Equals(mySqlBasedDBProductID)
                                        select item.Title.Text.ToString();

                    // Start MySQL based DB using installation info
                    string iniFileName = mysqlInstallInfo.Split(',')[3];
                    MySQLCommunityServerInstaller.StartConfiguredMySQL(
                        installationFolder, 
                        iniFileName, 
                        varSqlBasedDBName.FirstOrDefault().ToString());
                    return true;
                }
                catch (Exception ex)
                {
                    Trace.TraceError("StartMySQLServer Error: {0}, StackTrace: {1}", ex.Message, ex.StackTrace);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }        

        // Stop MySQL based database server
        public bool StopMySQLServer()
        {
            if ((installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.MySQLCommunityServerProductID))
                        || (installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.MariaDBProductID)))
            {
                return MySQLCommunityServerInstaller.StopMySQLBasedDB();
            }
            else
            {
                Trace.TraceError("MySQL based database server is not installed.");
                return false;
            }
        }

        // Restart MySQL based database server
        public bool RestartMySQLServer()
        {
            if ((installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.MySQLCommunityServerProductID))
                        || (installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.MariaDBProductID)))
            {
                if (MySQLCommunityServerInstaller.StopMySQLBasedDB())
                {
                    // Start MySQL server with existing data files
                    StartMySQLServer();
                    return true;
                }
                else
                {
                    Trace.TraceError("Could not stop MySQL based database server.");
                    return false;
                }
            }
            else
            {
                Trace.TraceError("MySQL based database server is not installed.");
                return false;
            }
        }
        
        // Start PHP Web Site
        public bool StartPHPWebSite()
        {
            // Start PHP Website only if PHP runtime is installed
            if (installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.PHPRuntimeProductID))
            {
                try
                {
                    // Re-configure PHP Runtime, as php.ini may contain old Windows Azure Drive letter
                    PHPRuntimeInstaller.ReConfigurePHPRuntime(
                        Path.Combine(applicationsAndRuntimeResourceFolder,
                        WindowsAzureVMManager.RuntimeFolderForPHP));

                    StartPHPWebSite(applicationsAndRuntimeResourceFolder);

                    // Also setup cron jobs for installed applications (if any)
                    SetupCronJobsForInstalledApplications();

                    return true;
                }
                catch (Exception ex)
                {
                    Trace.TraceError("StartPHPWebSite Error: {0}, StackTrave: {1}", ex.Message, ex.StackTrace);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        // Stop PHP Web Site
        public bool StopPHPWebSite()
        {
            // Get latest handle to PHP web site
            ServerManager serverManager = new ServerManager();
            Site secondaryWebSite = serverManager.Sites[WindowsAzureVMManager.SecondaryWebSiteName];
            if (secondaryWebSite != null)
            {
                try
                {
                    if (secondaryWebSite.State == ObjectState.Started)
                    {
                        // Stop all cron jobs for installed applications (if any)
                        StopAllCronJobs();
                        secondaryWebSite.Stop();
                        return true;
                    }
                    else
                    {
                        Trace.TraceError("PHP Website already stopped.");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError("StopPHPWebSite Error: {0}, StackTrace: {1}", ex.Message,  ex.StackTrace);
                    return false;
                }
            }
            else
            {
                Trace.TraceError("PHP Web site not yet created.");
                return false;
            }
        }

        // Restart PHP Web Site
        public bool RestartPHPWebSite()
        {
            // Get latest handle to PHP web site
            ServerManager serverManager = new ServerManager();
            Site secondaryWebSite = serverManager.Sites[WindowsAzureVMManager.SecondaryWebSiteName];
            if (secondaryWebSite != null)
            {
                try
                {
                    if (secondaryWebSite.State == ObjectState.Started)
                    {
                        // Stop PHP Web Site
                        StopPHPWebSite();

                        // Restart PHP Web Site
                        StartPHPWebSite();
                        Trace.TraceInformation("PHP Web site restarted.");

                        return true;
                    }
                    else
                    {
                        Trace.TraceError("PHP Website was not started.");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError("RestartPHPWebSite Error: {0}, StackTrace: {1}", ex.Message, ex.StackTrace);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        // Delete PHP Web Site
        public bool DeletePHPWebSite()
        {
            // Get latest handle to PHP web site
            ServerManager serverManager = new ServerManager();
            Site secondaryWebSite = serverManager.Sites[WindowsAzureVMManager.SecondaryWebSiteName];

            if (secondaryWebSite != null)
            {
                try
                {
                    if (secondaryWebSite.State == ObjectState.Started)
                    {
                        // Stop all cron jobs for installed applications (if any)
                        StopAllCronJobs();
                        secondaryWebSite.Stop();
                    }

                    // Kill all php-cgi.exe processes, if running
                    WindowsAzureVMManager.FindAndKillProcess("php-cgi");

                    // Clear FastCGI handlers
                    Configuration config = serverManager.GetApplicationHostConfiguration();
                    ConfigurationSection fastCgiSection = config.GetSection("system.webServer/fastCgi");
                    ConfigurationElementCollection fastCgiCollection = fastCgiSection.GetCollection();
                    fastCgiCollection.Clear();
                    
                    serverManager.Sites.Remove(secondaryWebSite);
                    serverManager.CommitChanges();

                    // Put some delay for IIS
                    Thread.Sleep(5000);

                    secondaryWebSite = null;

                    return true;
                }
                catch (Exception ex)
                {
                    Trace.TraceError("DeletePHPWebSite Error: {0}, StackTrace: {1}", ex.Message, ex.StackTrace);
                    return false;
                }
            }
            else
            {
                Trace.TraceError("PHP Web site not yet created.");
                return false;
            }
        }

        // Get application root folder
        public string GetApplicationsFolder()
        {
            return Path.Combine(applicationsAndRuntimeResourceFolder, WindowsAzureVMManager.ApplicationsFolder);
        }

        // Get php.ini file name
        public string GetPHPIniFileName()
        {
            // Start PHP Website only if PHP runtime is installed
            if (installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.PHPRuntimeProductID))
            {
                string phpIniFileName = 
                    Path.Combine(
                        Path.Combine(applicationsAndRuntimeResourceFolder, WindowsAzureVMManager.RuntimeFolderForPHP),
                        "php.ini");
                if (File.Exists(phpIniFileName))
                {
                    return phpIniFileName;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        // Get php.exe file name
        public string GetPHPExeFileName()
        {
            // Start PHP Website only if PHP runtime is installed
            if (installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.PHPRuntimeProductID))
            {
                string phpExeFileName =
                    Path.Combine(
                        Path.Combine(applicationsAndRuntimeResourceFolder, WindowsAzureVMManager.RuntimeFolderForPHP),
                        "php.exe");
                if (File.Exists(phpExeFileName))
                {
                    return phpExeFileName;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        // Get php log file name
        public string GetPHPLogFileName()
        {
            // Start PHP Website only if PHP runtime is installed
            if (installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.PHPRuntimeProductID))
            {
                // Set log folder and file name
                string logFileName = Path.Combine(
                    Path.Combine(
                        Path.Combine(applicationsAndRuntimeResourceFolder,
                        WindowsAzureVMManager.RuntimeFolderForPHP), 
                        "logs"), 
                        "log.txt");
                if (File.Exists(logFileName))
                {
                    return logFileName;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        // Set php.ini content
        public bool SetPHPIniContent(string content)
        {
            if (installationStatusCollection.AllKeys.Contains(WindowsAzureVMManager.PHPRuntimeProductID))
            {
                // Get php.ini file name
                string phpIniFileName = Path.Combine(
                        Path.Combine(applicationsAndRuntimeResourceFolder, WindowsAzureVMManager.RuntimeFolderForPHP),
                        "php.ini");
                if (File.Exists(phpIniFileName))
                {
                    // Update php.ini file
                    StreamWriter streamWriter = File.CreateText(phpIniFileName);
                    streamWriter.Write(content);
                    streamWriter.Close();

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        // Start PHP Web Site within specified resource
        /// <summary>
        /// Starts the PHP web site.
        /// </summary>
        /// <param name="resourceFolder">The resource folder.</param>
        private void StartPHPWebSite(string resourceFolder)
        {
            try
            {
                IPEndPoint httpEndpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["HttpIn"].IPEndpoint;

                string phpFullPath = Path.Combine(resourceFolder, WindowsAzureVMManager.RuntimeFolderForPHP + @"\php-cgi.exe");
                string phpArguments = "-c " + Path.Combine(resourceFolder, WindowsAzureVMManager.RuntimeFolderForPHP + @"\php.ini");

                // Get latest handle to PHP web site
                ServerManager serverManager = new ServerManager();
                Site secondaryWebSite = serverManager.Sites[WindowsAzureVMManager.SecondaryWebSiteName];

                // If available, get SSL certificate from service configuration
                string sslCertificateSHA1Thumbprint = null;
                try
                {
                    sslCertificateSHA1Thumbprint = RoleEnvironment.GetConfigurationSettingValue("SSLCertificateSHA1Thumbprint");
                }
                catch (Exception)
                {
                    // Ignore, it means SSLCertificateSHA1Thumbprint is not defined
                }

                // Get main web site application pool name
                string applicationPoolName = serverManager.Sites.First().Applications.First().ApplicationPoolName;

                if (secondaryWebSite == null)
                {
                    Configuration config = serverManager.GetApplicationHostConfiguration();
                    ConfigurationSection fastCgiSection = config.GetSection("system.webServer/fastCgi");
                    ConfigurationElementCollection fastCgiCollection = fastCgiSection.GetCollection();
                    try
                    {
                        ConfigurationElement applicationElement = fastCgiCollection.CreateElement("application");
                        applicationElement["fullPath"] = phpFullPath;
                        applicationElement["arguments"] = phpArguments;
                        applicationElement["maxInstances"] = 12;
                        applicationElement["idleTimeout"] = 1800;
                        applicationElement["activityTimeout"] = 1800;
                        applicationElement["requestTimeout"] = 1800;
                        applicationElement["instanceMaxRequests"] = 10000;
                        applicationElement["protocol"] = @"NamedPipe";
                        applicationElement["flushNamedPipe"] = false;

                        ConfigurationElementCollection environmentVariablesCollection = applicationElement.GetCollection("environmentVariables");                        

                        // Add php to the PATH environment variable
                        ConfigurationElement environmentVariableElement = environmentVariablesCollection.CreateElement("environmentVariable");
                        environmentVariableElement["name"] = @"PATH";
                        string path = Environment.GetEnvironmentVariable(@"PATH");
                        environmentVariableElement["value"] = path + ";" + Path.Combine(resourceFolder, WindowsAzureVMManager.RuntimeFolderForPHP);
                        environmentVariablesCollection.Add(environmentVariableElement);

                        // Set PHP_FCGI_MAX_REQUESTS envirionement variable
                        environmentVariableElement = environmentVariablesCollection.CreateElement("environmentVariable");
                        environmentVariableElement["name"] = @"PHP_FCGI_MAX_REQUESTS";
                        environmentVariableElement["value"] = @"10000";
                        environmentVariablesCollection.Add(environmentVariableElement);

                        fastCgiCollection.Add(applicationElement);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceWarning("Ignored error: {0}", ex.Message);
                    }

                    // Prepare web.config for PHP Web Site
                    var parameters = new NameValueCollection();
                    if (!resourceFolder.EndsWith(@"\"))
                    {
                        resourceFolder = resourceFolder + @"\";
                    }
                    parameters["resourceFolder"] = resourceFolder;
                    string approot = Environment.GetEnvironmentVariable("RoleRoot") + @"\approot";
                    string webConfigFileName = Path.Combine(
                        Path.Combine(resourceFolder, WindowsAzureVMManager.ApplicationsFolder),
                        "web.config");
                    if (File.Exists(webConfigFileName))
                        File.Delete(webConfigFileName);
                    FileUtils.WriteConfigFile(
                        parameters,
                        Path.Combine(approot, @"bin\ResourcesForPHP\InnerWeb.config"),
                        webConfigFileName);

                    // Add phpinfo.php file at the root of web site
                    string phpinfoFileName = Path.Combine(Path.Combine(resourceFolder, WindowsAzureVMManager.ApplicationsFolder),
                        "phpinfo.php");
                    if (!File.Exists(phpinfoFileName))
                    {
                        File.Copy(Path.Combine(approot, @"bin\ResourcesForPHP\phpinfo.php"),
                            phpinfoFileName);
                    }
                    
                    // Create new PHP Web Site
                    secondaryWebSite = serverManager.Sites.Add(WindowsAzureVMManager.SecondaryWebSiteName, 
                        "http",
                        httpEndpoint.Address.ToString() + ":" + httpEndpoint.Port.ToString() + ":",
                        Path.Combine(applicationsAndRuntimeResourceFolder, WindowsAzureVMManager.ApplicationsFolder));
                    if (!string.IsNullOrEmpty(sslCertificateSHA1Thumbprint))
                    {
                        EnableSSLForWebSite(secondaryWebSite, "HttpsIn", sslCertificateSHA1Thumbprint);
                    }
                    
                    // Set PHP web site application pool name
                    secondaryWebSite.Applications.First().ApplicationPoolName = applicationPoolName;
                    
                    // Set each installed product as application in IIS
                    IDictionary<string, string> productsInstalled = GetInstalledProductsInfo();
                    foreach (string productId in productsInstalled.Keys)
                    {
                        string valueInstalledProduct = productsInstalled[productId];
                        string[] productInstalledInfo = valueInstalledProduct.Split(',');
                        string installPath = productInstalledInfo[1].Trim();

                        if (!installPath.Equals("/"))
                        {
                            string applicationPath = Path.Combine(
                                Path.Combine(applicationsAndRuntimeResourceFolder, WindowsAzureVMManager.ApplicationsFolder),
                                installPath.Replace("/", "\\").Trim('\\'));

                            Application app = secondaryWebSite.Applications.Add(installPath, applicationPath);
                            if (app != null)
                            {
                                // Set application pool name as that of main web site
                                app.ApplicationPoolName = applicationPoolName;                            
                            }
                        }
                    }                    
                }

                serverManager.CommitChanges();

                // Put some delay for IIS
                Thread.Sleep(5000);

                serverManager.ApplicationPools[applicationPoolName].Recycle();

                // Start the new PHP Web Site
                secondaryWebSite.Start();

                // Put some delay for IIS
                Thread.Sleep(5000);

                if (!string.IsNullOrEmpty(sslCertificateSHA1Thumbprint))
                {
                    IPEndPoint httpsEndpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["HttpsIn"].IPEndpoint;
                    Trace.TraceInformation("Started PHP Web Site in IIS on http port {0} and https port {1}", 
                        httpEndpoint.Port.ToString(),
                        httpsEndpoint.Port.ToString());
                }
                else
                {
                    Trace.TraceInformation("Started PHP Web Site in IIS on http port {0}", 
                        httpEndpoint.Port.ToString());
                }
            }
            catch (Exception ex)
            {
                // Logging the exceptiom
                Trace.TraceError(ex.Message);
            }
        }

        // Output data received handler for invoked processes
        static void OutputDataReceivedHandler(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            Trace.TraceInformation(e.Data);
        }        

        // Download and extract web archive
        // TODO: Currently this can only handle .zip and self extracting .exe zip files
        // Handle other archives like .tar, .gz, .bz, .tar.gz or .tgz, .tar.bz or .tbz etc
        public static void DownloadAndExtractWebArchive(string downloadUrl,
            string downloadFileName,
            string downloadFolder,
            string extractFolder,
            string applicationPath)
        {
            // Get underlying VM Manager
            IVMManager vmManager = WindowsAzureVMManager.GetVMManager();

            if (string.IsNullOrEmpty(downloadUrl))
            {
                Trace.TraceError("Invalid download URL");
                throw new InstallException("Invalid download URL");
            }

            try
            {
                // Get file extension
                string extension = null;
                if (downloadUrl.ToLower().EndsWith(".zip"))
                {
                    extension = "zip";
                }
                else if (downloadUrl.ToLower().EndsWith(".exe"))
                {
                    extension = "exe";
                }
                else if (downloadUrl.ToLower().EndsWith(".tar") ||
                    downloadUrl.ToLower().EndsWith(".tgz") ||
                    downloadUrl.ToLower().EndsWith(".gz") ||
                    downloadUrl.ToLower().EndsWith(".gzip") ||
                    downloadUrl.ToLower().EndsWith(".tbz") ||
                    downloadUrl.ToLower().EndsWith(".tbz2") ||
                    downloadUrl.ToLower().EndsWith(".bz2") ||
                    downloadUrl.ToLower().EndsWith(".bzip2"))
                {
                    extension = "tar";
                }
                else if (downloadFileName != null)
                {
                    // Unknown extension in download url, try to use downloadFileName
                    if (downloadFileName.ToLower().EndsWith(".zip"))
                    {
                        extension = "zip";
                    }
                    else if (downloadFileName.ToLower().EndsWith(".exe"))
                    {
                        extension = "exe";
                    }
                    else
                    {
                        Trace.TraceError("Unknown file extension: {0}", downloadFileName);
                        throw new InstallException(string.Format("Unknown file extension: {0}", downloadFileName));
                    }
                }
                else
                {
                    Trace.TraceError("Unknown file extension: {0}", downloadUrl);
                    throw new InstallException(string.Format("Unknown file extension: {0}", downloadUrl));
                }

                // Download file with name as exe, as this extension will work for zip and exe files both
                vmManager.UpdateProgressInformation("Downloading " + downloadUrl + "...", false);

                if (extension.Equals("tar"))
                {
                    string tarFileName = Path.Combine(downloadFolder, "downloadedfile.tar");
                    WebClient client = new WebClient();
                    client.DownloadFile(downloadUrl, tarFileName);
                    vmManager.UpdateProgressInformation(downloadUrl + " download completed.", false);
                }
                if (extension.Equals("zip") || extension.Equals("tar"))
                {
                    string archiveFileName = Path.Combine(downloadFolder, "downloadedfile." + extension);
                    WebClient client = new WebClient();
                    client.DownloadFile(downloadUrl, archiveFileName);
                    vmManager.UpdateProgressInformation(downloadUrl + " download completed.", false);

                    string processFileName = string.Empty;
                    if (extension.Equals("zip"))
                    {
                        processFileName = Path.Combine(
                            Environment.GetEnvironmentVariable("SystemRoot"),
                            @"System32\cscript.exe");
                    }
                    else
                    {
                        processFileName = GetVMManager().GetPHPExeFileName();
                    }

                    string processArguments = null;
                    if (string.IsNullOrEmpty(applicationPath))
                    {
                        if (extension.Equals("zip"))
                        {
                            processArguments = @"/B /Nologo "
                                + "\""
                                + Environment.GetEnvironmentVariable("RoleRoot") + @"\approot\bin\" + WindowsAzureVMManager.ApplicationsUnzipUtility
                                + "\" "
                                + archiveFileName + " " + extractFolder;
                        }
                        else
                        {
                            processArguments = "\""
                                + Environment.GetEnvironmentVariable("RoleRoot") + @"\approot\bin\" + WindowsAzureVMManager.ApplicationsUntarUtility
                                + "\" "
                                + archiveFileName + " " + extractFolder;
                        }
                    }
                    else
                    {
                        if (extension.Equals("zip"))
                        {
                            processArguments = @"/B /Nologo "
                                + "\""
                                + Environment.GetEnvironmentVariable("RoleRoot") + @"\approot\bin\" + WindowsAzureVMManager.ApplicationsUnzipUtility
                                + "\" "
                                + archiveFileName + " " + extractFolder + " " + applicationPath;
                        }
                        else
                        {
                            processArguments = "\""
                                + Environment.GetEnvironmentVariable("RoleRoot") + @"\approot\bin\" + WindowsAzureVMManager.ApplicationsUntarUtility
                                + "\" "
                                + archiveFileName + " " + extractFolder + " " + applicationPath;
                        }
                    }

                    ExtractFilesInArchive(downloadUrl, processFileName, processArguments);

                    // Delete downloaded file
                    File.Delete(archiveFileName);
                    vmManager.UpdateProgressInformation("downloaded file deleted.", false);
                }
                else if (extension.Equals("exe"))
                {
                    WebClient client = new WebClient();
                    client.DownloadFile(downloadUrl, downloadFolder + @"downloadedfile.exe");
                    vmManager.UpdateProgressInformation(downloadUrl + " download completed.", false);

                    // Launch self extracting MSI
                    Process process = new Process();
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.FileName = downloadFolder + @"\downloadedfile.exe";
                    process.StartInfo.Arguments = "/Q /T:" + extractFolder;
                    process.Start();

                    // Wait until process is over
                    process.WaitForExit();

                    vmManager.UpdateProgressInformation(downloadUrl + " exe file extracted.", false);

                    // Delete downloaded file
                    File.Delete(downloadFolder + @"\downloadedfile.exe");
                }
                else
                {
                    Trace.TraceError("Unknown file extension: {0}", downloadUrl);
                    throw new InstallException(string.Format("Unknown file extension: {0}", downloadUrl));
                }                
            }
            catch (InstallException ex)
            {
                // Rethrow InstallException
                throw ex;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error in downloading file {0}, Error: {1}", downloadUrl, ex.Message);
                throw new InstallException(string.Format("Error in downloading file {0}, Error: {1}", downloadUrl, ex.Message));
            }
        }

        // Extract archive
        private static void ExtractFilesInArchive(string downloadUrl, 
            string processFileName,
            string processArguments)
        {
            Process extractProcess = new Process();
            extractProcess.StartInfo.UseShellExecute = false;
            extractProcess.StartInfo.RedirectStandardInput = true;
            extractProcess.StartInfo.RedirectStandardOutput = true;

            // Output data received handler for cscript process
            extractProcess.OutputDataReceived += new DataReceivedEventHandler(OutputDataReceivedHandler);

            // setting the file name and arguments
            extractProcess.StartInfo.FileName = processFileName;
            if (!File.Exists(extractProcess.StartInfo.FileName))
            {
                // Process to be invoked not found
                Trace.TraceError("File {0} not found.", extractProcess.StartInfo.FileName);
                throw new InstallException(string.Format("File {0} not found.", extractProcess.StartInfo.FileName));
            }
            extractProcess.StartInfo.Arguments = processArguments;

            try
            {
                // Get underlying VM Manager
                IVMManager vmManager = WindowsAzureVMManager.GetVMManager();

                vmManager.UpdateProgressInformation("Starting process: "
                    + extractProcess.StartInfo.FileName + " "
                    + extractProcess.StartInfo.Arguments, false);

                extractProcess.Start();

                // Start the asynchronous read of the output stream.
                extractProcess.BeginOutputReadLine();

                // Start extract process, wait for 30 minutes max
                vmManager.UpdateProgressInformation("Waiting (30 minutes max) to exit process "
                    + extractProcess.StartInfo.FileName, false);
                extractProcess.WaitForExit(30 * 60 * 1000);

                if (extractProcess.HasExited)
                {
                    if (extractProcess.ExitCode != 0)
                    {
                        // Unzip failed
                        Trace.TraceError("Failed to extract downloadUrl {0}", downloadUrl);
                        throw new InstallException(string.Format("Failed to extract downloadUrl {0}", downloadUrl));
                    }
                    else
                    {
                        vmManager.UpdateProgressInformation(downloadUrl + " archive extracted.", false);
                    }
                }
                else
                {
                    // Extract process not completed in time                    
                    extractProcess.Kill();
                    Trace.TraceError("Killed extract process as it has not completed in time. Failed to extract downloadUrl {0}", downloadUrl);
                    throw new InstallException(string.Format("Killed extract process as it has not completed in time. Failed to extract downloadUrl {0}", downloadUrl));
                }
                extractProcess.Close();
            }
            catch (InstallException ex)
            {
                // Rethrow InstallException
                throw ex;
            }
            catch (Exception ex)
            {
                // file extract failed
                Trace.TraceError("Failed to extract downloadUrl {0}, Error: {1}", downloadUrl, ex.Message);
                throw new InstallException(string.Format("Failed to extract downloadUrl {0}, Error: {1}", downloadUrl, ex.Message));
            }
        }

        // Get VMManager WCF service binding
        public static BasicHttpBinding GetVMManagerServiceBinding(string sslCertificateSHA1Thumbprint)
        {
            BasicHttpBinding binding = null;
            if (string.IsNullOrEmpty(sslCertificateSHA1Thumbprint))
            {
                binding = new BasicHttpBinding(BasicHttpSecurityMode.TransportCredentialOnly);
            }
            else
            {
                binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
            }                
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;            
            binding.SendTimeout = TimeSpan.FromMinutes(10);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
            binding.OpenTimeout = TimeSpan.FromMinutes(10);
            binding.CloseTimeout = TimeSpan.FromMinutes(10);
            binding.MaxReceivedMessageSize = 256 * 1024;
            binding.ReaderQuotas.MaxStringContentLength = 64 * 1024;            
            
            return binding;
        }

        // Get VMManager WCF service endpoint address
        public static string GetVMManagerServiceEndpointAddress(string sslCertificateSHA1Thumbprint)
        {
            if (string.IsNullOrEmpty(sslCertificateSHA1Thumbprint))
            {
                return String.Format("http://{0}/WindowsAzureVMManager", 
                    RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["VMManagerServiceExternalHttpPort"].IPEndpoint);
            }
            else
            {
                return String.Format("https://{0}/WindowsAzureVMManager", 
                    RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["VMManagerServiceExternalHttpsPort"].IPEndpoint);
            }
        }

        // Get VMManager WCF service endpoint
        public static IPEndPoint GetVMManagerServiceEndpoint(string sslCertificateSHA1Thumbprint)
        {
            if (string.IsNullOrEmpty(sslCertificateSHA1Thumbprint))
            {
                return RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["VMManagerServiceExternalHttpPort"].IPEndpoint;
            }
            else
            {
                return RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["VMManagerServiceExternalHttpsPort"].IPEndpoint;
            }            
        }

        // Get singleton instance of the service
        public static IVMManager GetVMManager()
        {            
            // If available, get SSL certificate from service configuration that is needed for WCF service
            string sslCertificateSHA1Thumbprint = null;
            try
            {
                sslCertificateSHA1Thumbprint = RoleEnvironment.GetConfigurationSettingValue("SSLCertificateSHA1Thumbprint");
            }
            catch (Exception)
            {
                // Ignore, it means SSLCertificateSHA1Thumbprint is not defined
            }

            // Get WCF service binding
            BasicHttpBinding binding = WindowsAzureVMManager.GetVMManagerServiceBinding(sslCertificateSHA1Thumbprint);
            ChannelFactory<IVMManager> cfactory = new ChannelFactory<IVMManager>(binding,
                    WindowsAzureVMManager.GetVMManagerServiceEndpointAddress(sslCertificateSHA1Thumbprint));

            ClientCredentials loginCredentials = new ClientCredentials();
            loginCredentials.UserName.UserName = RoleEnvironment.GetConfigurationSettingValue("AdminUserName");
            loginCredentials.UserName.Password = RoleEnvironment.GetConfigurationSettingValue("AdminPassword");

            var defaultCredentials = cfactory.Endpoint.Behaviors.Find<ClientCredentials>();
            cfactory.Endpoint.Behaviors.Remove(defaultCredentials); //remove default ones
            cfactory.Endpoint.Behaviors.Add(loginCredentials); //add required ones
            

            IVMManager vmManager = cfactory.CreateChannel();

            if (!string.IsNullOrEmpty(sslCertificateSHA1Thumbprint))
            {
                // Trust any certificate
                ServicePointManager.ServerCertificateValidationCallback
                   += RemoteCertificateValidate;

                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Ssl3;
            }

            return vmManager;
        }

        // Remotes the certificate validate.
        private static bool RemoteCertificateValidate(
           object sender, X509Certificate cert,
            X509Chain chain, SslPolicyErrors error)
        {
            // trust any certificate!!!
            return true;
        }

        // Create Storage Account using account information in service configuration file
        public static CloudStorageAccount GetStorageAccount(bool useHttps)
        {
            CloudStorageAccount storageAccount = null;
            if (RoleEnvironment.DeploymentId.StartsWith("deployment"))
            {
                storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
            }
            else
            {
                // Create Storage Credetials and account                
                StorageCredentialsAccountAndKey oStorageCredentialsAccountAndKey
                       = new StorageCredentialsAccountAndKey(
                           RoleEnvironment.GetConfigurationSettingValue("WindowsAzureStorageAccountName"),
                           RoleEnvironment.GetConfigurationSettingValue("WindowsAzureStorageAccountKey")
                           );

                // Create HTTPS storage endpoint
                storageAccount = new CloudStorageAccount(oStorageCredentialsAccountAndKey, useHttps);
            }

            return storageAccount;
        }

        // Check if blob exists
        private static bool BlobExists(CloudBlob blob)
        {
            try
            {
                blob.FetchAttributes();
                return true;
            }
            catch (StorageClientException e)
            {
                if (e.ErrorCode == StorageErrorCode.ResourceNotFound)
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        // Serialize List To Blob
        private static void SerializeListToBlob(List<string> collection, CloudBlob blob)
        {
            try
            {
                SoapFormatter ser = new SoapFormatter();
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    ser.Serialize(memoryStream, collection);
                    byte[] b = memoryStream.GetBuffer();
                    string serilizedNameValueCollection = Encoding.Default.GetString(b);
                    blob.UploadText(serilizedNameValueCollection);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to SerializeNameValueCollectionToBlob. Error: {0}", ex.Message);
            }
        }

        // Serialize NameValueCollection To Blob
        private static void SerializeNameValueCollectionToBlob(NameValueCollection collection, CloudBlob blob)
        {
            try
            {
                SoapFormatter ser = new SoapFormatter();
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    ser.Serialize(memoryStream, collection);
                    byte[] b = memoryStream.GetBuffer();
                    string serilizedNameValueCollection = Encoding.Default.GetString(b);
                    blob.UploadText(serilizedNameValueCollection);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to SerializeNameValueCollectionToBlob. Error: {0}", ex.Message);
            }
        }

        // Serialize NameValueCollection To string
        private static string SerializeNameValueCollectionToString(NameValueCollection collection)
        {
            string serilizedNameValueCollection = null;

            try
            {
                SoapFormatter ser = new SoapFormatter();
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    ser.Serialize(memoryStream, collection);
                    byte[] b = memoryStream.GetBuffer();
                    serilizedNameValueCollection = Encoding.Default.GetString(b);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to SerializeNameValueCollectionToString. Error: {0}", ex.Message);
            }

            return serilizedNameValueCollection;
        }

        // Deserialize NameValueCollection from Blob
        private static NameValueCollection DeserializeNameValueCollectionFromBlob(CloudBlob blob)
        {
            NameValueCollection collection = null;
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    SoapFormatter ser = new SoapFormatter();
                    blob.DownloadToStream(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    collection = ser.Deserialize(memoryStream) as NameValueCollection;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to DeserializeNameValueCollectionFromBlob. Error: {0}", ex.Message);
            }

            return collection;
        }

        // Deserialize NameValueCollection from String
        private static NameValueCollection DeserializeNameValueCollectionFromString(string serilizedNameValueCollection)
        {
            NameValueCollection collection = null;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(memoryStream);
                writer.Write(serilizedNameValueCollection);
                writer.Flush();
                memoryStream.Seek(0, SeekOrigin.Begin);
                SoapFormatter ser = new SoapFormatter();
                collection = ser.Deserialize(memoryStream) as NameValueCollection;
            }

            return collection;
        }

        // Get Installed Products Info
        public IDictionary<string, string> GetInstalledProductsInfo()
        {
            string blobName = RoleEnvironment.GetConfigurationSettingValue("InstallationStatusConfigFileBlob");
            CloudBlob blob = GetCloudBlobReference(blobName);
            NameValueCollection installedProductsInfo = DeserializeNameValueCollectionFromBlob(blob);

            // Transform NameValueCollection to Dictionary. 
            // TODO: Need to get rid of all this dirty work
            Dictionary<string, string> dicInstalledProductsInfo = new Dictionary<string, string>();
            foreach (string key in installedProductsInfo.AllKeys)
            {
                dicInstalledProductsInfo.Add(key, installedProductsInfo[key]);
            }
            
            return dicInstalledProductsInfo;
        }

        // Get cloud blob reference
        private static CloudBlob GetCloudBlobReference(string blobName)
        {
            // Create HTTPS storage endpoint
            CloudStorageAccount storageAccount = WindowsAzureVMManager.GetStorageAccount(true);

            // Create blob for installation status information
            string containerName = RoleEnvironment.GetConfigurationSettingValue("PHPApplicationsBackupContainerName");
            
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            if (container.CreateIfNotExist())
            {
                BlobContainerPermissions containerPermissions = new BlobContainerPermissions();
                containerPermissions.PublicAccess = BlobContainerPublicAccessType.Container;
                container.SetPermissions(containerPermissions);
            }

            return container.GetBlobReference(blobName);
        }

        // Kill processes starting with specified name. Needed to kill php-cgi.exe
        private static void FindAndKillProcess(string name)
        {
            // Get a list of all running processes on the computer
            foreach (Process process in Process.GetProcesses())
            {
                // Kill required processes
                if (process.ProcessName.StartsWith(name))
                {
                    Trace.TraceWarning("Killed process {0} with id={1}", name, process.Id);
                    try
                    {
                        if (process.HasExited == false)
                        {
                            process.Kill();
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Unable to kill process. Error: {0}", ex.Message);
                    }
                }
            }
        }        
    }
}
