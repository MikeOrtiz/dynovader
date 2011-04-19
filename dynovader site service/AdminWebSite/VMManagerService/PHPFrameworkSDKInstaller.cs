using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.ServiceModel.Syndication;

namespace WindowsAzureCompanion.VMManagerService
{
    class PHPFrameworkSDKInstaller : IInstaller
    {
        private SyndicationItem product = null;
        private string downloadFolder = null;
        private string downloadUrl = null;
        private string downloadFileName = null;
        private string installationFolder = null;
        private string applicationPath = null;

        public PHPFrameworkSDKInstaller(string installationFolder, string downloadFolder, SyndicationItem product, string productVersion)
        {
            this.product = product;
            this.downloadFolder = downloadFolder;
            this.downloadUrl = WindowsAzureVMManager.GetDownloadUrlFromProductVersion(product, productVersion);
            this.downloadFileName = WindowsAzureVMManager.GetAttributeValueFromProductVersion(product, productVersion, "downloadFileName");
            this.installationFolder = installationFolder;
            this.applicationPath = WindowsAzureVMManager.GetAttributeValueFromProductVersion(product, productVersion, "applicationPath");            
        }

        public void Install()
        {
            try
            {
                WindowsAzureVMManager.DownloadAndExtractWebArchive(downloadUrl, downloadFileName, downloadFolder, installationFolder, applicationPath);
                Trace.TraceInformation("Successfully installed {0}", product.Title.Text);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unable to install PHP Framework/SDK: {0}", downloadUrl);
                throw ex;
            }
        }
    }
}
