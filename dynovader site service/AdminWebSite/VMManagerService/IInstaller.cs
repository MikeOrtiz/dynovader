using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsAzureCompanion.VMManagerService
{
    // For installation errors
    public class InstallException: Exception
    {
       public InstallException()
       {
       }

       public InstallException(string message): base(message)
       {
       }
    }

    public interface IInstaller
    {
        void Install();
    }
}
