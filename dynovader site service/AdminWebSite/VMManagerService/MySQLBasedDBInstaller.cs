using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Syndication;
using System.Diagnostics;
using System.IO;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Threading;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace WindowsAzureCompanion.VMManagerService
{
    class MySQLBasedDBInstaller : IInstaller
    {
        private static string mySqlBasedDBName = null;
        private static Process mySqlBasedDBProcess = null;
        private static EventHandler exitHandler = null;
        private static SyndicationItem product = null;
        private string downloadFolder = null;
        private string downloadUrl = null;
        private string downloadFileName = null;
        private string installationFolder = null;
        private string applicationPath = null;
        private static NameValueCollection productProperties = null;

        public MySQLBasedDBInstaller(string installationFolder, 
            string downloadFolder, 
            SyndicationItem product, 
            string productVersion,
            NameValueCollection productProperties)
        {
            MySQLBasedDBInstaller.product = product;
            this.downloadFolder = downloadFolder;
            this.downloadUrl = WindowsAzureVMManager.GetDownloadUrlFromProductVersion(product, productVersion);
            this.downloadFileName = WindowsAzureVMManager.GetAttributeValueFromProductVersion(product, productVersion, "downloadFileName");
            this.installationFolder = installationFolder;
            this.applicationPath = WindowsAzureVMManager.GetAttributeValueFromProductVersion(product, productVersion, "applicationPath");
            MySQLBasedDBInstaller.productProperties = productProperties;
        }

        public void Install()
        {
            try
            {
                WindowsAzureVMManager.DownloadAndExtractWebArchive(downloadUrl, 
                    downloadFileName, 
                    downloadFolder,
                    installationFolder,
                    applicationPath);
                
                // Create thread for launching MySQL based database
                ThreadStart starter = delegate { StartMySQLBasedDB(); };
                Thread thread = new Thread(starter);
                thread.Start();
                
                Trace.TraceInformation("Successfully installed {0}", product.Title.Text);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unable to install {0}: {1}", product.Title.Text, downloadUrl);
                throw ex;
            }
        }

        // Stop MySQL based database
        public static bool StopMySQLBasedDB()
        {
            if (MySQLBasedDBInstaller.mySqlBasedDBProcess == null)
            {
                Trace.TraceError("{0} is not running.", MySQLBasedDBInstaller.mySqlBasedDBName);
                return false;
            }

            try
            {
                // Remove process exit handler                
                MySQLBasedDBInstaller.mySqlBasedDBProcess.Exited -= MySQLBasedDBInstaller.exitHandler;
                Trace.TraceInformation("Removed MySQL process exit handler.");

                if (MySQLBasedDBInstaller.mySqlBasedDBProcess.HasExited)
                {
                    Trace.TraceError("{0} already exited.", MySQLBasedDBInstaller.mySqlBasedDBName);
                    MySQLBasedDBInstaller.mySqlBasedDBProcess.Close();
                    MySQLBasedDBInstaller.mySqlBasedDBProcess = null;
                    return false;
                }
                else
                {
                    Trace.TraceInformation("Stopping {0}..", MySQLBasedDBInstaller.mySqlBasedDBName);
                    MySQLBasedDBInstaller.mySqlBasedDBProcess.Kill();
                    MySQLBasedDBInstaller.mySqlBasedDBProcess.Close();
                    MySQLBasedDBInstaller.mySqlBasedDBProcess = null;
                    Trace.TraceInformation("{0} stopped.", MySQLBasedDBInstaller.mySqlBasedDBName);
                }

                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to stop {0}. Error: {1}", MySQLBasedDBInstaller.mySqlBasedDBName, ex.Message);
                return false;
            }
        }

        // Clean MySQL process and restart
        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public static void CleanAndRestartMySQLBasedDBProcess()
        {
            if (MySQLBasedDBInstaller.mySqlBasedDBProcess != null)
            {
                if (MySQLBasedDBInstaller.mySqlBasedDBProcess.HasExited)
                {
                    MySQLBasedDBInstaller.mySqlBasedDBProcess.Close();
                    MySQLBasedDBInstaller.mySqlBasedDBProcess = null;
                }                
            }
        }

        // MySQL based DB process exit handler
        private static void MySQLBasedDBProcess_ExitHandler(object sender, System.EventArgs e)
        {
            Trace.TraceInformation("MySQL process crashed, attempting to restart MySQL...");
            MySQLCommunityServerInstaller.CleanAndRestartMySQLBasedDBProcess();

            // Get MySQL based DB settings
            IVMManager vmManager = WindowsAzureVMManager.GetVMManager();

            // Start MySQL Server again
            if (MySQLCommunityServerInstaller.StartConfiguredMySQL(
                vmManager.GetMySQLBasedDBInstallationFolder(),
                vmManager.GetMySQLBasedDBIniFileName(),
                vmManager.GetMySQLBasedDBName()))
            {
                Trace.TraceInformation("Restarted MySQL based server successfully.");
            }
            else
            {
                Trace.TraceError("Failed to start the MySQL based server.");
            }
        }

        // Start already configured MySQL
        public static bool StartConfiguredMySQL(string installationFolder, 
            string iniFileName, 
            string mySqlBasedDBName)
        {
            if (MySQLBasedDBInstaller.mySqlBasedDBProcess != null)
            {
                Trace.TraceError("{0} already running.", MySQLBasedDBInstaller.mySqlBasedDBName);
                return false;
            }

            try
            {
                // Get internal port number for MySQL based database
                string portNumber = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["MySQLBasedDBPort"].IPEndpoint.Port.ToString();

                // Create process and set required properties
                MySQLBasedDBInstaller.mySqlBasedDBProcess = new Process();
                MySQLBasedDBInstaller.mySqlBasedDBProcess.StartInfo.UseShellExecute = false;
                MySQLBasedDBInstaller.mySqlBasedDBProcess.StartInfo.RedirectStandardInput = true;
                MySQLBasedDBInstaller.mySqlBasedDBProcess.StartInfo.RedirectStandardOutput = true;
                MySQLBasedDBInstaller.mySqlBasedDBProcess.StartInfo.RedirectStandardError = true;

                // Output data received handler
                MySQLBasedDBInstaller.mySqlBasedDBProcess.OutputDataReceived +=
                    new DataReceivedEventHandler(OutputDataReceivedHandler);

                // Error data received handler
                MySQLBasedDBInstaller.mySqlBasedDBProcess.ErrorDataReceived +=
                    new DataReceivedEventHandler(ErrorDataReceivedHandler);

                // setting the process file name and arguments
                MySQLBasedDBInstaller.mySqlBasedDBProcess.StartInfo.WorkingDirectory = installationFolder;
                MySQLBasedDBInstaller.mySqlBasedDBProcess.StartInfo.FileName = Path.Combine(installationFolder, @"bin\mysqld.exe");
                MySQLBasedDBInstaller.mySqlBasedDBProcess.StartInfo.Arguments =
                    "--defaults-file=" + iniFileName
                    + " --port " + portNumber
                    + " --console --standalone";
                
                // Start MySQL based DB process
                Trace.TraceInformation("Staring {0} ({1}) with arguments {2}: ",
                    mySqlBasedDBName,
                    RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["MySQLBasedDBPort"].IPEndpoint.Address.ToString(),
                    MySQLBasedDBInstaller.mySqlBasedDBProcess.StartInfo.Arguments);

                // Setup MySQL based DB server process exit handler
                MySQLBasedDBInstaller.mySqlBasedDBProcess.EnableRaisingEvents = true;
                MySQLBasedDBInstaller.mySqlBasedDBProcess.Exited += new EventHandler(MySQLBasedDBProcess_ExitHandler);
                
                MySQLBasedDBInstaller.mySqlBasedDBProcess.Start();

                // Start the asynchronous read of the STDOUT stream.
                MySQLBasedDBInstaller.mySqlBasedDBProcess.BeginOutputReadLine();

                // Start the asynchronous read of the STDERR stream.
                MySQLBasedDBInstaller.mySqlBasedDBProcess.BeginErrorReadLine();

                // Set MySQL based database name
                MySQLBasedDBInstaller.mySqlBasedDBName = mySqlBasedDBName;
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                return false;
            }
        }
        
        private bool StartMySQLBasedDB()
        {
            // Get internal port number for MySQL based database
            string portNumber = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["MySQLBasedDBPort"].IPEndpoint.Port.ToString();

            if (MySQLBasedDBInstaller.mySqlBasedDBProcess != null)
            {
                Trace.TraceError("{0} already running.", product.Title.Text);
                return false;
            }

            try
            {
                // Get ini file name
                IVMManager vmManager = WindowsAzureVMManager.GetVMManager();
                string iniFileName = vmManager.GetMySQLBasedDBIniFileName();

                // Create process and set required properties
                MySQLBasedDBInstaller.mySqlBasedDBProcess = new Process();
                MySQLBasedDBInstaller.mySqlBasedDBProcess.StartInfo.UseShellExecute = false;
                MySQLBasedDBInstaller.mySqlBasedDBProcess.StartInfo.RedirectStandardInput = true;
                MySQLBasedDBInstaller.mySqlBasedDBProcess.StartInfo.RedirectStandardOutput = true;
                MySQLBasedDBInstaller.mySqlBasedDBProcess.StartInfo.RedirectStandardError = true;

                // Output data received handler
                MySQLBasedDBInstaller.mySqlBasedDBProcess.OutputDataReceived +=
                    new DataReceivedEventHandler(OutputDataReceivedHandler);

                // Error data received handler
                MySQLBasedDBInstaller.mySqlBasedDBProcess.ErrorDataReceived +=
                    new DataReceivedEventHandler(ErrorDataReceivedHandler);

                // setting the process file name and arguments
                MySQLBasedDBInstaller.mySqlBasedDBProcess.StartInfo.WorkingDirectory = vmManager.GetMySQLBasedDBInstallationFolder();
                MySQLBasedDBInstaller.mySqlBasedDBProcess.StartInfo.FileName = 
                    Path.Combine(MySQLBasedDBInstaller.mySqlBasedDBProcess.StartInfo.WorkingDirectory,
                    @"bin\mysqld.exe");
                MySQLBasedDBInstaller.mySqlBasedDBProcess.StartInfo.Arguments = 
                    "--defaults-file=" + iniFileName
                    + " --port " + portNumber 
                    + " --console --standalone";

                // Start MySQL based DB process
                Trace.TraceInformation("Staring {0} ({1}) with arguments: ",
                    vmManager.GetMySQLBasedDBName(),
                    portNumber,
                    MySQLBasedDBInstaller.mySqlBasedDBProcess.StartInfo.Arguments);

                // Setup MySQL based DB server process exit handler
                MySQLBasedDBInstaller.mySqlBasedDBProcess.EnableRaisingEvents = true;
                MySQLBasedDBInstaller.mySqlBasedDBProcess.Exited += new EventHandler(MySQLBasedDBProcess_ExitHandler);

                MySQLBasedDBInstaller.mySqlBasedDBProcess.Start();

                // Start the asynchronous read of the STDOUT stream.
                MySQLBasedDBInstaller.mySqlBasedDBProcess.BeginOutputReadLine();

                // Start the asynchronous read of the STDERR stream.
                MySQLBasedDBInstaller.mySqlBasedDBProcess.BeginErrorReadLine();

                // Wait 10s and update the password
                Thread.Sleep(10000);

                Trace.TraceInformation("Updating {0} Server root password...", product.Title.Text);
                Process adminToolProcess = new Process();
                adminToolProcess.StartInfo.UseShellExecute = false;
                adminToolProcess.StartInfo.RedirectStandardInput = true;
                adminToolProcess.StartInfo.RedirectStandardOutput = true;

                // Output data received handler
                adminToolProcess.OutputDataReceived +=
                    new DataReceivedEventHandler(OutputDataReceivedHandler);

                // Setting the process file name and arguments
                adminToolProcess.StartInfo.WorkingDirectory = vmManager.GetMySQLBasedDBInstallationFolder();
                adminToolProcess.StartInfo.FileName = Path.Combine(
                    MySQLBasedDBInstaller.mySqlBasedDBProcess.StartInfo.WorkingDirectory, 
                    @"bin\mysqladmin.exe");
                adminToolProcess.StartInfo.Arguments =
                    " -h 127.0.0.1 " 
                    + " -P " + portNumber
                    + " -u root password "
                    + MySQLBasedDBInstaller.productProperties["rootPassword"];

                // Start MySQL based database process
                adminToolProcess.Start();

                // Start the asynchronous read of the output stream.
                adminToolProcess.BeginOutputReadLine();

                adminToolProcess.WaitForExit();

                // Set MySQL based database name
                MySQLBasedDBInstaller.mySqlBasedDBName = product.Title.Text;

                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                return false;
            }
        }

        // Get MySQL based database name
        public static string GetMySQLBasedDBName()
        {
            return MySQLCommunityServerInstaller.mySqlBasedDBName;
        }

        // Get MySQL based database server port number
        public static string GetMySQLBasedDBServerPortNumber()
        {
            return RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["MySQLBasedDBPort"].IPEndpoint.Port.ToString();           
        }

        // Get MySQL based database server IP address
        public static string GetMySQLBasedDBServerIPAddress()
        {
            // Return loopback address
            return "127.0.0.1";
        }

        // Has MySQL Server started?
        public static bool IsMySQLServerStarted()
        {
            if (MySQLBasedDBInstaller.mySqlBasedDBProcess != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Output data received handler for invoked processes
        static void OutputDataReceivedHandler(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            Trace.TraceInformation("Process STDOUT: {0}", e.Data);
        }

        // Error data received handler for invoked processes
        static void ErrorDataReceivedHandler(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            Trace.TraceInformation("Process STDERR: {0}", e.Data);
        }
    }
}
