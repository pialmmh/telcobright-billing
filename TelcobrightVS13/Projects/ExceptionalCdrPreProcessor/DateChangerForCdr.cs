using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation;

namespace CdrRules
{

    [Export("ExceptionalCdrPreProcessor", typeof(IExceptionalCdrPreProcessor))]
    public class DateChangerForCdr : IExceptionalCdrPreProcessor
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string HelpText => "Custom date changer for cdr within date time range";
        public int Id => 1;
        public object Data { get; set; }
        public string[] Process(string[] cdr)
        {
            Dictionary<string, string> data = (Dictionary<string, string>) this.Data;
            bool randomizeDates = Convert.ToBoolean(data["random"]) ;
            DateTime changeDateStart = Convert.ToDateTime(data["changeDateStart"]);
            DateTime changeDateEnd = Convert.ToDateTime(data["changeDateEnd"]);
            return cdr;
        }
        
    }
}
