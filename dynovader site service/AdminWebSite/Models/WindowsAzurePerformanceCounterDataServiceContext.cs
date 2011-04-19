// ----------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
// ----------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace WindowsAzureCompanion.AdminWebSite.Models
{
    public class WindowsAzurePerformanceCounterDataServiceContext : TableServiceContext
    {
        public IQueryable<WindowsAzurePerformanceCounter> WindowsAzurePerformanceCounters
        {
            get
            {
                // Get diagnostics events for current deployment only
                return this.CreateQuery<WindowsAzurePerformanceCounter>("WADPerformanceCountersTable")
                    .AddQueryOption("$filter", string.Format("DeploymentId eq '{0}'", RoleEnvironment.DeploymentId));
            }
        }

        public WindowsAzurePerformanceCounterDataServiceContext(string baseAddress, StorageCredentials credentials)
            : base(baseAddress, credentials)
        {
        }
    }
}