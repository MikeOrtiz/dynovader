<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="System.IO" %>

<div class="wraper">
    <div class="pagetilte">PHP Logs</div>
    <%
    string phpLogFileName = ViewData["PHPLogFileName"] as string;
    if (string.IsNullOrEmpty(phpLogFileName))
    {
        %>
            <p>PHP Logs not found.</p>
        <%
    }
    else
    {
        %>
        <%= Html.TextArea("PHPLogsContent", File.ReadAllText(phpLogFileName), 29, 99, 
                    new { @wrap = "virtual", @readonly = "true" })%>
        <%
    }
    %>
</div>