<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="System.ServiceModel.Syndication"  %>
<%@ Import Namespace="System.Xml.Linq"  %>
<%@ Import Namespace="WindowsAzureCompanion.AdminWebSite.Controllers"  %>
<%@ Import Namespace="WindowsAzureCompanion.AdminWebSite.Views.Shared"  %>

<style type="text/css">
    #tooltip {
        position: absolute;
        display:none;
        padding:1px 2px 1px 2px;
        border: 1px solid black;
        background-color:#ebffc1;
    }
    .dropDownWidth{
        width: 200px;
    }
</style>

<script type="text/javascript">
    // This function checks the mouse event
    function checkEvent(e) {
        if (!e) e = window.event;
        if (e.target) targ = e.target;
        else if (e.srcElement) targ = e.srcElement;
        showHideToolTip(targ, e, e.type)
    }

    // This function shows/hides the tooltip
    function showHideToolTip(theDropDown, e, eType) {
        var toolTipObj = new Object();
        toolTipObj = document.getElementById("tooltip");
        toolTipObj.innerHTML = theDropDown.options[theDropDown.selectedIndex].download;
        if (eType == "mouseout") {
            toolTipObj.style.display = "none";
        } else {
            toolTipObj.style.display = "inline";
            toolTipObj.style.top = e.y + 15;
            toolTipObj.style.left = e.x + 10;
        }
    }
</script>  

<script type="text/javascript">
    function SelectDependencies(productId, dependencies) {
        if (dependencies) {
            var dependenciesArray = dependencies.split(",");
            for (i = 0; i < dependenciesArray.length; i++) {
                var checkBox = document.getElementById(dependenciesArray[i]);
                if (checkBox) {
                    checkBox.checked = true;
                }
            }
        }
    };
</script>

<%
Dictionary<string, Product> productsToInstallAll = ViewData["ProductsToInstallToAccept"] as Dictionary<string, Product>;
Dictionary<string, Product> productDependenciesToInstallAll = new Dictionary<string, Product>();

