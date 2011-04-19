using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Diagnostics;
using WindowsAzureCompanion.AdminWebSite.Models;
using WindowsAzureCompanion.VMManagerService;
using System.IO;
using System.Drawing;
using System.Collections.Specialized;
using System.Data.Services.Client;

namespace WindowsAzureCompanion.AdminWebSite.Controllers
{
    public class AdminController : BaseController
    {
        public AdminController()
        {
            ViewData["CurrentTab"] = "Admin";
        }

        //
        // GET: /Admin/

        [Authorize]
        public ActionResult Admin()
        {
            // Check for error message
            string errorMessage = ViewData["ErrorMessage"] as string;
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return RedirectToAction("Error", "Home", new { ErrorMessage = errorMessage });
            }

            if (Request.QueryString["Subtab"] == null)
            {
                ViewData["Subtab"] = "WindowsAzureLogs";
            }
            else
            {
                ViewData["Subtab"] = Request.QueryString["Subtab"];
            }

            try
            {
                IVMManager vmManager = WindowsAzureVMManager.GetVMManager();
                if (ViewData["Subtab"].ToString().Equals("WindowsAzureLogs"))
                {
                    try
                    {
                        CloudStorageAccount storageAccount = WindowsAzureVMManager.GetStorageAccount(true);
                        WindowsAzureLogDataServiceContext context = new WindowsAzureLogDataServiceContext(
                            storageAccount.TableEndpoint.ToString(), storageAccount.Credentials);
                        ViewData["WindowsAzureLogs"] = context.WindowsAzureLogs;
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Unable to create storage credetials and account: {0}", ex.Message);
                        return RedirectToAction("Error", "Home", new { ErrorMessage = "Unable to create storage credetials and account." });
                    }
                }
                else if (ViewData["Subtab"].ToString().Equals("PHPLogs"))
                {
                    try
                    {
                        // Set php log file name
                        ViewData["PHPLogFileName"] = vmManager.GetPHPLogFileName();
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Unable to get PHP log file: {0}", ex.Message);
                        return RedirectToAction("Error", "Home", new { ErrorMessage = "Unable to get PHP log file." });
                    }
                }
                else if (ViewData["Subtab"].ToString().Equals("PerformanceMonitor"))
                {
                    try
                    {
                        CloudStorageAccount storageAccount = WindowsAzureVMManager.GetStorageAccount(true);
                        WindowsAzurePerformanceCounterDataServiceContext context = new WindowsAzurePerformanceCounterDataServiceContext(
                            storageAccount.TableEndpoint.ToString(), storageAccount.Credentials);

                        // TODO: Currently used while loop, need to use LINQ
                        long cpuUsageCount = 0;                    
                        double maxCPUUsage = Double.NaN;
                        double minCPUUsage = Double.NaN;
                        double cpuUsageSum = 0;
                        long availableMemoryCount = 0;                    
                        double maxAvailableMemory = Double.NaN;
                        double minAvailableMemory = Double.NaN;
                        double availableMemorySum = 0;
                        foreach (WindowsAzurePerformanceCounter counter in context.WindowsAzurePerformanceCounters)
                        {
                            if (counter.CounterName.Equals(@"\Processor(_Total)\% Processor Time"))
                            {
                                if ((maxCPUUsage.Equals(Double.NaN)) || (counter.CounterValue > maxCPUUsage))
                                {
                                    maxCPUUsage = counter.CounterValue;
                                }
                                if ((minCPUUsage.Equals(Double.NaN)) || (counter.CounterValue < minCPUUsage))
                                {
                                    minCPUUsage = counter.CounterValue;
                                }

                                cpuUsageSum += counter.CounterValue;
                                cpuUsageCount++;
                            }
                            else if (counter.CounterName.Equals(@"\Memory\Available Mbytes"))
                            {
                                if ((maxAvailableMemory.Equals(Double.NaN)) || (counter.CounterValue > maxAvailableMemory))
                                {
                                    maxAvailableMemory = counter.CounterValue;
                                }
                                if ((minAvailableMemory.Equals(Double.NaN)) || (counter.CounterValue < minAvailableMemory))
                                {
                                    minAvailableMemory = counter.CounterValue;
                                }

                                availableMemorySum += counter.CounterValue;
                                availableMemoryCount++;
                            }
                        }

                        if ((cpuUsageCount != 0) && (availableMemoryCount != 0))
                        {
                            double avgCpuUsage = Math.Round(cpuUsageSum / cpuUsageCount, 2);
                            double avgAvailableMemory = Math.Round(availableMemorySum / availableMemoryCount, 2);

                            ViewData["CPUUsageCount"] = cpuUsageCount.ToString();
                            ViewData["AvailableMemoryCount"] = availableMemoryCount.ToString();

                            ViewData["MaxCPUUsage"] = Math.Round(maxCPUUsage, 2).ToString();
                            ViewData["MaxAvailableMemory"] = Math.Round(maxAvailableMemory, 2).ToString();

                            ViewData["MinCPUUsage"] = Math.Round(minCPUUsage, 2).ToString();
                            ViewData["MinAvailableMemory"] = Math.Round(minAvailableMemory, 2).ToString();

                            ViewData["AvgCPUUsage"] = avgCpuUsage.ToString();
                            ViewData["AvgAvailableMemory"] = avgAvailableMemory.ToString();

                            ViewData["DiagnosticsAndPerformanceCounterCaptureFrequencyInMinutes"] = 
                                RoleEnvironment.GetConfigurationSettingValue("DiagnosticsAndPerformanceCounterCaptureFrequencyInMinutes");
                        }
                    }
                    catch (DataServiceQueryException ex)
                    {
                        Trace.TraceError("Error fetching Performance. Error: {0}", ex.Message);
                        Trace.TraceInformation("Performance Counters not yet available.");
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Unable to create storage credetials and account: {0}", ex.Message);
                        return RedirectToAction("Error", "Home", new { ErrorMessage = "Unable to create storage credetials and account." });
                    }
                }            
                else if (ViewData["Subtab"].ToString().Equals("ConfigureRuntime"))
                {
                    // Set MySQL based server status                
                    string isMySQLBasedDBInstalled = vmManager.IsMySQLServerInstalled().ToString();
                    ViewData["IsMySQLBasedDBInstalled"] = isMySQLBasedDBInstalled;
                    if (isMySQLBasedDBInstalled.ToLower().Equals("true"))
                    {                    
                        ViewData["MySQLBasedDBName"] = vmManager.GetMySQLBasedDBName().ToString();
                        ViewData["IsMySQLBasedDBStarted"] = vmManager.IsMySQLServerStarted().ToString();
                        ViewData["MySQLBasedDBServerPortNumber"] = vmManager.GetMySQLBasedDBServerPortNumber();
                        ViewData["MySQLBasedDBServerIPAddress"] = vmManager.GetMySQLBasedDBServerIPAddress();
                    }

                    // Set php.ini file name
                    ViewData["PHPIniFileName"] = vmManager.GetPHPIniFileName();

                    // Set PHP Web Site status
                    if (vmManager.IsPHPWebSiteStarted())
                    {
                        ViewData["PHPWebSiteStatus"] = "Started";
                    }
                    else
                    {
                        ViewData["PHPWebSiteStatus"] = "Stopped";
                    }

                    // Set php log file name
                    ViewData["PHPLogFileName"] = vmManager.GetPHPLogFileName();
                }
                else if (ViewData["Subtab"].ToString().Equals("CronJobs"))
                {
                    try
                    {
                        // Set php cron jobs
                        ViewData["CronJobs"] = vmManager.GetCronJobs();
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Unable to get PHP Cron Jobs: {0}", ex.Message);
                        return RedirectToAction("Error", "Home", new { ErrorMessage = "Unable to get PHP Cron Jobs." });
                    }
                }
                else if (ViewData["Subtab"].ToString().Equals("BackupAndCleanup"))
                {
                    string message = Request.QueryString["Message"];
                    if (!string.IsNullOrEmpty(message))
                    {
                        ViewData["Message"] = message;
                    }

                    string resetWindowsAzureDrive = Request.QueryString["ResetWindowsAzureDrive"];
                    if (!string.IsNullOrEmpty(resetWindowsAzureDrive))
                    {
                        ViewData["ResetWindowsAzureDrive"] = resetWindowsAzureDrive;
                    }

                    // Get list of Windows Azure Drive Snapshots
                    try
                    {
                        List<string> snapshotUris = vmManager.GetWindowsAzureDriveSnapshots();
                        ViewData["WindowsAzureDriveSnapshots"] = snapshotUris;
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Unable to list Windows Azure Drive Snapshots: {0}", ex.Message);
                        return RedirectToAction("Error", "Home", new { ErrorMessage = "Unable to list Windows Azure Drive Snapshots." });
                    }
                }            
                else if (ViewData["Subtab"].ToString().Equals("ProgressInformation"))
                {
                    // Get progress information of current activity
                    IDictionary<string, string> progressInformation = vmManager.GetProgressInformation();
                    if (progressInformation == null || progressInformation.Count == 0)
                    {
                        // No progress information, redirect to caller if possible
                        string actionName = Request.QueryString["ActionName"];
                        string controllerName = Request.QueryString["ControllerName"];
                        string actionSubtabName = Request.QueryString["ActionSubtabName"];
                        string currentTab = Request.QueryString["CurrentTab"];
                        if ((actionName != null) && (controllerName != null) && (actionSubtabName != null))
                        {
                            return RedirectToAction(
                                actionName,
                                controllerName,
                                new
                                {
                                    Subtab = actionSubtabName,
                                    CurrentTab = currentTab
                                });
                        }
                        else
                        {
                            return RedirectToAction("Error", "Home", new { ErrorMessage = "No progress information available." });
                        }
                    }
                    else
                    {
                        // Get first entry as title and remove it
                        string titleKey = progressInformation.Keys.First();
                        string[] messageInfo = progressInformation[titleKey].Split('|');
                        string progressInformationTitle = messageInfo[1];
                        progressInformation.Remove(titleKey);

                        ViewData["ProgressInformation"] = progressInformation;
                        ViewData["ProgressInformationTitle"] = progressInformationTitle;

                        // Check if last message is an error
                        if (progressInformation.Count > 0)
                        {
                            string lastKey = progressInformation.Keys.Last();
                            messageInfo = progressInformation[lastKey].Split('|');
                            if (messageInfo[2].ToLower().Equals("true"))
                            {
                                // Error found indicating async operation failed, do not refresh page
                                ViewData["ErrorMessage"] = messageInfo[1];
                            }
                            else
                            {
                                // No error, refresh the page after 5 second
                                Response.AppendHeader("Refresh", "5; URL=" + Request.Url.PathAndQuery);
                            }
                        }
                        else
                        {
                            // No progress info yet, refresh the page after 5 second
                            Response.AppendHeader("Refresh", "5; URL=" + Request.Url.PathAndQuery);
                        }
                    }
                }
                else if (ViewData["Subtab"].ToString().Equals("ManageInstances"))
                {
                    // TODO for v2
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unknown error: {0}", ex.Message);
                return RedirectToAction("Error", "Home", new { ErrorMessage = string.Format("Unknown error: {0}", ex.Message) });
            }
            return View();
        }

