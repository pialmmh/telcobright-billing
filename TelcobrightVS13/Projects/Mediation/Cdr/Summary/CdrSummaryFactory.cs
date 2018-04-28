using System;
using System.Collections.Generic;
using LibraryExtensions;
using MediationModel;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Mediation.Cdr;

namespace TelcobrightMediation
{
    public abstract class CdrSummaryFactory<TSource> : AbstractSummaryFactory<TSource> where TSource : CdrExt
    {
        private MefServiceGroupsContainer MefServiceGroupsContainer { get; }

        public CdrSummaryFactory(MefServiceGroupsContainer mefServiceGroupsContainer)
        {
            this.MefServiceGroupsContainer = mefServiceGroupsContainer;
        }

        protected AbstractCdrSummary CreateInstanceWithoutDate(TSource summarySourceObject)
        {
            CdrExt cdrExt = summarySourceObject;
            sum_voice_day_01 newSummary = new sum_voice_day_01();
            newSummary.tup_switchid = Convert.ToInt32(cdrExt.Cdr.SwitchId);
            newSummary.tup_inpartnerid = Convert.ToInt32(cdrExt.Cdr.inPartnerId);
            newSummary.tup_outpartnerid = Convert.ToInt32(cdrExt.Cdr.outPartnerId);
            newSummary.tup_incomingroute = cdrExt.Cdr.incomingroute.ReplaceNullWith("");
            newSummary.tup_outgoingroute = cdrExt.Cdr.outgoingroute.ReplaceNullWith("");
            newSummary.tup_customerrate = 0;
            newSummary.tup_supplierrate = 0;
            newSummary.tup_incomingip = cdrExt.Cdr.OriginatingIP.ReplaceNullWith("");
            newSummary.tup_outgoingip = cdrExt.Cdr.TerminatingIP.ReplaceNullWith("");

            newSummary.totalcalls = 1;
            if (cdrExt.Cdr.ConnectTime != null)
            {
                newSummary.connectedcalls = 1; //connected flag by connect time
            }
            else newSummary.connectedcalls = 0;
            if (cdrExt.Cdr.field5 == 1)
            {
                newSummary.connectedcallsCC = 1; //connected flag
            }
            else newSummary.connectedcallsCC = 0;

            newSummary.successfulcalls = Convert.ToInt64(cdrExt.Cdr.ChargingStatus);
            newSummary.actualduration = cdrExt.Cdr.DurationSec;
            newSummary.roundedduration = Convert.ToDecimal(cdrExt.Cdr.roundedduration);
            newSummary.duration1 = Convert.ToDecimal(cdrExt.Cdr.Duration1);
            newSummary.duration2 = Convert.ToDecimal(cdrExt.Cdr.Duration2);
            newSummary.duration3 = Convert.ToDecimal(cdrExt.Cdr.Duration3);
            newSummary.PDD = Convert.ToDecimal(cdrExt.Cdr.PDD);

            //service group specific params, set default first then send through service group to set right value
            IServiceGroup serviceGroup =
                this.MefServiceGroupsContainer.IdServiceGroupWiseServiceGroups[cdrExt.Cdr.ServiceGroup];
            serviceGroup.SetServiceGroupWiseSummaryParams(cdrExt, newSummary);
            ReplaceNullsWithDefaultForTupleFields(newSummary);
            return newSummary;
        }

        void ReplaceNullsWithDefaultForTupleFields(AbstractCdrSummary newSummary)
        {
            newSummary.tup_countryorareacode = newSummary.tup_countryorareacode ?? "";
            newSummary.tup_matchedprefixcustomer = newSummary.tup_matchedprefixcustomer ?? "";
            newSummary.tup_matchedprefixsupplier = newSummary.tup_matchedprefixsupplier ?? "";
            newSummary.tup_sourceId = newSummary.tup_sourceId ?? "";
            newSummary.tup_destinationId = newSummary.tup_destinationId ?? "";
            newSummary.tup_customercurrency = newSummary.tup_customercurrency ?? "";
            newSummary.tup_suppliercurrency = newSummary.tup_suppliercurrency ?? "";
            newSummary.tup_tax1currency = newSummary.tup_tax1currency ?? "";
            newSummary.tup_tax2currency = newSummary.tup_tax2currency ?? "";
            newSummary.tup_vatcurrency = newSummary.tup_vatcurrency ?? "";
        }
    }
}
