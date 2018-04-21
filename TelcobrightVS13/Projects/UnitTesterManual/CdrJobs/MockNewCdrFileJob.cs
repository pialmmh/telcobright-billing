using System;
using System.Collections.Generic;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using System.Data.Common;
using System.IO;
using TelcobrightFileOperations;
using System.Linq;
using System.Text;
using MediationModel;
using System.Threading.Tasks;
using Decoders;
using FlexValidation;
using LibraryExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;
using Jobs;
using TelcobrightMediation.Mediation.Cdr;

namespace UnitTesterManual
{
    [Export("Job", typeof(ITelcobrightJob))]
    public class MockNewCdrFileJob : NewCdrFileJob
    {
        public IFileDecoder CdrDecoder { get; set; }
        public string OperatorName { get; set; }

        public MockNewCdrFileJob(IFileDecoder cdrDecoder, string operatorName)
        {
            this.CdrDecoder = cdrDecoder;
            this.OperatorName = operatorName;
        }

        private CdrJobInputData Input { get; set; }
        bool PartialCollectionEnabled=> this.Input.MediationContext.Tbc.CdrSetting
            .PartialCdrEnabledNeIds.Contains(this.Input.Ne.idSwitch);
        private int rawCount, nonPartialCount,uniquePartialCount,rawPartialCount,distinctPartialCount = 0;
        private decimal rawDurationWithoutInconsistents = 0;
        
        public override JobCompletionStatus Execute(ITelcobrightJobInput jobInputData)
        {
            CdrJobInputData input = (CdrJobInputData) jobInputData;
            this.Input = input;
            AutoIncrementManager autoIncrementManager = new AutoIncrementManager(input.Context);
            CdrCollectorInputData collectorInput =
                new CdrCollectorInputData(input, input.TelcobrightJob.JobName, autoIncrementManager);
            collectorInput.FullPath = $@"C:\telcobright\Vault\Resources\CDR\{this.OperatorName}\{collectorInput.Ne.SwitchName}\"
                                      + input.TelcobrightJob.JobName;
            List<cdrinconsistent> inconsistentCdrs;
            List<string[]> decodedCdrRows = this.CdrDecoder.DecodeFile(collectorInput, out inconsistentCdrs);
            NewCdrPreProcessor preProcessor =
                new NewCdrPreProcessor(decodedCdrRows, inconsistentCdrs, collectorInput);
            base.PreformatRawCdrs(preProcessor, collectorInput);
            preProcessor.TxtCdrRows.ForEach(txtRow => preProcessor.ConvertToCdrOrInconsistentOnFailure(txtRow));

            this.rawDurationWithoutInconsistents = preProcessor.TxtCdrRows.Select(r => Convert.ToDecimal(r[Fn.Durationsec])).Sum();
            if (this.PartialCollectionEnabled)
            {
                this.nonPartialCount = preProcessor.TxtCdrRows.Count(r =>r[Fn.Partialflag]=="0");
                var partialRows= preProcessor.TxtCdrRows.Where(r =>
                    r[Fn.Partialflag].ValueIn(new[] { "1", "2", "3" }) && r[Fn.Partialflag] != "0");
                this.rawPartialCount = partialRows.Count();
                this.distinctPartialCount = partialRows.GroupBy(r => r[Fn.Uniquebillid]).Count();
            }

            CdrCollectionResult newCollectionResult = null;
            CdrCollectionResult oldCollectionResult = null;
            preProcessor.GetCollectionResults(out newCollectionResult, out oldCollectionResult);
            CdrJobContext cdrJobContext = new CdrJobContext(input, autoIncrementManager,
                newCollectionResult.HoursInvolved);
            CdrProcessor cdrProcessor = new CdrProcessor(cdrJobContext, newCollectionResult);
            CdrEraser cdrEraser = oldCollectionResult?.IsEmpty==false
                ? new CdrEraser(cdrJobContext, oldCollectionResult) : null;
            CdrJob cdrJob = new CdrJob(cdrProcessor, cdrEraser, cdrProcessor.CollectionResult.RawCount);
            ExecuteCdrJob(input, cdrJob);
            return JobCompletionStatus.Complete;
        }

