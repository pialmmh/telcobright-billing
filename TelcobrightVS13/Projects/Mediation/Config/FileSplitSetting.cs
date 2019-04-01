namespace TelcobrightMediation
{
    public class FileSplitSetting
    {
        public string FileSplitType { get; set; }
        public int SplitFileIfSizeBiggerThanMbyte { get; set; }
        public int BytesPerRecord { get; set; }
        public int MaxRecordsInSingleFile { get; set; }
    }
}