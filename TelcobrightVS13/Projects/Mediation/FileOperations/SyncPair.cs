namespace TelcobrightFileOperations
{
    public class SyncPair
    {
        //don't use constructor, will be populated from json config file
        public string Name { get; set; }
        public bool SkipCopyingToDestination { get; set; }//this is for directory sync process, has effect on autoarchivecdr
        public bool SkipSourceFileListing { get; set; }
        public SyncLocation SrcSyncLocation { get; set; }
        public SyncLocation DstSyncLocation { get; set; }
        public SyncSettingsSource SrcSettings { get; set; }
        public SyncSettingsDest DstSettings { get; set; }
        public override string ToString()
        {
            return this.Name;
        }

        public SyncPair(string pairName)
        {
            this.Name = pairName;
            this.DstSyncLocation = this.DstSyncLocation;
            this.SrcSyncLocation = this.SrcSyncLocation;
        }

    }
}