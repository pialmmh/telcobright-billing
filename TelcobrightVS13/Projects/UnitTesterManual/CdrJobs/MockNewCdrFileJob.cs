using System;
using System.Collections.Generic;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using System.Data.Common;
using System.Diagnostics.Contracts;
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

        bool PartialCollectionEnabled => this.Input.MediationContext.Tbc.CdrSetting
            .PartialCdrEnabledNeIds.Contains(this.Input.Ne.idSwitch);
        public override JobCompletionStatus Execute(ITelcobrightJobInput jobInputData)
        {
            CdrJobInputData input = (CdrJobInputData) jobInputData;
            this.Input = input;
            AutoIncrementManager autoIncrementManager = new AutoIncrementManager(input.Context);
            CdrCollectorInputData collectorInput =
                new CdrCollectorInputData(input, input.TelcobrightJob.JobName, autoIncrementManager);
            collectorInput.FullPath =
                $@"C:\telcobright\Vault\Resources\CDR\{this.OperatorName}\{collectorInput.Ne.SwitchName}\"
                + input.TelcobrightJob.JobName;
            List<cdrinconsistent> inconsistentCdrs;
            List<string[]> decodedCdrRows = this.CdrDecoder.DecodeFile(collectorInput, out inconsistentCdrs);
            NewCdrPreProcessor preProcessor =
                new NewCdrPreProcessor(decodedCdrRows, inconsistentCdrs, collectorInput);
            base.PreformatRawCdrs(preProcessor, collectorInput);
            preProcessor.TxtCdrRows.ForEach(txtRow => preProcessor.ConvertToCdrOrInconsistentOnFailure(txtRow));

            this.rawDurationWithoutInconsistents = preProcessor.TxtCdrRows
                .Select(r => Convert.ToDecimal(r[Fn.Durationsec])).Sum();
            if (this.PartialCollectionEnabled)
            {
                this.nonPartialCount = preProcessor.TxtCdrRows.Count(r => r[Fn.Partialflag] == "0");
                List<string[]> partialRows = preProcessor.TxtCdrRows.Where(r =>
                    this.Input.CdrSetting.PartialCdrFlagIndicators.Contains(r[Fn.Partialflag])).ToList();
                this.rawPartialCount = partialRows.Count;
                if (preProcessor.TxtCdrRows.Count!=this.nonPartialCount+this.rawPartialCount)
                    throw new Exception("TxtCdr rows with partial & non-partial flag do not match total decoded text rows");
                this.distinctPartialCount = partialRows.GroupBy(r => r[Fn.UniqueBillId]).Count();
            }

            CdrCollectionResult newCollectionResult = null;
            CdrCollectionResult oldCollectionResult = null;
            preProcessor.GetCollectionResults(out newCollectionResult, out oldCollectionResult);

            CdrJobContext cdrJobContext = new CdrJobContext(input, autoIncrementManager,newCollectionResult.HoursInvolved);
            CdrProcessor cdrProcessor = new CdrProcessor(cdrJobContext, newCollectionResult);
            CdrEraser cdrEraser = oldCollectionResult?.IsEmpty == false
                ? new CdrEraser(cdrJobContext, oldCollectionResult): null;
            CdrJob cdrJob = new CdrJob(cdrProcessor, cdrEraser, cdrProcessor.CollectionResult.RawCount);
            ExecuteCdrJob(input, cdrJob);
            return JobCompletionStatus.Complete;
        }

        protected override void ExecuteCdrJob(CdrJobInputData input, CdrJob cdrJob)
        {
            if (cdrJob.CdrProcessor.CollectionResult.ConcurrentCdrExts.Count > 0)
            {
                cdrJob.CdrEraser?.RegenerateOldSummaries();
                cdrJob.CdrEraser?.ValidateSummaryReGeneration();
                cdrJob.CdrEraser?.UndoOldSummaries();
                cdrJob.CdrEraser?.UndoOldChargeables();
                cdrJob.CdrEraser?.DeleteOldCdrs();

                cdrJob.CdrProcessor?.Process();

                CdrBasedTransactionProcessor transactionProcessor =
                    new CdrBasedTransactionProcessor(cdrJob.CdrProcessor, cdrJob.CdrEraser, cdrJob.CdrJobContext);
                List<acc_transaction> incrementalTransactions = transactionProcessor.ProcessTransactionsIncrementally();
                transactionProcessor.ValidateTransactions(incrementalTransactions);
                cdrJob.CdrJobContext.AccountingContext.ExecuteTransactions(incrementalTransactions);

                base.ValidateWithMediationTester(input, cdrJob);
                CdrWritingResult cdrWritingResult = cdrJob.CdrProcessor?.WriteCdrs();
                if (this.PartialCollectionEnabled)
                {
                    base.PerformAdditionalValidationForPartialCdrCase(cdrJob, cdrWritingResult);
                }

                foreach (var summaryCache in cdrJob.CdrJobContext.CdrSummaryContext.TableWiseSummaryCache.Values)
                {
                    summaryCache.WriteAllChanges(cdrJob.CdrJobContext.DbCmd,
                        cdrJob.CdrJobContext.SegmentSizeForDbWrite);
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
            base.WriteJobCompletionIfCollectionNotEmpty(cdrJob.CdrProcessor,telcobrightJob);
        }

        protected void WriteJobCompletionIfCollectionIsEmpty(CdrJob cdrJob, job telcobrightJob)
        {
            base.WriteJobCompletionIfCollectionIsEmpty(cdrJob.CdrProcessor, telcobrightJob);
        }
    }
}
