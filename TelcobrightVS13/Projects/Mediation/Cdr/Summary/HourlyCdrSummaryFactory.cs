using LibraryExtensions;
using MediationModel;
using TelcobrightMediation.Cdr;

namespace TelcobrightMediation
{
    public class HourlyCdrSummaryFactory<TSource> : CdrSummaryFactory<TSource> where TSource : CdrExt
    {
        public HourlyCdrSummaryFactory(MefServiceGroupsContainer mefServiceGroupsContainer)
            : base(mefServiceGroupsContainer) { }
        public override ISummary CreateNewInstance(TSource summarySourceObject)
        {
            AbstractCdrSummary newSummary = base.CreateInstanceWithoutDate(summarySourceObject);
            newSummary.tup_starttime = summarySourceObject.StartTime.RoundDownToHour();
            return newSummary;
        }
    }
}