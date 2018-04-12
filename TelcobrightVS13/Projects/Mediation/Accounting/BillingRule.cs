using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelcobrightMediation.Config;
using Itenso.TimePeriod;
namespace TelcobrightMediation.Accounting
{
    public class BillingRule:IGenericJsonRule,IBillingRule,ITimeRangeImplementer
    {
        public int Id { get; }
        public string RuleName { get; }
        public string Description { get; set; }
        public bool IsPrepaid { get; set; }
        public List<AccBalanceThresholdAction> AccBalanceThresholdActions { get; set; }
        public int InvoiceOverdueInDay { get; set; }
        public string CronExpressionForBillingCycle { get; set; }
        public TimeRange TimeRangeAsBillingPeriod { get; set; }
        public ITimeRangePopulator TimeRangePopulator { get; set; }
        public Single AvgRateForVoice { get; set; }
        public Single AcdInMinuteForVoice { get; set; }
        public Dictionary<string,string> JsonParameters { get; set; }
        public BillingRule(int id,string ruleName)
        {
            this.RuleName = ruleName;
            this.Id = id;
        }
        
    }
}
