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
using FlexValidation;
using LibraryExtensions;
using MySql.Data.MySqlClient;
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;

namespace Jobs
{
    [Export("Job", typeof(ITelcobrightJob))]
    public class NewCdrFile : ITelcobrightJob
    {
        public virtual string RuleName => "JobNewCdrFile";
        public virtual string HelpText => "New Cdr Job, processes a new CDR file";
        public override string ToString() => this.RuleName;
        public virtual int Id => 1;
        public virtual JobCompletionStatus Execute(ITelcobrightJobInput jobInputData)
        {
            CdrJobInputData input = (CdrJobInputData)jobInputData;
            AutoIncrementManager autoIncrementManager = new AutoIncrementManager(input.Context);
            CdrCollectorInputData collectorInput = CreateCollectorInputDataInstance(input, autoIncrementManager);
            IEventCollector cdrCollector = new FileBasedTextCdrCollector(collectorInput, input.Context);
            NewCdrPreProcessor preProcessor = (NewCdrPreProcessor)cdrCollector.Collect();
            PrepareDecodedRawCdrs(preProcessor, collectorInput);
            CdrCollectionResult newCollectionResult = null;
            CdrCollectionResult oldCollectionResult = null;
            preProcessor.GetCollectionResults(out newCollectionResult, out oldCollectionResult);
            CdrJobContext cdrJobContext =
                new CdrJobContext(input, autoIncrementManager, newCollectionResult.HoursInvolved);
            CdrProcessor cdrProcessor = new CdrProcessor(cdrJobContext, newCollectionResult);
            CdrEraser cdrEraser = oldCollectionResult != null ? new CdrEraser(cdrJobContext, oldCollectionResult) : null;
            int rawCount = preProcessor.TxtCdrRows.Count;
            CdrJob cdrJob = new CdrJob(cdrProcessor, cdrEraser, rawCount);
            ExecuteCdrJob(input, cdrJob);
            return JobCompletionStatus.Complete;
        }

        protected CdrCollectorInputData CreateCollectorInputDataInstance(CdrJobInputData input,
            AutoIncrementManager autoIncrementManager)
        {
            Vault vault =
                input.MediationContext.Tbc.Vaults.First(c => c.Name == input.TelcobrightJob.ne.SourceFileLocations);
            FileLocation fileLocation = vault.LocalLocation.FileLocation;
            string fileName = fileLocation.GetOsNormalizedPath(fileLocation.StartingPath)
                              + Path.DirectorySeparatorChar + input.TelcobrightJob.JobName;
            return new CdrCollectorInputData(input, fileName, autoIncrementManager);
        }

        protected void PrepareDecodedRawCdrs(NewCdrPreProcessor preProcessor,
            CdrCollectorInputData collectorinput)
        {
            SetIdCallsInSameOrderAsCollected(preProcessor, collectorinput);
            FlexValidator<string[]> inconistentValidator = NewCdrPreProcessor.CreateValidatorForInconsistencyCheck(collectorinput);
            preProcessor.TxtCdrRows = preProcessor.FilterCdrsWithDuplicateBillIdsAsInconsistent(preProcessor.TxtCdrRows);
            Parallel.ForEach(preProcessor.TxtCdrRows, txtRow =>
            {
                preProcessor.SetAllBlankFieldsToZerolengthString(txtRow);
                preProcessor.RemoveIllegalCharacters(collectorinput.Tbc.CdrSetting
                    .IllegalStrToRemoveFromFields, txtRow);
                preProcessor.SetSwitchid(txtRow);
                preProcessor.SetJobNameWithFileName(collectorinput.TelcobrightJob.JobName, txtRow);
                preProcessor
                    .AdjustStartTimeBasedOnCdrSettingsForSummaryTimeField(
                        collectorinput.Tbc.CdrSetting.SummaryTimeField, txtRow);
                if (!collectorinput.CdrJobInputData.MediationContext.Tbc.CdrSetting.PartialCdrEnabledNeIds
                    .Contains(collectorinput.Ne.idSwitch))
                {
                    preProcessor.MarkRowAsFinalRecordWhenPartialCdrsDisabled(txtRow);
                }
                preProcessor.CheckAndConvertIfInconsistent(collectorinput.CdrJobInputData,
                    inconistentValidator, txtRow);
            });
            if (preProcessor.InconsistentCdrs.Any())
            {
                List<long> inconsistentIdCalls = preProcessor.InconsistentCdrs.Select(c => Convert.ToInt64(c.idcall)).ToList();
                preProcessor.TxtCdrRows = preProcessor.TxtCdrRows
                    .Where(c => !inconsistentIdCalls.Contains(Convert.ToInt64(c[Fn.Idcall])))
                    .ToList();
            }
            preProcessor.TxtCdrRows.ForEach(txtRow => preProcessor.ConvertToCdrOrInconsistentOnFailure(txtRow));
        }

        private static void SetIdCallsInSameOrderAsCollected(NewCdrPreProcessor preProcessor, CdrCollectorInputData collectorinput)
        {
            //keep the cdrs in the same order as received, don't use parallel
            preProcessor.TxtCdrRows.ForEach(txtRow => preProcessor.SetIdCall(collectorinput.AutoIncrementManager, txtRow));
        }

