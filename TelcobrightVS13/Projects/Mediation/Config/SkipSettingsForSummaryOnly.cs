namespace TelcobrightMediation
{
    public class SkipSettingsForSummaryOnly {
        public bool SkipCdr { get; set; } = false;
        public bool SkipHourlySummary { get; set; } = false;
        public bool SkipTransaction { get; set; } = false;
        public bool SkipChargeable { get; set; } = false;
    }
}