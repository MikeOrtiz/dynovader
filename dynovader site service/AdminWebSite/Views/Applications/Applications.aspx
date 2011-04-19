<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="applicationsTitle" ContentPlaceHolderID="ContentPlaceHolder_Title" runat="server">
    <%= ViewData["CurrentTab"]%>
</asp:Content>

<asp:Content ID="applicationsContent" ContentPlaceHolderID="ContentPlaceHolder_Body" runat="server">
    <div class="wraper">
        <div class="pagetilte"><%= ViewData["CurrentTab"]%></div>
        <% 
            string subTab = ViewData["Subtab"].ToString();
            Html.RenderPartial(subTab);
        %>
    </div>
</asp:Content>
