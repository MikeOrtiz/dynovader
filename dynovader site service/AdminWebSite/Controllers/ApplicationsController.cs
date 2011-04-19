using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WindowsAzureCompanion.VMManagerService;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Xml;
using System.ServiceModel.Syndication;
using System.Diagnostics;
using System.Xml.Linq;
using System.Collections.Specialized;
using System.IO;

namespace WindowsAzureCompanion.AdminWebSite.Controllers
{
    [HandleError]
    public class ApplicationsController : BaseProductsController
    {
        //
        // GET: /Applications/Applications/tabName
        [Authorize]
        public ActionResult Applications(string id)
        {            
            // Check for error message
            string errorMessage = ViewData["ErrorMessage"] as string;
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return RedirectToAction("Error", "Home", new { ErrorMessage = errorMessage });
            }

            // Set proper tab value
            string currentTab = null;
            if (!string.IsNullOrEmpty(id))
            {
                currentTab = id;                
            }
            else if (Request.QueryString["CurrentTab"] != null)
            {
                currentTab = Request.QueryString["CurrentTab"];
            }
            ViewData["CurrentTab"] = currentTab;

            try
            {
                // Show progress information if installation/reset activities are being performed
                IVMManager vmManager = WindowsAzureVMManager.GetVMManager();
                IDictionary<string, string> progressInformation = vmManager.GetProgressInformation();
                if (progressInformation != null && progressInformation.Count > 0)
                {
                    return RedirectToAction(
                                "ProgressInformation",
                                "Admin",
                                new
                                {
                                    ActionName = "Applications",
                                    ControllerName = "Applications",
                                    ActionSubtabName = "AvailableApplicationsList",
                                    CurrentTab = currentTab
                                }
                            );
                }

                if (Request.QueryString["Subtab"] == null
                    || 0 == Request.QueryString["Subtab"].CompareTo("AvailableApplicationsList")
                )
                {
                    ViewData["Subtab"] = "AvailableApplicationsList";

                    string productIDsChecked = Request.QueryString["ProductIDsChecked"];
                    if (!string.IsNullOrEmpty(productIDsChecked))
                    {
                        string[] productIDs = productIDsChecked.Split(',');
                        List<string> productIDList = new List<string>(productIDs.Length);
                        productIDList.AddRange(productIDs);
                        ViewData["ProductIDsChecked"] = productIDList;
                    }

                    return PrepareViewData(currentTab);
                }
                else if (0 == Request.QueryString["Subtab"].CompareTo("PreparedApplicationsList"))
                {
                    ViewData["Subtab"] = "PreparedApplicationsList";
                    ViewData["ProductsToInstall"] = Request.QueryString["ProductsToInstall"];
                
                    return PreparedApplicationssToInstallViewData();
                }
                else if (Request.QueryString["Subtab"].Equals("EditFile"))
                {
                    try
                    {
                        string productId = Request.QueryString["ProductId"];
                        string[] installInfo = vmManager.GetInstalledProductsInfo()[productId].Split(',');
                        string installPath = installInfo[1];
                        string editFileName = Request.QueryString["EditFileName"];

                        // Get application root path
                        string applicationInstallPath = Path.Combine(vmManager.GetApplicationsFolder(),
                            installPath.Replace("/", "\\").Trim('\\'));

                        string fileNameWithFullPath = Path.Combine(applicationInstallPath,
                            editFileName.Replace("/", "\\").Trim('\\'));
                        if (!System.IO.File.Exists(fileNameWithFullPath))
                        {
                            Trace.TraceError("Specified file does not exist.");
                            return RedirectToAction("Error", "Home", new { ErrorMessage = "Specified file does not exist." });
                        }

                        var productTitles = from item in (IEnumerable<SyndicationItem>)ViewData["ProductListXmlFeedItems"]
                                            where item.ElementExtensions.ReadElementExtensions<string>("productId", "http://www.w3.org/2005/Atom")[0].Equals(productId)
                                            select item.Title.Text;
                        ViewData["Subtab"] = "EditFile";
                        ViewData["ProductId"] = productId;
                        ViewData["ProductTitle"] = productTitles.First();
                        ViewData["EditFileName"] = editFileName;
                        ViewData["EditFileNameWithFullPath"] = fileNameWithFullPath;                    

                        return View();
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Unable to edit file: {0}", ex.Message);
                        return RedirectToAction("Error", "Home", new { ErrorMessage = "Unable to edit file." });
                    }
                }
                else
                {
                    ViewData["Subtab"] = Request.QueryString["Subtab"];
                    return View();
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unknown error: {0}", ex.Message);
                return RedirectToAction("Error", "Home", new { ErrorMessage = string.Format("Unknown error: {0}", ex.Message)});
            }
        }
        
