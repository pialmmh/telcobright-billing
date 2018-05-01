using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TelcobrightMediation.Cdr;

namespace TelcobrightMediation
{
    public class MediationTester
    {
        private decimal DecimalComparisionTollerance { get; }

        public MediationTester(decimal decimalComparisionTollerance)
        {
            this.DecimalComparisionTollerance = decimalComparisionTollerance;
        }

        public bool DurationSumOfNonPartialRawPartialsAndRawDurationAreTollerablyEqual(CdrProcessor cdrProcessor)
        {
            var processedCdrExts = cdrProcessor.CollectionResult.ProcessedCdrExts;
            var nonPartialCdrs =processedCdrExts.Where(c => c.Cdr.PartialFlag == 0);
            var partialCdrExts = processedCdrExts.Where(c => c.Cdr.PartialFlag > 0);
            var newRawPartialCdrs = partialCdrExts.SelectMany(c => c.PartialCdrContainer.NewRawInstances);
            return Math.Abs(nonPartialCdrs.Sum(c => c.Cdr.DurationSec) + newRawPartialCdrs.Sum(c => c.DurationSec)
                    -cdrProcessor.CollectionResult.RawDurationTotalOfConsistentCdrs)<=this.DecimalComparisionTollerance;
        }
        public bool DurationSumInCdrAndSummaryAreTollerablyEqual(CdrProcessor cdrProcessor)
        {
            return Math.Abs(cdrProcessor.CollectionResult.ProcessedCdrExts.Sum(c => Convert.ToDecimal(c.Cdr?.DurationSec))
                    - cdrProcessor.CollectionResult.ProcessedCdrExts.Sum(
                        c => c.TableWiseSummaries.Values.Sum(s => Convert.ToDecimal(s?.actualduration))))
                   <= this.DecimalComparisionTollerance;
        }

        public bool DurationSumInCdrAndTableWiseSummariesAreTollerablyEqual(CdrCollectionResult collectionResult)
        {
            return Math.Abs(collectionResult.ProcessedCdrExts.Sum(c => Convert.ToDecimal(c.Cdr?.DurationSec))
                    - collectionResult.ProcessedCdrExts.SelectMany(c => c.TableWiseSummaries.Values)
                        .Sum(s => s.actualduration)) <= this.DecimalComparisionTollerance;
        }

        public bool SummaryCountTwiceAsCdrCount(CdrProcessor cdrProcessor)
        {
            var cdrCount = cdrProcessor.CollectionResult.ProcessedCdrExts.Count();
            int summaryInstanceCount = cdrProcessor.CollectionResult.ProcessedCdrExts
                .SelectMany(c => c.TableWiseSummaries.Values).Count();
            return 2 * cdrCount == summaryInstanceCount;
        }

        public bool SumOfPrevDayWiseDurationsAndNewSummaryInstancesIsEqualToSameInMergedSummaryCache
            (CdrProcessor cdrProcessor)
        {
            var summaryTargetTables = cdrProcessor.CdrJobContext.MediationContext.MefServiceGroupContainer
                .IdServiceGroupWiseServiceGroups.SelectMany(sg => sg.Value.GetSummaryTargetTables().Keys)
                .Distinct().ToList();

            foreach (string summaryTableName in summaryTargetTables)
            {
                List<DateTime> datesOrHoursInvolved = summaryTableName.Contains("day")
                    ? cdrProcessor.CdrJobContext.DatesInvolved
                    : cdrProcessor.CdrJobContext.HoursInvolved;
                string sql = $@"SELECT tup_starttime,ifnull(sum(actualduration),0) actualduration
                        FROM {summaryTableName}
                        where actualduration>0
                        and tup_starttime in 
                        ({string.Join(",", datesOrHoursInvolved.Select(c => c.ToMySqlStyleDateTimeStrWithQuote()))})
                        group by tup_starttime";
                var cmd = cdrProcessor.CdrJobContext.DbCmd;
                cmd.CommandText = sql;
                var reader = cmd.ExecuteReader();
                Dictionary<DateTime, decimal> dayWisePrevDurations = new Dictionary<DateTime, decimal>();
                //for some reason sqlquery<dateWithDouble> could not retrieve value for double, so used reader
                //cdrProcessor.CdrJobContext.CdrjobInputData.Context.Database.SqlQuery<DateWithDouble>
                //    (sql).ToDictionary(dateWithDouble => dateWithDouble.Date, dateWithDouble => dateWithDouble.Value);
                while (reader.Read())
                {
                    dayWisePrevDurations.Add(reader.GetDateTime(0), reader.GetDecimal(1));
                }
                reader.Close();

                List<AbstractCdrSummary> summariesForThisTable = cdrProcessor.CollectionResult.ProcessedCdrExts
                    .SelectMany(
                        c => c.TableWiseSummaries.Where(kv => kv.Key == summaryTableName).Select(kv => kv.Value))
                    .ToList();
                Dictionary<DateTime, decimal> newDayOrHourWiseCdrExtSummaries = summariesForThisTable
                    .GroupBy(s => s.tup_starttime).ToDictionary(g => g.Key, g => g.Sum(s => s.actualduration));
                foreach (KeyValuePair<DateTime, decimal> kv in newDayOrHourWiseCdrExtSummaries)
                {
                    decimal newDurationForThisDayOrHourInNewCdrExtSummaries = kv.Value;
                    decimal prevDurationForThisDayOrHourFromSummaryTable = 0;
                    dayWisePrevDurations.TryGetValue(kv.Key, out prevDurationForThisDayOrHourFromSummaryTable);
                    var mergedDurationInSummaryContext = cdrProcessor.CdrJobContext.CdrSummaryContext
                        .TableWiseSummaryCache
                        .Where(keyValue => keyValue.Key == summaryTableName).Select(keyValue => keyValue.Value)
                        .SelectMany(summaryCache => summaryCache.GetItems().Where(s => s.tup_starttime == kv.Key))
                        .Sum(summary => summary.actualduration);
                    decimal totalSupposedToBeDuration =
                        newDurationForThisDayOrHourInNewCdrExtSummaries + prevDurationForThisDayOrHourFromSummaryTable;
                    if (Math.Abs(totalSupposedToBeDuration-mergedDurationInSummaryContext)>this.DecimalComparisionTollerance)
                        return false;
                }
            }
            return true;
        }
    }
}
