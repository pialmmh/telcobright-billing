using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions.ConfigHelper;
using TelcobrightMediation;

namespace LibraryExtensions
{
    public class ConfigPathHelper
    {
        private string TopShelfDirName { get; }
        private string PortalDirName { get; }
        private string UtilInstallConfigDirnameOnly { get; }
        private string GeneratorsHome { get; }
        public ConfigPathHelper(string topShelfDirName, string portalDirName,
            string utilInstallConfigDirnameOnly, string generatorsHome)
        {
            this.TopShelfDirName = topShelfDirName;
            this.PortalDirName = portalDirName;
            this.UtilInstallConfigDirnameOnly = utilInstallConfigDirnameOnly;
            this.GeneratorsHome = generatorsHome;
        }
        public string GetTopShelfConfigDir()
        {
            return Directory.GetParent(Directory
                       .GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName)
                       .FullName).FullName
                   + Path.DirectorySeparatorChar.ToString() + this.TopShelfDirName
                   + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar + "config";
        }
        public string GetPortalBinPath()
        {
            return Directory.GetParent(Directory
                   .GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName).FullName).FullName
                   + Path.DirectorySeparatorChar.ToString() + this.PortalDirName
                   + Path.DirectorySeparatorChar + "bin";
        }
        public string GetOperatorWiseConfigDirInUtil(string operatorShortName)
        {
            return Directory.GetParent(Directory
                       .GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName)
                       .FullName).FullName + Path.DirectorySeparatorChar + this.UtilInstallConfigDirnameOnly
                       + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar +
                        operatorShortName;
        }
        public string GetOperatorWiseTargetFileNameInUtil(string operatorShortName)
        {
            return GetOperatorWiseConfigDirInUtil(operatorShortName) + Path.DirectorySeparatorChar + operatorShortName +
                ".conf";
        }
        public string GetTemplateConfigFileName(string templateFileNameOnly)
        {
            return GetTopShelfConfigDir() + Path.DirectorySeparatorChar + templateFileNameOnly;
        }

        public string GetTargetFileNameForPortal(string operatorShortName)
        {
            return GetPortalBinPath() + Path.DirectorySeparatorChar
                               + operatorShortName + ".conf";
        }
        public string GetUtilInstallConfigFullPath()
        {
            return Directory.GetParent(Directory
                       .GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName)
                       .FullName).FullName + Path.DirectorySeparatorChar + this.UtilInstallConfigDirnameOnly;
        }
        public string getTelcoBillingHome()
        {
            return GetUtilInstallConfigFullPath() + Path.DirectorySeparatorChar
                   + this.GeneratorsHome + Path.DirectorySeparatorChar + "telcoBilling";
        }

        public string getTelcoBillingDbScriptsHome()
        {
            return getTelcoBillingHome() + Path.DirectorySeparatorChar + "dbscripts";
        }

        public string getTelcoBillingSeedDataSqlHome()
        {
            return getTelcoBillingDbScriptsHome() + Path.DirectorySeparatorChar + "seedData"
                   + Path.DirectorySeparatorChar + "sql";
        }
        public string getTelcoBillingSeedDataJsonHome()
        {
            return getTelcoBillingDbScriptsHome() + Path.DirectorySeparatorChar + "seedData"
                   + Path.DirectorySeparatorChar + "json";
        }
        public string getTelcoBillingDdlSqlHome()
        {
            return getTelcoBillingDbScriptsHome() + Path.DirectorySeparatorChar + "ddl"
                   + Path.DirectorySeparatorChar + "sql";
        }
        public string getTelcoBillingDdlJsonHome()
        {
            return getTelcoBillingDbScriptsHome() + Path.DirectorySeparatorChar + "ddl"
                   + Path.DirectorySeparatorChar + "json";
        }
        public string getTelcoBillingDmlSqlHome()
        {
            return getTelcoBillingDbScriptsHome() + Path.DirectorySeparatorChar + "dml"
                   + Path.DirectorySeparatorChar + "sql";
        }
        public string getTelcoBillingDmlJsonHome()
        {
            return getTelcoBillingDbScriptsHome() + Path.DirectorySeparatorChar + "dml"
                   + Path.DirectorySeparatorChar + "json";
        }
    }
}
