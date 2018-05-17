using System;
using System.Collections.Generic;
using MediationModel;
using TelcobrightMediation.Cdr;

namespace TelcobrightMediation
{
    public interface IServiceGroup
    {
        string RuleName { get; }
        string HelpText { get; }
        int Id { get; }
        void SetAdditionalParams(Dictionary<string, object> additionalParams);
        void Execute(cdr thisCdr, CdrProcessor cdrProcessor);
        void ExecutePostRatingActions(CdrExt cdrExt, object postRatingData);
        Dictionary<string, Type> GetSummaryTargetTables();
        void SetServiceGroupWiseSummaryParams(CdrExt cdrExt, AbstractCdrSummary newSummary);
    }
}