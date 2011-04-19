<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="System.IO" %>

<div class="wraper">
    <div class="pagetilte">Configure Runtime</div>

<%
// Show MySQL based databse information
string isMySQLBasedDBInstalled = ViewData["IsMySQLBasedDBInstalled"] as string;
if (isMySQLBasedDBInstalled.ToLower().Equals("true"))
{
    string mySQLBasedDBName = ViewData["MySQLBasedDBName"] as string;
    string isMySQLBasedDBStarted = ViewData["IsMySQLBasedDBStarted"] as string;
    string mySQLBasedDBServerPortNumber = ViewData["MySQLBasedDBServerPortNumber"] as string;
    string mySQLBasedDBServerIPAddress = ViewData["MySQLBasedDBServerIPAddress"] as string;
    
    %>
    <div class="grey-border-alt">
        <div class="table-head"><%= mySQLBasedDBName %> (<%= mySQLBasedDBServerIPAddress%>:<%= mySQLBasedDBServerPortNumber %>)</div>        

        <div class="tabPadding">
            <%
                if (isMySQLBasedDBStarted.ToLower().Equals("true"))
                {
                    %>
                    <p><%= mySQLBasedDBName %> Server Status: <b>Started</b></p>
                    <table>
                        <tr>
                            <td>
                            <% using (Html.BeginForm("RestartMySQLServer", "Admin"))
                            {
                                %><input type="submit" value="Restart" style="width:80px;" /><%
                            }
                            %>
                            </td>
                            <td>
                            <% using (Html.BeginForm("StopMySQLServer", "Admin"))
                            {
                                    %><input type="submit" value="Stop" style="width:80px;" /><%
                            }
                            %>
                            </td>
                        </tr>
                    </table>                    
                    <%
                }
                else
                {
                    %>
                    <p><%= mySQLBasedDBName %> Server Status: <b>Stopped</b></p>

                    <table>
                        <tr>
                            <td>
                            <% using (Html.BeginForm("StartMySQLServer", "Admin"))
                               {
                                    %><input type="submit" value="Start" style="width:80px;" /><%
                               }
                            %>
                            </td>
                        </tr>
                    </table>                    
                    <%
                }
            %>
        </div>
    </div>
    <br/>
    <%
}

string phpIniFileName = ViewData["PHPIniFileName"] as string;
if (string.IsNullOrEmpty(phpIniFileName))
{
    %>
    <p>PHP Runtime is not yet installed. Please install the runtime first.</p>
    <%
}
else
{
    %>
    <div class="grey-border-alt">
        <div class="table-head">Manage PHP Web Site</div>
        <div class="tabPadding">
            <%
                if (ViewData["PHPWebSiteStatus"].Equals("Started"))
                {
                    %>
                    <p>PHP Web Site Status: <b>Started</b></p>
                    <table>
                        <tr>
                            <td>
                            <% using (Html.BeginForm("RestartPHPRuntime", "Admin"))
                            {
                                    %><input type="submit" value="Restart" style="width:80px;" /><%
                            }
                            %>
                            </td>
                            <td>
                            <% using (Html.BeginForm("StopPHPRuntime", "Admin"))
                            {
                                    %><input type="submit" value="Stop" style="width:80px;" /><%
                            }
                            %>
                            </td>
                        </tr>
                    </table>
                    <%
                }
                else
                {
                    %>
                    <p>PHP Web Site Status: <b>Stopped</b></p>
                    <table>
                        <tr>
                            <td>
                            <% using (Html.BeginForm("StartPHPRuntime", "Admin"))
                                {
                                    %><input type="submit" value="Start" style="width:80px;" /><%
                                }
                            %>
                            </td>
                        </tr>
                    </table>
                    <%
                }
            %>
    </div>
    </div>
    <br/>

    <div class="grey-border-alt">
        <div class="table-head">Edit php.ini</div>
        <div class="tabPadding">
        <%
        if (File.Exists(phpIniFileName))
        {
            using (Html.BeginForm("UpdatePHPRuntime", "Admin"))
            {
                %>
                <%= Html.TextArea("PHPIniContent", File.ReadAllText(phpIniFileName), 22, 92,
                        new { @wrap = "virtual" })%>
                <div class="floatRight">
                    <br/>
                    <input type="submit" value="Update" class="btnOrange" />
                </div>
                <br/><br/><br/>
            <%
            }
        }
        else
        {
            %><p>php.ini does not exist at desired location.</p><%
        }
        %>
        </div>
	<br/>
    </div>
    <%    
}
%>
</div>