<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="adminTitle" ContentPlaceHolderID="ContentPlaceHolder_Title" runat="server">
    Admin
</asp:Content>

<asp:Content ID="adminContent" ContentPlaceHolderID="ContentPlaceHolder_Body" runat="server">
    <%
        string subtab = ViewData["Subtab"].ToString();
    %>
    <div class="subNav">
        <ul>
            <li><a <% if (subtab.Equals("WindowsAzureLogs")) { %>class="selected" <% } %> href="/Admin/Admin?Subtab=WindowsAzureLogs">Windows Azure Logs</a></li>
            <li>|</li>
            <li><a <% if (subtab.Equals("PHPLogs")) { %>class="selected" <% } %> href="/Admin/Admin?Subtab=PHPLogs">PHP Logs</a></li>
            <li>|</li>
            <li><a <% if (subtab.Equals("ConfigureRuntime")) { %>class="selected" <% } %> href="/Admin/Admin?Subtab=ConfigureRuntime">Configure Runtime</a></li>
            <li>|</li>
            <li><a <% if (subtab.Equals("CronJobs")) { %>class="selected" <% } %> href="/Admin/Admin?Subtab=CronJobs">Cron Jobs</a></li>
            <li>|</li>
            <li><a <% if (subtab.Equals("PerformanceMonitor")) { %>class="selected" <% } %> href="/Admin/Admin?Subtab=PerformanceMonitor">Performance Monitor</a></li>
            <li>|</li>
            <li><a <% if (subtab.Equals("BackupAndCleanup")) { %>class="selected" <% } %> href="/Admin/Admin?Subtab=BackupAndCleanup">Backup and Cleanup</a></li>
        </ul>
    </div>
    <% Html.RenderPartial(subtab); %>    
</asp:Content>