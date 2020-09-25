using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MediationModel;
using TelcobrightMediation;

namespace Default
{

    [Export("ExceptionalCdrPreProcessor", typeof(IExceptionalCdrPreProcessor))]
    public class DateChangerForCdr : IExceptionalCdrPreProcessor
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string HelpText => "Custom date changer for cdr within date time range";
        public int Id => 1;
        public object Data { get; set; }
        public void Prepare(object input)
        {
            MediationContext mediationContext = (MediationContext)input;
            this.Data = mediationContext.CdrSetting.ExceptionalCdrPreProcessingData;
        }
        public cdr Process(cdr c)
        {
            var allRulesData = (Dictionary<string, Dictionary<string, string>>) this.Data;
            Dictionary<string, string> data = allRulesData["DateChangerForCdr"];
            bool randomizeDates = Convert.ToBoolean(data["random"]) ;
            DateTime changeDateStart = Convert.ToDateTime(data["changeDateStart"]);
            DateTime changeDateEnd = Convert.ToDateTime(data["changeDateEnd"]);
            List<string> jobNames = data["jobNames"].Split(',').Select(s => s.Trim()).ToList();
            if (!jobNames.Contains(c.FileName))
            {
                return c;
            }
            double duration = Convert.ToDouble(c.DurationSec);
            DateTime newStartTime=changeDateStart;
            if (randomizeDates)
            {
                newStartTime = GetRandomDateTimeBetweenRange(changeDateStart, changeDateEnd);
            }
            DateTime newEndTime = newStartTime.AddSeconds(duration);
            c.StartTime = newStartTime;
            c.EndTime = newEndTime;
            c.SignalingStartTime = c.StartTime;
            return c;
        }

        private DateTime GetRandomDateTimeBetweenRange(DateTime startDate,DateTime endDate)
        {
            var randomTest = new Random();

            TimeSpan timeSpan = endDate - startDate;
            TimeSpan newSpan = new TimeSpan(0, randomTest.Next(0, (int)timeSpan.TotalMinutes), 0);
            DateTime newDate = startDate + newSpan;
            return newDate;
        }
        
    }
}
