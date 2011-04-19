<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="WindowsAzureCompanion.AdminWebSite.Models"  %>
<%@ Import Namespace="Microsoft.WindowsAzure.Diagnostics"  %>

<div class="wraper">
    <div class="pagetilte">Backup and Cleanup</div>

    <div class="grey-border-alt">
        <div class="table-head">Windows Azure Drive Backup</div>
        <div class="tabPadding">
    <% 
    string message = ViewData["Message"] as string;
    if (!string.IsNullOrEmpty(message))
    {
        %>            
            <div class="message"><% =message%></div>
        <%
    }

    string resetWindowsAzureDrive = ViewData["ResetWindowsAzureDrive"] as string;
    if (!string.IsNullOrEmpty(resetWindowsAzureDrive))
    {
        if (resetWindowsAzureDrive.Equals("True"))
        {
            %>
            <div class="message">Successfully reset the Windows Azure Drive.</div>
            <%
        }
        else
        {
            %>
            <div class="message">Failed to reset the Windows Azure Drive.</div>
            <%
        }
    }
    %>

    <% using (Html.BeginForm("CreateSnapshot", "Admin"))
    {
        %>
        <div><strong>Create Windows Azure Drive Snapshot: </strong><br/>
        Comments:<br/>
        <%= Html.TextArea("SnapshotComment", "", new { @class="comments" })%>
        </p>
        <input type="submit" class="btnOrange" value="Create Snapshot" />
        <%
    }
    %>
    </div>
    
    <%
    List<string> snapshotUris = ViewData["WindowsAzureDriveSnapshots"] as List<string>;
    if (snapshotUris != null && snapshotUris.Count() > 0)
    {
        %>
        <br/>
        <div class="padding5"><strong>Sanapshots:</strong></div>
        <div class="grey-border">
            <table width="100%" class="body-content" border="0" cellpadding="0" cellspacing="0">
                <tr>
                    <td class="tab-head">
                        TimeStamp (UTC)
                    </td>
                    <td class="tab-head">
                        Comment
                    </td>
                    <td class="tab-head">
                        Snapshot Uri
                    </td>
                    <td class="tab-head"></td>
                    <td class="tab-head"></td>
                </tr>
                <%
                int i = 0;
                foreach (string uri in snapshotUris)
                {
                    string[] snapshotDetails = uri.Split(',');
                    %>                        
                    <tr <% if (i % 2 == 1) { %>bgcolor="#fbfbfb"<% }%>>
                        <td><%= snapshotDetails[0]%></td>
                        <td><%= snapshotDetails[1]%></td>
                        <td>                                
                            <%= snapshotDetails[2]%>
                        </td>
                        <td>
                            <%
                            using (Html.BeginForm("PromoteSnapshot", "Admin"))
                            {
                                %>
                                <input type="hidden" name="snapshotUri" value="<%= snapshotDetails[2]%>" />
                                <input type="submit" value="Promote" style="width:70px;" />
                                <%
                            } 
                            %>
                        </td>
                        <td>
                            <%
                            using (Html.BeginForm("DeleteSnapshot", "Admin"))
                            {
                                %>
                                <input type="hidden" name="snapshotUri" value="<%= snapshotDetails[2]%>" />
                                <input type="submit" value="Delete" style="width:70px;" />
                                <%
                            } 
                            %>
                        </td>
                    </tr>
                    <%
                    i++;
                }
                %>
            </table>
        </div>
        <%
    } 
    %>    
    </div>
</div>
<br/>

<div class="grey-border-alt">
    <div class="table-head">Reset Windows Azure Drive</div>
    <div class="tabPadding">
        <% using (Html.BeginForm("ResetWindowsAzureDrive", "Admin"))
        {
            %>            
            <div class="left-ft"><div class="btnOrange"><input type="submit" class="btnOrange" value="Reset"/></div></div>
            <%
        }
        %>

        <div class="left-ft messageItalic"><em>(* This is a long running process and may take several minutes to complete.)</em></div>
        <div class="clear"></div>
        </div>
    </div>
</div>