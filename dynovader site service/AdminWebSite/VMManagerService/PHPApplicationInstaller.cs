using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.ServiceModel.Syndication;
using System.Collections.Specialized;

namespace WindowsAzureCompanion.VMManagerService
{
    class PHPApplicationInstaller : IInstaller
    {
        private SyndicationItem product = null;
        private string downloadFolder = null;
        private string downloadUrl = null;
        private string downloadFileName = null;
        private string installPath = null;
        private string installationFolder = null;
        private string applicationPath = null;
        private NameValueCollection productProperties = null;

        public PHPApplicationInstaller(string installationFolder, 
            string installPath, 
            string downloadFolder, 
            SyndicationItem product, 
            string productVersion,
            NameValueCollection productProperties)
        {
            this.product = product;
            this.productProperties = productProperties;
            this.downloadFolder = downloadFolder;
            this.downloadUrl = WindowsAzureVMManager.GetDownloadUrlFromProductVersion(product, productVersion);
            this.downloadFileName = WindowsAzureVMManager.GetAttributeValueFromProductVersion(product, productVersion, "downloadFileName");
            this.installPath = installPath;
            this.installationFolder = Path.Combine(installationFolder, installPath.Replace("/", "\\").Trim('\\'));
            this.applicationPath = WindowsAzureVMManager.GetAttributeValueFromProductVersion(product, productVersion, "applicationPath");
        }

        public void Install()
        {
            try                
            {
                // Create folder for application if it does not exist
                if (!Directory.Exists(installationFolder))
                {
                    Directory.CreateDirectory(installationFolder);
                }

                WindowsAzureVMManager.DownloadAndExtractWebArchive(downloadUrl, downloadFileName, downloadFolder, installationFolder, applicationPath);

                // Setup as application in IIS, if specified in the application properties
                IVMManager vmManager = WindowsAzureVMManager.GetVMManager();
                string productId = product.ElementExtensions.ReadElementExtensions<string>("productId", "http://www.w3.org/2005/Atom")[0];
                if (!installPath.Equals("/"))
                {
                    if (vmManager.SetAsApplicationInIIS(installPath, installationFolder))
                    {
                        Trace.TraceInformation("Successfully installed {0} as IIS Application.", product.Title.Text);
                    }
                    else
                    {
                        Trace.TraceInformation("Successfully installed {0}", product.Title.Text);
                    }
                }
                else
                {
                    Trace.TraceInformation("Successfully installed {0}", product.Title.Text);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unable to install Web Application: {0}", downloadUrl);
                throw ex;
            }
        }
    }
}
