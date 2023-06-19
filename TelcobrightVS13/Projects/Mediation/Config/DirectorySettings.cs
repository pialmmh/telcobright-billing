using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelcobrightFileOperations;
using System.IO;
namespace TelcobrightMediation.Config
{
    public class DirectorySettings : IConfigurationSection
    {
        public string SectionName { get; set; } = "Directory Settings";
        public int SectionOrder { get; set; } = 1;
        public string RootDirectory { get; set; }
        public string VaultDirectory { get; set; } = "vault";
        public string ResourcesDirectory { get; set; } = "resources";

        public string FullPathVault => this.RootDirectory + Path.DirectorySeparatorChar +
                                       this.VaultDirectory;
        public string FullPathResources => this.FullPathVault + Path.DirectorySeparatorChar +
                                       this.ResourcesDirectory;
        public Dictionary<string, SyncPair> SyncPairs { get; set; }
        public DirectorySettings()//string rootDirectory)
        {
            //this.RootDirectory = rootDirectory;
        }
    }
}
