<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="WindowsAzureCompanion.AdminWebSite.Models"  %>
<%@ Import Namespace="Microsoft.WindowsAzure.Diagnostics"  %>

<div class="wraper">
    <div class="pagetilte">Performance Monitor</div>

<%
string cpuUsageCount = ViewData["CPUUsageCount"] as string;
string availableMemoryCount = ViewData["AvailableMemoryCount"] as string;

if ((string.IsNullOrEmpty(cpuUsageCount)) || (string.IsNullOrEmpty(availableMemoryCount)))
{
    %>
    <p>Performance Counters not yet available. Please try again later.</p>
    <%
}
else
{    
    string maxCPUUsage = ViewData["MaxCPUUsage"] as string;
    string maxAvailableMemory = ViewData["MaxAvailableMemory"] as string;
    string minCPUUsage = ViewData["MinCPUUsage"] as string;
    string minAvailableMemory = ViewData["MinAvailableMemory"] as string;
    string avgCPUUsage = ViewData["AvgCPUUsage"] as string;
    string avgAvailableMemory = ViewData["AvgAvailableMemory"] as string;
    string samplingFrequency = 
           ViewData["DiagnosticsAndPerformanceCounterCaptureFrequencyInMinutes"] as string;

    Double totalCounters = Math.Round((Double.Parse(cpuUsageCount) + Double.Parse(availableMemoryCount)) / 2);
    %>
    <p style="font-size: 14px;">Following performence counters are based on <b><%= totalCounters%> samples</b> collected with sampling frequency of <b><%= samplingFrequency%> minute(s)</b>.</p>
    <div class="grey-border">
        <table width="100%" class="body-content" border="0" cellpadding="0" cellspacing="0">    
            <tr>
                <td class="tab-head"></td>
                <td class="tab-head">CPU Usage</td>
                <td class="tab-head">Available Memory</td>
            </tr>
            <tr>
                <td>Maximum</td>
                <td><%= maxCPUUsage%> %</td>
                <td><%= maxAvailableMemory%> MB</td>
            </tr>
            <tr bgcolor="#fbfbfb">
                <td>Minimum</td>
                <td><%= minCPUUsage%> %</td>
                <td><%= minAvailableMemory%> MB</td>
            </tr>
            <tr>
                <td>Average</td>
                <td><%= avgCPUUsage%> %</td>
                <td><%= avgAvailableMemory%> MB</td>
            </tr>
        </table>
    </div>
    <%
}
%>
</div>