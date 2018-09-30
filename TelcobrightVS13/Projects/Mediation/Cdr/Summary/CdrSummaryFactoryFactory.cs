using System;
using MediationModel;
using MediationModel.enums;
using TelcobrightMediation.Cdr;

namespace TelcobrightMediation
{
    public class CdrSummaryFactoryFactory
    {
        public static CdrSummaryFactory<CdrExt> Create(CdrSummaryType targetTableName,MefServiceGroupsContainer mefServiceGroupsContainer)
        {
            if (targetTableName.ToString().Contains("_day_"))
                return new DailyCdrSummaryFactory<CdrExt>(mefServiceGroupsContainer);
            else if (targetTableName.ToString().Contains("_hr_"))
            {
                return new HourlyCdrSummaryFactory<CdrExt>(mefServiceGroupsContainer);
            }
            else throw new ArgumentOutOfRangeException(@"Summary table name must contain '_day_' or '_hr_'");
        }
    }
}