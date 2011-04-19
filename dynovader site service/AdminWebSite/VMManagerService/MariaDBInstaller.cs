using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Syndication;
using System.Diagnostics;
using System.IO;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Threading;
using System.Collections.Specialized;

namespace WindowsAzureCompanion.VMManagerService
{
    class MariaDBInstaller : MySQLBasedDBInstaller
    {
        public MariaDBInstaller(string installationFolder, 
            string downloadFolder, 
            SyndicationItem product, 
            string productVersion,
            NameValueCollection properties)
            : base(installationFolder, downloadFolder, product, productVersion, properties)
        {            
        }
    }
}
