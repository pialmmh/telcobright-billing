using System;
using TelcobrightMediation.Cdr;

namespace TelcobrightMediation
{
    public class CdrSummaryFactoryFactory
    {
        public static CdrSummaryFactory<CdrExt> Create(string targetTableName,MefServiceGroupsContainer mefServiceGroupsContainer)
        {
            if (targetTableName.Contains("_day_"))
                return new DailyCdrSummaryFactory<CdrExt>(mefServiceGroupsContainer);
            else if (targetTableName.Contains("_hr_"))
            {
                return new HourlyCdrSummaryFactory<CdrExt>(mefServiceGroupsContainer);
            }
            else throw new ArgumentOutOfRangeException(@"Summary table name must contain '_day_' or '_hr_'");
        }
    }
}