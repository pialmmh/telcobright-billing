using TelcobrightMediation.Cdr;

namespace TelcobrightMediation
{
    public class CdrWritingResult
    {
        public long CdrCount { get; set; }
        public long CdrErrorCount { get; set; }
        public long CdrInconsistentCount { get; set; }
        public PartialCdrWriter PartialCdrWriter { get; set; }
        public long NonPartialCdrCount { get; set; }
        public long NonPartialErrorCount { get; set; }
        public long NormalizedPartialCount { get; set; }
    }
}