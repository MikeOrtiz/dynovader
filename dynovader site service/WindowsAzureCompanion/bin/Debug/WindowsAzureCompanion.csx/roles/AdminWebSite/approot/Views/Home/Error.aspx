<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<System.Web.Mvc.HandleErrorInfo>" %>

<asp:Content ID="errorTitle" ContentPlaceHolderID="ContentPlaceHolder_Title" runat="server">
    Error
</asp:Content>

<asp:Content ID="errorContent" ContentPlaceHolderID="ContentPlaceHolder_Body" runat="server">
    <div class="wraper">
        <div class="pagetilte">Error</div>        
        <%
            string errorMessage = ViewData["ErrorMessage"] as string;
        %>
        <p>Sorry, an error occurred while processing your request.</p>
        <p style="color:red;"><%= errorMessage %></p>
    </div>
</asp:Content>