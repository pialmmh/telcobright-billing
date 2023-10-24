using System;
using System.Collections.Generic;
namespace TelcobrightMediation
{
    public interface EventPreprocessingRule//each iJob corresponding to one enumjobdefinition
    {
        string RuleName { get; }
        string HelpText { get; }
        bool ProcessCollectionOnly { get; set; }
        bool IsPrepared { get; set; }
        object RuleConfigData { get; set; }
        void PrepareRule();
        void Execute(Object input);
    }
}