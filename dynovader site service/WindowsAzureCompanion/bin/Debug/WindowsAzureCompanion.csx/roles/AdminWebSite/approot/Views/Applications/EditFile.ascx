<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="System.IO" %>

<%
string currentTab = ViewData["CurrentTab"] as string;
string productId = ViewData["ProductId"] as string;
string productTitle = ViewData["ProductTitle"] as string;
string editFileName = ViewData["EditFileName"] as string;
string fileNameWithFullPath = ViewData["EditFileNameWithFullPath"] as string;
%>

<%
using (Html.BeginForm("UpdateFile", "Applications"))
{
    %>
    <div class="grey-border-alt">
        <div class="table-head">Edit "<%= editFileName%>" file for "<%= productTitle%>" application</div>
        <%= Html.TextArea("FileContent", File.ReadAllText(fileNameWithFullPath), 22, 97,
                new { @wrap = "virtual" })%>
        <input type="hidden" name="CurrentTab" value="<%= Html.Encode(currentTab) %>" />
        <input type="hidden" name="productId" value="<%= Html.Encode(productId) %>" />
        <input type="hidden" name="editFileName" value="<%= Html.Encode(editFileName) %>" />
        <div class="floatRight">
            <br/>
            <button type="button" class="btnOrange" onclick="history.go(-1);return true;">Back</button>
            <input type="submit" name="Update" value="Update" class="btnOrange" />
        </div>
    </div>
    <%
}
%>
