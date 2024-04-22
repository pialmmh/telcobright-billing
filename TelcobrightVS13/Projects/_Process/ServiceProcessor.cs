using TelcobrightMediation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using TelcobrightFileOperations;
using MediationModel;
using System.Data.Common;
using System.Data.Entity;
using Quartz;
using Newtonsoft.Json;
using QuartzTelcobright;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;
using System.IO;
using WebSocketSharp.Server;

namespace Process
{
    [DisallowConcurrentExecution]
    [Export("TelcobrightProcess", typeof(AbstractTelcobrightProcess))]
    public class ServiceProcessor : AbstractTelcobrightProcess
    {
        public override string ToString()
        {
            return this.RuleName;
        }

        public override string RuleName => this.GetType().ToString();
        public override string HelpText => "Processes various services through web socket";
        public override int ProcessId => 109;

        public override void Execute(IJobExecutionContext schedulerContext)
        {
            Console.WriteLine("Starting Web Server");
            string url = "ws://localhost:8888";
            var wssv = new WebSocketServer(url);
            wssv.AddWebSocketService<PrepaidService>("/prepaid");
            wssv.Start();
            Console.WriteLine("Websocket Server has been started");
            Console.ReadKey(true);
            wssv.Stop();
            return;
            string operatorName = schedulerContext.JobDetail.JobDataMap.GetString("operatorName");
            TelcobrightConfig tbc = ConfigFactory.GetConfigFromSchedulerExecutionContext(
                schedulerContext, operatorName);
            CdrSetting cdrSetting = tbc.CdrSetting;
            //string entityConStr = ConnectionManager.GetEntityConnectionStringByOperator(operatorName, tbc);
            //PartnerEntities context = new PartnerEntities(entityConStr);

            //try
            //{
            //    context.Database.Connection.Open();
            //    var mediationContext = new MediationContext(tbc, context);
            //    tbc.GetPathIndependentApplicationDirectory();
            //    foreach (ne ne in mediationContext.Nes.Values)
            //    {
            //        NeAdditionalSetting neAdditionalSetting = null;
            //        cdrSetting.NeWiseAdditionalSettings.TryGetValue(ne.idSwitch, out neAdditionalSetting);
            //        if (ne.UseIdCallAsBillId == 1 && ne.FilterDuplicateCdr == 1)
            //        {
            //            throw new Exception("Idcall cannot be used as uniquebillid when duplicate filtering is on.");
            //        }
            //        if (!neAdditionalSetting.AggregationStyle.IsNullOrEmptyOrWhiteSpace())
            //        {
            //            if (ne.FilterDuplicateCdr == 0)
            //            {
            //                throw new Exception("Duplicate Filtering must be on when aggregation is required");
            //            }
            //            //if(neAdditionalSetting.ProcessMultipleCdrFilesInBatch==true)
            //            //    throw new Exception("Merge processing is not allowed when aggregation is required.");
            //        }
            //        int minRowCountForBatchProcessing = neAdditionalSetting?.MinRowCountToStartBatchCdrProcessing ?? 1;
            //        Dictionary<long, NewCdrWrappedJobForMerge> mergedJobsDic =
            //            new Dictionary<long, NewCdrWrappedJobForMerge>(); //key=idJob
            //        NewCdrWrappedJobForMerge headJobForMerge = null;
            //        int rowCountSoFarForMerge = 0;
            //        Action resetMergeJobStatus = () =>
            //        {
            //            mergedJobsDic = new Dictionary<long, NewCdrWrappedJobForMerge>(); //key=idJob
            //            headJobForMerge = null;
            //            rowCountSoFarForMerge = 0;
            //        };
            //        resetMergeJobStatus();
            //        try
            //        {
            //            if (ne.SkipCdrDecoded == 1 || CheckIncompleteExists(context, mediationContext, ne) == false)
            //                continue;


            //            List<job> newCdrJobs = GetNewCdrJobs(tbc, context, ne, ne.DecodingSpanCount,
            //                neAdditionalSetting);
            //            var jobsWithError = newCdrJobs
            //                .Where(j => !j.Error.IsNullOrEmptyOrWhiteSpace() ||
            //                            !j.JobAdditionalInfo.IsNullOrEmptyOrWhiteSpace()).ToList();
            //            var jobsWithoutError =
            //                newCdrJobs.Where(ij => !jobsWithError.Select(ej => ej.id).Contains(ij.id)).ToList();
            //            newCdrJobs = jobsWithoutError.Concat(jobsWithError).ToList();

            //            List<job> reprocessJobs = new List<job>();
            //            if (!cdrSetting.useCasStyleProcessing)
            //            {
            //                reprocessJobs = GetReProcessJobs(context, ne, ne.DecodingSpanCount);
            //            }
            //            else//cas
            //            {
            //                if (cdrSetting.ProcessNewCdrJobsBeforeReProcess)
            //                {
            //                    if (newCdrJobs.Any()) //process new cdr jobs first, don't add any reprocess job in the queue
            //                    {
            //                        reprocessJobs = new List<job>();
            //                    }
            //                    else
            //                    {
            //                        reprocessJobs = GetReProcessJobs(context, ne, ne.DecodingSpanCount);
            //                    }
            //                }
            //                else
            //                {
            //                    reprocessJobs = GetReProcessJobs(context, ne, ne.DecodingSpanCount);
            //                }

            //            }
            //            List<job> incompleteJobs = cdrSetting.ProcessNewCdrJobsBeforeReProcess
            //                ? newCdrJobs.Concat(reprocessJobs).ToList()
            //                : reprocessJobs.Concat(newCdrJobs).ToList();

            //            //incompleteJobs.AddRange(newCdrJobs); //combine
            //            //jobs with error to be processed as single job and add them to the first of the list, adding them to the last is a bit difficult to manage merge processing


            //            //Thread.Sleep(20000);
            //            CdrJobInputData cdrJobInputData = null;
            //            ITelcobrightJob telcobrightJob = null;
            //            using (DbCommand cmd = context.Database.Connection.CreateCommand())
            //            {
            //                foreach (job job in incompleteJobs) //for each job***************
            //                {
            //                    Console.WriteLine("Processing CdrJob for Switch:" + ne.SwitchName + ", JobName:" +
            //                                      job.JobName);
            //                    try
            //                    {
            //                        closeDbConnection(
            //                            cmd); //connection will be opened inside newcdr file job after creating daywise tables as table creation ddl asks for transaction restart
            //                        mediationContext.MefJobContainer.DicExtensionsIdJobWise.TryGetValue(
            //                            job.idjobdefinition.ToString(), out telcobrightJob);
            //                        if (telcobrightJob == null)
            //                            throw new Exception("JobRule not found in MEF collection.");
            //                        cdrJobInputData =
            //                            new CdrJobInputData(mediationContext, context, ne, job);
            //                        if (job.idjobdefinition != 1
            //                        ) //error process or re-process job, not merging, process as a single job*************
            //                        {
            //                            checkIfProcessingIntendedForThisServer(tbc, ne);
            //                            cdrJobInputData.MergedJobsDic = new Dictionary<long, NewCdrWrappedJobForMerge>();
            //                            object retVal = telcobrightJob.Execute(cdrJobInputData); //EXECUTE
            //                            if (job.idjobdefinition == 1) telcobrightJob.PostprocessJob(retVal);
            //                            cmd.ExecuteCommandText(" commit; ");
            //                            closeDbConnection(cmd);
            //                            continue;
            //                        }
            //                        if (neAdditionalSetting == null ||
            //                            neAdditionalSetting?.ProcessMultipleCdrFilesInBatch == false
            //                        ) //new cdr job, not merging, process as single job
            //                        {
            //                            cdrJobInputData.MergedJobsDic = new Dictionary<long, NewCdrWrappedJobForMerge>();
            //                            object retVal = telcobrightJob.Execute(cdrJobInputData); //EXECUTE
            //                            telcobrightJob.PostprocessJob(retVal);
            //                            cmd.ExecuteCommandText(" commit; ");
            //                            closeDbConnection(cmd);
            //                            continue;
            //                        }
            //                        if (!job.Error.IsNullOrEmptyOrWhiteSpace()) //jobs with error, process as single job
            //                        {
            //                            cdrJobInputData.MergedJobsDic = new Dictionary<long, NewCdrWrappedJobForMerge>();
            //                            object retVal = telcobrightJob.Execute(cdrJobInputData); //EXECUTE
            //                            telcobrightJob.PostprocessJob(retVal);
            //                            cmd.ExecuteCommandText(" commit; ");
            //                            closeDbConnection(cmd);
            //                            continue;
            //                        }
            //                        if (neAdditionalSetting?.ProcessMultipleCdrFilesInBatch == true
            //                        ) //merge new cdr jobs for batch processing
            //                        {
            //                            cdrJobInputData.MergedJobsDic = mergedJobsDic;
            //                            var inputForPreprocess = new Dictionary<string, object>
            //                            {
            //                                {"cdrJobInputData", cdrJobInputData}
            //                            };
            //                            NewCdrPreProcessor preProcessor =
            //                                (NewCdrPreProcessor)telcobrightJob.PreprocessJob(inputForPreprocess); //execute pre-processing
            //                            if (headJobForMerge == null &&
            //                                preProcessor.NewAndInconsistentCount >=
            //                                minRowCountForBatchProcessing) //already large job, process as single
            //                            {//but if merge in progress, do not process as single job, headjob for merge!=null means mergeInProgress
            //                                cdrJobInputData.MergedJobsDic = new Dictionary<long, NewCdrWrappedJobForMerge>();
            //                                object retVal = telcobrightJob.Execute(cdrJobInputData); //not merging, process as single job************
            //                                telcobrightJob.PostprocessJob(retVal);
            //                                cmd.ExecuteCommandText(" commit; ");
            //                                closeDbConnection(cmd);
            //                                continue;
            //                            }
            //                            //merge job for batch processing*******************
            //                            NewCdrWrappedJobForMerge newWrappedJob =
            //                                new NewCdrWrappedJobForMerge(job, preProcessor);
            //                            if (headJobForMerge == null
            //                            ) //empty list of merged job, add the first one (head)
            //                            {
            //                                headJobForMerge = newWrappedJob;
            //                                mergedJobsDic.Add(headJobForMerge.Job.id, newWrappedJob);
            //                                rowCountSoFarForMerge = newWrappedJob.NewAndInconsistentCount;
            //                            }
            //                            else //one of the tail jobs
            //                            {
            //                                mergedJobsDic.Add(newWrappedJob.Job.id, newWrappedJob);
            //                                rowCountSoFarForMerge =
            //                                    headJobForMerge
            //                                        .AppendTailJobRows(newWrappedJob); //apend head+new tail jobs rows
            //                                if (rowCountSoFarForMerge >= minRowCountForBatchProcessing
            //                                ) //enough jobs have been merged for batch processing 
            //                                {
            //                                    cdrJobInputData.MergedJobsDic = mergedJobsDic;
            //                                    object retVal = telcobrightJob.Execute(cdrJobInputData); //Execute as merged job**
            //                                    telcobrightJob.PostprocessJob(retVal);
            //                                    cmd.ExecuteCommandText(" commit; ");
            //                                    closeDbConnection(cmd);
            //                                    resetMergeJobStatus();
            //                                }
            //                            }
            //                        }
            //                        else throw new Exception("Job must be processed as single or in batch (merged).");
            //                    }
            //                    catch (Exception e)
            //                    {
            //                        if (e.Message.Contains("OutOfMemoryException"))
            //                        {
            //                            Console.WriteLine("WARNING!!!!!!!! MANUAL GARBAGE COLLECTION AND COMPACTION OF LOH.");
            //                            GarbageCollectionHelper.CompactGCNowForOnce();
            //                        }
            //                        try
            //                        {
            //                            List<CdrMergedJobError> mergedJobErrors = new List<CdrMergedJobError>();
            //                            foreach (var customError in e.Data.Values)
            //                            {
            //                                if(customError.GetType()==typeof(CdrMergedJobError))
            //                                    mergedJobErrors.Add((CdrMergedJobError)customError);
            //                            }
            //                            resetMergeJobStatus();
            //                            if (cmd.Connection.State != ConnectionState.Open) cmd.Connection.Open();
            //                            cmd.ExecuteCommandText(" rollback; ");
            //                            bool cacheLimitExceeded =
            //                                RateCacheCleaner.CheckAndClearRateCache(mediationContext, e);
            //                            if (cacheLimitExceeded) continue;
            //                            cacheLimitExceeded = RateCacheCleaner.ClearTempRateTable(e, cmd);
            //                            if (cacheLimitExceeded) continue;
            //                            PrintErrorMessageToConsole(ne, job, e);
            //                            ErrorWriter.WriteError(e, "ProcessCdr", job,
            //                                "CdrJob processing error.", tbc.Telcobrightpartner.CustomerName, context);
            //                            try
            //                            {
            //                                if (!mergedJobErrors.Any())
            //                                {
            //                                    UpdateJobWithErrorInfo(cmd, job, e);
            //                                }
            //                                else
            //                                {
            //                                    foreach (var mergedJobError in mergedJobErrors)
            //                                    {
            //                                        UpdateJobWithErrorInfo(cmd, mergedJobError.Job, e);
            //                                        closeDbConnection(cmd);
            //                                    }
            //                                }
            //                                closeDbConnection(cmd);
            //                            }
            //                            catch (Exception e2)
            //                            {
            //                                if (e2.Message.Contains("OutOfMemoryException"))
            //                                {
            //                                    Console.WriteLine("WARNING!!!!!!!! MANUAL GARBAGE COLLECTION AND COMPACTION OF LOH.");
            //                                    GarbageCollectionHelper.CompactGCNowForOnce();
            //                                }
            //                                resetMergeJobStatus();
            //                                closeDbConnection(cmd);
            //                                ErrorWriter.WriteError(e2, "ProcessCdr", job,
            //                                    "Exception within catch block.",
            //                                    tbc.Telcobrightpartner.CustomerName, context);
            //                                GarbageCollectionHelper.CompactGCNowForOnce();
            //                                continue;
            //                            }
            //                            continue; //with next cdr or job
            //                        }
            //                        catch (Exception e3)
            //                        {
            //                            if (e3.Message.Contains("OutOfMemoryException"))
            //                            {
            //                                Console.WriteLine("WARNING!!!!!!!! MANUAL GARBAGE COLLECTION AND COMPACTION OF LOH.");
            //                                GarbageCollectionHelper.CompactGCNowForOnce();
            //                            }
            //                            resetMergeJobStatus();
            //                            try
            //                            {
            //                                context.Database.Connection
            //                                    .Close(); ///////////reaching here would be database problem
            //                                continue;
            //                            }
            //                            catch (Exception exception)
            //                            {
            //                                if (exception.Message.Contains("OutOfMemoryException"))
            //                                {
            //                                    Console.WriteLine("WARNING!!!!!!!! MANUAL GARBAGE COLLECTION AND COMPACTION OF LOH.");
            //                                    GarbageCollectionHelper.CompactGCNowForOnce();
            //                                }
            //                                context.Database.Connection.Dispose();
            //                                Console.WriteLine(exception);
            //                                continue;
            //                            }
            //                        }
            //                    } //end catch
            //                } //for each job
            //                if (headJobForMerge != null) //mergedjob row count didn't hit maxvalue, process them when 
            //                {
            //                    //there are no more jobs
            //                    cdrJobInputData.MergedJobsDic = mergedJobsDic;
            //                    object retVal = telcobrightJob.Execute(cdrJobInputData); //process as merged job************************
            //                    telcobrightJob.PostprocessJob(retVal);
            //                    cmd.ExecuteCommandText(" commit; ");
            //                    closeDbConnection(cmd);
            //                    resetMergeJobStatus();
            //                    //GarbageCollectionHelper.CompactGCNowForOnce();
            //                }
            //            } //using mysql command
            //        } //try for each NE

            //        catch (Exception e1)
            //        {
            //            if (e1.Message.Contains("OutOfMemoryException"))
            //            {
            //                Console.WriteLine("WARNING!!!!!!!! MANUAL GARBAGE COLLECTION AND COMPACTION OF LOH.");
            //                GarbageCollectionHelper.CompactGCNowForOnce();
            //            }
            //            Console.WriteLine(e1);
            //            ErrorWriter.WriteError(e1, "ProcessCdr", null, "NE:" + ne.idSwitch,
            //                tbc.Telcobrightpartner.CustomerName, context);
            //            continue; //with next switch
            //        }
            //    } //for each NE
            //} //try
            //catch (Exception e1)
            //{
            //    if (e1.Message.Contains("OutOfMemoryException"))
            //    {
            //        Console.WriteLine("WARNING!!!!!!!! MANUAL GARBAGE COLLECTION AND COMPACTION OF LOH.");
            //        GarbageCollectionHelper.CompactGCNowForOnce();
            //    }
            //    Console.WriteLine(e1);
            //    ErrorWriter.WriteError(e1, "ProcessCdr", null, "", operatorName, context);
            //}
        }

