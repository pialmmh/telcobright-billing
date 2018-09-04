using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Itenso.TimePeriod;
using LibraryExtensions;
using MediationModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TelcobrightMediation.Cdr;
using CdrSummaryTuple = System.ValueTuple<int, int, int, string, string, decimal, decimal, System.ValueTuple<string, string, string, string, string, string, string, System.ValueTuple<string, string, string, string, string, string>>>;
namespace TelcobrightMediation
{
    public class MediationTester
    {
        private decimal FractionComparisionTollerance { get; }
        public MediationTester(decimal fractionComparisionTollerance)
        {
            this.FractionComparisionTollerance = fractionComparisionTollerance;
        }

        public bool DurationSumOfNonPartialRawPartialsAndRawDurationAreEqual(CdrProcessor cdrProcessor)
        {
            var processedCdrExts = cdrProcessor.CollectionResult.ProcessedCdrExts;
            var nonPartialCdrs =processedCdrExts.Where(c => c.Cdr.PartialFlag == 0);
            var partialCdrExts = processedCdrExts.Where(c => c.Cdr.PartialFlag > 0);
            var newRawPartialCdrs = partialCdrExts.SelectMany(c => c.PartialCdrContainer.NewRawInstances);
            var nonPartialDurationSum = nonPartialCdrs.Sum(c => c.Cdr.DurationSec);
            var rawPartialDurationSum = newRawPartialCdrs.Sum(c => c.DurationSec);
            var errorDuration = cdrProcessor.CollectionResult.CdrExtErrors.Sum(c => Convert.ToDecimal(c.CdrError.DurationSec));
            bool result = nonPartialDurationSum + rawPartialDurationSum + errorDuration
                          == cdrProcessor.CollectionResult.RawDurationTotalOfConsistentCdrs;
            return result;
        }
        public bool DurationSumInCdrAndSummaryAreEqual(ParallelQuery<CdrExt> processedCdrExts)
        {
            decimal durationSumInCdr = processedCdrExts.Sum(c => Convert.ToDecimal(c.Cdr?.DurationSec));
            decimal durationSumInSummaries = processedCdrExts.
                Sum(c => c.TableWiseSummaries.Values.Sum(s => Convert.ToDecimal(s?.actualduration)));
            int summaryTypeCount = 2;//at this moment just hr & day
            bool result = durationSumInCdr == decimal.Divide(durationSumInSummaries, summaryTypeCount);
            return result;
        }
        public bool CdrDurationMatchesSumOfInsertedAndUpdatedSummaryDurationInCache(CdrProcessor cdrProcessor)
        {
            var processedCdrExts = cdrProcessor.CollectionResult.ProcessedCdrExts.AsParallel();
            decimal durationSumInCdr = processedCdrExts.Sum(c => Convert.ToDecimal(c.Cdr?.DurationSec));
            var daySummaryCaches = cdrProcessor.CdrJobContext.CdrSummaryContext.TableWiseSummaryCache
                .Where(kv => kv.Key.ToString().Contains("day")).Select(kv => kv.Value).ToList();
            var inserteDuration = daySummaryCaches.SelectMany(c => c.GetInsertedItems()).Sum(s => s.actualduration);
            var updatedDuration = daySummaryCaches.SelectMany(c => c.GetUpdatedItems()).Sum(s => s.actualduration);

            bool result = durationSumInCdr == inserteDuration + updatedDuration;
            return result;
        }
        public bool SummaryCountTwiceAsCdrCount(ParallelQuery<CdrExt> processedCdrExts)
        {
            var cdrCount = processedCdrExts.Count();
            int summaryInstanceCount = processedCdrExts.SelectMany(c => c.TableWiseSummaries.Values).Count();
            return 2 * cdrCount == summaryInstanceCount;
        }

        public bool SumOfPrevDayWiseDurationsAndNewSummaryInstancesIsEqualToSameInMergedSummaryCache
            (CdrProcessor cdrProcessor)
        {
            var summaryTargetTables = cdrProcessor.CdrJobContext.MediationContext.MefServiceGroupContainer
                .IdServiceGroupWiseServiceGroups.SelectMany(sg => sg.Value.GetSummaryTargetTables().Keys)
                .Distinct().ToList();

            foreach (CdrSummaryType summaryTableName in summaryTargetTables)
            {
                List<DateTime> datesOrHoursInvolved = summaryTableName.ToString().Contains("day")
                    ? cdrProcessor.CdrJobContext.DatesInvolved
                    : cdrProcessor.CdrJobContext.HoursInvolved;
                string sql = $@"SELECT tup_starttime,ifnull(sum(actualduration),0) actualduration
                        FROM {summaryTableName}
                        where actualduration>0
                        and tup_starttime in 
                        ({string.Join(",", datesOrHoursInvolved.Select(c => c.ToMySqlFormatWithQuote()))})
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
                    var updatedDurationInSummaryContext = cdrProcessor.CdrJobContext.CdrSummaryContext
                        .TableWiseSummaryCache
                        .Where(keyValue => keyValue.Key == summaryTableName).Select(keyValue => keyValue.Value)
                        .SelectMany(summaryCache => summaryCache.GetItems().Where(s => s.tup_starttime == kv.Key))
                        .Sum(summary => summary.actualduration);
                    decimal totalSupposedToBeDuration =
                        newDurationForThisDayOrHourInNewCdrExtSummaries + prevDurationForThisDayOrHourFromSummaryTable;
                    if (Math.Abs(totalSupposedToBeDuration-updatedDurationInSummaryContext)>this.FractionComparisionTollerance)
                        return false;
                }
            }
            return true;
        }
    }
}