        #region Prepare Applications for Install

        /// <summary>
        /// Prepares platform listing for installation acceptance.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult PreparedApplicationsList(FormCollection form)
        {
            // Get current tab
            string currentTab = form.Get("CurrentTab");

            List<string> editFileNameButtons = form.AllKeys.Where(k => k.StartsWith("EditFileNameButton_")).ToList<string>();
            if (editFileNameButtons.Count == 1)
            {
                string productId = editFileNameButtons[0].Split('_')[1];
                string editFileNameTextBoxName = "EditFileNameTextBox_" + productId;
                string editFileName = form.Get(editFileNameTextBoxName);

                return RedirectToAction(
                            "Applications",
                            "Applications",
                            new
                            {
                                CurrentTab = currentTab,
                                Subtab = "EditFile",
                                ProductId = productId,                     
                                EditFileName = editFileName
                            }
                        );
            }
            else
            {
                try
                {
                    List<string> productsToInstall = new List<string>();
                    List<string> keys = form.AllKeys.ToList<string>();

                    if (0 < keys.Count())
                    {
                        // Prepare information about selected product 
                        for (int i = 0; i < keys.Count(); i++)
                        {
                            string productID = keys[i];
                            if (form.Get(productID).StartsWith("true"))
                            {
                                productsToInstall.Add(productID);
                            }
                        }
                    }

                    return RedirectToAction(
                            "Applications",
                            "Applications",
                            new
                            {
                                CurrentTab = currentTab,
                                Subtab = "PreparedApplicationsList",
                                ProductsToInstall = String.Join(",", productsToInstall.ToArray()),
                            }
                        );
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Unable to prepare platform listing for installation acceptance: {0}", ex.Message);
                    return RedirectToAction("Error", "Home", new { ErrorMessage = "Unable to prepare platform listing for installation acceptance." });
                }
            }
        }

        /// <summary>
        /// Prepares the applications.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult PreparedApplicationssToInstallViewData()
        {
            return PrepareProductsToInstallViewData();
        }
        #endregion

        /// <summary>
        /// Update the file
        /// </summary>
        /// <param name="form">The form.</param>
        /// <returns></returns>
        [Authorize, ValidateInput(false)]
        public ActionResult UpdateFile(FormCollection form)
        {
            try
            {
                // Get current tab
                string currentTab = form.Get("CurrentTab");

                string productId = form.Get("productId");
                string editFileName = form.Get("editFileName");

                IVMManager vmManager = WindowsAzureVMManager.GetVMManager();
                string[] installInfo = vmManager.GetInstalledProductsInfo()[productId].Split(',');
                string installPath = installInfo[1];

                // Get application root path                
                string applicationInstallPath = Path.Combine(vmManager.GetApplicationsFolder(),
                    installPath.Replace("/", "\\").Trim('\\'));
                string fileNameWithFullPath = Path.Combine(applicationInstallPath,
                        editFileName.Replace("/", "\\").Trim('\\'));

                // Get updated file content
                string content = form.Get("FileContent");

                // Update file content
                StreamWriter streamWriter = System.IO.File.CreateText(fileNameWithFullPath);
                streamWriter.Write(content);
                streamWriter.Close();

                return RedirectToAction(
                            "Applications",
                            "Applications",
                            new
                            {
                                CurrentTab = currentTab,
                                ActionSubtabName = "AvailableApplicationsList"
                            }
                        );
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unable to update file: {0}", ex.Message);
                return RedirectToAction("Error", "Home", new { ErrorMessage = "Unable to update file." });
            }
        }

        #region Install Applications