        private static void checkIfProcessingIntendedForThisServer(TelcobrightConfig tbc, ne ne)
        {
            string vaultName = ne.SourceFileLocations;
            //Vault vault = tbc.DirectorySettings.Vaults.First(c => c.Name == vaultName);
            FileLocation fileLocation = tbc.DirectorySettings.FileLocations[vaultName];
            string vaultPath = fileLocation.StartingPath;
            if (!Directory.Exists(vaultPath))
                throw new Exception($"Vault path '{vaultPath}' not found, may be processing is not intended to be running from this server.");
        }

        private static void closeDbConnection(DbCommand cmd)
        {
            if (cmd.Connection.State != ConnectionState.Closed) cmd.Connection.Close();
        }

        private static void PrintErrorMessageToConsole(ne ne, job telcobrightJob, Exception e)
        {
            Console.WriteLine("xxxErrorxxx Processing CdrJob for Switch:" +
                              ne.SwitchName + ", JobName:" + telcobrightJob.JobName);
            Console.WriteLine(e.Message);
        }

        private static void UpdateJobWithErrorInfo(DbCommand cmd, job telcobrightJob, Exception e)
        {
            List<CdrMergedJobError> mergedJobErrors = new List<CdrMergedJobError>();
            foreach (var jobError in e.Data.Values)
            {
                if(jobError.GetType()!= typeof(CdrMergedJobError))
                    continue;
                var errorWithoutJob = (CdrMergedJobError)jobError;
                errorWithoutJob.Job = null;//to avoid some circult reference during serialization
                mergedJobErrors.Add(errorWithoutJob);
            }
            string errorDetailAsTxt = JsonConvert.SerializeObject(mergedJobErrors).Replace("'", "");
            string customError="";
            if (e.Data.Contains("customError"))
                customError = (string)e.Data["customError"];
            long jobid = -1;
            if (e.Data.Contains("jobId"))
            {
                jobid= (long) e.Data["jobId"];
            }
            else
            {
                jobid = telcobrightJob.id;
            }
            cmd.CommandText = $" update job set jobadditionalinfo='{customError}', `Error`= '" +
                              e.Message.Replace("'", "") + errorDetailAsTxt +
                              Environment.NewLine + (e.InnerException?.ToString().Replace("'", "") ?? "")
                              + "' " + " where id=" + jobid + ";commit;";
            cmd.ExecuteNonQuery();
        }

