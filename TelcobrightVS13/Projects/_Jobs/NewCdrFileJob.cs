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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;

namespace Jobs
{
    [Export("Job", typeof(ITelcobrightJob))]
    public class NewCdrFileJob : ITelcobrightJob
    {
        public virtual string RuleName => "JobNewCdrFile";
        public virtual string HelpText => "New Cdr Job, processes a new CDR file";
        public override string ToString() => this.RuleName;
        public virtual int Id => 1;
        protected int RawCount, NonPartialCount, UniquePartialCount, RawPartialCount, DistinctPartialCount = 0;
        protected decimal RawDurationTotalOfConsistentCdrs = 0;
        protected CdrJobInputData Input { get; set; }
        protected CdrCollectorInputData CollectorInput { get; set; }
        protected bool PartialCollectionEnabled => this.Input.MediationContext.Tbc.CdrSetting
            .PartialCdrEnabledNeIds.Contains(this.Input.Ne.idSwitch);
        protected Action<NewCdrPreProcessor, string[]> CdrConverter = (preProcessor, txtRow) =>
        {
            cdrinconsistent cdrInconsistent = null;
            preProcessor.ConvertToCdr(txtRow, out cdrInconsistent);
            if (cdrInconsistent != null) preProcessor.InconsistentCdrs.Add(cdrInconsistent);
        };
        public virtual JobCompletionStatus Execute(ITelcobrightJobInput jobInputData)
        {
            this.Input = (CdrJobInputData)jobInputData;
            NewCdrPreProcessor preProcessor = this.CollectRaw();
            PreformatRawCdrs(preProcessor);
            preProcessor.TxtCdrRows.ForEach(txtRow => this.CdrConverter(preProcessor, txtRow));

            CdrCollectionResult newCollectionResult, oldCollectionResult = null;
            preProcessor.GetCollectionResults(out newCollectionResult, out oldCollectionResult);

            PartialCdrTesterData partialCdrTesterData = OrganizeTestDataForPartialCdrs(preProcessor, newCollectionResult);
            CdrJob cdrJob = (new CdrJobFactory(this.Input, this.RawCount)).
                CreateCdrJob(preProcessor, newCollectionResult, oldCollectionResult, partialCdrTesterData);
            ExecuteCdrJob(cdrJob);
            return JobCompletionStatus.Complete;
        }

        protected virtual NewCdrPreProcessor CollectRaw()
        {
            Vault vault = this.Input.MediationContext.Tbc.DirectorySettings.Vaults.First(
                c => c.Name == this.Input.TelcobrightJob.ne.SourceFileLocations);
            FileLocation fileLocation = vault.LocalLocation.FileLocation;
            string fileName = fileLocation.GetOsNormalizedPath(fileLocation.StartingPath)
                              + Path.DirectorySeparatorChar + this.Input.TelcobrightJob.JobName;
            this.CollectorInput = new CdrCollectorInputData(this.Input, fileName);
            IEventCollector cdrCollector = new FileBasedTextCdrCollector(this.CollectorInput);
            return (NewCdrPreProcessor)cdrCollector.Collect();
        }

