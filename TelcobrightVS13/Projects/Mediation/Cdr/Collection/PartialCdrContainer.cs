using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LibraryExtensions;
using MediationModel;
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Mediation.Cdr;

namespace TelcobrightMediation
{
    public class PartialCdrContainer
    {
        public string UniqueBillId => this.NewCdrEquivalent.UniqueBillId;
        public DateTime StartTime => this.NewCdrEquivalent.StartTime;//handled by aggregator already in case of multiple instances have diff date somehow
        public List<cdrpartialrawinstance> NewRawInstances { get; }
        public List<cdrpartialrawinstance> PrevRawInstances { get; }
        public cdrpartialreference CdrPartialReference { get; private set; }
        public cdrpartiallastaggregatedrawinstance LastProcessedAggregatedRawInstance { get; }
        public cdrpartiallastaggregatedrawinstance NewAggregatedRawInstance { get; private set; }
        public cdr PrevProcessedCdrInstance { get; }
        public cdrerror PrevProcessedErrorInstance { get; }
        public cdr NewCdrEquivalent { get; private set; }
        public bool IsErrorCdr { get; } = false;//prev agg instances of a new partial instance can already be in error
        public List<cdrpartialrawinstance> CombinedNewAndOldUnprocessedInstance { get; private set; }

        public PartialCdrContainer(List<cdrpartialrawinstance> newRawInstances,
            List<cdrpartialrawinstance> prevRawInstances,
            cdrpartialreference cdrPartialreference,
            cdrpartiallastaggregatedrawinstance lastAggregatedRawInstance,
            cdr prevProcessedCdrInstance, cdrerror prevProcessedErrorInstance)
        {
            this.NewRawInstances = newRawInstances;
            this.PrevRawInstances = prevRawInstances;
            this.CdrPartialReference = cdrPartialreference;
            this.LastProcessedAggregatedRawInstance = lastAggregatedRawInstance;
            this.PrevProcessedCdrInstance = prevProcessedCdrInstance;
            if (prevProcessedErrorInstance != null)
            {
                this.PrevProcessedErrorInstance = prevProcessedErrorInstance;
                this.IsErrorCdr = true;
            }
        }

        public void Aggregate()
        {
            this.CombinedNewAndOldUnprocessedInstance = this.PrevRawInstances.Concat(this.NewRawInstances).ToList();
            this.NewCdrEquivalent =
                new IcdrImplConverter<cdr>().Convert(CdrConversionUtil.Clone(this.NewRawInstances.Last()));
            this.NewCdrEquivalent.DurationSec =
                this.CombinedNewAndOldUnprocessedInstance.Sum(c => c.DurationSec);
            if (this.LastProcessedAggregatedRawInstance != null) //if there is a prev instance
                this.NewCdrEquivalent.StartTime = this.LastProcessedAggregatedRawInstance.StartTime;
            this.NewCdrEquivalent.AnswerTime = this.CombinedNewAndOldUnprocessedInstance.Min(c => c.AnswerTime);
            this.NewCdrEquivalent.EndTime = this.CombinedNewAndOldUnprocessedInstance.Max(c => c.EndTime);
            DateTime? minConnectTime = this.CombinedNewAndOldUnprocessedInstance.Where(c => c.ConnectTime != null)
                .Select(c => c.ConnectTime).FirstOrDefault();
            if (minConnectTime != null) this.NewCdrEquivalent.ConnectTime = minConnectTime;
            //this.NewCdrEquivalent.PartialFlag = this.CombinedNewAndOldUnprocessedInstance.Max(c=>c.PartialFlag);
            if(this.NewCdrEquivalent.PartialFlag<=0) throw new Exception("Partial flag must be > 0 for aggregated " +
                                                                    "partial equivalent cdr.");
            this.NewCdrEquivalent.FinalRecord = 1;
            this.NewAggregatedRawInstance =
                new IcdrImplConverter<cdrpartiallastaggregatedrawinstance>().Convert(this.NewCdrEquivalent);
            UpdatePartialCdrReferences(this.NewCdrEquivalent, this.CombinedNewAndOldUnprocessedInstance);
            if (this.IsErrorCdr)
            {
                NewCdrEquivalent.ErrorCode = this.PrevProcessedErrorInstance.ErrorCode;
            }
        }

        private void UpdatePartialCdrReferences(cdr newMediatableCdrInstance,
            List<cdrpartialrawinstance> concatedNewAndOldRawinstances)
        {
            this.CdrPartialReference = this.CdrPartialReference ??
                                       new cdrpartialreference()
                                       {
                                           UniqueBillId = newMediatableCdrInstance.UniqueBillId,
                                           switchid = newMediatableCdrInstance.SwitchId,
                                           CallDate = newMediatableCdrInstance.StartTime.Date,
                                       };
            this.CdrPartialReference.lastFilename = newMediatableCdrInstance.FileName;
            this.CdrPartialReference.lastIdcall = newMediatableCdrInstance.IdCall;
            this.CdrPartialReference.commaSepIdcallsForAllInstances
                = string.Join(",", concatedNewAndOldRawinstances.Select(c => c.IdCall.ToString()));
        }

        public void ValidateAggregation()
        {
            List<long> commaSepidCalls = this.CdrPartialReference.commaSepIdcallsForAllInstances.Split(',')
                .AsEnumerable().Select(id => Convert.ToInt64(id)).ToList();
            if(commaSepidCalls.Sum()!=this.CombinedNewAndOldUnprocessedInstance.Sum(c=>c.IdCall))
                throw new Exception("Idcalls sum validation mismatch after aggregation.");
            if(this.NewCdrEquivalent.DurationSec!=this.CombinedNewAndOldUnprocessedInstance.Sum(c=>c.DurationSec))
                throw new Exception("Equivalent duration of aggregated partial cdr does not match duration total of " +
                                    "raw instances.");
            if(this.NewCdrEquivalent.StartTime.Date!=this.CdrPartialReference.CallDate)
                throw new Exception("Call date of new cdr equivalent and partial cdr reference must match " +
                                    "after aggregation.");
        }
    }
}