        bool CheckIncompleteExists(PartnerEntities context, MediationContext mediationContext)
        {
            List<int> idJobDefs = context.enumjobdefinitions.Where(c => c.JobQueue == this.ProcessId).Select(c => c.id)
                .ToList();
            return context.jobs.Any(c => c.CompletionTime == null && idJobDefs.Contains(c.idjobdefinition));
        }

        bool CheckIncompleteExists(PartnerEntities context, MediationContext mediationContext, ne ne)
        {
            List<int> idJobDefs = context.enumjobdefinitions.Where(c => c.JobQueue == this.ProcessId).Select(c => c.id)
                .ToList();
            return context.jobs.Any(c => c.CompletionTime == null && idJobDefs.Contains(c.idjobdefinition)
                                         && c.idNE == ne.idSwitch);
        }

        public MefJobContainer ComposeMefJobData()
        {
            MefJobContainer mefJobData = new MefJobContainer();
            mefJobData.CmpJob.Compose();
            foreach (ITelcobrightJob ext in mefJobData.CmpJob.Jobs)
            {
                mefJobData.DicExtensions.Add(ext.Id.ToString(), ext);
            }
            return mefJobData;
        }

        public MefDecoderContainer ComposeMefDecoders(PartnerEntities context)
        {
            MefDecoderContainer mefDecoders = new MefDecoderContainer(context);
            mefDecoders.CmpDecoder.Compose();
            foreach (var ext in mefDecoders.CmpDecoder.Decoders)
                mefDecoders.DicExtensions.Add(ext.Id, ext);
            return mefDecoders;
        }

