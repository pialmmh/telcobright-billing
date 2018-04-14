using System;
using System.Collections.Generic;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using System.Data.Common;
using System.IO;
using TelcobrightFileOperations;
using System.Linq;
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

namespace UnitTesterManual
{
    [Export("Job", typeof(ITelcobrightJob))]
    public class MockNewCdrFileJob : NewCdrFile
    {
        public IFileDecoder CdrDecoder { get; set; }
        public string OperatorName { get; set; }
        public MockNewCdrFileJob(IFileDecoder cdrDecoder,string operatorName)
        {
            this.CdrDecoder = cdrDecoder;
            this.OperatorName = operatorName;
        }

        public override JobCompletionStatus Execute(ITelcobrightJobInput jobInputData)
        {
            CdrJobInputData input = (CdrJobInputData) jobInputData;
            AutoIncrementManager autoIncrementManager = new AutoIncrementManager(input.Context);
            CdrCollectorInputData collectorInput =
                new CdrCollectorInputData(input, input.TelcobrightJob.JobName, autoIncrementManager);
            collectorInput.FullPath = $@"C:\telcobright\Vault\Resources\CDR\{this.OperatorName}\{collectorInput.Ne.SwitchName}\"
                                      + input.TelcobrightJob.JobName;
            List<cdrinconsistent> inconsistentCdrs;
            List<string[]> decodedCdrRows = this.CdrDecoder.DecodeFile(collectorInput, out inconsistentCdrs);
            NewCdrPreProcessor newAndErrorCdrPreProcessor =
                new NewCdrPreProcessor(decodedCdrRows, inconsistentCdrs, collectorInput);
            base.PrepareDecodedRawCdrs(newAndErrorCdrPreProcessor, collectorInput);

            CdrCollectionResult newCollectionResult = null;
            CdrCollectionResult oldCollectionResult = null;
            newAndErrorCdrPreProcessor.GetCollectionResults(out newCollectionResult, out oldCollectionResult);
            CdrJobContext cdrJobContext = new CdrJobContext(input, autoIncrementManager,
                newCollectionResult.HoursInvolved);
            CdrProcessor cdrProcessor = new CdrProcessor(cdrJobContext, newCollectionResult);
            CdrEraser cdrEraser = oldCollectionResult != null
                ? new CdrEraser(cdrJobContext, oldCollectionResult) : null;
            CdrJob cdrJob = new CdrJob(cdrProcessor, cdrEraser, cdrProcessor.CollectionResult.RawCount);
            ExecuteCdrJob(input, cdrJob);
            return JobCompletionStatus.Complete;
        }

        protected override void ExecuteCdrJob(CdrJobInputData input, CdrJob cdrJob)
        {
            if (cdrJob.CdrProcessor.CollectionResult.ConcurrentCdrExts.Count > 0)
            {
                //cdrJob.Execute();
                cdrJob.CdrEraser?.Process();
                cdrJob.CdrEraser?.WriteChangesExceptContext();
                cdrJob.CdrProcessor?.Process();
                Assert.IsTrue(MediationTester.DurationSumInCdrAndMergedCachedAreTollerablyEqual(cdrJob.CdrProcessor));
                Assert.IsTrue(MediationTester.DurationSumInCdrAndSummaryAreTollerablyEqual(cdrJob.CdrProcessor));
                Assert.IsTrue(MediationTester.SummaryCountTwiceAsCdrCount(cdrJob.CdrProcessor));
                Assert.IsTrue(MediationTester.
                    SumOfPrevDayWiseDurationsAndNewSummaryInstancesIsEqualToSameInMergedSummaryCache(cdrJob.CdrProcessor));
                cdrJob.CdrProcessor?.WriteChangesExceptContext();
                cdrJob.CdrJobContext.WriteChanges();
                WriteJobCompletionIfCollectionNotEmpty(cdrJob, input.TelcobrightJob);
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