        // GET: /Admin/WindowsAzureLogs
        [Authorize]
        public ActionResult WindowsAzureLogs()
        {
            return RedirectToAction("Admin", "Admin", new { Subtab = "WindowsAzureLogs" });
        }

        // GET: /Admin/PHPLogs
        [Authorize]
        public ActionResult PHPLogs()
        {
            return RedirectToAction("Admin", "Admin", new { Subtab = "PHPLogs" });
        }

        // GET: /Admin/PerformanceMonitor
        [Authorize]
        public ActionResult PerformanceMonitor()
        {
            return RedirectToAction("Admin", "Admin", new { Subtab = "PerformanceMonitor" });
        }

        // GET: /Admin/ConfigureRuntime
        [Authorize]
        public ActionResult ConfigureRuntime()
        {
            return RedirectToAction("Admin", "Admin", new { Subtab = "ConfigureRuntime" });
        }

        // GET: /Admin/CronJobs
        [Authorize]
        public ActionResult CronJobs()
        {
            return RedirectToAction("Admin", "Admin", new { Subtab = "CronJobs" });
        }

        // GET: /Admin/ConfigureRuntime
        [Authorize, ValidateInput(false)]
        public ActionResult UpdatePHPRuntime(FormCollection form)
        {
            // Update php.ini content
            IVMManager vmManager = WindowsAzureVMManager.GetVMManager();
            string phpIniFileName = vmManager.GetPHPIniFileName();
            if (System.IO.File.Exists(phpIniFileName))
            {
                // Get new ini file content
                string content =  form.Get("PHPIniContent");

                // Update php.ini file
                StreamWriter streamWriter = System.IO.File.CreateText(phpIniFileName);
                streamWriter.Write(content);
                streamWriter.Close();

                // Now restart the PHP web site (required by FastCGI/IIS)
                vmManager.RestartPHPWebSite();
            }

            // Redirect to same ConfigureRuntime subtab
            return RedirectToAction("Admin", "Admin", new { Subtab = "ConfigureRuntime" });
        }

