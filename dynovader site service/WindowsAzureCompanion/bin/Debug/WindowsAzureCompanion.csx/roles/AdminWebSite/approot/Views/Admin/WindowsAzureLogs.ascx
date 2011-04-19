<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="WindowsAzureCompanion.AdminWebSite.Models"  %>
<%@ Import Namespace="Microsoft.WindowsAzure.Diagnostics"  %>

<div class="wraper">
    <div class="pagetilte">Windows Azure Logs</div>

    <div class="grey-border">
        <table width="100%" class="body-content" border="0" cellpadding="0" cellspacing="0">
            <tr>
                <td width="20%" class="tab-head">
                    TimeStamp (UTC)
                </td>
                <td width="10%" class="tab-head">
                    Level
                </td>
                <td width="70%" class="tab-head">
                    Message
                </td>
            </tr>
            <%
            try
            {
                IQueryable<WindowsAzureLog> windowsAzureLogs = ViewData["WindowsAzureLogs"] as IQueryable<WindowsAzureLog>;
                int i = 0;
                foreach (WindowsAzureLog log in windowsAzureLogs)
                {
                    LogLevel level = (LogLevel)Enum.ToObject(typeof(LogLevel), int.Parse(log.Level));
                    %>
                    <tr <% if (i % 2 == 1) { %>bgcolor="#fbfbfb"<% }%>>
                        <td>
                            <%= Html.Encode(log.Timestamp.ToString(("MMM dd yyyy HH:mm:ss")))%>
                        </td> 
                        <td>
                            <%= Html.Encode(level.ToString())%>
                        </td> 
                        <td style="word-break:break-all;">  
                            <%= Html.Encode(log.Message.Replace("; TraceSource 'WaWorkerHost.exe' event", ""))%> 
                        </td>
                    </tr> 
                    <%
                    i++;
                }
            }
            catch (Exception)
            {
                // Ignore error
            }
            %>
        </table>
    </div>
</div>