        /// <summary>
        /// Installs the applications.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult InstallApplications(FormCollection form)
        {
            try
            {
                // Get current tab
                string currentTab = form.Get("CurrentTab");

                // All product download urls' versions (selected and not selected both)
                string[] productVersions = form.Get("downloadUrls").Split(',');

                // Remove unnecessary keys in order to preserve proper order
                form.Remove("downloadUrls");
                form.Remove("buttonAccept");
                form.Remove("CurrentTab");

                List<string> productIDs = form.AllKeys.Where(k => !k.StartsWith("Parameter_")).ToList<string>();
                Dictionary<string, string> productsToInstall = new Dictionary<string, string>();

                if (0 < productIDs.Count())
                {
                    // Prepare information about selected product 
                    for (int i = 0; i < productIDs.Count(); i++)
                    {
                        string productID = productIDs[i];
                        if (form.Get(productID).StartsWith("true"))
                        {
                            List<string> parameters = new List<string>();
                            parameters.Add(productVersions[i]);

                            foreach (string k in form.AllKeys.
                                Where(k => k.StartsWith("Parameter_" + productID)).ToArray<string>())
                            {
                                string param = k.Split('=')[1];
                                string value = form.Get(k);
                                parameters.Add(param + "=" + value);
                            }

                            productsToInstall.Add(
                                    productID,
                                    string.Join(",",
                                        parameters.ToArray()
                                    )
                                );
                        }
                    }
                }

                if (null != productsToInstall && 0 < productsToInstall.Count)
                {
                    try
                    {
                        // Install applications
                        IVMManager vmManager = WindowsAzureVMManager.GetVMManager();
                        vmManager.InstallApplications(productsToInstall);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Unable to install applications: {0}", ex.Message);
                        return RedirectToAction("Error", "Home", new { ErrorMessage = "Unable to install applications." });
                    }
                }

                return RedirectToAction(
                            "ProgressInformation",
                            "Admin",
                            new
                            {
                                ActionName = "Applications",
                                ControllerName = "Applications",
                                ActionSubtabName = "AvailableApplicationsList",
                                CurrentTab = currentTab
                            }
                        );
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unable to gather information to install platform products: {0}", ex.Message);
                return RedirectToAction("Error", "Home", new { ErrorMessage = "Unable to gather information to install platform products." });
            }
        }

        #endregion

        protected override ActionResult PrepareViewData()
        {
            return null;
        }

        /// <summary>
        /// Prepares the view data.
        /// </summary>
        /// <returns></returns>
        protected ActionResult PrepareViewData(string tabName)
        {
            ActionResult viewResult = this.PrepareProductsViewData();
            if (null != viewResult)
            {
                return viewResult;
            }

            try
            {
                IEnumerable<ProductCategoryGroup> enumProductCategoryGroup
                    = from item in (IEnumerable<SyndicationItem>)ViewData["ProductListXmlFeedItems"]
                      where item.ElementExtensions.ReadElementExtensions<string>("tabName", "http://www.w3.org/2005/Atom")[0].Equals(tabName)
                      group item by item.ElementExtensions.ReadElementExtensions<string>("productCategory", "http://www.w3.org/2005/Atom")[0] into g
                      select new ProductCategoryGroup
                      {
                          Category = g.Key,
                          Products = g
                      };

                List<ProductCategoryGroup> listProductCategoryGroup = enumProductCategoryGroup.ToList();
                ViewData["UITabName"] = tabName;
                ViewData["ApplicationCategoryGroups"]
                    = 0 < listProductCategoryGroup.Count
                        ? listProductCategoryGroup
                        : null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unable to read list of Platform products: {0}", ex.Message);
                ViewData["ErrorMessage"] = "Unable to read list of Platform products";
            }

            ActionResult result = PrepareInstalledProductsViewData();
            if (null != result)
            {
                return result;
            }

            // Set PHP Web Site status            
            try
            {
                IVMManager vmManager = WindowsAzureVMManager.GetVMManager();
                if (vmManager.IsPHPWebSiteStarted())
                {
                    ViewData["PHPWebSiteStatus"] = "Started";
                }
                else
                {
                    ViewData["PHPWebSiteStatus"] = "Stopped";
                }
            }
            catch (Exception ex)
            {
                ViewData["PHPWebSiteStatus"] = "Unknown";
                Trace.TraceError("Unable to get PHPWebSiteStatus. Error: {0}", ex.Message);
            }

            return PartialView();
        }
    }
}
