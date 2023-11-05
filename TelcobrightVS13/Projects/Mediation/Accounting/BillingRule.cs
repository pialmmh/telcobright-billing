using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelcobrightMediation.Config;
using Itenso.TimePeriod;
using LibraryExtensions;
using LibraryExtensions.TimeCycle;
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
        public Single AvgRateForVoice { get; set; }
        public Single AcdInMinuteForVoice { get; set; }
        public Dictionary<string, string> JsonParameters { get; set; }
        public TimeCycleFactory TimeCycleFactory { get; set; }
        private ITimeCycle TimeCycle { get; set; }
        public BillingRule(int id, string ruleName)
        {
            this.RuleName = ruleName;
            this.Id = id;
        }
        public TimeRange GetBillingCycleByBillableItemsDate(DateTime billablesDate)
        {
            DateTimeOffset billDate = DateTime.SpecifyKind(billablesDate, DateTimeKind.Local);
            CronExpression cron = new CronExpression(CronExpressionForBillingCycle);
            DateTimeOffset? nextValidTriggerDate = cron.GetNextValidTimeAfter(billDate);
            if (nextValidTriggerDate == null)
                throw new Exception("No next valid occurance found from cron, possibly invalid expression.");
            DateTime nextTriggerDate = ((DateTimeOffset)nextValidTriggerDate).LocalDateTime;
            var extensionDir=new DirectoryInfo(FileAndPathHelperReadOnly.GetCurrentExecPath()).Parent.GetDirectories()
                .Single(c=>c.Name=="Extensions");
            this.TimeCycleFactory.ComposeMefRules(extensionDir.FullName);
            ITimeCycle timeCycle= this.TimeCycleFactory.GetTimeCycle();
            var data = new Dictionary<string, object>()
            {
                {"billDate",billDate},
                {"nextTriggerDate",nextTriggerDate},
                {"cycleDuration",this.TimeCycleFactory.Duration}
            };
            return timeCycle.Resolve(data);
        }

        //private TimeRange ResolveTimeCycle(DateTime nextTriggerDate)
        //{
        //    DateTime timeRangeStart = new DateTime();
        //    DateTime timeRangeEnd = new DateTime();
        //    int cycleDuration = this.TimeCycle.Duration;
        //    switch (this.TimeCycle.Interval)
        //    {
        //        case TimeCycleInterval.Minute:
        //            timeRangeStart = nextTriggerDate.AddMinutes(-1 * cycleDuration);//-1* more readable
        //            break;
        //        case TimeCycleInterval.Hour:
        //            timeRangeStart = nextTriggerDate.AddHours(-1 * cycleDuration);
        //            break;
        //        case TimeCycleInterval.Day:
        //            timeRangeStart = nextTriggerDate.AddDays(-1 * cycleDuration);
        //            timeRangeEnd = nextTriggerDate.AddSeconds(-1);
        //            break;
        //        case TimeCycleInterval.Week:
        //            timeRangeStart = nextTriggerDate.AddDays(-1 * cycleDuration * 7);
        //            timeRangeEnd = nextTriggerDate.AddSeconds(-1);
        //            break;
        //        case TimeCycleInterval.Month:
        //            timeRangeStart = nextTriggerDate.AddMonths(-1 * cycleDuration);
        //            timeRangeEnd = nextTriggerDate.AddSeconds(-1);
        //            break;
        //        case TimeCycleInterval.Year:
        //            timeRangeStart = nextTriggerDate.AddYears(-1 * cycleDuration);
        //            timeRangeEnd = nextTriggerDate.AddSeconds(-1);
        //            break;
        //    }
        //    return new TimeRange(timeRangeStart, timeRangeEnd);
        //}
    }
}
