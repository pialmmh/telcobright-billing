using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using TelcobrightMediation;

namespace TelcobrightInfra
{
    public class DeploymentHelper
    {
        public DeploymentPlatform DeploymentPlatform { get; }
        public TelcobrightConfig Tbc { get; }
        public string SolutionDir { get; }
        private string srcBinaryFullPath;
        private string wsTopShelfDir;
        Func<string,string> getSrcConfigDir = wsTopShelfDir=> wsTopShelfDir + Path.DirectorySeparatorChar + "bin"
                                        + Path.DirectorySeparatorChar + "config";
        Func<string,string> getDstConfigDir = wsTopShelfDir=> wsTopShelfDir + Path.DirectorySeparatorChar + "config";
        private string srcTemplateConfigFileNameOnly = "telcobright.conf";
        private string deploymentBaseDir;
        private string dstBatchFileTargetDir;
        private string DebugOrReleaseBinariesPath = "";
        public DeploymentHelper(TelcobrightConfig tbc,string solutionDir,DeploymentPlatform deploymentPlatform)
        {
            Tbc = tbc;
            SolutionDir = solutionDir;
            this.DeploymentPlatform = deploymentPlatform;
            this.DebugOrReleaseBinariesPath = tbc.DeploymentProfile.DebugOrReleaseBinaryPath;
            this.wsTopShelfDir = this.SolutionDir + Path.DirectorySeparatorChar
                                   + "WS_Topshelf_Quartz";
            this.srcBinaryFullPath = this.wsTopShelfDir + Path.DirectorySeparatorChar + "bin";
            this.deploymentBaseDir = wsTopShelfDir + Path.DirectorySeparatorChar + "deployedInstances";
            if (!Directory.Exists(deploymentBaseDir))
            {
                Directory.CreateDirectory(deploymentBaseDir);
            }
            this.dstBatchFileTargetDir = this.deploymentBaseDir +Path.DirectorySeparatorChar + getOperatorShortName(tbc);
            if (Directory.Exists(this.dstBatchFileTargetDir) == false)
            {
                Directory.CreateDirectory(this.dstBatchFileTargetDir);
            }
        }
        public void deploy()
        {
            //createBatchFile(this.srcBinaryFullPath, this.dstBinaryFullPath);
            string srcConfigFile = new UpwordPathFinder<DirectoryInfo>("config").FindAndGetFullPath()
                + Path.DirectorySeparatorChar +Tbc.Telcobrightpartner.databasename
                                   + Path.DirectorySeparatorChar + Tbc.Telcobrightpartner.databasename + ".conf";
            string destConfigFile = this.dstBatchFileTargetDir + Path.DirectorySeparatorChar
                                    + this.Tbc.Telcobrightpartner.databasename + ".conf";
            if (File.Exists(destConfigFile))
            {
                File.Delete(destConfigFile);
            }
            File.Copy(srcConfigFile, destConfigFile);

            string batchFileOrShellScript;
            string targetScriptFileName;
            if (this.DeploymentPlatform == DeploymentPlatform.Win32 ||
                this.DeploymentPlatform == DeploymentPlatform.Win64)
            {
                batchFileOrShellScript = wsTopShelfDir + Path.DirectorySeparatorChar +
                                         "bin" + Path.DirectorySeparatorChar + this.DebugOrReleaseBinariesPath + Path.DirectorySeparatorChar +
                                         "WS_Telcobright_Topshelf.exe \"" + destConfigFile + "\"";
                targetScriptFileName = this.dstBatchFileTargetDir + Path.DirectorySeparatorChar
                                              + this.Tbc.Telcobrightpartner.databasename + ".bat";
                File.WriteAllText(targetScriptFileName, batchFileOrShellScript);
            }
            else
            {
                throw new NotImplementedException();//write shell script for linux here
                batchFileOrShellScript = wsTopShelfDir + Path.DirectorySeparatorChar +
                                         "bin" + Path.DirectorySeparatorChar + "debug" + Path.DirectorySeparatorChar +
                                         "WS_Telcobright_Topshelf.exe \"" + destConfigFile + "\"";
                targetScriptFileName = this.dstBatchFileTargetDir + Path.DirectorySeparatorChar
                                       + this.Tbc.Telcobrightpartner.databasename + ".sh";
                File.WriteAllText(targetScriptFileName, batchFileOrShellScript);
                //make .sh file executable here...
            }
        }

        private static string getOperatorShortName(TelcobrightConfig tbc)
        {
            return tbc.Telcobrightpartner.databasename;
        }
    }
}
