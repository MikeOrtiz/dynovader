using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace WindowsAzureCompanion.VMManagerService
{
    public class PHPRuntimeInstaller : IInstaller
    {
        private SyndicationItem product = null;
        private string productVersion = null;
        private string downloadFolder = null;
        private string downloadUrl = null;
        private string downloadFileName = null;
        private string installationFolder = null;
        
        public PHPRuntimeInstaller(string installationFolder, string downloadFolder, SyndicationItem product, string productVersion)
        {
            this.product = product;
            this.productVersion = productVersion;
            this.downloadFolder = downloadFolder;
            this.downloadUrl = WindowsAzureVMManager.GetDownloadUrlFromProductVersion(product, productVersion);
            this.downloadFileName = WindowsAzureVMManager.GetAttributeValueFromProductVersion(product, productVersion, "downloadFileName");
            this.installationFolder = installationFolder;
        }

        // Install PHP Runtime
        public void Install()
        {
            try
            {
                WindowsAzureVMManager.DownloadAndExtractWebArchive(downloadUrl, downloadFileName, downloadFolder, installationFolder, null);
                // Configure PHP runtime
                ConfigurePHPRuntime();
                Trace.TraceInformation("Successfully installed {0}", product.Title.Text);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unable to install PHP Runtime: {0}", downloadUrl);
                throw ex;
            }
        }

        // Configure PHP runtime
        private void ConfigurePHPRuntime()
        {
            // Create php.ini from standard template file provided in runtime
            string phpIniFileName = Path.Combine(installationFolder, "php.ini");
            if (File.Exists(Path.Combine(installationFolder, "php.ini-recommended")))
            {
                File.Copy(Path.Combine(installationFolder, @"php.ini-recommended"), phpIniFileName);
            }
            else if (File.Exists(Path.Combine(installationFolder , @"php.ini-production")))
            {
                File.Copy(Path.Combine(installationFolder, @"php.ini-production"), phpIniFileName);
            }
            else if (File.Exists(Path.Combine(installationFolder , @"php.ini-development")))
            {
                File.Copy(Path.Combine(installationFolder, @"php.ini-development"), phpIniFileName);
            }
            else
            {
                Trace.TraceError("php.ini template not found. Unable to install PHP Runtime.");
                return;
            }

            // Set extension dir
            string extensionDir = Path.Combine(installationFolder, WindowsAzureVMManager.ExtensionsFolderForPHP);
            FileUtils.AppendToFile(phpIniFileName, "extension_dir = \"" + extensionDir + "\"");

            // Set log folder and file name
            string logFolderName = Path.Combine(installationFolder, "logs");
            Directory.CreateDirectory(logFolderName);
            string logFileName = Path.Combine(logFolderName, "log.txt");

            // Set library/framwwork directory
            string includePath = Path.Combine(installationFolder, WindowsAzureVMManager.LibraryFolderForPHP);

            // Set session path dir
            string tmpPath = Directory.CreateDirectory(Path.Combine(installationFolder, "tmp")).FullName;
            FileUtils.AppendToFile(phpIniFileName, "session.save_path = \"" + tmpPath + "\"");

            // Add and enable extensions
            // Update php.ini and enabled all extension dll specified
            XElement downloadUrlsElement = product.ElementExtensions.Where<SyndicationElementExtension>
                (x => x.OuterName == "installerFileChoices").FirstOrDefault().GetObject<XElement>();

            // TODO: Use Linq Query instead of foreach loop
            foreach (XElement extension in downloadUrlsElement.Elements())
            {
                if (extension.Attribute("version").Value.Equals(productVersion))
                {
                    // Iterate through properties
                    foreach (XElement propertyExtension in extension.Elements().First().Elements())
                    {
                        if (propertyExtension.Attribute("name").Value.Equals("extensions"))
                        {
                            string dllNames = propertyExtension.Attribute("value").Value;

                            // Add each extension to php.ini
                            foreach (string dllName in dllNames.Split(','))
                            {
                                // Enable extension only if dll is available
                                if (File.Exists(Path.Combine(extensionDir, dllName)))
                                {
                                    FileUtils.AppendToFile(phpIniFileName, "extension = " + dllName);
                                    Trace.TraceInformation("Enabled PHP extension {0}", dllName);
                                }
                                else
                                {
                                    Trace.TraceWarning("Ignored PHP extension as {0} is not available.", dllName);
                                }
                            }
                            break;
                        }
                    }
                }
            }                       

            // Set time zone
            FileUtils.AppendToFile(phpIniFileName, "date.timezone = UTC");
            FileUtils.AppendToFile(phpIniFileName, "max_execution_time = 900");
            FileUtils.AppendToFile(phpIniFileName, "max_input_time = 900");
            FileUtils.AppendToFile(phpIniFileName, "mysql.connect_timeout = 600");
            FileUtils.AppendToFile(phpIniFileName, "display_errors = On");
            FileUtils.AppendToFile(phpIniFileName, "display_startup_errors = On");
            FileUtils.AppendToFile(phpIniFileName, "post_max_size = 200M");
            FileUtils.AppendToFile(phpIniFileName, "upload_max_filesize = 200M");
            FileUtils.AppendToFile(phpIniFileName, "default_socket_timeout = 600");
            FileUtils.AppendToFile(phpIniFileName, "error_log = \"" + logFileName + "\"");
            FileUtils.AppendToFile(phpIniFileName, "error_reporting = E_ALL");
            FileUtils.AppendToFile(phpIniFileName, "cgi.force_redirect = 0");
            FileUtils.AppendToFile(phpIniFileName, "cgi.fix_pathinfo = 1");
            FileUtils.AppendToFile(phpIniFileName, "fastcgi.impersonate = 1");
            FileUtils.AppendToFile(phpIniFileName, "fastcgi.logging = 0");
            FileUtils.AppendToFile(phpIniFileName, "include_path = \"" + includePath + ";.\"");
            FileUtils.AppendToFile(phpIniFileName, "variables_order=EGPCS");      
        }

        // Re-configure PHP runtime
        public static void ReConfigurePHPRuntime(string installationFolder)
        {
            // Do not reconfigure in devfabric as we do not have xdrive path in php.ini
            if (RoleEnvironment.DeploymentId.StartsWith("deployment"))
            {
                return;
            }

            if (!Directory.Exists(installationFolder))
            {
                throw new System.ArgumentException(
                        String.Format("Directory does not exist: {0}", installationFolder),
                        "installationFolder"
                        );
            }

            StreamReader streamReader = null;
            StreamWriter streamWriter = null;

            // Read file content
            string phpIniFileName = Path.Combine(installationFolder, "php.ini");

            if (!File.Exists(phpIniFileName))
            {
                throw new System.InvalidOperationException(
                        String.Format("File 'php.ini' does not exist within this Directory: {0}", installationFolder)
                        );
            }
            streamReader = File.OpenText(phpIniFileName);
            string contents = streamReader.ReadToEnd();
            streamReader.Close();

            // Replace old XDrive letter with new XDrive letter
            string letter = Path.GetPathRoot(installationFolder).Substring(0, 1);
            contents = Regex.Replace(contents, "\"[a-zA-Z]:\\\\", "\"" + letter + @":\");
            contents = Regex.Replace(contents, "\"[a-zA-Z]:/", "\"" + letter + ":/");
            File.Delete(phpIniFileName);
            streamWriter = File.CreateText(phpIniFileName);
            streamWriter.Write(contents);
            streamWriter.Close();
        }
    }
}