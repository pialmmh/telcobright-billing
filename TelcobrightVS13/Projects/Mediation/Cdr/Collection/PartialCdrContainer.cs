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
        public string UniqueBillId => this.NewMediatableCdrInstance.UniqueBillId;
        public DateTime StartTime => this.NewMediatableCdrInstance.StartTime;//handled by aggregator already in case of multiple instances have diff date somehow
        public List<cdrpartialrawinstance> NewRawInstances { get; }
        public List<cdrpartialrawinstance> PrevRawInstances { get; }
        public cdrpartialreference CdrPartialReference { get; private set; }
        public cdrpartiallastaggregatedrawinstance PrevAggregatedRawInstance { get; }
        public cdrpartiallastaggregatedrawinstance NewAggregatedRawInstance { get; private set; }
        public cdr PrevProcessedCdrInstance { get; }
        public cdr NewMediatableCdrInstance { get; private set; }
        public List<cdrpartialrawinstance> ConcatedNewAndOldUnprocessedInstance { get; private set; }
        public PartialCdrContainer(List<cdrpartialrawinstance> newRawInstances,
                                    List<cdrpartialrawinstance> prevRawInstances,
                                    cdrpartialreference cdrPartialreference,
                                    cdrpartiallastaggregatedrawinstance prevAggregatedRawInstance,
                                    cdr prevProcessedCdrInstance)
        {
            this.NewRawInstances = newRawInstances;
            this.PrevRawInstances = prevRawInstances;
            this.CdrPartialReference = cdrPartialreference;
            this.PrevAggregatedRawInstance = prevAggregatedRawInstance;
            this.PrevProcessedCdrInstance = prevProcessedCdrInstance;
        }

        public void Aggregate()
        {
            this.NewMediatableCdrInstance =
                new IcdrImplConverter<cdr>().Convert(
                    CdrManipulatingUtil.Clone(this.NewRawInstances.Last()));
            this.NewMediatableCdrInstance.DurationSec =
                this.ConcatedNewAndOldUnprocessedInstance.Sum(c => c.DurationSec);
            if (this.PrevAggregatedRawInstance != null) //if there is a prev instance
                this.NewMediatableCdrInstance.StartTime = this.PrevAggregatedRawInstance.StartTime;
            this.NewMediatableCdrInstance.AnswerTime = this.ConcatedNewAndOldUnprocessedInstance.Min(c => c.AnswerTime);
            this.NewMediatableCdrInstance.EndTime = this.ConcatedNewAndOldUnprocessedInstance.Max(c => c.EndTime);
            DateTime? minConnectTime = this.ConcatedNewAndOldUnprocessedInstance.Where(c => c.ConnectTime != null)
                .Select(c => c.ConnectTime).FirstOrDefault();
            if (minConnectTime != null) this.NewMediatableCdrInstance.ConnectTime = minConnectTime;
            this.NewMediatableCdrInstance.PartialFlag = 1;
            this.NewMediatableCdrInstance.FinalRecord = 1;
            this.NewAggregatedRawInstance =
                new IcdrImplConverter<cdrpartiallastaggregatedrawinstance>().Convert(this.NewMediatableCdrInstance);
            this.ConcatedNewAndOldUnprocessedInstance = this.PrevRawInstances.Concat(this.NewRawInstances).ToList();
            UpdatePartialCdrReferences(this.NewMediatableCdrInstance, this.ConcatedNewAndOldUnprocessedInstance);
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