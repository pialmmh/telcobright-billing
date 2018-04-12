using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LibraryExtensions;
using MediationModel;
using TelcobrightFileOperations;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Mediation.Cdr;
using CdrSummaryTuple = System.ValueTuple<int, int, int, string, string, decimal, decimal, System.ValueTuple<string, string, string, string, string, string, string, System.ValueTuple<string, string, string, string, string, string>>>;

namespace TelcobrightMediation
{
    public class CdrCollectionResult
    {
        public int RawCount { get; }
        public ne Ne { get; }
        public List<DateTime> DatesInvolved { get; }
        public List<DateTime> HoursInvolved { get; }
        public ConcurrentDictionary<string, CdrExt> ConcurrentCdrExts { get;}=new ConcurrentDictionary<string, CdrExt>();

        public IEnumerable<CdrExt> MediationCompletedCdrExts => this.ConcurrentCdrExts.Values.Where(
            c => c.Cdr != null && c.Cdr.mediationcomplete == 1);
        public List<cdrinconsistent> CdrInconsistents { get; }
        public BlockingCollection<cdrerror> CdrErrors { get; } = new BlockingCollection<cdrerror>();
        public BlockingCollection<CdrExt> ProcessedCdrExts { get; }=new BlockingCollection<CdrExt>();
        public bool IsEmpty => this.DatesInvolved == null || this.DatesInvolved.Any() == false ||
                               !this.ConcurrentCdrExts.Any() && !this.CdrInconsistents.Any();
        public CollectionResultProcessingSummary CollectionResultProcessingSummary { get; }
        public CdrCollectionResult(ne ne, List<CdrExt> cdrExts,
            List<cdrinconsistent> cdrInconsistents, int rawCount)
        {
            this.Ne = ne;
            cdrExts.ForEach(c =>
            {
                if(this.ConcurrentCdrExts.TryAdd(c.UniqueBillId, c)==false)
                    throw new Exception("Could not add cdrExt to concurrent dictionary, probably duplicate item.");
            });
            this.HoursInvolved = this.ConcurrentCdrExts.Values.Select(c => c.StartTime.RoundDownToHour()).Distinct().ToList();
            this.DatesInvolved = this.HoursInvolved.Select(h => h.Date).Distinct().ToList();
            this.CdrInconsistents = cdrInconsistents;
            this.CollectionResultProcessingSummary = new CollectionResultProcessingSummary();
            this.RawCount = rawCount;
        }
    }
}
