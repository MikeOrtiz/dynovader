using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.ServiceModel.Syndication;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.ServiceModel;
using WindowsAzureCompanion.VMManagerService;

namespace WindowsAzureCompanion.AdminWebSite.Controllers
{
    [HandleError]
    public class HomeController : BaseController
    {
        public HomeController()
        {
            ViewData["CurrentTab"] = "Home";
        }

        public ActionResult Index()
        {
            // Check for error message
            string errorMessage = ViewData["ErrorMessage"] as string;
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return RedirectToAction("Error", "Home", new { ErrorMessage = errorMessage });
            }

            return View();
        }

        public ActionResult Error()
        {
            if (Request.QueryString["ErrorMessage"] == null)
            {
                ViewData["ErrorMessage"] = "Unknown Error.";
            }
            else
            {
                ViewData["ErrorMessage"] = Request.QueryString["ErrorMessage"];
            }

            return View();
        }
    }
}
