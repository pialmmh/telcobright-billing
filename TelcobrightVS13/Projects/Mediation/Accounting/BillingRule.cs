using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelcobrightMediation.Config;
using Itenso.TimePeriod;
using Quartz;

namespace TelcobrightMediation.Accounting
{
    public class BillingRule:IGenericJsonRule
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
        public Dictionary<string, string> JsonParameters { get; set; }
        private IBillingCycle BillingCycle { get; set; }
        public BillingRule(int id, string ruleName)
        {
            this.RuleName = ruleName;
            this.Id = id;
        }
        public TimeRange GetBillingCycleByBillableItemsDate(DateTime billablesDate)
        {
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.Local;
            DateTimeOffset billDate = DateTime.SpecifyKind(billablesDate, DateTimeKind.Local);
            CronExpression quartzHelper = new CronExpression(CronExpressionForBillingCycle);
            //quartzHelper.TimeZone = timeZoneInfo;
            DateTimeOffset? nextScheduledJob = quartzHelper.GetNextValidTimeAfter(billDate);
            DateTimeOffset nextTriggerDateTime = new DateTimeOffset();
            if (nextScheduledJob != null)
            {
                int billDuration = this.BillingCycle.BillDuration;
                TimeSpan ts = (DateTimeOffset) nextScheduledJob - billDate;
                if (ts.TotalSeconds > 0) billDuration = -billDuration;
                switch (this.BillingInterval)
                {
                    case DateInterval.Minute:
                        nextTriggerDateTime = ((DateTimeOffset) nextScheduledJob).LocalDateTime.AddMinutes(billDuration);
                        break;
                    case DateInterval.Hour:
                        nextTriggerDateTime = ((DateTimeOffset) nextScheduledJob).LocalDateTime.AddHours(billDuration);
                        break;
                    case DateInterval.Day:
                        nextTriggerDateTime = ((DateTimeOffset) nextScheduledJob).LocalDateTime.AddDays(billDuration);
                        nextScheduledJob = ((DateTimeOffset) nextScheduledJob).LocalDateTime.AddDays(-1);
                        break;
                    case DateInterval.Week:
                        nextTriggerDateTime = ((DateTimeOffset) nextScheduledJob).LocalDateTime.AddDays(billDuration * 7);
                        nextScheduledJob = ((DateTimeOffset) nextScheduledJob).LocalDateTime.AddSeconds(-1);
                        break;
                    case DateInterval.Month:
                        nextTriggerDateTime = ((DateTimeOffset) nextScheduledJob).LocalDateTime.AddMonths(billDuration);
                        nextScheduledJob = ((DateTimeOffset) nextScheduledJob).LocalDateTime.AddSeconds(-1);
                        break;
                    case DateInterval.Year:
                        nextTriggerDateTime = ((DateTimeOffset) nextScheduledJob).LocalDateTime.AddYears(billDuration);
                        nextScheduledJob = ((DateTimeOffset) nextScheduledJob).LocalDateTime.AddDays(-1);
                        break;
                }
                return new TimeRange(nextTriggerDateTime.DateTime, ((DateTimeOffset) nextScheduledJob).DateTime);
            }
            else throw new Exception("Billing cycle definition expression not recognized.");
        }

        
        
    }
}