        public List<job> GetReProcessJobs(PartnerEntities contextTb, ne thisSwitch, int? decodingSpan)
        {
            var jobs = contextTb.jobs //jobs other than newcdr
                .Where(c => c.CompletionTime == null
                            && c.idNE == thisSwitch.idSwitch
                            && c.idjobdefinition != 1)
                .Include(c => c.ne.enumcdrformat)
                .Include(c => c.ne.telcobrightpartner)
                .Where(c => c.enumjobdefinition.JobQueue == this.ProcessId)
                .OrderBy(c => c.priority)
                .Take(Convert.ToInt32(decodingSpan)).ToList();
            return jobs;
        }

        public List<job> GetNewCdrJobs(TelcobrightConfig tbc, PartnerEntities contextTb, ne thisSwitch,
            int? decodingSpan,
            NeAdditionalSetting neAdditionalSetting)
        {
            List<job> jobs = null;
            var preDecodeAsTextFile = neAdditionalSetting != null && neAdditionalSetting.PreDecodeAsTextFile;
            int jobStatusToFetch = preDecodeAsTextFile == true
                ? 2 //status 2=prepared
                : 7; //status 7=downloaded
            if (tbc.CdrSetting.DescendingOrderWhileProcessingListedFiles == true)
            {
                jobs = contextTb.jobs
                    .Where(c => c.CompletionTime == null
                                && c.idNE == thisSwitch.idSwitch
                                && c.Status == jobStatusToFetch && c.idjobdefinition == 1) //downloaded & new cdr
                    .Include(c => c.ne.enumcdrformat)
                    .Include(c => c.ne.telcobrightpartner)
                    .OrderByDescending(c => c.JobName)
                    .Take(Convert.ToInt32(decodingSpan)).ToList();
            }
            else
            {
                jobs = contextTb.jobs
                    .Where(c => c.CompletionTime == null
                                && c.idNE == thisSwitch.idSwitch
                                && c.Status == jobStatusToFetch && c.idjobdefinition == 1) //downloaded & new cdr
                    .Include(c => c.ne.enumcdrformat)
                    .Include(c => c.ne.telcobrightpartner)
                    .OrderBy(c => c.JobName)
                    .Take(Convert.ToInt32(decodingSpan)).ToList();
            }
            return jobs;
        }
    }
}