using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using TelcobrightMediation;

namespace InstallConfig
{
    public class DeploymentHelper
    {
        public DeploymentPlatform DeploymentPlatform { get; }
        public TelcobrightConfig Tbc { get; }
        public string SolutionDir { get; }
        private string srcBinaryFullPath;
        private string wsTopShelfDir;
        Func<string,string> getConfigDir = wsTopShelfDir=> wsTopShelfDir + Path.DirectorySeparatorChar + "bin"
                                        + Path.DirectorySeparatorChar + "Config";
        private string srcTemplateConfigFileNameOnly = "telcobright.conf";
        private string deploymentBaseDir;
        private string dstBinaryFullPath;
        public DeploymentHelper(TelcobrightConfig tbc, string solutionDir,DeploymentPlatform deploymentPlatform)
        {
            Tbc = tbc;
            SolutionDir = solutionDir;
            this.DeploymentPlatform = deploymentPlatform;
            string srcExecFolderNameOnly = this.DeploymentPlatform == DeploymentPlatform.Win32
                ? "debug"
                : "x64" + Path.DirectorySeparatorChar + "debug";
            this.wsTopShelfDir = this.SolutionDir + Path.DirectorySeparatorChar
                                   + "WS_Topshelf_Quartz";
            this.srcBinaryFullPath = this.wsTopShelfDir + Path.DirectorySeparatorChar
                                 + "bin" + Path.DirectorySeparatorChar + srcExecFolderNameOnly;
            this.deploymentBaseDir = wsTopShelfDir + Path.DirectorySeparatorChar + "deployedInstances";
            if (!Directory.Exists(deploymentBaseDir))
            {
                Directory.CreateDirectory(deploymentBaseDir);
            }
            this.dstBinaryFullPath = this.deploymentBaseDir +Path.DirectorySeparatorChar + getOperatorShortName(tbc);
        }
        public void deploy()
        {                       
            CopyBinaries(this.srcBinaryFullPath, this.dstBinaryFullPath);
            string srcConfigFile = this.getConfigDir(this.wsTopShelfDir) + Path.DirectorySeparatorChar
                                   + this.srcTemplateConfigFileNameOnly;
            string destConfigFile = this.getConfigDir(this.dstBinaryFullPath) + Path.DirectorySeparatorChar
                                    + this.srcTemplateConfigFileNameOnly;
            File.Copy(srcConfigFile, destConfigFile);
        }

        void CopyBinaries(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(sourceFolder))
            {
                throw new Exception($"Source folder '{sourceFolder}' does not exist.");
            }
            if (Directory.Exists(destFolder))
            {
                Directory.Delete(destFolder,true);
            }
            Directory.CreateDirectory(destFolder);
            DirectoryInfo srcDir = new DirectoryInfo(sourceFolder);
            srcDir.DeepCopy(destFolder);
        }
        private static string getOperatorShortName(TelcobrightConfig tbc)
        {
            return tbc.Telcobrightpartner.databasename;
        }
    }
}
