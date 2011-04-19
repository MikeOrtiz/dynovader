using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using WindowsAzureCompanion.VMManagerService;
using System.ServiceModel;
using System.Diagnostics;
using Microsoft.WindowsAzure.StorageClient;
using System.Threading;
using System.ServiceModel.Security;
using System.ServiceModel.Description;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Soap;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Net.Security;

namespace WindowsAzureCompanion.AdminWebSite
{
    public class WebRole : RoleEntryPoint
    {
        private DiagnosticMonitor diagnosticMonitor = null;
        private CloudStorageAccount storageAccount = null;
        private ServiceHost wcfVMManagerServiceHost = null;
        private IVMManager vmManager = null;

        public override bool OnStart()
        {
            Trace.TraceInformation("WebRole starting...");

            // Create HTTPS storage endpoint
            storageAccount = WindowsAzureVMManager.GetStorageAccount(true);
            
            // Start Windows Azure Diagnostics
            StartDiagnosticMonitor();

            try
            {
                // Start VM Manager WCF service
                vmManager = StartVMManagerService();

                // Wait for 10 sec
                Thread.Sleep(10000);

                // Mount XDrive
                if (vmManager.MountXDrive())
                {
                    // Setup Runtime Servers
                    SetupRuntimeServers();
                }
                else
                {
                    Trace.TraceError("Could not start the service as Windows Azure Drive was not mounted.");
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                Trace.TraceError(ex.StackTrace);
            }

            RoleEnvironment.Changing += RoleEnvironmentChanging;

            Trace.TraceInformation("WebRole started!!!");
            return base.OnStart();
        }

        // Start Windows Azure Diagnostics Monitor
        private void StartDiagnosticMonitor()
        {
            // Enable Diagnostics trace listener (needed for WCF service)
            Trace.Listeners.Add(new DiagnosticMonitorTraceListener()); 

            // Get Diagnostics and Performance Counter Capture Frequency (in minutes)
            TimeSpan timeSpan = TimeSpan.FromMinutes(
                Double.Parse(RoleEnvironment.GetConfigurationSettingValue("DiagnosticsAndPerformanceCounterCaptureFrequencyInMinutes")));

            var cfg = DiagnosticMonitor.GetDefaultInitialConfiguration();
            cfg.Logs.ScheduledTransferPeriod = timeSpan;
            cfg.Logs.ScheduledTransferLogLevelFilter = LogLevel.Verbose;

            // Infrastructure logs
            cfg.DiagnosticInfrastructureLogs.ScheduledTransferPeriod = timeSpan;
            cfg.DiagnosticInfrastructureLogs.ScheduledTransferLogLevelFilter = LogLevel.Error;
            cfg.DiagnosticInfrastructureLogs.BufferQuotaInMB = 10;

            // Add event collection from the Windows Event Log
            cfg.WindowsEventLog.DataSources.Add("System!*");
            cfg.WindowsEventLog.ScheduledTransferPeriod = timeSpan;
            cfg.WindowsEventLog.DataSources.Add("Application!*[System[Provider[@Name='HostableWebCore']]]");

            // Add performance counter monitoring
            cfg.PerformanceCounters.DataSources.Add(
                            new PerformanceCounterConfiguration()
                            {
                                CounterSpecifier = @"\Processor(_Total)\% Processor Time",
                                SampleRate = timeSpan
                            });
            cfg.PerformanceCounters.DataSources.Add(
                new PerformanceCounterConfiguration()
                {
                    CounterSpecifier = @"\Memory\Available Mbytes",
                    SampleRate = timeSpan
                });
            cfg.PerformanceCounters.ScheduledTransferPeriod = timeSpan;

            // Start Diagnostics
            diagnosticMonitor = DiagnosticMonitor.Start(storageAccount, cfg);
        }

        // Start WCF Service to manage VM
        private IVMManager StartVMManagerService()
        {
            try
            {
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

                // Get VMManager WCF service binding
                BasicHttpBinding binding = WindowsAzureVMManager.GetVMManagerServiceBinding(sslCertificateSHA1Thumbprint);                
                wcfVMManagerServiceHost = new ServiceHost(typeof(WindowsAzureVMManager));
                wcfVMManagerServiceHost.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.Custom;
                wcfVMManagerServiceHost.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = new WindowsAzureVMManagerUsernamePasswordValidator();

                UseRequestHeadersForMetadataAddressBehavior requestHeaderBehavior = new UseRequestHeadersForMetadataAddressBehavior();
                IPEndPoint ep = WindowsAzureVMManager.GetVMManagerServiceEndpoint(sslCertificateSHA1Thumbprint);  
                if (string.IsNullOrEmpty(sslCertificateSHA1Thumbprint))
                {
                    requestHeaderBehavior.DefaultPortsByScheme.Add("http", int.Parse(ep.Port.ToString()));
                }
                else
                {
                    requestHeaderBehavior.DefaultPortsByScheme.Add("https", int.Parse(ep.Port.ToString()));
                    wcfVMManagerServiceHost.Credentials.ServiceCertificate.SetCertificate(
                        StoreLocation.LocalMachine,
                        StoreName.My,
                        X509FindType.FindByThumbprint,
                        RoleEnvironment.GetConfigurationSettingValue("SSLCertificateSHA1Thumbprint")
                    );
                }
                wcfVMManagerServiceHost.Description.Behaviors.Add(requestHeaderBehavior);                

                // Enable WCF service debugging
                ServiceDebugBehavior sdb = wcfVMManagerServiceHost.Description.Behaviors.Find<ServiceDebugBehavior>();
                if (sdb == null)
                {
                    sdb = new ServiceDebugBehavior();
                    wcfVMManagerServiceHost.Description.Behaviors.Add(sdb);
                }
                sdb.IncludeExceptionDetailInFaults = true;

                string endpoint = WindowsAzureVMManager.GetVMManagerServiceEndpointAddress(sslCertificateSHA1Thumbprint);
                wcfVMManagerServiceHost.AddServiceEndpoint(typeof(IVMManager), binding, endpoint);
                
                ServiceMetadataBehavior metadataBehavior = new ServiceMetadataBehavior();                
                if (string.IsNullOrEmpty(sslCertificateSHA1Thumbprint))
                {
                    metadataBehavior.HttpGetEnabled = true;
                    metadataBehavior.HttpGetUrl = new Uri(endpoint);
                }
                else
                {
                    metadataBehavior.HttpsGetEnabled = true;
                    metadataBehavior.HttpsGetUrl = new Uri(endpoint);
                }                   
                                
                wcfVMManagerServiceHost.Description.Behaviors.Add(metadataBehavior);

                wcfVMManagerServiceHost.Open();
                Trace.TraceInformation("External WCF Service started on endpoint: {0}", endpoint);                
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                Trace.TraceError(ex.StackTrace);
            }

            // Return service instance
            return WindowsAzureVMManager.GetVMManager();
        }
                
        // Setup Runtime Servers
        private void SetupRuntimeServers()
        {
            string containerName = RoleEnvironment.GetConfigurationSettingValue("PHPApplicationsBackupContainerName");
            string blobName = RoleEnvironment.GetConfigurationSettingValue("InstallationStatusConfigFileBlob");

            // Create blob container
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            if (container.CreateIfNotExist())
            {
                BlobContainerPermissions containerPermissions = new BlobContainerPermissions();
                containerPermissions.PublicAccess = BlobContainerPublicAccessType.Container;
                container.SetPermissions(containerPermissions);
            }

            CloudBlob blob = container.GetBlobReference(blobName);
            if (!BlobExists(blob))
            {
                // Serialize empty NameValueCollection to blob
                NameValueCollection values = new NameValueCollection();
                SoapFormatter ser = new SoapFormatter();
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    ser.Serialize(memoryStream, values);
                    byte[] b = memoryStream.GetBuffer();
                    string serilizedNameValueCollection = Encoding.Default.GetString(b);
                    blob.UploadText(serilizedNameValueCollection);
                }
            }
            else
            {
                // Setup Runtime Servers
                vmManager.SetupRuntimeServers();
            }
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

        public override void OnStop()
        {
            Trace.TraceInformation("WebRole stopping...");

            try
            {
                // Stop all cron jobs
                vmManager.StopAllCronJobs();

                // Stop all Runtimes
                vmManager.StopPHPWebSite();
                vmManager.StopMySQLServer();

                // Unmount all windows azure drives
                vmManager.UnmountXDrive();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error in OnStop: {0}", ex.Message);
            }

            if (wcfVMManagerServiceHost != null)
            {
                wcfVMManagerServiceHost.Close();
            }

            diagnosticMonitor.Shutdown();

            Trace.TraceInformation("WebRole stopped!!!");
            base.OnStop();
        }

        private void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e)
        {
            // If a user configuration setting is changing
            if ((from change in e.Changes.OfType<RoleEnvironmentConfigurationSettingChange>()
                 where !(change.ConfigurationSettingName.StartsWith("Microsoft.WindowsAzure.Plugins.RemoteAccess.") ||
                         change.ConfigurationSettingName.StartsWith("Microsoft.WindowsAzure.Plugins.RemoteForwarder."))
                 select change).Any())
            {
                // Set e.Cancel to true to restart this role instance
                e.Cancel = true;
            }
        }
    }
}