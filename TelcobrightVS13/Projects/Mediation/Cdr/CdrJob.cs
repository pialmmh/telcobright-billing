using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TelcobrightMediation.Cdr
{
    public class CdrJob : ISegmentedJob
    {
        public CdrProcessor CdrProcessor { get; }
        public CdrEraser CdrEraser { get; }
        public int ActualStepsCount { get; }
        public CdrJobContext CdrJobContext { get; }
        private MediationContext MediationContext => this.CdrJobContext.CdrjobInputData.MediationContext;
        private CdrSetting CdrSetting => this.MediationContext.Tbc.CdrSetting;
        private PartialCdrTesterData PartialCdrTesterData { get; }
        private cdrmeta CdrMetaBeforeCdrjob { get; }

        public CdrJob(CdrProcessor cdrProcessor, CdrEraser cdrEraser, int actualStepsCount,
            PartialCdrTesterData partialCdrTesterData)
        {
            this.CdrJobContext = cdrProcessor?.CdrJobContext ?? cdrEraser.CdrJobContext;
            this.CdrProcessor = cdrProcessor;
            this.CdrEraser = cdrEraser;
            this.ActualStepsCount = actualStepsCount;
            this.PartialCdrTesterData = partialCdrTesterData;
            this.CdrMetaBeforeCdrjob = ReadCdrMetaFromDb();
            //todo: remove temp code
            //this.CdrJobContext.DbCmd.ExecuteCommandText("delete from cdrmismatch;");
            //this.CdrJobContext.DbCmd.ExecuteCommandText($@"delete from cdrtempduration");
            //
        }

        public void Execute()
        {

            this.CdrEraser?.RegenerateOldSummaries();

            //todo: remove temp code
            //var oldCdrs = CdrEraser.CollectionResult.ProcessedCdrExts;
            //var oldCdrDuration = oldCdrs.Sum(c => c.Cdr.DurationSec);
            //var daySummaryCaches = this.CdrJobContext.CdrSummaryContext.TableWiseSummaryCache
            //    .Where(kv => kv.Key.Contains("day")).Select(kv => kv.Value).ToList();
            //var summDurationInCacheStart = daySummaryCaches.SelectMany(s => s.GetItems()).Sum(c => c.actualduration);
            //temp

            this.CdrEraser?.ValidateSummaryReGeneration();
            if (this.CdrEraser != null)
            {
                ValidateCdrEraserWithMediationTester(this.CdrJobContext.CdrjobInputData,
                    this.CdrEraser.CollectionResult.ConcurrentCdrExts.Values.AsParallel());
            }
            this.CdrEraser?.UndoOldSummaries();

            //todo: remove or find a way to kep this test
            //var summDurationInCache = daySummaryCaches.SelectMany(s => s.GetItems()).Sum(c => c.actualduration);
            //var updateItems = daySummaryCaches.SelectMany(s => s.GetUpdatedItems()).ToList();
            //var updatedDurationFromUpdatedItems=updateItems.Sum(c => c.actualduration);
            //var updateCacheDurationAftSubstract = summDurationInCache - summDurationInCacheStart;
            //if (daySummaryCaches.SelectMany(c=>c.GetInsertedItems()).Any())
            //    throw new Exception("Summary cache cannot contain inserted items after prev summary substraction.");
            //end

            this.CdrEraser?.UndoOldChargeables();
            this.CdrEraser?.DeleteOldCdrs();

            //todo: remove temp code
            var durationTobeProcessed =
                this.CdrProcessor.CollectionResult.ConcurrentCdrExts.Values.Sum(c => c.Cdr.DurationSec);
            //end temp code

            this.CdrProcessor?.Mediate();
            
            ParallelQuery<CdrExt> parallelCdrExts= this.CdrProcessor?.GenerateSummaries();

            //todo: remove temp code
            //updateItems = daySummaryCaches.SelectMany(s => s.GetUpdatedItems()).ToList();
            //end

            this.CdrProcessor?.MergeNewSummariesIntoCache(parallelCdrExts);


            //todo: remove temp code
            //updateItems = daySummaryCaches.SelectMany(s => s.GetUpdatedItems()).ToList();
            //end

            //todo: remove temp code
            //var newCdrDuration = CdrProcessor.CollectionResult.ProcessedCdrExts.Sum(c => c.Cdr.DurationSec);
            //var newSummaryDuration = CdrProcessor.CollectionResult.ProcessedCdrExts
            //    .SelectMany(c => c.TableWiseSummaries)
            //    .Where(c => c.Key.Contains("day")).Select(kv => kv.Value).Sum(s => s.actualduration);
            //summDurationInCache = daySummaryCaches.SelectMany(s => s.GetItems()).Sum(c => c.actualduration);
            //var durationInInsertCache = daySummaryCaches.SelectMany(s => s.GetInsertedItems()).Sum(c => c.actualduration);
            //var durationInUpdateCache = newCdrDuration - durationInInsertCache + updateCacheDurationAftSubstract;
            //end

            this.CdrProcessor?.ProcessChargeables(parallelCdrExts);

            IncrementalTransactionCreator transactionProcessor = new IncrementalTransactionCreator(this);
            List<acc_transaction> incrementalTransactions= transactionProcessor.CreateIncrementalTransactions();
            transactionProcessor.ValidateTransactions();
            this.CdrJobContext.AccountingContext.ExecuteTransactions(incrementalTransactions);

            if (this.CdrProcessor!=null)
            {
                ValidateCdrProcessorWithMediationTester(this.CdrJobContext.CdrjobInputData,
                    parallelCdrExts);
            }
            var cdrWritingResult = this.CdrProcessor?.WriteCdrs(parallelCdrExts);

            if (this.CdrProcessor != null && this.CdrProcessor.PartialProcessingEnabled)
            {
                PartialCdrTester partialCdrTester =
                    new PartialCdrTester(this, cdrWritingResult, this.PartialCdrTesterData);
                partialCdrTester.ValidatePartialCdrMediation();
            }

            foreach (var summaryCache in this.CdrJobContext.CdrSummaryContext.TableWiseSummaryCache.Values)
            {
                summaryCache.WriteAllChanges(this.CdrJobContext.DbCmd,
                    this.CdrJobContext.SegmentSizeForDbWrite);
            }
            this.CdrJobContext.AccountingContext.WriteAllChanges();
            this.CdrJobContext.AutoIncrementManager.WriteAllChanges();
            UpdateCdrMetaData();
        }

        protected void ValidateCdrProcessorWithMediationTester(CdrJobInputData input,ParallelQuery<CdrExt> processedCdrExts)
        {
            MediationTester mediationTester =
                new MediationTester(input.Tbc.CdrSetting.FractionalNumberComparisonTollerance);
            if(!mediationTester.DurationSumInCdrAndSummaryAreTollerablyEqual(processedCdrExts))
                throw new Exception("Duration sum in cdr and summary are not tollerably equal");
            if(!mediationTester.SummaryCountTwiceAsCdrCount(processedCdrExts))
                throw new Exception("Summary count is not twice as cdr count");
            //if (!mediationTester.CdrDurationMatchesSumOfInsertedAndUpdatedSummaryDurationInCache(this.CdrProcessor)) ;
            //throw new Exception("Cdr duration does not match inserted & updated summary instances duration in cache. ");
            //if(!mediationTester
            //    .SumOfPrevDayWiseDurationsAndNewSummaryInstancesIsEqualToSameInMergedSummaryCache(
            //        this.CdrProcessor))
            //    throw new Exception("Sum of prev day wise durations and new summary instances is not equal to same " +
            //                        "in merged summary cache");
            if (!mediationTester.DurationSumOfNonPartialRawPartialsAndRawDurationAreTollerablyEqual(this.CdrProcessor))
                throw new Exception("Duration sum Of non partial and raw Partial cdrs " +
                                    "are not equal to duration in raw instances.");
        }
        protected void ValidateCdrEraserWithMediationTester(CdrJobInputData input, ParallelQuery<CdrExt> processedCdrExts)
        {
            MediationTester mediationTester =
                new MediationTester(input.Tbc.CdrSetting.FractionalNumberComparisonTollerance);
            if (!mediationTester.DurationSumInCdrAndSummaryAreTollerablyEqual(processedCdrExts))
                throw new Exception("Duration sum in cdr and summary are not tollerably equal");
            if (!mediationTester.SummaryCountTwiceAsCdrCount(processedCdrExts))
                throw new Exception("Summary count is not twice as cdr count");
            //if(!mediationTester
            //    .SumOfPrevDayWiseDurationsAndNewSummaryInstancesIsEqualToSameInMergedSummaryCache(
            //        this.CdrProcessor))
            //    throw new Exception("Sum of prev day wise durations and new summary instances is not equal to same " +
            //                        "in merged summary cache");
            //if (!mediationTester.DurationSumOfNonPartialRawPartialsAndRawDurationAreTollerablyEqual(this.CdrProcessor))
            //    throw new Exception("Duration sum Of non partial and raw Partial cdrs " +
            //                        "are not equal to duration in raw instances.");
        }
        protected void AssertWithMediationTester(CdrJobInputData input, ParallelQuery<CdrExt> processedCdrExts)
        {
            MediationTester mediationTester =
                new MediationTester(input.Tbc.CdrSetting.FractionalNumberComparisonTollerance);
            Assert.IsTrue(mediationTester.DurationSumInCdrAndSummaryAreTollerablyEqual(processedCdrExts));
            Assert.IsTrue(mediationTester.SummaryCountTwiceAsCdrCount(processedCdrExts));
            //todo: do something about this test which makes database trip & need to modify this to work with both eraser & processor
            //Assert.IsTrue(mediationTester
              //  .SumOfPrevDayWiseDurationsAndNewSummaryInstancesIsEqualToSameInMergedSummaryCache(
            //        this.CdrProcessor));
            Assert.IsTrue(
                mediationTester.DurationSumOfNonPartialRawPartialsAndRawDurationAreTollerablyEqual(this.CdrProcessor));
        }
        private void UpdateCdrMetaData()
        {
            string jobname = this.CdrJobContext.TelcobrightJob.JobName;

            var processedCdrExts = this.CdrProcessor.CollectionResult.ProcessedCdrExts;
            decimal newProcessedDuration = processedCdrExts.Sum(c => c.Cdr.DurationSec);
            decimal newRawDuration = this.CdrProcessor.CollectionResult.RawDurationTotalOfConsistentCdrs;
            cdrmeta cdrMetaAfterCdrJob = ReadCdrMetaFromDb();

            if (newProcessedDuration != cdrMetaAfterCdrJob.totalInsertedDuration - this.CdrMetaBeforeCdrjob.totalInsertedDuration)
            {
                throw new Exception("New procesed duration does not match duration difference in cdrMeta before" +
                                    "and after cdr job.");
            }
            decimal deletedDurationInCdrTable = cdrMetaAfterCdrJob.totalDeletedDuration;
            decimal insertedDurationInCdrTable = cdrMetaAfterCdrJob.totalInsertedDuration;


            var oldCollectionResult = this.CdrEraser?.CollectionResult;
            decimal durationSupposedToBeDeleted = 0;
            cdrMetaAfterCdrJob.lastJobSegmentDeletedDuration = 0;
            if (oldCollectionResult != null)
            {
                durationSupposedToBeDeleted = oldCollectionResult.ConcurrentCdrExts.Values.Sum(c => c.Cdr.DurationSec);
                if (durationSupposedToBeDeleted
                    != oldCollectionResult.ProcessedCdrExts.Sum(c => c.Cdr.DurationSec))
                {
                    throw new Exception("Mismatch 1");
                }
                if (durationSupposedToBeDeleted != cdrMetaAfterCdrJob.totalDeletedDuration -
                    this.CdrMetaBeforeCdrjob.totalDeletedDuration)
                {
                    throw new Exception("Duration supposed to be deleted does not match duration " +
                                        "difference in cdrMeta before and after cdr job.");
                }
                cdrMetaAfterCdrJob.lastJobSegmentDeletedDuration = durationSupposedToBeDeleted;
            }
            cdrMetaAfterCdrJob.lastJobSegmentInsertedDuration = newProcessedDuration;

            decimal durationWrittenInCdrTable = insertedDurationInCdrTable - deletedDurationInCdrTable;
            decimal summaryDurationWritten = this.CdrJobContext.Context.Database.SqlQuery<decimal>(
                $@"select sum(actualduration) actualduration from
                (
                select sum(actualduration) actualduration, sum(roundedduration) roundedduration, sum(duration1) duration1, sum(duration2) duration2, sum(duration3) duration3 from sum_voice_day_01 union all
                select sum(actualduration) actualduration, sum(roundedduration) roundedduration, sum(duration1) duration1, sum(duration2) duration2, sum(duration3) duration3 from sum_voice_day_02 union all
                select sum(actualduration) actualduration, sum(roundedduration) roundedduration, sum(duration1) duration1, sum(duration2) duration2, sum(duration3) duration3 from sum_voice_day_03 union all
                select sum(actualduration) actualduration, sum(roundedduration) roundedduration, sum(duration1) duration1, sum(duration2) duration2, sum(duration3) duration3 from sum_voice_day_04
                ) dayWiseSummaries;").ToList().First();

            var daySummaryCaches = this.CdrJobContext.CdrSummaryContext.TableWiseSummaryCache
                .Where(kv => kv.Key.Contains("day")).Select(kv => kv.Value).ToList();
            var summaryDurationFromCache = daySummaryCaches
                .SelectMany(sc => sc.GetItems()).Sum(s => s.actualduration);

            if (durationWrittenInCdrTable != summaryDurationWritten)
            {
                throw new Exception("Cdr metadata mismatch, duration in cdr and summary tables are not consistent.");
            }
            var sql = cdrMetaAfterCdrJob.GetUpdateCommand(c => $" where id={c.id}").ToString();
            this.CdrJobContext.DbCmd.ExecuteCommandText(sql);
        }

        private cdrmeta ReadCdrMetaFromDb()
        {
            this.CdrJobContext.DbCmd.CommandText = "select * from cdrmeta";
            cdrmeta cdrMetaAfterCdrJob = this.CdrJobContext.DbCmd.GetObjectsByQuery(r =>
            new cdrmeta()
            {
                id = r.GetInt32(0),
                lastJobSegmentInsertedDuration = r.GetDecimal(1),
                lastJobSegmentDeletedDuration = r.GetDecimal(2),
                totalInsertedDuration = r.GetDecimal(3),
                totalDeletedDuration = r.GetDecimal(4)
            }
            ).First();
            return cdrMetaAfterCdrJob;
        }
    }
}