        protected override void ExecuteCdrJob(CdrJobInputData input, CdrJob cdrJob)
        {
            if (cdrJob.CdrProcessor.CollectionResult.ConcurrentCdrExts.Count > 0)
            {
                cdrJob.CdrEraser?.UndoOldSummaries();
                cdrJob.CdrEraser?.UndoOldChargeables();
                cdrJob.CdrEraser?.DeleteOldCdrs();

                cdrJob.CdrProcessor?.Process();
                CdrBasedTransactionProcessor transactionProcessor =
                    new CdrBasedTransactionProcessor(cdrJob.CdrProcessor, cdrJob.CdrEraser, cdrJob.CdrJobContext);
                List<acc_transaction> incrementalTransactions = transactionProcessor.ProcessTransactionsIncrementally();
                transactionProcessor.ValidateTransactions(incrementalTransactions);
                cdrJob.CdrJobContext.AccountingContext.ExecuteTransactions(incrementalTransactions);
                Assert.IsTrue(MediationTester.DurationSumInCdrAndMergedCachedAreTollerablyEqual(cdrJob.CdrProcessor));
                Assert.IsTrue(MediationTester.DurationSumInCdrAndSummaryAreTollerablyEqual(cdrJob.CdrProcessor));
                Assert.IsTrue(MediationTester.SummaryCountTwiceAsCdrCount(cdrJob.CdrProcessor));
                Assert.IsTrue(MediationTester.
                    SumOfPrevDayWiseDurationsAndNewSummaryInstancesIsEqualToSameInMergedSummaryCache(cdrJob.CdrProcessor));
                CdrWritingResult cdrWritingResult= cdrJob.CdrProcessor?.WriteCdrs();
                if (this.PartialCollectionEnabled)
                {
                    //partial cdrs tests here...
                    var collectionResult = cdrJob.CdrProcessor.CollectionResult;
                    if (collectionResult.CdrInconsistents.Count > 0)
                        throw new Exception("Cannot continue with tests because inconistent cdrs exist.");
                    var processedCdrExts = collectionResult.ProcessedCdrExts;
                    var nonPartialCdrExts = collectionResult.ProcessedCdrExts.Where(c => c.Cdr.PartialFlag==0).ToList();
                    var partialCdrExts = collectionResult.ProcessedCdrExts.Where(
                                c => c.Cdr.PartialFlag>0 && c.PartialCdrContainer != null).ToList();

                    Assert.AreEqual(cdrWritingResult.CdrCount,collectionResult.ProcessedCdrExts.Count);
                    Assert.AreEqual(cdrWritingResult.CdrErrorCount,collectionResult.CdrErrors.Count);
                    Assert.AreEqual(cdrWritingResult.CdrInconsistentCount, collectionResult.CdrInconsistents.Count);
                    Assert.AreEqual(cdrWritingResult.TrueNonPartialCount, nonPartialCdrExts.Count);
                    Assert.AreEqual(cdrWritingResult.NormalizedPartialCount, partialCdrExts.Count);
                    Assert.AreEqual(cdrWritingResult.CdrCount, (nonPartialCdrExts.Count+partialCdrExts.Count));
                    Assert.AreEqual(this.distinctPartialCount+this.nonPartialCount, 
                        (nonPartialCdrExts.Count + partialCdrExts.Count+collectionResult.CdrErrors.Count));

                    Assert.AreEqual(cdrWritingResult.PartialCdrWriter.WrittenCdrPartialReferences, partialCdrExts.Count);
                    Assert.AreEqual(cdrWritingResult.PartialCdrWriter.WrittenNewRawInstances+this.nonPartialCount, collectionResult.RawCount);
                    partialCdrExts.ForEach(c=>Assert.IsNotNull(c.PartialCdrContainer.NewAggregatedRawInstance));
                    partialCdrExts.ForEach(c => Assert.IsNotNull(c.PartialCdrContainer.NewCdrEquivalent));
                    Assert.AreEqual(partialCdrExts.Select(c=>c.PartialCdrContainer.NewAggregatedRawInstance).Count(),
                        partialCdrExts.Select(c=>c.PartialCdrContainer.NewCdrEquivalent).Count());

                    decimal nonPartialDuration = nonPartialCdrExts.Sum(c => c.Cdr.DurationSec);
                    decimal partialNormalizedDuration = partialCdrExts.Sum(c => c.Cdr.DurationSec);
                    Assert.AreEqual(this.rawDurationWithoutInconsistents,
                        nonPartialDuration + partialNormalizedDuration +
                        collectionResult.CdrErrors.Sum(c => Convert.ToDecimal(c.DurationSec)));
                    Assert.AreEqual(collectionResult.ProcessedCdrExts.Sum(c => c.Cdr.DurationSec),
                        (nonPartialDuration + partialNormalizedDuration));
                    Assert.AreEqual(partialNormalizedDuration,
                        partialCdrExts.SelectMany(c => c.PartialCdrContainer.NewRawInstances).Sum(c => c.DurationSec));
                    //long newAggRawInstancesCount =
                    //  .Select(c => c.PartialCdrContainer.NewAggregatedRawInstance).Count();
                    //Assert.AreEqual(cdrWritingResult.NormalizedPartialCount==));
                }
                foreach (var summaryCache in cdrJob.CdrJobContext.CdrSummaryContext.TableWiseSummaryCache.Values)
                {
                    summaryCache.WriteAllChanges(cdrJob.CdrJobContext.DbCmd, cdrJob.CdrJobContext.SegmentSizeForDbWrite);
                }
                cdrJob.CdrJobContext.AccountingContext.WriteAllChanges();
                cdrJob.CdrJobContext.AutoIncrementManager.WriteState();
            }
            else
            {
                if (cdrJob.CdrProcessor.CollectionResult.CdrInconsistents.Count > 0)
                {
                    if (input.TelcobrightJob.idjobdefinition == 1 &&
                        cdrJob.CdrProcessor.CollectionResult.CdrInconsistents.Count > 0) //newcdr
                    {
                        cdrJob.CdrProcessor.WriteCdrInconsistent();
                    }
                }
                if (cdrJob.CdrProcessor.CdrJobContext.MediationContext.Tbc.CdrSetting.ConsiderEmptyCdrFilesAsValid ==
                    false)
                {
                    throw new Exception("Empty new cdr files are not considered valid as per cdr setting.");
                }
                WriteJobCompletionIfCollectionIsEmpty(cdrJob, input.TelcobrightJob);
            }

            //code reaching here means no error
            using (DbCommand cmd = ConnectionManager.CreateCommandFromDbContext(input.Context))
            {
                cmd.CommandText = " commit; ";
                cmd.ExecuteNonQuery();
            }

            //create file copy job for all backup locations, async-don't wait
            //Task.Run(() => ArchiveAndDeleteJobCreation(tbc, ThisJob));
            //vault.DeleteSingleFile(ThisJob.JobName);
            //File.Delete(fileName);
            //ArchiveAndDeleteJobCreation(input.MediationContext.Tbc, cdrJob.TelcobrightJob);
        }

