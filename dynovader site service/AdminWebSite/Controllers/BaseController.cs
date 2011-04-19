using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Xml;
using System.ServiceModel.Syndication;
using System.Diagnostics;
using WindowsAzureCompanion.VMManagerService;

namespace WindowsAzureCompanion.AdminWebSite.Controllers
{
    // Common base controller
    public abstract class BaseController : Controller
    {
        protected SyndicationFeed ProductsSyndicationFeed { get; private set; }
        protected IEnumerable<SyndicationItem> ProductListXmlFeedItems { get; private set; }

        public BaseController()
        {
            // Set application title and description
            ViewData["ApplicationTitle"] = RoleEnvironment.GetConfigurationSettingValue("ApplicationTitle");
            ViewData["ApplicationDescription"] = RoleEnvironment.GetConfigurationSettingValue("ApplicationDescription");
            
            if (RoleEnvironment.IsAvailable)
            {
                try
                {
                    // Check Windows Azure Drive status
                    IVMManager vmManager = WindowsAzureVMManager.GetVMManager();
                    if (!vmManager.IsDriveMounted())
                    {
                        string message = String.Format("Windows Azure Drive is not mounted. Please make sure that Windows Azure " +
                            " storage settings are correct in ServiceConfiguration.cscfg file and the page blob specified in " +
                            " 'XDrivePageBlobName' settings is not locked by any application (including another instance of {0})." +
                            " Please restart the service in Windows Azure portal after releasing the page blob lock.",
                            ViewData["ApplicationTitle"]);
                        ViewData["ErrorMessage"] = message;
                        Trace.TraceError(message);
                    }

                    // Set application feed
                    string productFeedUrl = String.Empty;
                    try
                    {
                        productFeedUrl = RoleEnvironment.GetConfigurationSettingValue("ProductListXmlFeed");
                        ViewData["ProductListXmlFeed"] = productFeedUrl;

                        XmlReader reader = XmlReader.Create(productFeedUrl);
                        SyndicationFeed feed = SyndicationFeed.Load(reader);

                        ProductsSyndicationFeed = feed;
                        ProductListXmlFeedItems = feed.Items;
                        ViewData["ProductListXmlFeedItems"] = feed.Items;

                        // Set list of tabs to be shown in UI
                        string[] productTabs = (from item in feed.Items
                                                select item.ElementExtensions.ReadElementExtensions<string>("tabName", "http://www.w3.org/2005/Atom")[0]).Distinct().ToArray<string>();
                        ViewData["ProductTabs"] = productTabs;
                    }
                    catch (Exception ex)
                    {
                        string message =
                            String.Format(
                                        "Unable to get product syndication feed{0}: {1}",
                                        String.IsNullOrEmpty(productFeedUrl) ? String.Empty : " '" + productFeedUrl + "'",
                                        ex.Message
                                    );
                        ViewData["ErrorMessage"] = message;
                        Trace.TraceError(message);
                    }
                }
                catch (Exception ex)
                {
                    string message =
                            String.Format("Failed to get status of Windows Azure Drive. Error: {0}", ex.Message);
                    ViewData["ErrorMessage"] = message;
                    Trace.TraceError(message);
                }                
            }
            else
            {
                ViewData["ErrorMessage"] = "RoleEnvironment is not available.";
            }
        }
    }
}
