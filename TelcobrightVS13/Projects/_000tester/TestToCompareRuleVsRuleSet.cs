using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils
{
    public class TestToCompareRuleVsRuleSet
    {
        public void TestAndCompare()
        {
            List<cdrfieldlist> cdrfieldlists;
            using (PartnerEntities context = new PartnerEntities())
            {
                cdrfieldlists = context.cdrfieldlists.ToList();
            }
            TestFluentValidationRules testFluent = new TestFluentValidationRules(cdrfieldlists);
            var start = DateTime.Now;
            testFluent.Test();
            var end = DateTime.Now;
            var ruleDifference = (end - start).TotalSeconds;
            Console.WriteLine();
            TestFluentValidationRuleSets testFluentRuleSets = new TestFluentValidationRuleSets(cdrfieldlists);
            var startRuleSet = DateTime.Now;
            testFluentRuleSets.Test();
            var endRuleSet = DateTime.Now;
            var ruleSetDifference = (endRuleSet - startRuleSet).TotalSeconds;
            Console.WriteLine();
            Console.WriteLine("Total Elapsed Time for Rules: " + ruleDifference + " seconds");
            Console.WriteLine("Total Elapsed Time for Ruleset: " + ruleSetDifference + " seconds");
            Console.Read();
        }
    }
}