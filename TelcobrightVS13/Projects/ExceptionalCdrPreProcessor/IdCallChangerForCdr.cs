using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MediationModel;
using TelcobrightMediation;
using LibraryExtensions;

namespace Default
{

    [Export("ExceptionalCdrPreProcessor", typeof(IExceptionalCdrPreProcessor))]
    public class IdCallChangerForCdr : IExceptionalCdrPreProcessor
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string HelpText => "Id call changer";
        public int Id => 2;
        public object Data { get; set; }
        public void Prepare(object input)
        {
            MediationContext mediationContext = (MediationContext)input;
            this.Data = mediationContext.CdrSetting.ExceptionalCdrPreProcessingData;
        }
        public cdr Process(cdr c)
        {
            var allRulesData = (Dictionary<string, Dictionary<string, string>>)this.Data;
            Dictionary<string, string> data = allRulesData["IdCallChangerForCdr"];
            List<string> jobNames = data["jobNames"].Split(',').Select(s => s.Trim()).ToList();
            if (!jobNames.Contains(c.FileName))
            {
                return c;
            }
            var randomId = RandomStringGenerator.Next(25);
            c.UniqueBillId = randomId;
            return c;
        }
    }
}
