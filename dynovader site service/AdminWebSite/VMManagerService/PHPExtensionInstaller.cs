using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Syndication;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

namespace WindowsAzureCompanion.VMManagerService
{
    class PHPExtensionInstaller : IInstaller
    {
        private SyndicationItem product = null;
        private string productVersion = null;
        private string downloadFolder = null;
        private string downloadUrl = null;
        private string downloadFileName = null;
        private string installationFolder = null;

        public PHPExtensionInstaller(string installationFolder, string downloadFolder, SyndicationItem product, string productVersion)
        {
            this.product = product;
            this.productVersion = productVersion;
            this.downloadFolder = downloadFolder;
            this.downloadUrl = WindowsAzureVMManager.GetDownloadUrlFromProductVersion(product, productVersion);
            this.downloadFileName = WindowsAzureVMManager.GetAttributeValueFromProductVersion(product, productVersion, "downloadFileName");
            this.installationFolder = installationFolder;
        }

        // Install PHP Custom Extension
        public void Install()
        {
            try
            {
                WindowsAzureVMManager.DownloadAndExtractWebArchive(downloadUrl, 
                    downloadFileName,
                    downloadFolder,
                    Path.Combine(installationFolder, WindowsAzureVMManager.ExtensionsFolderForPHP),
                    null);
            
                // Get php.ini file name
                string phpIniFileName = Path.Combine(installationFolder, "php.ini");

                // Update php.ini and enabled all extension dll specified
                XElement downloadUrlsElement = product.ElementExtensions.Where<SyndicationElementExtension>
                    (x => x.OuterName == "installerFileChoices").FirstOrDefault().GetObject<XElement>();

                // TODO: Use Linq Query instead of foreach loop
                foreach (XElement extension in downloadUrlsElement.Elements())
                {
                    if (extension.Attribute("version").Value.Equals(productVersion))
                    {
                        // Iterate through properties
                        foreach (XElement propertyExtension in extension.Elements().First().Elements())
                        {
                            if (propertyExtension.Attribute("name").Value.Equals("extensions"))
                            {
                                string dllNames = propertyExtension.Attribute("value").Value;
                                foreach (string dllName in dllNames.Split(','))
                                {
                                    // Add each extension to php.ini
                                    FileUtils.AppendToFile(phpIniFileName, "extension=" + dllName);
                                    Trace.TraceInformation("Enabled PHP extension {0}", dllName);
                                }
                                break;
                            }
                        }
                    }
                }

                Trace.TraceInformation("Successfully installed PHP Extension {0}", product.Title.Text);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unable to install PHP Extension {0}: {1}", product.Title.Text, ex.Message);
                throw ex;
            }
        }
    }
}