if ((null == productsToInstallAll) || (productsToInstallAll.Count() == 0))
{
    %>
    <p>No Products to be installed defined.</p>
    <%
}
else
{
    %>
    <p>These platforms are provided by third parties. You are responsible for and must seperately locate,
    read and accept these third party license terms.</p>

    <% 
    using (Html.BeginForm("InstallApplications", "Applications"))
    {
        // Get application URL
        string applicationURL = Server.HtmlEncode(Request.Url.Scheme)
            + "://" + Request.Url.Host + ":" + ViewData["ProductListXmlFeedPort"];          
        %>
    <div id="productslist">
        <div class="grey-border-alt">
            <div class="table-head">Product Installation Listing - Application and their Platform Dependencies</div>
            <div class="tabPadding">
        <%
        foreach (KeyValuePair<string, Product> kvp in productsToInstallAll)
        {
            string productID = kvp.Key;
            Product p = kvp.Value;

            if (p.IsInstalled)
            {
                continue;
            }
            
            if (p.IsDependency)
            {
                productDependenciesToInstallAll.Add(productID, p);
                continue;
            }

            string licenseURL = p.ProductItem.ElementExtensions.
                                        ReadElementExtensions<string>("licenseURL", "http://www.w3.org/2005/Atom")[0];            
            string productCategory = p.ProductItem.ElementExtensions.
                               ReadElementExtensions<string>("productCategory", "http://www.w3.org/2005/Atom")[0];
            string productTitle = p.ProductItem.Title.Text;

            // Populate downloadUrls dropdown
            List<ProductDropdownItem> downloadUrls = new List<ProductDropdownItem>();
            if (p.IsInstallValid)
            {
                foreach (ProductDownload pd in p.DownloadURLs)
                {
                    downloadUrls.Add(
                        new ProductDropdownItem
                        {
                            Text = pd.Version,
                            Value = pd.Version,
                            Title = pd.DownloadURL
                        }
                    );
                }
            }
                  
            %>
                <div>
                    <div class="listingHead">
                        <%= Html.CheckBox(productID, true, null) %> 
                        <%= productTitle%>            
                    </div>

                    <div class="listing-tab">
                        <span class="licenseURL"><a href="<%= licenseURL %>" target="_new">View license terms</a></span>
                    
                        <div id="tooltip"></div>
                        <p>
                        Version:
                        <% 
                        if (p.IsInstallValid)
                        {
                            %>
                            <select name="downloadUrls" id="downloadUrls" 
                            class="dropDownWidth" 
                            onmouseover="checkEvent(this.event);" onmouseout="checkEvent(this.event);" onmousemove="checkEvent(this.event);">
                            <% 
                            foreach (ProductDropdownItem downloadURL in downloadUrls)
                            {
                                %>
                                <option value="<%= downloadURL.Value %>" download="<%= downloadURL.Title %>" ><%= downloadURL.Text%></option>
                                <%
                            }
                            %>
                            </select>
                            <%
                            if (downloadUrls.Count > 1)
                            {
                                %>(*Please ensure that the selected version is compatible with other selected/installed products)<%
                            }
                        }
                        else
                        {
                            %>
                            <span style="color:Red">No Valid Downloads.</span>
                            <% 
                        }
                        %>
                        </p>
                        <%
                        // Show product properties (if any)
                        SyndicationElementExtension productPropertiesElementExtension = 
                            p.ProductItem.ElementExtensions.Where<SyndicationElementExtension>
                                (x => x.OuterName == "productProperties").FirstOrDefault();
                        if (productPropertiesElementExtension != null)
                        {
                            foreach (XElement productPropertyExtension in productPropertiesElementExtension.GetObject<XElement>().Elements())
                            {
                                XAttribute captionAttribute = productPropertyExtension.Attribute("caption");
                                if (captionAttribute != null)
                                {
                                    XAttribute defaultValueAttribute = productPropertyExtension.Attribute("defaultValue");
                                    string defaultValue = string.Empty;
                                    if (defaultValueAttribute != null)
                                    {
                                        defaultValue = defaultValueAttribute.Value;
                                    }
                                    %>
                                    <p><%= captionAttribute.Value %>: <%= Html.TextBox(
                                                string.Join("=",
                                                new string[] 
                                                {
                                                    "Parameter_" + productID,
                                                    productPropertyExtension.Attribute("name").Value
                                                }
                                                ),
                                                defaultValue)%></p>
                                    <%
                                }
                            }
                        }
                    %>
                </div>
             </div>
            <%
        }

        foreach (KeyValuePair<string, Product> kvp in productDependenciesToInstallAll)
        {
            string productID = kvp.Key;
            Product p = kvp.Value;
            
            if (p.IsInstalled)
            {
                continue;
            }

            string licenseURL = p.ProductItem.ElementExtensions.
                                        ReadElementExtensions<string>("licenseURL", "http://www.w3.org/2005/Atom")[0];            
            string productCategory = p.ProductItem.ElementExtensions.
                               ReadElementExtensions<string>("productCategory", "http://www.w3.org/2005/Atom")[0];
            string productTitle = p.ProductItem.Title.Text;

            // Populate downloadUrls dropdown
            List<ProductDropdownItem> downloadUrls = new List<ProductDropdownItem>();
            if (p.IsInstallValid)
            {
                foreach (ProductDownload pd in p.DownloadURLs)
                {
                    downloadUrls.Add(
                        new ProductDropdownItem
                        {
                            Text = pd.Version,
                            Value = pd.Version,
                            Title = pd.DownloadURL
                        }
                    );
                }
            }
                  
            %>
                <div>
                    <div class="listingHead">
                        Dependency: <%= 
                            Html.CheckBox(productID, true, null)
                                                        %> 
                        <%= productTitle %>                                  
                    </div>

                    <div class="listing-tab">
                    <span class="licenseURL"><a href="<%= licenseURL %>" target="_new">View license terms</a></span>

                    <%
                    if (p.IsInstallValid)
                    {
                        %>
                        <p>
                        <div id="tooltip"></div>
                        Version:

                        <select name="downloadUrls" id="Select1" class="dropDownWidth" onmouseover="checkEvent(this.event);" onmouseout="checkEvent(this.event);" onmousemove="checkEvent(this.event);">
                        <% 
                            foreach (ProductDropdownItem downloadURL in downloadUrls)
                            {
                                %>
                              <option value="<%= downloadURL.Value %>" download="<%= downloadURL.Title %>" ><%= downloadURL.Text %></option>
                                <% 
                            }
                        %>
                        </select>
                        <%
                        if (downloadUrls.Count > 1)
                        {
                            %>(*Please ensure that the selected version is compatible with other selected/installed products)<%
                        }
                        %>
                        </p>

                        <%
                        // Show product properties (if any)
                        SyndicationElementExtension productPropertiesElementExtension = 
                            p.ProductItem.ElementExtensions.Where<SyndicationElementExtension>
                                (x => x.OuterName == "productProperties").FirstOrDefault();
                        if (productPropertiesElementExtension != null)
                        {
                            foreach (XElement productPropertyExtension in productPropertiesElementExtension.GetObject<XElement>().Elements())
                            {
                                XAttribute captionAttribute = productPropertyExtension.Attribute("caption");
                                if (captionAttribute != null)
                                {
                                    XAttribute defaultValueAttribute = productPropertyExtension.Attribute("defaultValue");
                                    string defaultValue = string.Empty;
                                    if (defaultValueAttribute != null)
                                    {
                                        defaultValue = defaultValueAttribute.Value;
                                    }
                                    %>
                                    <p><%= captionAttribute.Value %>: <%= Html.TextBox(
                                                string.Join("=",
                                                new string[] 
                                                {
                                                    "Parameter_" + productID,
                                                    productPropertyExtension.Attribute("name").Value
                                                }
                                                ),
                                                defaultValue)%></p>
                                    <%
                                }
                            }
                        }
                    }
                    else
                    {
                        %>
                        <p><span style="color:Red; font-style:italic; font-weight:bold">No Valid Downloads</span></p>
                        <% 
                    }
                %>
               </div>
            </div>
            <%
        }
        %>
        </div>
    </div>

    <div id="buttonbarBottom" class="buttonStandardBottom">
        <p>By clicking "Accept", you agree to the license term of each of the application you have selected.</p>
        <input type="hidden" name="CurrentTab" value="<%= Html.Encode(ViewData["CurrentTab"]) %>" />
	<div class="floatRight">
            <button type="button" class="btnOrange" onclick="history.go(-1);return true;">Back</button>
            <input type="submit" name="buttonAccept" value="Accept" class="btnOrange" />
	</div>
    </div>
    <%
    }
}
%>
