using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LibraryExtensions;
using MediationModel;
using TelcobrightMediation.Accounting;
using TelcobrightMediation.EntityHelpers;
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
        public cdr NewCdrEquivalent { get; private set; }
        public List<cdrpartialrawinstance> CombinedNewAndOldUnprocessedInstance { get; private set; }
        public PartialCdrContainer(List<cdrpartialrawinstance> newRawInstances,
                                    List<cdrpartialrawinstance> prevRawInstances,
                                    cdrpartialreference cdrPartialreference,
                                    cdrpartiallastaggregatedrawinstance lastAggregatedRawInstance,
                                    cdr prevProcessedCdrInstance)
        {
            this.NewRawInstances = newRawInstances;
            this.PrevRawInstances = prevRawInstances;
            this.CdrPartialReference = cdrPartialreference;
            this.LastProcessedAggregatedRawInstance = lastAggregatedRawInstance;
            this.PrevProcessedCdrInstance = prevProcessedCdrInstance;
        }

        public void Aggregate()
        {
            this.CombinedNewAndOldUnprocessedInstance = this.PrevRawInstances.Concat(this.NewRawInstances).ToList();
            this.NewCdrEquivalent =
                new IcdrImplConverter<cdr>().Convert(CdrManipulatingUtil.Clone(this.NewRawInstances.Last()));
            this.NewCdrEquivalent.DurationSec =
                this.CombinedNewAndOldUnprocessedInstance.Sum(c => c.DurationSec);
            if (this.LastProcessedAggregatedRawInstance != null) //if there is a prev instance
                this.NewCdrEquivalent.StartTime = this.LastProcessedAggregatedRawInstance.StartTime;
            this.NewCdrEquivalent.AnswerTime = this.CombinedNewAndOldUnprocessedInstance.Min(c => c.AnswerTime);
            this.NewCdrEquivalent.EndTime = this.CombinedNewAndOldUnprocessedInstance.Max(c => c.EndTime);
            DateTime? minConnectTime = this.CombinedNewAndOldUnprocessedInstance.Where(c => c.ConnectTime != null)
                .Select(c => c.ConnectTime).FirstOrDefault();
            if (minConnectTime != null) this.NewCdrEquivalent.ConnectTime = minConnectTime;
            this.NewCdrEquivalent.PartialFlag = 1;
            this.NewCdrEquivalent.FinalRecord = 1;
            this.NewAggregatedRawInstance =
                new IcdrImplConverter<cdrpartiallastaggregatedrawinstance>().Convert(this.NewCdrEquivalent);
            
            UpdatePartialCdrReferences(this.NewCdrEquivalent, this.CombinedNewAndOldUnprocessedInstance);
        }

        private void UpdatePartialCdrReferences(cdr newMediatableCdrInstance,
            List<cdrpartialrawinstance> concatedNewAndOldRawinstances)
        {
            this.CdrPartialReference = this.CdrPartialReference ??
                                       new cdrpartialreference()
                                       {
                                           UniqueBillId = newMediatableCdrInstance.UniqueBillId,
                                           switchid = newMediatableCdrInstance.SwitchId,
                                           CallDate = newMediatableCdrInstance.StartTime,
                                       };
            this.CdrPartialReference.lastFilename = newMediatableCdrInstance.FileName;
            this.CdrPartialReference.lastIdcall = newMediatableCdrInstance.idcall;
            this.CdrPartialReference.commaSepIdcallsForAllInstances
                = string.Join(",", concatedNewAndOldRawinstances.Select(c => c.idcall.ToString()));
        }
    }
}