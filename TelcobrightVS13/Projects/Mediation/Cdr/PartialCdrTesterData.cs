namespace TelcobrightMediation.Cdr
{
    public class PartialCdrTesterData
    {
        public int NonPartialCount { get; set; }

        public int RawCount { get; set; }

        public decimal RawDurationWithoutInconsistents { get; set; }

        public int RawPartialCount { get; set; }

        public PartialCdrTesterData(int nonPartialCount, int rawCount, decimal rawDurationWithoutInconsistents, int rawPartialCount)
        {
            this.NonPartialCount = nonPartialCount;
            this.RawCount = rawCount;
            this.RawDurationWithoutInconsistents = rawDurationWithoutInconsistents;
            this.RawPartialCount = rawPartialCount;
        }
    }
}