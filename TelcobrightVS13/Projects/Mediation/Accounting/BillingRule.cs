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
    public class BillingRule:IGenericJsonRule,IBillingRule,ITimeRangeImplementer
    {
        public int Id { get; }
        public string RuleName { get; }
        public string Description { get; set; }
        public bool IsPrepaid { get; set; }
        public TimeRange GetBillingCycleByBillableItemsDate(DateTime billablesDate)
        {
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.Local;
            DateTimeOffset billDate = DateTime.SpecifyKind(billablesDate, DateTimeKind.Local);
            CronExpression quartzHelper = new CronExpression(CronExpressionForBillingCycle);
            //quartzHelper.TimeZone = timeZoneInfo;
            DateTimeOffset? nextScheduledJob = quartzHelper.GetNextValidTimeAfter(billDate);
            DateTimeOffset prevScheduledJob = new DateTimeOffset();
            if (nextScheduledJob != null)
            {
                int billDuration = this.BillDuration;
                TimeSpan ts = (DateTimeOffset) nextScheduledJob - billDate;
                if (ts.TotalSeconds > 0) billDuration = -billDuration;
                switch (this.BillingInterval)
                {
                    case DateInterval.Minutes:
                        prevScheduledJob = ((DateTimeOffset) nextScheduledJob).LocalDateTime.AddMinutes(billDuration);
                        break;
                    case DateInterval.Hours:
                        prevScheduledJob = ((DateTimeOffset) nextScheduledJob).LocalDateTime.AddHours(billDuration);
                        break;
                    case DateInterval.Days:
                        prevScheduledJob = ((DateTimeOffset) nextScheduledJob).LocalDateTime.AddDays(billDuration);
                        nextScheduledJob = ((DateTimeOffset) nextScheduledJob).LocalDateTime.AddDays(-1);
                        break;
                    case DateInterval.Weeks:
                        prevScheduledJob = ((DateTimeOffset) nextScheduledJob).LocalDateTime.AddDays(billDuration * 7);
                        nextScheduledJob = ((DateTimeOffset) nextScheduledJob).LocalDateTime.AddDays(-1);
                        break;
                    case DateInterval.Months:
                        prevScheduledJob = ((DateTimeOffset) nextScheduledJob).LocalDateTime.AddMonths(billDuration);
                        nextScheduledJob = ((DateTimeOffset) nextScheduledJob).LocalDateTime.AddSeconds(-1);
                        break;
                    case DateInterval.Years:
                        prevScheduledJob = ((DateTimeOffset) nextScheduledJob).LocalDateTime.AddYears(billDuration);
                        nextScheduledJob = ((DateTimeOffset) nextScheduledJob).LocalDateTime.AddDays(-1);
                        break;
                }
                return new TimeRange(prevScheduledJob.DateTime, ((DateTimeOffset) nextScheduledJob).DateTime);
            }
            else return null;
        }

        public List<AccBalanceThresholdAction> AccBalanceThresholdActions { get; set; }
        public int InvoiceOverdueInDay { get; set; }
        public string CronExpressionForBillingCycle { get; set; }
        public TimeRange TimeRangeAsBillingPeriod { get; set; }
        public ITimeRangePopulator TimeRangePopulator { get; set; }
        public Single AvgRateForVoice { get; set; }
        public Single AcdInMinuteForVoice { get; set; }
        public Dictionary<string,string> JsonParameters { get; set; }
        public DateInterval? BillingInterval { get; set; }
        public int BillDuration { get; set; }

        public BillingRule(int id,string ruleName)
        {
            this.RuleName = ruleName;
            this.Id = id;
        }
        
    }
}