        public PartialCdrTesterData OrganizeTestDataForPartialCdrs(NewCdrPreProcessor preProcessor,
            CdrCollectionResult newCollectionResult)
        {
            this.RawCount = preProcessor.RawCount;
            newCollectionResult.RawDurationTotalOfConsistentCdrs =
                preProcessor.NonPartialCdrs.Sum(c => c.DurationSec) + preProcessor.PartialCdrContainers
                    .SelectMany(pc => pc.NewRawInstances).Sum(r => r.DurationSec);
            this.RawDurationTotalOfConsistentCdrs = newCollectionResult.RawDurationTotalOfConsistentCdrs;
            PartialCdrTesterData partialCdrTesterData = null;
            if (this.PartialCollectionEnabled)
            {
                this.NonPartialCount = preProcessor.TxtCdrRows.Count(r => r[Fn.Partialflag] == "0");
                List<string[]> partialRows = preProcessor.TxtCdrRows.Where(r =>
                    this.Input.CdrSetting.PartialCdrFlagIndicators.Contains(r[Fn.Partialflag])).ToList();
                this.RawPartialCount = partialRows.Count;
                if (preProcessor.TxtCdrRows.Count != this.NonPartialCount + this.RawPartialCount)
                    throw new Exception("TxtCdr rows with partial & non-partial flag do not match total decoded text rows");
                this.DistinctPartialCount = partialRows.GroupBy(r => r[Fn.UniqueBillId]).Count();
                partialCdrTesterData = new PartialCdrTesterData(this.NonPartialCount, this.RawCount,
                    newCollectionResult.RawDurationTotalOfConsistentCdrs, this.RawPartialCount);
            }
            return partialCdrTesterData;
        }

        protected virtual void ExecuteCdrJob(CdrJob cdrJob)
        {
            if (cdrJob.CdrProcessor.CollectionResult.ConcurrentCdrExts.Count > 0)
            {
                cdrJob.Execute();
                WriteJobCompletionIfCollectionNotEmpty(cdrJob.CdrProcessor, this.Input.TelcobrightJob);
            }
            else
            {
                if (cdrJob.CdrProcessor.CollectionResult.CdrInconsistents.Count > 0)
                {
                    if (this.Input.TelcobrightJob.idjobdefinition == 1 &&
                        cdrJob.CdrProcessor.CollectionResult.CdrInconsistents.Count > 0) //newcdr
                    {
                        cdrJob.CdrProcessor.WriteCdrInconsistent();
                    }
                }
                else
                {
                    if (!cdrJob.CdrProcessor.CdrJobContext.MediationContext.Tbc.CdrSetting.EmptyFileAllowed)
                    {
                        throw new Exception("Empty new cdr files are not considered valid as per cdr setting.");
                    }
                }
                WriteJobCompletionIfCollectionIsEmpty(cdrJob.CdrProcessor, this.Input.TelcobrightJob);
            }
            if (this.Input.CdrSetting.DisableCdrPostProcessingJobCreationForAutomation == false)
            {
                CreateNewCdrPostProcessingJobs(this.Input.Context, this.Input.MediationContext.Tbc,
                    cdrJob.CdrProcessor.CdrJobContext.TelcobrightJob);
            }
        }

        protected void PreformatRawCdrs(NewCdrPreProcessor preProcessor)
        {
            var collectorinput = this.CollectorInput;
            SetIdCallsInSameOrderAsCollected(preProcessor, collectorinput);
            if (this.CollectorInput.CdrSetting.UseIdCallAsBillId == true)
            {
                SetIdCallAsBillId(preProcessor);
            }
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
            });
            MefValidator<string[]> inconistentValidator =
                NewCdrPreProcessor.CreateValidatorForInconsistencyCheck(collectorinput);
            var cdrSetting = collectorinput.CdrJobInputData.MediationContext.Tbc.CdrSetting;
            if (cdrSetting.PartialCdrEnabledNeIds
                .Contains(collectorinput.Ne.idSwitch))
            {
                if (cdrSetting.AutoCorrectDuplicateBillId == true)
                {
                    preProcessor.TxtCdrRows = AbstractCdrJobPreProcessor.ChangeDuplicateBillIds(preProcessor.TxtCdrRows);
                }
            }
            else preProcessor.TxtCdrRows = preProcessor.FilterCdrsWithDuplicateBillIdsAsInconsistent(preProcessor.TxtCdrRows);

