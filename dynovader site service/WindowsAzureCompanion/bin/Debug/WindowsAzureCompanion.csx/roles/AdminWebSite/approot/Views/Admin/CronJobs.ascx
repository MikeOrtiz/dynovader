<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<div class="wraper">
    <div class="pagetilte">Cron Jobs</div>

<% 
List<string> cronJobs = ViewData["CronJobs"] as List<string>;
if (cronJobs == null)
{
    %> 
    <p>No cron jobs found.</p>
    <%
}
else
{
    %>
    <div class="grey-border">
        <table width="100%" class="body-content" border="0" cellpadding="0" cellspacing="0">
            <tr>
                <td class="tab-head">Application</td>
                <td class="tab-head">CronJob Name</td>
                <td class="tab-head">Description</td>
                <td class="tab-head">Status</td>
                <td class="tab-head"></td>
            </tr>
            <% 
            int rowCount = 0;
            foreach (string cronJobsInfo in cronJobs)
            {
                string[] cronJobsInfoArray = cronJobsInfo.Split(',');
                for (int i = 1; i < cronJobsInfoArray.Length; i++)
                {
                    string[] cronJobInfoArray = cronJobsInfoArray[i].Split(';');
                    %>
                    <tr <% if (rowCount % 2 == 1) { %>bgcolor="#fbfbfb"<% }%>>
                        <td><%= cronJobsInfoArray[0]%></td>
                        <td><%= cronJobInfoArray[0]%></td>
                        <td><%= cronJobInfoArray[1]%></td>
                        <%
                        string status = cronJobInfoArray[4];
                        if (status.ToLower().Equals("true"))
                        {
                            %>
                            <td>Started</td>
                            <td>
                            <% using (Html.BeginForm("StopCronJob", "Admin"))
                            {                                   
                                %>
                                <input type="hidden" name="productId" value="<%= cronJobsInfoArray[0]%>" />
                                <input type="hidden" name="cronJobIndex" value="<%= (i - 1) %>" />
                                <input type="submit" value="Stop" style="width:80px;" />
                                <%
                            }
                            %></td><%
                        }
                        else
                        {
                           %>
                            <td>Stopped</td>
                            <td>
                            <% using (Html.BeginForm("StartCronJob", "Admin"))
                            {                                   
                                %>
                                <input type="hidden" name="productId" value="<%= cronJobsInfoArray[0]%>" />
                                <input type="hidden" name="cronJobIndex" value="<%= (i - 1) %>" />
                                <input type="submit" value="Start" style="width:80px;" />
                                <%
                            }
                            %></td><%
                        }
                        %>                        
                    </tr>
                    <%
                    rowCount++;
                }
            }
        %>        
        </table>
    </div>
    <%
}
%>
</div>
