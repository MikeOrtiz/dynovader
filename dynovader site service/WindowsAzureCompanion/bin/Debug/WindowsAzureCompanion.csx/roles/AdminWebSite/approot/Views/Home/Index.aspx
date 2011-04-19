<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="WindowsAzureCompanion.AdminWebSite.Controllers" %>

<asp:Content ID="homeTitle" ContentPlaceHolderID="ContentPlaceHolder_Title" runat="server">
    Home Page
</asp:Content>

<asp:Content ID="homeContent" ContentPlaceHolderID="ContentPlaceHolder_Body" runat="server">
    <div class="wraper">
        <div class="pagetilte">Welcome</div>
        <p><%= ViewData["ApplicationDescription"] %></p>
    </div>
</asp:Content>
