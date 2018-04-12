using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.Mediation.Cdr
{
    public class CollectionResultProcessingSummary
    {
        public double TotalDuration { get; set; }
        public double PartialDuration { get; set; }
        public int FailedCount { get; set; }
        public int SuccessfulCount { get; set; }
        public long MinIdCall { get; set; }
        public long MaxIdCall { get; set; }
        public long StartSequenceNumber { get; set; }
        public long EndSequenceNumber { get; set; }
        public DateTime MaxCallStartTime { get; set; }
        public DateTime MinCallStartTime { get; set; }
    }

}
