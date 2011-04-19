<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<div class="wraper">    
<%
    string progressInformationTitle = ViewData["ProgressInformationTitle"] as string;
    if (string.IsNullOrEmpty(progressInformationTitle))
    {
        %>
        <div class="pagetilte">Progress Information</div>        
        <p style="color:Red;">No progress information available.</p>
        <%
    }
    else
    {
        %>
        <div class="pagetilte"><%= progressInformationTitle%></div>
        <%
        IDictionary<string, string> progressInformation = ViewData["ProgressInformation"] as IDictionary<string, string>;

        // Show messages in reverse order
        string[] keys = progressInformation.Keys.ToArray();
        string errorMessage = string.Empty;
        if (keys.Length > 0)
        {
            Array.Reverse(keys);

            // Check if there is any error information
            errorMessage = ViewData["ErrorMessage"] as string;
            if (!string.IsNullOrEmpty(errorMessage))
            {
                %>
                <p style="color:Red;"><%= errorMessage%></p>
                
                <% using (Html.BeginForm("ClearProgressInformation", "Admin"))
                {
                    %>
                    <p style="color:Red;">Please look at <span class="textlink"><a href="/Admin/WindowsAzureLogs">Windows Azure Logs</a></span> for details or click
                    <input type="submit" value="Clear"  style="width:200;" /> to clear the error information and retry or perform other operations.</p>
                    <%
                }
            }
        }
                
        %>
        <br />
        <div class="grey-border">
            <table width="100%" class="body-content" border="0" cellpadding="0" cellspacing="0">
                <tr>
                    <td width="20%" class="tab-head">
                        TimeStamp (UTC)
                    </td>
                    <td width="80%" class="tab-head">
                        Message
                    </td>
                </tr>
            <%
                // Show messages in reverse order
                int i = 0;
                foreach (string key in keys)
                {
                    string[] valueInfo = progressInformation[key].Split('|');                    
                    %>
                        <tr <% if (i % 2 == 1) { %>bgcolor="#fbfbfb"<% }%>>
                            <td><%= valueInfo[0] %></td>
                            <td><%= valueInfo[1] %></td>
                        </tr>
                    <%
                    i++;
                }
            %>            
            </table>
        </div>
        <%         
        if (string.IsNullOrEmpty(errorMessage))
        {
            %>    
            <!-- Show animation -->
            <img alt="Loading" src="/Content/Images/loading.gif"
                style="Z-INDEX: 101; LEFT: 700px; POSITION: absolute; TOP: 240px"
             />
            <%
        }
    } 
%>
</div>