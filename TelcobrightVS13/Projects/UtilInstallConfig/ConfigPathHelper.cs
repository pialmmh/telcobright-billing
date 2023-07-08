﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions.ConfigHelper;
using TelcobrightMediation;

namespace InstallConfig
{
    class ConfigPathHelper
    {
        private string TopShelfDirName { get; }
        private string PortalDirName { get; }
        private string UtilInstallConfigDir { get; }
        private string SchedulerSchemaScriptPath { get; }
        public ConfigPathHelper(string topShelfDirName, string portalDirName,string utilInstallConfigDir, string schedulerSchemaScriptPath)
        {
            this.TopShelfDirName = topShelfDirName;
            this.PortalDirName = portalDirName;
            this.UtilInstallConfigDir = utilInstallConfigDir;
            this.SchedulerSchemaScriptPath = schedulerSchemaScriptPath;
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
                       .FullName).FullName + Path.DirectorySeparatorChar +this.UtilInstallConfigDir+Path.DirectorySeparatorChar+
                        operatorShortName;
        }
        public string GetOperatorWiseTargetFileNameInUtil(string operatorShortName)
        {
            return GetOperatorWiseConfigDirInUtil(operatorShortName) + Path.DirectorySeparatorChar+ operatorShortName+
                ".conf";
        }
        public string GetOperatorWiseTargetFileNameInTopShelf(string operatorShortName)
        {
            return GetTopShelfConfigDir()+Path.DirectorySeparatorChar+ operatorShortName + ".conf";
        }

        public string GetTargetFileNameForPortal(string operatorShortName)
        {
            return GetPortalBinPath() + Path.DirectorySeparatorChar
                               + operatorShortName + ".conf";
        }
        public string GetSchedulerScriptPath()
        {
            return Directory.GetParent(Directory
                       .GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName)
                       .FullName).FullName + Path.DirectorySeparatorChar + this.UtilInstallConfigDir + Path.DirectorySeparatorChar +
                       this.SchedulerSchemaScriptPath;
        }
    }
}