        // GET: /Admin/BackupAndCleanup
        [Authorize]
        public ActionResult CreateSnapshot(FormCollection form)
        {
            try
            {
                // Create Windows Azure Drive Snapshot
                IVMManager vmManager = WindowsAzureVMManager.GetVMManager();
                string uristring = vmManager.CreateWindowsAzureDriveSnapshot(form.Get("SnapshotComment"));
                if (string.IsNullOrEmpty(uristring))
                {
                    Trace.TraceError("Unable to Create Windows Azure Drive Snapshot.");

                    return RedirectToAction("Error", "Home", new { ErrorMessage = "Unable to Create Windows Azure Drive Snapshot." });                    
                }
                else
                {
                    // Redirect to same BackupAndCleanup subtab
                    return RedirectToAction("Admin", "Admin", 
                        new { 
                            Subtab = "BackupAndCleanup",
                            Message = "Created new Windows Azure Drive Snapshot: " + uristring
                        });
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unable to create Windows Azure Drive Snapshot: {0}", ex.Message);
                return RedirectToAction("Error", "Home", new { ErrorMessage = "Unable to create Windows Azure Drive Snapshot." });
            }
        }

        // GET: /Admin/BackupAndCleanup
        [Authorize]
        public ActionResult PromoteSnapshot(FormCollection form)
        {
            try
            {
                // Promote Windows Azure Drive Snapshot
                IVMManager vmManager = WindowsAzureVMManager.GetVMManager();
                bool result = vmManager.PromoteWindowsAzureDriveSnapshot(form.Get("snapshotUri"));
                if (!result)
                {
                    Trace.TraceError("Unable to promote Windows Azure Drive Snapshot.");

                    return RedirectToAction("Error", "Home", new { ErrorMessage = "Unable to promote Windows Azure Drive Snapshot." });
                }
                else
                {
                    return RedirectToAction(
                       "ProgressInformation",
                       "Admin",
                       new
                       {
                           ActionName = "Admin",
                           ControllerName = "Admin",
                           ActionSubtabName = "BackupAndCleanup",
                           CurrentTab = "Admin"
                       }
                   );                    
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unable to propote Windows Azure Drive Snapshot: {0}", ex.Message);
                return RedirectToAction("Error", "Home", new { ErrorMessage = "Unable to create Windows Azure Drive Snapshot." });
            }
        }

        // GET: /Admin/BackupAndCleanup
        [Authorize]
        public ActionResult DeleteSnapshot(FormCollection form)
        {
            try
            {
                // Delete Windows Azure Drive Snapshot
                IVMManager vmManager = WindowsAzureVMManager.GetVMManager();
                bool result = vmManager.DeleteWindowsAzureDriveSnapshot(form.Get("snapshotUri"));
                if (!result)
                {
                    Trace.TraceError("Unable to delete Windows Azure Drive Snapshot.");

                    return RedirectToAction("Error", "Home", new { ErrorMessage = "Unable to delete Windows Azure Drive Snapshot." });
                }
                else
                {
                    // Redirect to same BackupAndCleanup subtab
                    return RedirectToAction("Admin", "Admin",
                        new
                        {
                            Subtab = "BackupAndCleanup",
                            Message = "Deleted Windows Azure Drive snapshot " + form.Get("snapshotUri")
                        });
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unable to delete Windows Azure Drive Snapshot: {0}", ex.Message);
                return RedirectToAction("Error", "Home", new { ErrorMessage = "Unable to delete Windows Azure Drive Snapshot." });
            }
        }

        // GET: /Admin/BackupAndCleanup
        [Authorize]
        public ActionResult ResetWindowsAzureDrive()
        {            
            // Create Windows Azure Drive Snapshot
            IVMManager vmManager = WindowsAzureVMManager.GetVMManager();
            if (vmManager.ResetWindowsAzureDrive())
            {
                return RedirectToAction(
                   "ProgressInformation",
                   "Admin",
                   new
                   {
                       ActionName = "Admin",
                       ControllerName = "Admin",
                       ActionSubtabName = "BackupAndCleanup",
                       CurrentTab = "Admin"
                   }
               );
            }
            else
            {
                return RedirectToAction("Error", "Home", new { ErrorMessage = "Failed to Reset Windows Azure Drive." });
            }
        }

        // GET: /Admin/ConfigureRuntime
        [Authorize]
        public ActionResult StopPHPRuntime()
        {
            try
            {
                // Stop PHP Web Site
                IVMManager vmManager = WindowsAzureVMManager.GetVMManager();
                vmManager.StopPHPWebSite();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to stop PHP web site. Error: {0}", ex.Message);
                return RedirectToAction("Error", "Home", new { ErrorMessage = "Failed to stop PHP web site." });
            }

            // Redirect to same ConfigureRuntime page
            return RedirectToAction("Admin", "Admin", new { Subtab = "ConfigureRuntime" });
        }

        // GET: /Admin/ConfigureRuntime
        [Authorize]
        public ActionResult StartPHPRuntime()
        {
            try
            {
                // Start PHP Web Site
                IVMManager vmManager = WindowsAzureVMManager.GetVMManager();
                vmManager.StartPHPWebSite();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to start PHP web site. Error: {0}", ex.Message);
                return RedirectToAction("Error", "Home", new { ErrorMessage = "Failed to start PHP web site." });
            }

            // Check is PHP web site startup is requested from applications tab
            string actionName = Request.QueryString["ActionName"];
            string controllerName = Request.QueryString["ControllerName"];
            string actionSubtabName = Request.QueryString["ActionSubtabName"];
            string currentTab = Request.QueryString["CurrentTab"];
            if ((actionName != null) && (controllerName != null) && (actionSubtabName != null) && (currentTab != null))
            {
                return RedirectToAction(
                    actionName,
                    controllerName,
                    new
                    {
                        Subtab = actionSubtabName,
                        CurrentTab = currentTab
                    });
            }
            else
            {
                // Redirect to same ConfigureRuntime page
                return RedirectToAction("Admin", "Admin", new { Subtab = "ConfigureRuntime" });
            }
        }

        // GET: /Admin/ConfigureRuntime
        [Authorize]
        public ActionResult RestartPHPRuntime()
        {
            try
            {
                // Restart PHP Web Site
                IVMManager vmManager = WindowsAzureVMManager.GetVMManager();
                vmManager.RestartPHPWebSite();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to restart PHP web site. Error: {0}", ex.Message);
                return RedirectToAction("Error", "Home", new { ErrorMessage = "Failed to restart PHP web site." });
            }

            // Redirect to same ConfigureRuntime page
            return RedirectToAction("Admin", "Admin", new { Subtab = "ConfigureRuntime" });
        }

        // GET: /Admin/ConfigureRuntime
        [Authorize]
        public ActionResult StopMySQLServer()
        {
            try
            {
                // Stop MySQL Server
                IVMManager vmManager = WindowsAzureVMManager.GetVMManager();
                vmManager.StopMySQLServer();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to stop MySQL based server. Error: {0}", ex.Message);
                return RedirectToAction("Error", "Home", new { ErrorMessage = "Failed to stop MySQL based server." });
            }

            // Redirect to same ConfigureRuntime page
            return RedirectToAction("Admin", "Admin", new { Subtab = "ConfigureRuntime" });
        }

        // GET: /Admin/ConfigureRuntime
        [Authorize]
        public ActionResult StartMySQLServer()
        {
            try
            {
                // Start MySQL based server
                IVMManager vmManager = WindowsAzureVMManager.GetVMManager();
                vmManager.StartMySQLServer();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to start MySQL based server. Error: {0}", ex.Message);
                return RedirectToAction("Error", "Home", new { ErrorMessage = "Failed to start MySQL based server." });
            }

            // Redirect to same ConfigureRuntime page
            return RedirectToAction("Admin", "Admin", new { Subtab = "ConfigureRuntime" });
        }

        // GET: /Admin/ConfigureRuntime
        [Authorize]
        public ActionResult RestartMySQLServer()
        {
            try
            {
                // Restart MySQL based server
                IVMManager vmManager = WindowsAzureVMManager.GetVMManager();
                vmManager.RestartMySQLServer();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to restart MySQL based server. Error: {0}", ex.Message);
                return RedirectToAction("Error", "Home", new { ErrorMessage = "Failed to restart MySQL based server." });
            }

            // Redirect to same ConfigureRuntime page
            return RedirectToAction("Admin", "Admin", new { Subtab = "ConfigureRuntime" });
        }

        /// <summary>
        /// Start the Cron Job
        /// </summary>
        /// <param name="form">The form.</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult StartCronJob(FormCollection form)
        {
            try
            {
                string productId = form.Get("productId");
                int cronJobIndex = int.Parse(form.Get("cronJobIndex"));

                // Start cron job
                IVMManager vmManager = WindowsAzureVMManager.GetVMManager();
                if (!vmManager.StartCronJob(productId, cronJobIndex))
                {
                    Trace.TraceError("Failed to start cron job.");
                    return RedirectToAction("Error", "Home", new { ErrorMessage = "Failed to start cron job." });
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to start cron job. Error: {0}", ex.Message);
                return RedirectToAction("Error", "Home", new { ErrorMessage = "Failed to start cron job." });
            }

            return RedirectToAction("Admin", "Admin", new { Subtab = "CronJobs" });
        }

        /// <summary>
        /// Stop the Cron Job
        /// </summary>
        /// <param name="form">The form.</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult StopCronJob(FormCollection form)
        {
            try
            {
                string productId = form.Get("productId");
                int cronJobIndex = int.Parse(form.Get("cronJobIndex"));

                // Stop cron job
                IVMManager vmManager = WindowsAzureVMManager.GetVMManager();
                if (!vmManager.StopCronJob(productId, cronJobIndex))
                {
                    Trace.TraceError("Failed to stop cron job.");
                    return RedirectToAction("Error", "Home", new { ErrorMessage = "Failed to stop cron job." });
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to stop cron job. Error: {0}", ex.Message);
                return RedirectToAction("Error", "Home", new { ErrorMessage = "Failed to stop cron job." });
            }

            return RedirectToAction("Admin", "Admin", new { Subtab = "CronJobs" });
        }

        // GET: /Admin/ProgressInformation
        [Authorize]
        public ActionResult ClearProgressInformation()
        {
            try
            {
                // Clear Progress Information
                IVMManager vmManager = WindowsAzureVMManager.GetVMManager();
                vmManager.ClearProgressInformation();
            
                return RedirectToAction("Admin", "Admin");
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to clear profress information. Error: {0}", ex.Message);
                return RedirectToAction("Error", "Home", new { ErrorMessage = "Failed to clear progress information." });
            }
        }

        // GET: /Admin/BackupAndCleanup
        [Authorize]
        public ActionResult BackupAndCleanup()
        {
            return RedirectToAction("Admin", "Admin", new { Subtab = "BackupAndCleanup" });
        }

        // GET: /Admin/ManageInstances
        [Authorize]
        public ActionResult ManageInstances()
        {
            return RedirectToAction("Admin", "Admin", new { Subtab = "ManageInstances" });
        }

        // GET: /Admin/ProgressInformation
        public ActionResult ProgressInformation()
        {
            // Passon caller information, if any
            string actionName = Request.QueryString["ActionName"];
            string controllerName = Request.QueryString["ControllerName"];
            string actionSubtabName = Request.QueryString["ActionSubtabName"];
            string currentTab = Request.QueryString["CurrentTab"];
            if ((actionName != null) && (controllerName != null) && (actionSubtabName != null))
            {
                return RedirectToAction(
                    "Admin",
                    "Admin",
                    new
                    {
                        Subtab = "ProgressInformation",
                        ActionName = actionName,
                        ControllerName = controllerName,
                        ActionSubtabName = actionSubtabName,
                        CurrentTab = currentTab
                    });
            }
            else
            {
                return RedirectToAction("Admin", "Admin", new { Subtab = "ProgressInformation" });
            }
        }
    }
}