        protected virtual void ExecuteCdrJob(CdrJobInputData input, CdrJob cdrJob)
        {
            if (cdrJob.CdrProcessor.CollectionResult.ConcurrentCdrExts.Count > 0)
            {
                cdrJob.Execute();
                WriteJobCompletionIfCollectionNotEmpty(cdrJob.CdrProcessor, input.TelcobrightJob);
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
                if (cdrJob.CdrProcessor.CdrJobContext.MediationContext.Tbc.CdrSetting.ConsiderEmptyCdrFilesAsValid == false)
                {
                    throw new Exception("Empty new cdr files are not considered valid as per cdr setting.");
                }
                WriteJobCompletionIfCollectionIsEmpty(cdrJob.CdrProcessor, input.TelcobrightJob);
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
            ArchiveAndDeleteJobCreation(input.MediationContext.Tbc, cdrJob.CdrProcessor.CdrJobContext.TelcobrightJob);
        }

        protected void WriteJobCompletionIfCollectionNotEmpty(CdrProcessor cdrProcessor, job telcobrightJob)
        {
            using (DbCommand cmd = ConnectionManager.CreateCommandFromDbContext(cdrProcessor.CdrJobContext.Context))
            {
                string sql = " update job set CompletionTime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'," +
                             " NoOfRecords=" + cdrProcessor.CollectionResult.RawCount + ",StartSequenceNumber=" + cdrProcessor.CollectionResult.CollectionResultProcessingSummary.StartSequenceNumber + "," +
                             " EndSequenceNumber=" + cdrProcessor.CollectionResult.CollectionResultProcessingSummary.EndSequenceNumber + "," +
                             " FailedCount=" + cdrProcessor.CollectionResult.CollectionResultProcessingSummary.FailedCount + "," +
                             " SuccessfulCount=" + cdrProcessor.CollectionResult.CollectionResultProcessingSummary.SuccessfulCount + "," +
                             " TotalDuration=" + cdrProcessor.CollectionResult.CollectionResultProcessingSummary.TotalDuration + "," +
                             " PartialDuration=" + cdrProcessor.CollectionResult.CollectionResultProcessingSummary.PartialDuration + "," +
                             " Status=1 " + "," +
                             " MinCallStartTime='" + cdrProcessor.CollectionResult.CollectionResultProcessingSummary.MinCallStartTime.ToString("yyyy-MM-dd HH:mm:ss") + "'," +
                             " MaxCallStartTime='" + cdrProcessor.CollectionResult.CollectionResultProcessingSummary.MaxCallStartTime.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                             " Error='' " +
                             " where id=" + telcobrightJob.id;
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

        protected void WriteJobCompletionIfCollectionIsEmpty(CdrProcessor cdrProcessor, job telcobrightJob)
        {
            using (DbCommand cmd = ConnectionManager.CreateCommandFromDbContext(cdrProcessor.CdrJobContext.Context))
            {
                string sql = " update job set CompletionTime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'," +
                             " NoOfRecords=" + cdrProcessor.CollectionResult.RawCount +
                             ",StartSequenceNumber=" + cdrProcessor.CollectionResult.CollectionResultProcessingSummary.StartSequenceNumber + "," +
                             " EndSequenceNumber=" + cdrProcessor.CollectionResult.CollectionResultProcessingSummary.EndSequenceNumber + "," +
                             " FailedCount=" + cdrProcessor.CollectionResult.CollectionResultProcessingSummary.FailedCount + "," +
                             " SuccessfulCount=" + cdrProcessor.CollectionResult.CollectionResultProcessingSummary.SuccessfulCount + "," +
                             " TotalDuration=" + cdrProcessor.CollectionResult.CollectionResultProcessingSummary.TotalDuration + "," +
                             " PartialDuration=" + cdrProcessor.CollectionResult.CollectionResultProcessingSummary.PartialDuration + "," +
                             " Status=1 " +
                             " where id=" + telcobrightJob.id;
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

        protected void ArchiveAndDeleteJobCreation(TelcobrightConfig tbc, job thisJob)
        {
            List<long> dependentJobIdsBeforeDelete = new List<long>() { thisJob.id };//cdrJob itself
            //create archiving job
            using (PartnerEntities context = new PartnerEntities(tbc.DatabaseSetting.DatabaseName))
            {
                if (tbc.CdrSetting.BackupSyncPairNames != null)
                {
                    foreach (string syncPairname in tbc.CdrSetting.BackupSyncPairNames)
                    {
                        long idJob = FileUtil.CreateFileCopyJob(tbc, syncPairname, thisJob.JobName, context);
                        dependentJobIdsBeforeDelete.Add(idJob);
                    }
                }

                //create delete job
                string vaultName = tbc.Vaults.Where(c => c.Name == thisJob.ne.SourceFileLocations).Select(c => c.Name).First();
                FileUtil.CreateFileDeleteJob(thisJob.JobName, tbc.DirectorySettings.FileLocations[vaultName], context,
                    new JobPreRequisite()
                    {
                        ExecuteAfterJobs = dependentJobIdsBeforeDelete,
                    }
                );
            }

        }
    }
}