        protected void WriteJobCompletionIfCollectionNotEmpty(CdrJob cdrJob, job telcobrightJob)
        {
            using (DbCommand cmd =
                ConnectionManager.CreateCommandFromDbContext(cdrJob.CdrProcessor.CdrJobContext.Context))
            {
                string sql = " update job set CompletionTime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'," +
                             " NoOfRecords=" + cdrJob.CdrProcessor.CollectionResult.RawCount + ",StartSequenceNumber=" +
                             cdrJob.CdrProcessor.CollectionResult.CollectionResultProcessingSummary
                                 .StartSequenceNumber + "," +
                             " EndSequenceNumber=" + cdrJob.CdrProcessor.CollectionResult
                                 .CollectionResultProcessingSummary.EndSequenceNumber + "," +
                             " FailedCount=" + cdrJob.CdrProcessor.CollectionResult.CollectionResultProcessingSummary
                                 .FailedCount + "," +
                             " SuccessfulCount=" + cdrJob.CdrProcessor.CollectionResult
                                 .CollectionResultProcessingSummary.SuccessfulCount + "," +
                             " TotalDuration=" + cdrJob.CdrProcessor.CollectionResult.CollectionResultProcessingSummary
                                 .TotalDuration + "," +
                             " PartialDuration=" + cdrJob.CdrProcessor.CollectionResult
                                 .CollectionResultProcessingSummary.PartialDuration + "," +
                             " Status=1 " + "," +
                             " MinCallStartTime='" + cdrJob.CdrProcessor.CollectionResult
                                 .CollectionResultProcessingSummary.MinCallStartTime.ToString("yyyy-MM-dd HH:mm:ss") +
                             "'," +
                             " MaxCallStartTime='" + cdrJob.CdrProcessor.CollectionResult
                                 .CollectionResultProcessingSummary.MaxCallStartTime.ToString("yyyy-MM-dd HH:mm:ss") +
                             "', " +
                             " Error='' " +
                             " where id=" + telcobrightJob.id;
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

        protected void WriteJobCompletionIfCollectionIsEmpty(CdrJob cdrJob, job telcobrightJob)
        {
            using (DbCommand cmd =
                ConnectionManager.CreateCommandFromDbContext(cdrJob.CdrProcessor.CdrJobContext.Context))
            {
                string sql = " update job set CompletionTime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'," +
                             " NoOfRecords=" + cdrJob.CdrProcessor.CollectionResult.RawCount +
                             ",StartSequenceNumber=" + cdrJob.CdrProcessor.CollectionResult
                                 .CollectionResultProcessingSummary.StartSequenceNumber + "," +
                             " EndSequenceNumber=" + cdrJob.CdrProcessor.CollectionResult
                                 .CollectionResultProcessingSummary.EndSequenceNumber + "," +
                             " FailedCount=" + cdrJob.CdrProcessor.CollectionResult.CollectionResultProcessingSummary
                                 .FailedCount + "," +
                             " SuccessfulCount=" + cdrJob.CdrProcessor.CollectionResult
                                 .CollectionResultProcessingSummary.SuccessfulCount + "," +
                             " TotalDuration=" + cdrJob.CdrProcessor.CollectionResult.CollectionResultProcessingSummary
                                 .TotalDuration + "," +
                             " PartialDuration=" + cdrJob.CdrProcessor.CollectionResult
                                 .CollectionResultProcessingSummary.PartialDuration + "," +
                             " Status=1 " +
                             " where id=" + telcobrightJob.id;
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }
    }
}
