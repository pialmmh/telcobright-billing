using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelcobrightFileOperations;

namespace TelcobrightMediation.Config
{
    public class DirectorySettings : IConfigurationSection
    {
        public string SectionName { get; set; }
        public int SectionOrder { get; set; }
        public string ApplicationRootDirectory { get; set; }
        public List<Vault> Vaults { get; set; } = new List<Vault>();
        public Dictionary<string, FileLocation> FileLocations { get; set; }
        public Dictionary<string, SyncLocation> SyncLocations { get; set; }
        public Dictionary<string, SyncPair> SyncPairs { get; set; }
        public DirectorySettings(string SectionName)
        {
            this.SectionName = SectionName;
            this.SectionOrder = 1;
            this.FileLocations = new Dictionary<string, FileLocation>();
            this.SyncLocations = new Dictionary<string, SyncLocation>();
            this.SyncPairs = new Dictionary<string, SyncPair>();
        }
    }
}
