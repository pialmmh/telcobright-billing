using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions.TimeCycle;
using TelcobrightMediation.Accounting;
namespace InstallConfig
{

    public static class BIllingRulesDefiner
    {
        public static List<BillingRule> BillingRules = new List<BillingRule>()
        {
            new BillingRule(id:1,ruleName: "Prepaid")
            {
                IsPrepaid = true,
                Description = "VoiceServiceBlockingBasedOnPortCost",
                AccBalanceThresholdActions = new List<AccBalanceThresholdAction>()
                {
                    new AccBalanceThresholdAction(thresholdNumber:1),
                    new AccBalanceThresholdAction(thresholdNumber:2),
                    new AccBalanceThresholdAction(thresholdNumber:3)
                }
            },
            new BillingRule(id:2,ruleName: "OnFirstDayOfEachMonth,ForPreviousMonth")
            {
                IsPrepaid = false,
                Description = "",
                InvoiceOverdueInDay = 30,
                CronExpressionForBillingCycle = "0 0 0 1 * ? *",
                TimeCycleFactory = new TimeCycleFactory()
                {
                    TimeCycleName = "MonthlyTimeCycle",
                    Duration = 1
                }
            },
            new BillingRule(id:3,ruleName: "OnFirstDayOfEachWeek,ForPreviousWeek")
            {
                IsPrepaid = false,
                Description = "",
                InvoiceOverdueInDay = 7,
                CronExpressionForBillingCycle = "0 0 0 ? * SUN *",
                TimeCycleFactory = new TimeCycleFactory()
                {
                    TimeCycleName = "WeeklyTimeCycle",
                    Duration = 7
                }
            },
            new BillingRule(id:4,ruleName: "OnFirstAndFifteenthDayOfEachMonth,ForPreviousTwoWeek")
            {
                IsPrepaid = false,
                Description = "",
                InvoiceOverdueInDay = 15,
                CronExpressionForBillingCycle = "0 0 0 1,15 * ? *",
                TimeCycleFactory = new TimeCycleFactory()
                {
                    TimeCycleName = "FortnightlyTimeCycle",
                    Duration = 15
                }
            }

        };
    }
}
