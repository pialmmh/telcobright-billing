using System;
using System.Collections.Generic;
using LibraryExtensions;
using MediationModel;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Mediation.Cdr;

namespace TelcobrightMediation
{
    public class DailyCdrSummaryFactory<TSource> : CdrSummaryFactory<TSource> where TSource : CdrExt
    {
        public DailyCdrSummaryFactory(MefServiceGroupsContainer mefServiceGroupsContainer)
            :base(mefServiceGroupsContainer){}
        public override ISummary CreateNewInstance(TSource summarySourceObject)
        {
            AbstractCdrSummary newSummary = base.CreateInstanceWithoutDate(summarySourceObject);
            newSummary.tup_starttime= summarySourceObject.StartTime.RoundDownToDay();
            return newSummary;
        }
    }
}
