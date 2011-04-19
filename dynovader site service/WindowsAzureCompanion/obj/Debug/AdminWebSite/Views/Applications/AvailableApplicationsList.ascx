<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="System.ServiceModel.Syndication" %>
<%@ Import Namespace="System.Xml.Linq" %>
<%@ Import Namespace="WindowsAzureCompanion.AdminWebSite.Controllers" %>
<%@ Import Namespace="WindowsAzureCompanion.VMManagerService" %>

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
// Get PHP Web Site Status    
bool isPHPWebSiteStarted = ViewData["PHPWebSiteStatus"].Equals("Started");
        
// Get installed Applications products
IDictionary<string, string> installedProducts = ViewData["InstalledProducts"] as IDictionary<string, string>;

// Categorize Applications products
IEnumerable<ProductCategoryGroup> productCategoryGroups = ViewData["ApplicationCategoryGroups"] as IEnumerable<ProductCategoryGroup>;

List<string> productIDList = ViewData["ProductIDsChecked"] as List<string>;

if ((null == productCategoryGroups) || (productCategoryGroups.Count() == 0))
{
%>
    <p>No Application Category groups defined.</p>
<%
}
else
{   
%>
<p>This list is coming from the feed <span class="textlink"><a target="_blank" href="<%= ViewData["ProductListXmlFeed"]%>"><%= ViewData["ProductListXmlFeed"]%></a></span>.<p>

<p>These applications are provided by third parties. You are responsible for and must separately locate,
read and accept these third party license terms.</p>


<% using (Html.BeginForm("PreparedApplicationsList", "Applications"))
    {
        // Get application URL
        string applicationURL = Server.HtmlEncode(Request.Url.Scheme)
            + "://" + Request.Url.Host + ":" + ViewData["ProductListXmlFeedPort"];
        %>
        <div id="buttonbarTop" class="buttonStandardTop">
            <input type="hidden" name="CurrentTab" value="<%= Html.Encode(ViewData["CurrentTab"]) %>" />
            <div class="floatRight">
                <input type="submit" value="Next" class="btnOrange" />
            </div>
            <br/>
        </div>
    <br />
    <div id="productslist">
        <%
        foreach (var categoryGroup in productCategoryGroups)
        { 
            %>
            <div class="grey-border-alt">
                <div class="table-head"><%= categoryGroup.Category%></div>
                <div class="tabPadding">
            <%
                foreach (SyndicationItem productItem in categoryGroup.Products)
                {
                    string productId = productItem.ElementExtensions.
                        ReadElementExtensions<string>("productId", "http://www.w3.org/2005/Atom")[0];
                    string installCategory = productItem.ElementExtensions.
                        ReadElementExtensions<string>("installCategory", "http://www.w3.org/2005/Atom")[0];

                    XElement hiddenXElement = productItem.ElementExtensions.
                                ReadElementExtensions<XElement>("hidden", "http://www.w3.org/2005/Atom").SingleOrDefault();
                    if (hiddenXElement != null)
                    {
                        string isHidden = productItem.ElementExtensions.
                            ReadElementExtensions<string>("hidden", "http://www.w3.org/2005/Atom")[0];
                        if (!string.IsNullOrEmpty(isHidden))
                        {
                            if (isHidden.ToLower().Equals("true"))
                            {
                                // Ignore hidden application
                                continue;
                            }
                        }
                    }
                
                    // Populate dependencies 
                    object htmlAttributes = null;
                    XElement dependenciesElement = productItem.ElementExtensions.
                                ReadElementExtensions<XElement>("dependencies", "http://www.w3.org/2005/Atom").SingleOrDefault();
                    if (dependenciesElement != null)
                    {
                        htmlAttributes
                            = new
                            {
                                onClick = "if (this.checked) SelectDependencies('"
                                    + productId + "', '"
                                    + dependenciesElement.Value + "');"
                            };
                    }                              

                    string licenseURL = productItem.ElementExtensions.
                                        ReadElementExtensions<string>("licenseURL", "http://www.w3.org/2005/Atom")[0];
                    
                    bool isProductInstalled = installedProducts.Keys.Contains(productId);                    
                    
                    string productInstalledDatetime = String.Empty;
                    string productInstalledPath = String.Empty;
                    string productInstalledVersion = String.Empty;
                    string installPath = String.Empty;
                    
                    bool isChecked = false;

                    if ( (null != productIDList) 
                        && (0 < productIDList.Count) 
                        && productIDList.Contains(productId, StringComparer.CurrentCultureIgnoreCase)
                    ) {
                        isChecked = true;
                    }
                    
                    if (isProductInstalled)
                    {
                        isChecked = true;

                        htmlAttributes
                            = new
                            {
                                disabled = "disabled"
                            };

                        string valueInstalledProduct = installedProducts[productId];
                        string[] productInstalledInfo = valueInstalledProduct.Split(',');

                        productInstalledDatetime = productInstalledInfo[0].Trim();
                        productInstalledPath = productInstalledInfo[1].Trim();
                        productInstalledVersion = productInstalledInfo[2].Trim();
                                                
                        if (!string.IsNullOrEmpty(productInstalledPath))
                        {
                            installPath = applicationURL + productInstalledPath;
                        }
                        else
                        {
                            installPath = applicationURL;
                        }
                    }

                    string productTitle = productItem.Title.Text;                                        
                    %>
                        <div>
                            <%
                            if (isProductInstalled)
                            {
                                %><div class="listingHeadInstalled"><%
                            }
                            else
                            {
                                %><div class="listingHead"><%
                            }
                            %>
                                <%= 
                                    Html.CheckBox(
                                        productId,
                                        isChecked,
                                        htmlAttributes
                                    )
                                %> 
                                <% 
                                    if (isProductInstalled)
                                    {
                                        if (isPHPWebSiteStarted)
                                        {
                                            %>
                                                <%= productTitle %> <span style="font-size:10px;">(Version <%=productInstalledVersion%> installed on <%=Html.Encode(DateTime.Parse(productInstalledDatetime).ToString("MMM dd yyyy HH:mm:ss"))%>.)</span>
                                            <%  
                                        }
                                        else
                                        {
                                            %>
                                                <%= productTitle %> <span class="productInstalled" style="font-size:10px;">(Version <%= productInstalledVersion%> installed on <%=Html.Encode(DateTime.Parse(productInstalledDatetime).ToString("MMM dd yyyy HH:mm:ss"))%>.)</span>
                                            <%  
                                        }                                    
                                    }
                                    else
                                    {
                                        %>
                                        <%= productTitle%>
                                        <%
                                    }
                                %>                            
                            </div>

                            <div class="listing-tab">
                            <br/><span><%= Html.Encode(productItem.Summary.Text)%></span><br/><br/>
                            <span class="licenseURL"><a href="<%= licenseURL %>" target="_new">View license terms</a></span>

                            <%
                            if (isProductInstalled)
                            {
                                if (isPHPWebSiteStarted)
                                {
                                    %><a class="btn" href="<%= installPath %>" target="_companionApp"><span>Launch...</span></a>
                                    <br/><br/>
                                    <%
                                }
                                else
                                {
                                    %>
                                    <p style="font-size:14px;color:Red;">PHP Web Site is stopped, so application cannot be launched.
                                    Please click <span class="textlink"><a href="/Admin/StartPHPRuntime?ActionName=Applications&ControllerName=Applications&ActionSubtabName=AvailableApplicationsList&CurrentTab=<%= ViewData["CurrentTab"]%>">here</a></span> to start the PHP Web site.</p>
                                    <%
                                }
                                %>
                                <% 
                                if (installCategory.Equals(WindowsAzureVMManager.WebApplicationCategoryForPHP))
                                {
                                    %>
                                    <table>
                                        <tr>
                                            <td>Edit File:</td>
                                            <td><%= Html.TextBox("EditFileNameTextBox_" + productId)%></td>
                                            <td><input type="submit" name="EditFileNameButton_<%= productId%>" value="Edit..." style="width:80px;" /></td>
                                        </tr>
                                    </table>
                                    <%
                                }
                            }
                            else
                            {
                                %>
                                <br/><br/>
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
        <br/>
    <%
    }
    %>            
    </div>
    <div id="buttonbarBottom" class="buttonStandardBottom">    
        <div class="floatRight">
            <input type="submit" value="Next" class="btnOrange" />
        </div>
    </div>
    <%
    }
}
%>