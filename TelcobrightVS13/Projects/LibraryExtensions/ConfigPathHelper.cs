using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions.ConfigHelper;

namespace LibraryExtensions
{
    public class ConfigPathHelper
    {
        private string TopShelfDirName { get; }
        private string PortalDirName { get; }
        private string UtilInstallConfigDirnameOnly { get; }
        private string GeneratorsHome { get; }
        string DeploymentProfileName { get; }

        public ConfigPathHelper(string topShelfDirName, string portalDirName,
            string utilInstallConfigDirnameOnly, string generatorsHome,string deploymentProfile)
        {
            this.TopShelfDirName = topShelfDirName;
            this.PortalDirName = portalDirName;
            this.UtilInstallConfigDirnameOnly = utilInstallConfigDirnameOnly;
            this.GeneratorsHome = generatorsHome;
            this.DeploymentProfileName = deploymentProfile;
        }
        public string GetTopShelfDir()
        {
            return Directory.GetParent(Directory
                       .GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName)
                       .FullName).FullName
                   + Path.DirectorySeparatorChar.ToString() + this.TopShelfDirName;
        }
        public string GetDeployedInstancesDir()
        {
            return this.GetTopShelfDir()
                   + Path.DirectorySeparatorChar.ToString() + "deployedInstances";
        }
        public string GetTopShelfConfigDir()
        {
            return GetTopShelfDir()
                   + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar + "config";
        }

        public string GetTopShelfConfigDirForCas()
        {
            return GetTopShelfDir()
                   + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar + "Debug";
        }
        public string GetPortalBinPath()
        {
            return Directory.GetParent(Directory
                   .GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName).FullName).FullName
                   + Path.DirectorySeparatorChar.ToString() + this.PortalDirName
                   + Path.DirectorySeparatorChar + "bin";
        }
        public string GetOperatorWiseConfigDirInUtil(string operatorShortName,string configRoot)
        {
           string directoryPrefix = configRoot == string.Empty ? "" : configRoot + Path.DirectorySeparatorChar;
            return Directory.GetParent(Directory
                       .GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName)
                       .FullName).FullName + Path.DirectorySeparatorChar + this.UtilInstallConfigDirnameOnly
                       + Path.DirectorySeparatorChar + "config"  + Path.DirectorySeparatorChar +
                   directoryPrefix + operatorShortName;
        }
        public string GetOperatorWiseTargetFileNameInUtil(string operatorShortName,string configRoot)
        {
            return GetOperatorWiseConfigDirInUtil(operatorShortName,configRoot) + Path.DirectorySeparatorChar + operatorShortName +
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
        public string getDbscriptHome()
        {
            return GetUtilInstallConfigFullPath() + Path.DirectorySeparatorChar
                   + "config" + Path.DirectorySeparatorChar
                   +this.DeploymentProfileName + Path.DirectorySeparatorChar + "_dbscripts";
        }

        public string getTelcoBillingSeedDataSqlHome()
        {
            return getDbscriptHome() + Path.DirectorySeparatorChar + "seedData"
                   + Path.DirectorySeparatorChar + "sql";
        }
        public string getTelcoBillingSeedDataJsonHome()
        {
            return getDbscriptHome() + Path.DirectorySeparatorChar + "seedData"
                   + Path.DirectorySeparatorChar + "json";
        }
        public string getTelcoBillingDdlHome()
        {
            return getDbscriptHome() + Path.DirectorySeparatorChar + "ddl";
        }
        public string getTelcoBillingDdlSqlHome()
        {
            return getDbscriptHome() + Path.DirectorySeparatorChar + "ddl"
                   + Path.DirectorySeparatorChar + "sql";
        }
        public string getTelcoBillingDdlJsonHome()
        {
            return getDbscriptHome() + Path.DirectorySeparatorChar + "ddl"
                   + Path.DirectorySeparatorChar + "json";
        }
        public string getTelcoBillingDmlSqlHome()
        {
            return getDbscriptHome() + Path.DirectorySeparatorChar + "dml"
                   + Path.DirectorySeparatorChar + "sql";
        }
        public string getTelcoBillingDmlJsonHome()
        {
            return getDbscriptHome() + Path.DirectorySeparatorChar + "dml"
                   + Path.DirectorySeparatorChar + "json";
        }
    }
}
