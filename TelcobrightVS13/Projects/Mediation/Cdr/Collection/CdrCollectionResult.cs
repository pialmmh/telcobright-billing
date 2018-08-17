using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using LibraryExtensions;
using MediationModel;
using TelcobrightFileOperations;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Mediation.Cdr;
using CdrSummaryTuple = System.ValueTuple<int, int, int, string, string, decimal, decimal, System.ValueTuple<string, string, string, string, string, string, string, System.ValueTuple<string, string, string, string, string, string>>>;

namespace TelcobrightMediation
{
    public enum CollectionResultProcessingState
    {
        BeforeMediation,
        AfterMediation
    }
    public class CdrCollectionResult
    {
        public CollectionResultProcessingState CollectionResultProcessingState { get; set; } =
            CollectionResultProcessingState.BeforeMediation;
        private readonly BlockingCollection<CdrExt> cdrExtErrors = new BlockingCollection<CdrExt>();
        public IReadOnlyCollection<CdrExt> CdrExtErrors => this.cdrExtErrors.ToList().AsReadOnly();
        public int RawCount { get; }
        public decimal RawDurationTotalOfConsistentCdrs { get; set; }
        private readonly ConcurrentDictionary<string, CdrExt> concurrentCdrExts = new ConcurrentDictionary<string, CdrExt>();
        public ne Ne { get; }
        public List<DateTime> DatesInvolved { get; }
        public List<DateTime> HoursInvolved { get; }

        public ConcurrentDictionary<string, CdrExt> ConcurrentCdrExts
        {
            get
            {
                if (this.CollectionResultProcessingState==CollectionResultProcessingState.AfterMediation)
                {
                    throw new Exception("Property ConcurrentCdrExts cannot accessed after mediation, " +
                        "use ProcessedCdrExts & CdrErrors instead.");
                }
                return this.concurrentCdrExts;
            }
        }

        public List<cdrinconsistent> CdrInconsistents { get; }
        public BlockingCollection<CdrExt> ProcessedCdrExts { get; }=new BlockingCollection<CdrExt>();

        public bool IsEmpty
        {
            get
            {
                if (this.CdrInconsistents.Any())
                {
                    return false;
                }
                return this.DatesInvolved == null || this.DatesInvolved.Any() == false ||
                       !this.ConcurrentCdrExts.Any()&& !this.CdrExtErrors.Any() && !this.ProcessedCdrExts.Any();
            }
        }

        public CdrCollectionResult(ne ne, List<CdrExt> cdrExts,
            List<cdrinconsistent> cdrInconsistents, int rawCount)
        {
            this.Ne = ne;
            cdrExts.ForEach(c =>
            {
                if (this.ConcurrentCdrExts.TryAdd(c.UniqueBillId, c)==false)
                    throw new Exception("Could not add cdrExt to concurrent dictionary, probably duplicate item.");
            });
            this.HoursInvolved = this.ConcurrentCdrExts.Values.Select(c => c.StartTime.RoundDownToHour()).Distinct().ToList();
            this.DatesInvolved = this.HoursInvolved.Select(h => h.Date).Distinct().ToList();
            this.CdrInconsistents = cdrInconsistents;
            this.RawCount = rawCount;
        }

        private cdrerror ConvertCdrToCdrError(ICdr cdr, string validationMsg)
        {
            cdr.MediationComplete = 0;
            cdr.ErrorCode = validationMsg;
            return CdrManipulatingUtil.ConvertCdrToCdrError(cdr);
        }
        public void AddNonPartialCdrExtToCdrErrors(CdrExt cdrExt, string errorMessage)
        {
            if (cdrExt.Cdr.PartialFlag != 0)
            {
                throw new Exception("Partial cdrExt must be added to cdrErrors through appropriate method.");
            }
            cdrExt.CdrError = ConvertCdrToCdrError(cdrExt.Cdr, errorMessage);
            this.cdrExtErrors.Add(cdrExt);
            CdrExt removedCdrExt = null;
            if (this.ConcurrentCdrExts.TryRemove(cdrExt.UniqueBillId, out removedCdrExt)==false)
            {
                throw new Exception("Could not remove cdrExt from collection after converting cdr " +
                                    "to cdrError.");
            }
        }

        public void AddPartialCdrToCdrErrors(CdrExt cdrExt,string errorMessage)
        {
            if (cdrExt.Cdr.PartialFlag == 0 && cdrExt.PartialCdrContainer == null)
            {
                throw new Exception("Non partial cdrExt must be added to cdrErrors through appropriate method.");
            }
            cdrExt.CdrError = ConvertCdrToCdrError(cdrExt.Cdr, errorMessage);
            this.cdrExtErrors.Add(cdrExt);
            CdrExt removedCdrExt = null;
            if (this.ConcurrentCdrExts.TryRemove(cdrExt.UniqueBillId, out removedCdrExt) == false)
            {
                throw new Exception("Could not remove cdrExt from collection after converting rawPartial instance " +
                                    "to cdrError.");
            }
        }
    }
}
