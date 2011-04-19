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
using System.Collections.Specialized;
using System.Xml.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace WindowsAzureCompanion.AdminWebSite.Controllers
{
    public class ProductDownloadItem
    {
        public ProductDownload Download { get; set; }
        public List<string> Versions { get; set; }
    }

    public class ProductDownload
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string DownloadURL { get; set; }
        public string DownloadCondition { get; set; }

        public ProductConditions Conditions { get; set; }
    }

    [DataContract(Name = "ProductConditions")]
    public sealed class ProductConditions
    {
        [DataMember(Name = "Conditions", Order = 1)]
        public List<ProductCondition> Conditions { get; set; }
    }

    [DataContract(Name = "ProductCondition")]
    public sealed class ProductCondition
    {
        [DataMember(Name = "ProductID", Order = 1)]
        public string ProductID { get; set; }

        [DataMember(Name = "Versions", Order = 2)]
        public List<ProductVersion> Versions { get; set; }
    }

    [DataContract(Name = "ProductVersion")]
    public class ProductVersion : object
    {
        [DataMember(Name = "Version")]
        public string Version { get; set; }
    }

    public class Product
    {
        public string ProductID { get; set; }
        public bool IsDependency { get; set; }
        public SyndicationItem ProductItem { get; set; }
        public string Title { get; set; }
        public string EulaURL { get; set; }
        public string InstallPath { get; set; }
        public List<ProductDownload> DownloadURLs { get; set; }
        public List<string> Dependencies { get; set; }
        public bool IsInstalled { get; set; }
        public bool IsInstallValid { get; set; }
    }

    public class ProductCategoryGroup
    {
        public string Category { get; set; }
        public IEnumerable<SyndicationItem> Products { get; set; }
    }

    public abstract class BaseProductsController : BaseController
    {
        protected IDictionary<string, string> ProductsInstalledFeed { get; private set; }
        protected Dictionary<string, string> ProductsInstalled { get; private set; }        
        protected IEnumerable<ProductCategoryGroup> ProductCategoryGroups { get; private set; }
        protected int ProductCategoryGroupsCount { get; private set; }

        protected abstract ActionResult PrepareViewData();        

        /// <summary>
        /// Prepares the installed products view data.
        /// </summary>
        /// <returns></returns>
        protected ActionResult PrepareInstalledProductsViewData()
        {
            try
            {
                // Pass list of installed applications
                IVMManager vmManager = WindowsAzureVMManager.GetVMManager();
                this.ProductsInstalledFeed = vmManager.GetInstalledProductsInfo();

                this.ProductsInstalled = new Dictionary<string, string>();
                foreach (string productId in this.ProductsInstalledFeed.Keys)
                {
                    string valueInstalledProduct = this.ProductsInstalledFeed[productId];
                    string[] productInstalledInfo = valueInstalledProduct.Split(',');

                    string productInstalledVersion = productInstalledInfo[2].Trim();

                    this.ProductsInstalled.Add(productId, productInstalledVersion);                    
                }

                ViewData["InstalledProducts"] = this.ProductsInstalledFeed;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unable to read installed products status: {0}", ex.Message);
                return RedirectToAction("Error", "Home", new { ErrorMessage = "Unable to read installed products status" });
            }

            return null;
        }

        /// <summary>
        /// Prepares the products view data.
        /// </summary>
        /// <returns></returns>
        protected ActionResult PrepareProductsViewData()
        {
            ActionResult result = null;
           
            // Set port number for applications website
            if (Server.HtmlEncode(Request.Url.Scheme).Equals("http"))
            {
                ViewData["ProductListXmlFeedPort"] = RoleEnvironment.CurrentRoleInstance.
                    InstanceEndpoints["HttpIn"].IPEndpoint.Port.ToString();
            }
            else
            {
                ViewData["ProductListXmlFeedPort"] = RoleEnvironment.CurrentRoleInstance.
                    InstanceEndpoints["HttpsIn"].IPEndpoint.Port.ToString();
            }
            
            try
            {
                if (0 < this.ProductListXmlFeedItems.Count())
                {
                    // Categorize products
                    this.ProductCategoryGroups = from item in (IEnumerable<SyndicationItem>)ViewData["ProductListXmlFeedItems"]
                                                 group item by item.ElementExtensions.ReadElementExtensions<string>("productCategory", "http://www.w3.org/2005/Atom")[0] into g
                                                 select new ProductCategoryGroup
                                                 {
                                                     Category = g.Key,
                                                     Products = g
                                                 };

                    this.ProductCategoryGroupsCount = this.ProductCategoryGroups.Count();
                }
                else
                {
                    this.ProductCategoryGroups = null;
                    this.ProductCategoryGroupsCount = 0;
                }

                result = PrepareInstalledProductsViewData();
                if (null != result)
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unable to read list of Products from Atom Feed: {0}", ex.Message);
                return RedirectToAction("Error", "Home", new { ErrorMessage = "Unable to read list of Products from Atom Feed" });
            }

            return null;
        }

        /// <summary>
        /// Prepares the product category groups.
        /// </summary>
        /// <param name="productCategories">The product categories.</param>
        /// <param name="productCategoryGroups">The product category groups.</param>
        /// <returns></returns>
        protected ActionResult PrepareProductCategoryGroups(
            string productCategoriesConfig,
            string productCategoryGroups
        ) {
            string productCategoryListing = String.Empty;
            try
            {
                productCategoryListing = RoleEnvironment.GetConfigurationSettingValue(productCategoriesConfig);

                if (string.IsNullOrEmpty(productCategoryListing))
                {
                    throw new System.NullReferenceException("Unable to read product category configuration setting");
                }
            }
            catch ( Exception ex )
            {
                Trace.TraceError("Unable to read product category configuration setting: {0}", ex.Message);
                return RedirectToAction("Error", "Home", new { ErrorMessage = "Unable to read product category configuration setting." });
            }

            ActionResult viewResult = this.PrepareProductsViewData();
            if (null != viewResult)
            {
                return viewResult;
            }

            try
            {
                if (0 < this.ProductCategoryGroupsCount)
                {
                    char[] commaSplit = { ',' };
                    string[] categoriesArray = productCategoryListing.TrimEnd(commaSplit).Split(commaSplit);
                    if (null == categoriesArray || 0 == categoriesArray.Count())
                    {
                        throw new System.NullReferenceException();
                    }

                    List<string> categoriesList = new List<string>(categoriesArray.Length);
                    categoriesList.AddRange(categoriesArray);
                    List<string> categoriesListTrimmed 
                        = categoriesList.ConvertAll<string>
                            (
                                new Converter<string, string>(
                                    delegate(string str)
                                    {
                                        str = str.Trim();
                                        return str;
                                    }
                                )
                            );

                    IEnumerable<ProductCategoryGroup> enumProductCategoryGroup 
                        = from productCategoryGroup in this.ProductCategoryGroups
                                where categoriesListTrimmed.Contains(productCategoryGroup.Category, StringComparer.OrdinalIgnoreCase)
                                select productCategoryGroup;

                    List<ProductCategoryGroup> listProductCategoryGroup = enumProductCategoryGroup.ToList();

                    ViewData[productCategoryGroups]
                        = 0 < listProductCategoryGroup.Count
                            ? listProductCategoryGroup
                            : null;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unable to read list of Platform products: {0}", ex.Message);
                return RedirectToAction("Error", "Home", new { ErrorMessage = "Unable to read list of Platform products." });
            }

            ActionResult result = PrepareInstalledProductsViewData();
            if (null != result)
            {
                return result;
            }

            return View();
        }

        /// <summary>
        /// Prepares the products to install view data.
        /// </summary>
        /// <returns></returns>
        protected ActionResult PrepareProductsToInstallViewData()
        {
            string productsSelectedToInstall = ViewData["ProductsToInstall"] as string;

            if (!string.IsNullOrEmpty(productsSelectedToInstall))
            {
                string[] productIDsSelectedToInstall = productsSelectedToInstall.Split(',');

                if (0 < productIDsSelectedToInstall.Count())
                {
                    Dictionary<string, bool> productIDsToInstall = new Dictionary<string, bool>();
                    foreach (string productID in productIDsSelectedToInstall)
                    {
                        productIDsToInstall.Add(productID, false);
                    }

                    ActionResult result = PrepareInstalledProductsViewData();
                    if (null != result)
                    {
                        return result;
                    }

                    Dictionary<string, Product> productsToInstallAll = new Dictionary<string, Product>();

                    BuildProductList(productIDsToInstall, ref productsToInstallAll);

                    ValidateProductListDependencies(ref productsToInstallAll);

                    ViewData["ProductsToInstallToAccept"] = productsToInstallAll;
                }
            }

            return View();
        }

        /// <summary>
        /// Validates the product list dependencies.
        /// </summary>
        /// <param name="productsToInstallAll">The products to install all.</param>
        private void ValidateProductListDependencies(ref Dictionary<string, Product> productsToInstallAll)
        {
            Dictionary<string, List<string>> dicProductDependenciesAll = new Dictionary<string, List<string>>();

            foreach (KeyValuePair<string, Product> kvpProductInstall in productsToInstallAll)
            {
                string productID = kvpProductInstall.Key;
                Product p = kvpProductInstall.Value;

                Dictionary<string, List<string>> dicProductDependencies = new Dictionary<string, List<string>>();

                foreach (ProductDownload pd in p.DownloadURLs)
                {
                    if (null == pd.Conditions)
                    {
                        continue;
                    }                   

                    foreach (ProductCondition pc in pd.Conditions.Conditions)
                    {
                        string productIdCondition = pc.ProductID;

                        List<string> listVersions = new List<string>();
                        foreach (ProductVersion v in pc.Versions )
                        {
                            listVersions.Add(v.Version);
                        }

                        if (!dicProductDependencies.Keys.Contains(productIdCondition, StringComparer.OrdinalIgnoreCase))
                        {
                            dicProductDependencies.Add(productIdCondition, listVersions);
                        }
                        else
                        {
                            dicProductDependencies[productIdCondition] 
                                = dicProductDependencies[productIdCondition].Union(listVersions).ToList();
                        }
                    }
                }

                foreach (KeyValuePair<string, List<string>> kvpProductDependency in dicProductDependencies)
                {
                    string productIdDependency = kvpProductDependency.Key;
                    List<string> productIdVersions = kvpProductDependency.Value;
                    if (!dicProductDependenciesAll.Keys.Contains(productIdDependency, StringComparer.OrdinalIgnoreCase))
                    {
                        dicProductDependenciesAll.Add(productIdDependency, productIdVersions);
                    }
                    else
                    {
                        dicProductDependenciesAll[productIdDependency]
                            = dicProductDependenciesAll[productIdDependency].Intersect(productIdVersions).ToList();
                    }
                }
            }

            foreach (KeyValuePair<string, Product> kvpProduct in productsToInstallAll)
            {
                Product p = kvpProduct.Value;

                List<ProductDownload> downloadURLsValid = new List<ProductDownload>();                

                foreach (ProductDownload pd in p.DownloadURLs)
                {
                    if (null == pd.Conditions)
                    {
                        downloadURLsValid.Add(pd);
                        continue;
                    }

                    Dictionary<string, List<string>> dicProductDependencies = new Dictionary<string, List<string>>();

                    foreach (ProductCondition pc in pd.Conditions.Conditions)
                    {
                        List<string> listVersions = new List<string>();
                        foreach (ProductVersion v in pc.Versions)
                        {
                            listVersions.Add(v.Version);
                        }

                        if (dicProductDependencies.Keys.Contains(pc.ProductID, StringComparer.OrdinalIgnoreCase))
                        {
                            dicProductDependencies[pc.ProductID]
                                = dicProductDependencies[pc.ProductID].Union(listVersions).ToList();
                        }
                        else
                        {
                            dicProductDependencies[pc.ProductID] = listVersions;
                        }
                    }

                    bool areConditionsValid = true;
                    foreach (KeyValuePair<string, List<string>> kvpProductCondition in dicProductDependencies)
                    {
                        string productConditionID = kvpProductCondition.Key;
                        List<string> versionsCondition = kvpProductCondition.Value;

                        if (!dicProductDependenciesAll.Keys.Contains(productConditionID, StringComparer.OrdinalIgnoreCase))
                        {
                            areConditionsValid = false;
                            break;
                        }

                        if (0 == dicProductDependenciesAll[productConditionID].Count)
                        {
                            areConditionsValid = false;
                            break;
                        }

                        List<string> productConditionVersionsIntersect
                            = dicProductDependenciesAll[productConditionID].Intersect(versionsCondition).ToList();

                        if (0 == productConditionVersionsIntersect.Count)
                        {
                            areConditionsValid = false;
                            break;
                        }

                        if (this.ProductsInstalled.Keys.Contains(productConditionID, StringComparer.OrdinalIgnoreCase))
                        {
                            if (!productConditionVersionsIntersect.Contains(this.ProductsInstalled[productConditionID], StringComparer.OrdinalIgnoreCase))
                            {
                                areConditionsValid = false;
                                break;
                            }
                        }
                    }

                    if ( areConditionsValid )
                    {
                        downloadURLsValid.Add(pd);
                    }
                }

                if (0 == downloadURLsValid.Count)
                {
                    p.IsInstallValid = false;
                }
                else
                {
                    p.IsInstallValid = true;
                    p.DownloadURLs = downloadURLsValid;
                }
            }
        }

        /// <summary>
        /// Builds the product list.
        /// </summary>
        /// <param name="feed">The feed.</param>
        /// <param name="productsToInstallNew">The products to install new.</param>
        /// <param name="productsInstallList">The products list.</param>
        private void BuildProductList (
            Dictionary<string, bool> productIDsToInstallNew,
            ref Dictionary<string, Product> productsToInstallAll
        )
        {
            if (0 == productIDsToInstallNew.Count)
            {
                return;
            }

            Dictionary<string, bool> productIDsToInstall = new Dictionary<string, bool>(productIDsToInstallNew);
            productIDsToInstallNew.Clear();

            IEnumerable<Product> Products
                = from item in this.ProductListXmlFeedItems
                  where productIDsToInstall.Keys.Contains(item.ElementExtensions.ReadElementExtensions<string>("productId", "http://www.w3.org/2005/Atom")[0])
                  select new Product
                  {
                      ProductID = item.ElementExtensions.ReadElementExtensions<string>("productId", "http://www.w3.org/2005/Atom")[0],
                      ProductItem = item,
                      IsInstalled = this.ProductsInstalled.Keys.Contains(item.ElementExtensions.ReadElementExtensions<string>("productId", "http://www.w3.org/2005/Atom")[0]),
                      IsDependency = productIDsToInstall[item.ElementExtensions.ReadElementExtensions<string>("productId", "http://www.w3.org/2005/Atom")[0]]
                  };

            foreach (Product p in Products)
            {
                string productID = p.ProductID;

                if (p.IsInstalled)
                {
                    continue;
                }

                if (productsToInstallAll.Keys.Contains(productID, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                p.DownloadURLs = null;
                if (0 < p.ProductItem.ElementExtensions.Where<SyndicationElementExtension>(x => x.OuterName == "installerFileChoices").Count())
                {
                    XElement installerFileChoicesElement
                        = p.ProductItem.ElementExtensions.Where<SyndicationElementExtension>(x => x.OuterName == "installerFileChoices").FirstOrDefault().GetObject<XElement>();

                    if (null != installerFileChoicesElement)
                    {
                        p.DownloadURLs = new List<ProductDownload>();

                        foreach (XElement extension in installerFileChoicesElement.Elements())
                        {
                            ProductDownload pd
                                = new ProductDownload
                                    {
                                        DownloadURL = extension.Attribute("url").Value,
                                        Version = (null != extension.Attribute("version")) ? extension.Attribute("version").Value : String.Empty
                                    };

                            if (!string.IsNullOrEmpty(pd.DownloadCondition))
                            {
                                string jsonRaw = Server.HtmlDecode(pd.DownloadCondition);

                                ProductConditions pc = null;
                                this.ConvertJSON(jsonRaw, ref pc);

                                pd.Conditions = pc;
                            }

                            p.DownloadURLs.Add(pd);
                        }
                    }
                }

                // Set dependancies
                p.Dependencies = null;
                XElement dependenciesElement = p.ProductItem.ElementExtensions.
                                ReadElementExtensions<XElement>("dependencies", "http://www.w3.org/2005/Atom").SingleOrDefault();
                if (dependenciesElement != null)
                {                    
                    p.Dependencies = new List<string>();
                    foreach (string dependency in dependenciesElement.Value.Split(','))
                    {
                        p.Dependencies.Add(dependency);

                        if (!productIDsToInstallNew.Keys.Contains(dependency, StringComparer.OrdinalIgnoreCase))
                        {
                            productIDsToInstallNew.Add(dependency, true);
                        }
                    }                    
                }

                productsToInstallAll.Add(
                        productID,
                        p
                    );
            }

            BuildProductList(productIDsToInstallNew, ref productsToInstallAll);
        }

        /// <summary>
        /// Converts the JSON.
        /// </summary>
        /// <param name="jsonRaw">The json raw.</param>
        /// <param name="pc">The pc.</param>
        /// <returns></returns>
        private bool ConvertJSON(string jsonRaw, ref ProductConditions pc)
        {
            bool success = false;
            try
            {
                string json = Regex.Replace(jsonRaw, "\'", "\"");

                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ProductConditions));
                using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
                {
                    pc = ser.ReadObject(ms) as ProductConditions;
                }

                success = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return success;
        }
    }
}
