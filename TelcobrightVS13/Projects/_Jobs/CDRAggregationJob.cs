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
using FlexValidation;
using LibraryExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;

namespace Jobs
{
    [Export("Job", typeof(ITelcobrightJob))]
    public class CDRAggregationJob : ITelcobrightJob
    {
        public virtual string RuleName => "JobNewCdrFile";
        public virtual string HelpText => "New Cdr Job, processes a new CDR file";
        public override string ToString() => this.RuleName;
        public virtual int Id => 17;
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
            string fileLocationName = this.Input.Ne.SourceFileLocations;
            FileLocation fileLocation = this.Input.MediationContext.Tbc.DirectorySettings.FileLocations[fileLocationName];
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
                    preProcessor.TxtCdrRows =
                        AbstractCdrJobPreProcessor.ChangeDuplicateBillIds(preProcessor.TxtCdrRows);
                }
            }
            else
            {
                preProcessor.TxtCdrRows =
                    preProcessor.FilterCdrsWithDuplicateBillIdsAsInconsistent(preProcessor.TxtCdrRows);
                preProcessor.TxtCdrRows.AsParallel().ForAll(row=>row[Fn.Partialflag]="0");
            }

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
                preProcessor.RemoveIllegalCharacters(this.CollectorInput.Tbc.CdrSetting
                    .IllegalStrToRemoveFromFields, txtRow);
                preProcessor.SetSwitchid(txtRow);
                preProcessor.SetJobNameWithFileName(this.CollectorInput.TelcobrightJob.JobName, txtRow);
                preProcessor
                    .AdjustStartTimeBasedOnCdrSettingsForSummaryTimeField(
                        this.CollectorInput.Tbc.CdrSetting.SummaryTimeField, txtRow);
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

        protected void CreateNewCdrPostProcessingJobs(PartnerEntities context, TelcobrightConfig tbc, job cdrJob)
        {
            string jobParam = cdrJob.JobParameter;
            string unsplitFileName = jobParam.Split('=')[1];
            if (unsplitFileName.IsNullOrEmptyOrWhiteSpace())
            {
                createJobsForUnsplitCase(context, tbc, cdrJob);
            }
            else
            {
                createJobsForSplitCase(context, tbc, cdrJob,unsplitFileName);
            }
            

        }

        private static void createJobsForSplitCase(PartnerEntities context, TelcobrightConfig tbc, job cdrJob,
            string unsplitFileName)
        {
            List<long> dependentJobIdsBeforeDelete = new List<long>();
            if (tbc.CdrSetting.BackupSyncPairNames != null)
            {
                foreach (string syncPairname in tbc.CdrSetting.BackupSyncPairNames)
                {
                    job fileCopyJob = FileUtil.CreateFileCopyJob(tbc, syncPairname, unsplitFileName, context);
                    string doubleSlashNormalizedFileName = fileCopyJob.JobName.Replace(@"\\", @"\");
                    bool fileCopyJobExists =
                        context.jobs.Any(j => j.JobName == doubleSlashNormalizedFileName && j.idjobdefinition == 6);
                    if (fileCopyJobExists == false)
                    {
                        long fileCopyJobsId = FileUtil.WriteFileCopyJobSingle(fileCopyJob, context.Database.Connection);
                        dependentJobIdsBeforeDelete.Add(fileCopyJobsId);
                        //create delete job for unsplitFile
                        string vaultName = tbc.DirectorySettings.SyncPairs[cdrJob.ne.SourceFileLocations].Name;
                        FileLocation srcFileLocation = tbc.DirectorySettings.SyncPairs[vaultName]
                            .DstSyncLocation.FileLocation;
                        job newDelJob = FileUtil.CreateFileDeleteJob(unsplitFileName,
                            //tbc.DirectorySettings.FileLocations[vaultName],
                            srcFileLocation,
                            context,
                            new JobPreRequisite()
                            {
                                ExecuteAfterJobs = dependentJobIdsBeforeDelete,
                            }
                        );
                        newDelJob.JobName = newDelJob.JobName.Replace(@"\\", @"\");
                        InsertOrUpdateDeleteJob(context, fileCopyJobsId, newDelJob);

                    }
                }
            }
        }

        private static void createJobsForUnsplitCase(PartnerEntities context, TelcobrightConfig tbc, job cdrJob)
        {
            string fileToCopy = cdrJob.JobName;
            ne thisNe = cdrJob.ne;
            List<string> cdrBackupSyncPairNames = new List<string>();
            IEnumerable<string> syncPairnames = thisNe.BackupFileLocations?.Split(',').Select(s => s.Trim());
            if (syncPairnames != null)
                foreach (string syncPairname in syncPairnames)
                {
                    cdrBackupSyncPairNames.Add(syncPairname);
                }
            List<long> dependentJobIdsBeforeDelete = new List<long>() {cdrJob.id};
            if (tbc.CdrSetting.BackupSyncPairNames != null)
            {
                foreach (string syncPairname in tbc.CdrSetting.BackupSyncPairNames)
                {
                    if (cdrBackupSyncPairNames.Contains(syncPairname) == false) continue;
                    job fileCopyJob = FileUtil.CreateFileCopyJob(tbc, syncPairname, fileToCopy, context);
                    if (fileCopyJob != null)
                    {
                        bool jobExists =
                            context.jobs.Any(j => j.JobName == fileCopyJob.JobName && j.idjobdefinition == 6);
                        if (jobExists == false)
                        {
                            long insertedJobsId = FileUtil.WriteFileCopyJobSingle(fileCopyJob, context.Database.Connection);
                            dependentJobIdsBeforeDelete.Add(insertedJobsId);
                        }
                    }
                    //else job exists already, no need to create again.
                }
            }
            //create delete job
            string vaultName = cdrJob.ne.SourceFileLocations;
            FileLocation fileLocation = tbc.DirectorySettings.FileLocations[vaultName];
            job newDelJob= FileUtil.CreateFileDeleteJob(cdrJob.JobName, fileLocation, context,
                new JobPreRequisite()
                {
                    ExecuteAfterJobs = dependentJobIdsBeforeDelete,
                }
            );
            newDelJob.JobName = newDelJob.JobName.Replace(@"\\", @"\");
            InsertOrUpdateDeleteJob(context, -1, newDelJob);
        }
        private static void InsertOrUpdateDeleteJob(PartnerEntities context, long fileCopyJobsId, job newDelJob)
        {
            job existingDelJob =
                context.jobs.FirstOrDefault(c => c.idjobdefinition == newDelJob.idjobdefinition &&
                                                 c.JobName == newDelJob.JobName);
            if (existingDelJob != null)
            {
                //exists, update job prerequisite
                existingDelJob.JobParameter = existingDelJob.JobParameter.Replace(@"\", @"`");
                JobParamFileDelete existingDeljobParam = JsonConvert.DeserializeObject<JobParamFileDelete>(existingDelJob.JobParameter);
                existingDeljobParam.FileName = existingDeljobParam.FileName.Replace("`", @"\");
                existingDeljobParam.FileLocation.PathSeparator =
                    existingDeljobParam.FileLocation.PathSeparator.Replace("`", @"\");
                existingDeljobParam.JobPrerequisite.ExecuteAfterJobs.Add(fileCopyJobsId);
                existingDelJob.JobParameter = JsonConvert.SerializeObject(existingDeljobParam);
                DbCommand cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = $@"update job set JobParameter='{existingDelJob.JobParameter}'
                                                 where id={existingDelJob.id};";
                cmd.ExecuteNonQuery();
            }
            else
            {
                DbCommand cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = newDelJob.GetExtInsertCustom(
                    j =>
                    {
                        string jobName = newDelJob.JobName.Replace(@"\", @"\\");
                        return new StringBuilder(
                                $@"insert into job(idjobdefinition,priority,idne,jobname,status,jobparameter,creationtime) 
                                         values ({newDelJob.idjobdefinition},{newDelJob.priority},{0},'{jobName}',
                                         {newDelJob.Status},'{newDelJob.JobParameter}',{
                                        newDelJob.CreationTime.ToMySqlField()
                                    });")
                            .ToString();
                    }).ToString();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
