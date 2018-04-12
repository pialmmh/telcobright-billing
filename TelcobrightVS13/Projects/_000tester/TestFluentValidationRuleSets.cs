using System.Collections.Generic;

namespace Utils
{
    public class TestFluentValidationRuleSets:AbstractFluentValidationTester
    {
        public override void Test()
        {
            base.Test(FluentTestingType.RuleSetBased, new CdrFieldListValidatorRuleSetBased());
        }
        public TestFluentValidationRuleSets(List<cdrfieldlist> cdrfieldlists) : base(cdrfieldlists)
        {
        }
    }
}