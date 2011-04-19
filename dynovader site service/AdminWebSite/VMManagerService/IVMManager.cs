using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Collections.Specialized;

namespace WindowsAzureCompanion.VMManagerService
{
    [ServiceContract]
    public interface IVMManager
    {
        // Install specified applications
        [OperationContract]
        bool InstallApplications(IDictionary<string, string> applicationsToInstall);

        // Uninstall specified applications
        [OperationContract]
        bool UninstallApplications(List<string> applications);

        // Set as application in IIS
        [OperationContract]
        bool SetAsApplicationInIIS(string installPath, string applicationPath);

        // Get Installed Products Info
        [OperationContract]
        IDictionary<string, string> GetInstalledProductsInfo();

        // Get list of Windows Azure Drive Snapshots
        [OperationContract]
        List<string> GetWindowsAzureDriveSnapshots();

        // Create Windows Azure Drive Snapshot
        [OperationContract]
        string CreateWindowsAzureDriveSnapshot(string snapshotComment);

        // Promote Windows Azure Drive Snapshot
        [OperationContract]
        bool PromoteWindowsAzureDriveSnapshot(string uri);

        // Delete Windows Azure Drive Snapshot
        [OperationContract]
        bool DeleteWindowsAzureDriveSnapshot(string uri);

        // Mount Windows Azure Drive
        [OperationContract]
        bool MountXDrive();

        // Unmount Windows Azure Drive
        [OperationContract]
        void UnmountXDrive();

        // Get drive mount status
        [OperationContract]
        bool IsDriveMounted();

        // Reset Windows Azure Drive
        [OperationContract]
        bool ResetWindowsAzureDrive();        
        
        // Whether PHP Web Site is started
        [OperationContract]
        bool IsPHPWebSiteStarted();

        // Setup Runtime Servers
        [OperationContract]
        void SetupRuntimeServers();

        // Start PHP Web Site
        [OperationContract]
        bool StartPHPWebSite();

        // Stop PHP Web Site
        [OperationContract]
        bool StopPHPWebSite(); 

        // Restart PHP Web Site
        [OperationContract]
        bool RestartPHPWebSite();

        // Delete PHP Web Site
        [OperationContract]
        bool DeletePHPWebSite();

        // Has MySQL Server installed?
        [OperationContract]
        bool IsMySQLServerInstalled();

        // Has MySQL Server started?
        [OperationContract]
        bool IsMySQLServerStarted();

        // Get MySQL based database name
        [OperationContract]
        string GetMySQLBasedDBName();

        // Get MySQL based database server port number
        [OperationContract]
        string GetMySQLBasedDBServerPortNumber();

        // Get MySQL based database ini file name
        [OperationContract]
        string GetMySQLBasedDBIniFileName();

        // Get MySQL based database installation folder
        [OperationContract]
        string GetMySQLBasedDBInstallationFolder();

        // Get MySQL based database server IP address
        [OperationContract]
        string GetMySQLBasedDBServerIPAddress();

        // Start MySQL Server
        [OperationContract]
        bool StartMySQLServer();

        // Stop MySQL Server
        [OperationContract]
        bool StopMySQLServer();

        // Restart MySQL Server
        [OperationContract]
        bool RestartMySQLServer();

        // Get application root folder
        [OperationContract]
        string GetApplicationsFolder();

        // Get php.ini file name
        [OperationContract]
        string GetPHPIniFileName();

        // Get php.exe file name
        [OperationContract]
        string GetPHPExeFileName();

        // Get php log file name
        [OperationContract]
        string GetPHPLogFileName();

        // Stop all cron jobs
        [OperationContract]
        void StopAllCronJobs();

        // Get all cron jobs with their status
        [OperationContract]
        List<string> GetCronJobs();

        // Is Cron Job started for specified product id and cron job index?
        [OperationContract]
        bool IsCronJobStarted(string productId, int cronJobIndex);

        // Start cron job for specified product id and cron job index
        [OperationContract]
        bool StartCronJob(string productId, int cronJobIndex);

        // Stop cron job for specified product id and cron job index
        [OperationContract]
        bool StopCronJob(string productId, int cronJobIndex);

        // Restart cron job for specified product id and cron job index
        [OperationContract]
        bool RestartCronJob(string productId, int cronJobIndex);

        // Add progress information
        [OperationContract]
        void UpdateProgressInformation(string message, bool isError);

        // Get progress information of current activity
        [OperationContract]
        IDictionary<string, string> GetProgressInformation();

        // Clear progress information
        [OperationContract]
        void ClearProgressInformation();

        // Is background task running (installation/reset)
        [OperationContract]
        bool IsBackgroundTaskRunning();
    }    
}