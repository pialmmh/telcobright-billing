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
    public class NewCdrFileJob : ITelcobrightJob
    {
        public string RuleName => "JobNewCdrFile";
        public virtual string HelpText => "New Cdr Job, processes a new CDR file";
        public override string ToString() => this.RuleName;
        public virtual int Id => 1;
        public bool PreDecodingStageOnly { get; private set; }
        protected int RawCount, NonPartialCount, UniquePartialCount, RawPartialCount, DistinctPartialCount = 0;
        protected decimal RawDurationTotalOfConsistentCdrs = 0;
        protected CdrJobInputData Input { get; set; }
        protected CdrCollectorInputData CollectorInput { get; set; }
        protected bool PartialCollectionEnabled => this.Input.MediationContext.Tbc.CdrSetting
            .PartialCdrEnabledNeIds.Contains(this.Input.Ne.idSwitch);
        protected Func<NewCdrPreProcessor, List<string[]>, List<CdrAndInconsistentWrapper>> parallelConvertToCdr = (preProcessor, txtRows) =>
        {
            //cdrinconsistent cdrInconsistent = null;
            //CdrAndInconsistentWrapper cdrAndInconsistentWrapper = preProcessor.ConvertToCdr(txtRow, out cdrInconsistent);

            ParallelIterator<string[], CdrAndInconsistentWrapper> parallelConverter=  
                                                new ParallelIterator<string[], CdrAndInconsistentWrapper>(txtRows);
            List<CdrAndInconsistentWrapper> cdrAndInconsistents =
                parallelConverter.getOutput(r => preProcessor.ConvertToCdr(r));
            return cdrAndInconsistents;
        };
        public object PreprocessJob(object data)
        {
            Dictionary<string, object> dataAsDic = (Dictionary<string, object>) data;
            this.Input = (CdrJobInputData)dataAsDic["cdrJobInputData"];
            object preDecodingStageOnly=null;
            if (dataAsDic.TryGetValue("preDecodingStageOnly", out preDecodingStageOnly) == false)
            {
                this.PreDecodingStageOnly = false;
            }
            else
            {
                this.PreDecodingStageOnly = (bool) preDecodingStageOnly;
            }
            NewCdrPreProcessor preProcessor = DecodeNewCdrFile();
            if (PreDecodingStageOnly)
            {
                return preProcessor;
            }
            preProcessor = preFormatRawCdrs(preProcessor);
            return preProcessor;
        }
        public virtual Object Execute(ITelcobrightJobInput jobInputData)
        {
            NewCdrPreProcessor preProcessor = null;//preprecessor.txtrows contains decoded raw cdrs in string[] format
            this.Input = (CdrJobInputData)jobInputData;

            if (this.Input.IsBatchJob == false)//not batch job
            {
                preProcessor = DecodeNewCdrFile();
                preProcessor = preFormatRawCdrs(preProcessor);
            }
            else//batch job
            {
                Dictionary<long, NewCdrWrappedJobForMerge> jobWiseRawCollection = this.Input.MergedJobsDic;
                if (jobWiseRawCollection.Any() == false)//merged info must be present in cdr job input data
                {
                    throw new Exception("New cdr vs raw collection cannot be empty for merged cdr job. There must be at least one job.");
                }
                NewCdrWrappedJobForMerge head = jobWiseRawCollection.First().Value;
                List<NewCdrWrappedJobForMerge> tail = jobWiseRawCollection.Skip(1).Select(kv => kv.Value).ToList();
                foreach (var kv in jobWiseRawCollection)//make sure empty files haven't been merged.
                {
                    long idJob = kv.Key;
                    var job = kv.Value.TelcobrightJob;
                    var rows = kv.Value.PreProcessor.TxtCdrRows;
                    if (rows.Any() == false)
                    {
                        throw new Exception($"Empty files cannot be merged. Job id:{idJob}, job name:{job.JobName}");
                    }
                }
                int headCdrCount = head.PreProcessor.TxtCdrRows.Count;
                int tailCdrCount = tail.Sum(job => job.PreProcessor.TxtCdrRows.Count);
                if (headCdrCount!=tailCdrCount)
                {
                    throw new Exception($"Head cdr count must match sum of tail jobs for merge processing. Job id:{head.TelcobrightJob.id}, job name:{head.TelcobrightJob.JobName}");
                }
                preProcessor = head.PreProcessor;
            }

            CdrCollectionResult newCollectionResult, oldCollectionResult = null;
            preProcessor.GetCollectionResults(out newCollectionResult, out oldCollectionResult);
            newCollectionResult.FinalNonDuplicateEvents = preProcessor.FinalNonDuplicateEvents;
            //newCollectionResult.DuplicateEvents = preProcessor.DuplicateEvents;
            foreach (string[] row in preProcessor.DuplicateEvents)
            {
                row[Fn.Switchid] = this.Input.Ne.idSwitch.ToString();
                row[Fn.Filename] = this.CollectorInput.TelcobrightJob.JobName;
                newCollectionResult.DuplicateEvents.Add(row);
            }

            PartialCdrTesterData partialCdrTesterData = OrganizeTestDataForPartialCdrs(preProcessor, newCollectionResult);
            CdrJob cdrJob = (new CdrJobFactory(this.Input, this.RawCount)).
                CreateCdrJob(preProcessor, newCollectionResult, oldCollectionResult, partialCdrTesterData);
            if (cdrJob.CdrProcessor.CollectionResult.ConcurrentCdrExts.Count > 0) //job not empty, or has records
            {
                cdrJob.Execute(); //MAIN EXECUTION/MEDIATION METHOD
            }
            else
            {
                handleAndFinalizeEmptyJob(cdrJob);
            }
            if (this.Input.IsBatchJob == false) //single job, not merged
            {
                FinalizeNonMergedJob(cdrJob);
            }
            else //
            {
                FinalizeMergedJobs(cdrJob);
            }
            return JobCompletionStatus.Complete;
        }

        private void handleAndFinalizeEmptyJob(CdrJob cdrJob)
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
            WriteJobCompletionIfCollectionIsEmpty(0, this.Input.TelcobrightJob, cdrJob.CdrProcessor.CdrJobContext.Context);
            if (this.Input.CdrSetting.DisableCdrPostProcessingJobCreationForAutomation == false)
            {
                CreateNewCdrPostProcessingJobs(this.Input.Context, this.Input.MediationContext.Tbc, cdrJob.CdrProcessor.CdrJobContext.TelcobrightJob);
                DeletePreDecodedFile(this.Input.Context, this.Input.MediationContext.Tbc,
                    cdrJob.CdrProcessor.CdrJobContext.TelcobrightJob);
            }
        }

        private void FinalizeNonMergedJob(CdrJob cdrJob)
        {
            var collectionResult = cdrJob.CdrProcessor.CollectionResult;
            if (collectionResult.OriginalRowsBeforeMerge.Count > 0) //job not empty, or has records
            {
                WriteJobCompletionIfCollectionNotEmpty(cdrJob.CdrProcessor.CollectionResult.RawCount,
                    this.Input.TelcobrightJob, cdrJob.CdrProcessor.CdrJobContext.Context);
            }
            else
            {
                if (cdrJob.CdrProcessor.CollectionResult.CdrInconsistents.Count <= 0)
                {
                    if (!cdrJob.CdrProcessor.CdrJobContext.MediationContext.Tbc.CdrSetting.EmptyFileAllowed)
                    {
                        throw new Exception("Empty new cdr files are not considered valid as per cdr setting.");
                    }
                }
                WriteJobCompletionIfCollectionIsEmpty(
                    cdrJob.CdrProcessor.CollectionResult.OriginalRowsBeforeMerge.Count,
                    this.Input.TelcobrightJob, cdrJob.CdrProcessor.CdrJobContext.Context);
            }
            if (this.Input.CdrSetting.DisableCdrPostProcessingJobCreationForAutomation == false)
            {
                CreateNewCdrPostProcessingJobs(this.Input.Context, this.Input.MediationContext.Tbc,
                    cdrJob.CdrProcessor.CdrJobContext.TelcobrightJob);
                DeletePreDecodedFile(this.Input.Context, this.Input.MediationContext.Tbc,
                    cdrJob.CdrProcessor.CdrJobContext.TelcobrightJob);

            }
        }

        private void FinalizeMergedJobs(CdrJob cdrJob)
        {
            Dictionary<long, NewCdrWrappedJobForMerge> mergedJobsDic =
                cdrJob.CdrProcessor.CdrJobContext.CdrjobInputData.MergedJobsDic;
            PartnerEntities context = cdrJob.CdrJobContext.Context;
            foreach (var newCdrWrappedJobForMerge in mergedJobsDic.Values)
            {
                FinalizeSingleMergedJob(newCdrWrappedJobForMerge, context);
            }
        }

        private void FinalizeSingleMergedJob(NewCdrWrappedJobForMerge mergedJob, PartnerEntities context)
        {
            job telcobrightJob = mergedJob.TelcobrightJob;
            var preProcessor = mergedJob.PreProcessor;
            if (preProcessor.TxtCdrRows.Any() == false)
            {
                throw new Exception($"Instance in a merged new cdr job cannot contain 0 record. Job id:{telcobrightJob.id}, Jobname:{telcobrightJob.JobName}");
            }
            WriteJobCompletionIfCollectionNotEmpty(preProcessor.TxtCdrRows.Count, this.Input.TelcobrightJob, context);
            if (this.Input.CdrSetting.DisableCdrPostProcessingJobCreationForAutomation == false)
            {
                CreateNewCdrPostProcessingJobs(this.Input.Context, this.Input.MediationContext.Tbc,telcobrightJob);
                DeletePreDecodedFile(this.Input.Context, this.Input.MediationContext.Tbc, telcobrightJob);
            }
        }

        public object PostprocessJob(object data)
        {
            throw new NotImplementedException();
        }

        private NewCdrPreProcessor DecodeNewCdrFile()
        {
            NewCdrPreProcessor preProcessor = this.CollectRaw();
            return preProcessor;
        }

        private NewCdrPreProcessor preFormatRawCdrs(NewCdrPreProcessor preProcessor)
        {
            PreProcessRawCdrs(preProcessor);
            //preProcessor.TxtCdrRows.ForEach(txtRow => this.CdrConverter(preProcessor, txtRow));
            List<CdrAndInconsistentWrapper> cdrAndInconsistents =
                parallelConvertToCdr(preProcessor, preProcessor.TxtCdrRows);
            cdrAndInconsistents.ForEach(c => preProcessor.AddToBaseCollection(c));
            return preProcessor;
        }

        protected virtual NewCdrPreProcessor CollectRaw()
        {
            string fileLocationName = this.Input.Ne.SourceFileLocations;
            FileLocation fileLocation =
                this.Input.MediationContext.Tbc.DirectorySettings.FileLocations[fileLocationName];
            string fileName = fileLocation.GetOsNormalizedPath(fileLocation.StartingPath)
                              + Path.DirectorySeparatorChar + this.Input.TelcobrightJob.JobName;
            this.CollectorInput =new CdrCollectorInputData(this.Input, fileName);
            var cdrCollector = new FileBasedTextCdrCollector(this.CollectorInput);
            if (this.PreDecodingStageOnly)
            {
                List<cdrinconsistent> cdrinconsistents = new List<cdrinconsistent>();
                var decoder = cdrCollector.getDecoder();
                var decodedCdrRows = decoder.DecodeFile(this.CollectorInput, out cdrinconsistents);
                NewCdrPreProcessor newCdrPreProcessor =
                    new NewCdrPreProcessor(decodedCdrRows, cdrinconsistents, this.CollectorInput);
                return newCdrPreProcessor;
            }
            return (NewCdrPreProcessor) cdrCollector.Collect();
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
        

        protected void PreProcessRawCdrs(NewCdrPreProcessor preProcessor)
        {
            var collectorinput = this.CollectorInput;
            SetIdCallsInSameOrderAsCollected(preProcessor, collectorinput);

            //private static void SetIdCallAsBillId(NewCdrPreProcessor preProcessor)
            //{
            //    preProcessor.TxtCdrRows.ForEach(txtRow => txtRow[98] = txtRow[1]);
            //}
            Parallel.ForEach(preProcessor.TxtCdrRows, txtRow =>
            {
                txtRow[98] = txtRow[1];
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

        protected void WriteJobCompletionIfCollectionNotEmpty(int rawCount, job telcobrightJob, PartnerEntities context)
        {
            using (DbCommand cmd = ConnectionManager.CreateCommandFromDbContext(context))
            {
                string sql =
                    $" update job set CompletionTime={DateTime.Now.ToMySqlField()}, " +
                    $" status=1, " +
                    $"NoOfSteps={rawCount}," +
                    $"progress={rawCount}," +
                    $"Error=null where id={telcobrightJob.id}";
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

        protected void WriteJobCompletionIfCollectionIsEmpty(int rawCount, job telcobrightJob, PartnerEntities context)
        {
            using (DbCommand cmd = ConnectionManager.CreateCommandFromDbContext(context))
            {
                string sql =
                    $" update job set CompletionTime={DateTime.Now.ToMySqlField()}, " +
                    $" status=1, " +
                    $"NoOfSteps={rawCount}," +//could be non zero if inconsistents exist
                    $"progress={rawCount}," +
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
        protected void DeletePreDecodedFile(PartnerEntities context, TelcobrightConfig tbc, job cdrJob)
        {
            string fileLocationName = this.CollectorInput.Ne.SourceFileLocations;
            FileLocation fileLocation = this.CollectorInput.Tbc.DirectorySettings.FileLocations[fileLocationName];
            string newCdrFileName = fileLocation.GetOsNormalizedPath(fileLocation.StartingPath)
                                    + Path.DirectorySeparatorChar + cdrJob.JobName;
            FileInfo newCdrFileInfo = new FileInfo(newCdrFileName);
            string predecodedDirName = newCdrFileInfo.DirectoryName + Path.DirectorySeparatorChar + "predecoded";
            string preDecodedFileName = predecodedDirName + Path.DirectorySeparatorChar + newCdrFileInfo.Name + ".predecoded";
            if (File.Exists(preDecodedFileName))
                File.Delete(preDecodedFileName);
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
