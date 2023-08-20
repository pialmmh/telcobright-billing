using System.Collections.Generic;
using MediationModel;

namespace LibraryExtensions
{
    public class MefNerRulesContainer
    {
        public MefNerCalculationRuleComposer NerComposer = new MefNerCalculationRuleComposer();
        public IDictionary<string, INerCalculationRule> DicExtensions = new Dictionary<string, INerCalculationRule>();
    }
}