            if (cdrSetting.AutoCorrectBillIdsWithPrevChargeableIssue == true)
            {
                preProcessor.TxtCdrRows = CdrJob.ChangeBillIdsWithPrevChargeableIssue(preProcessor.TxtCdrRows);
            }
            Parallel.ForEach(preProcessor.TxtCdrRows, txtRow =>
            {
                preProcessor.CheckAndConvertIfInconsistent(collectorinput.CdrJobInputData,
                inconistentValidator, txtRow);
            });
            if (preProcessor.InconsistentCdrs.Any())
            {
                List<long> inconsistentIdCalls = preProcessor.InconsistentCdrs.Select(c => Convert.ToInt64(c.IdCall)).ToList();
                preProcessor.TxtCdrRows = preProcessor.TxtCdrRows
                    .Where(c => !inconsistentIdCalls.Contains(Convert.ToInt64(c[Fn.IdCall])))
                    .ToList();
            }

        }

        protected void PreformatRawCdrsForExceptionalCircumstances(NewCdrPreProcessor preProcessor)
        {
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
            });
        }


        private static void SetIdCallsInSameOrderAsCollected(NewCdrPreProcessor preProcessor, CdrCollectorInputData collectorinput)
        {
            //keep the cdrs in the same order as received, don't use parallel
            preProcessor.TxtCdrRows.ForEach(txtRow => preProcessor.SetIdCall(collectorinput.AutoIncrementManager, txtRow));
        }
        private static void SetIdCallAsBillId(NewCdrPreProcessor preProcessor)
        {
            preProcessor.TxtCdrRows.ForEach(txtRow => txtRow[98] = txtRow[1]);
        }


        protected void WriteJobCompletionIfCollectionNotEmpty(CdrProcessor cdrProcessor, job telcobrightJob)
        {
            using (DbCommand cmd = ConnectionManager.CreateCommandFromDbContext(cdrProcessor.CdrJobContext.Context))
            {
                string sql =
                    $" update job set CompletionTime={DateTime.Now.ToMySqlField()}, " +
                    $" status=1, " +
                    $"NoOfSteps={cdrProcessor.CollectionResult.RawCount}," +
                    $"progress={cdrProcessor.CollectionResult.RawCount}," +
                    $"Error=null where id={telcobrightJob.id}";
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

        protected void WriteJobCompletionIfCollectionIsEmpty(CdrProcessor cdrProcessor, job telcobrightJob)
        {
            using (DbCommand cmd = ConnectionManager.CreateCommandFromDbContext(cdrProcessor.CdrJobContext.Context))
            {
                string sql =
                    $" update job set CompletionTime={DateTime.Now.ToMySqlField()}, " +
                    $" status=1, " +
                    $"NoOfSteps={cdrProcessor.CollectionResult.RawCount}," +//could be non zero if inconsistents exist
                    $"progress={cdrProcessor.CollectionResult.RawCount}," +
                    $"Error=null where id={telcobrightJob.id}";
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

        protected void CreateNewCdrPostProcessingJobs(PartnerEntities context, TelcobrightConfig tbc, job thisJob)
        {
            List<long> dependentJobIdsBeforeDelete = new List<long>() { thisJob.id }; //cdrJob itself
            //create archiving job
            if (tbc.CdrSetting.BackupSyncPairNames != null)
            {
                foreach (string syncPairname in tbc.CdrSetting.BackupSyncPairNames)
                {
                    job fileCopyJob = FileUtil.CreateFileCopyJob(tbc, syncPairname, thisJob.JobName, context);
                    long insertedJobsId = FileUtil.WriteFileCopyJobSingle(fileCopyJob, context.Database.Connection);
                    dependentJobIdsBeforeDelete.Add(insertedJobsId);
                }
            }
            //create delete job
            string vaultName = tbc.DirectorySettings.Vaults.Where(c => c.Name == thisJob.ne.SourceFileLocations)
                .Select(c => c.Name)
                .First();
            FileUtil.CreateFileDeleteJob(thisJob.JobName, tbc.DirectorySettings.FileLocations[vaultName], context,
                new JobPreRequisite()
                {
                    ExecuteAfterJobs = dependentJobIdsBeforeDelete,
                }
            );
        }
    }
}
