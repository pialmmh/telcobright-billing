using TelcobrightMediation;
using MySql.Data.MySqlClient;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using TelcobrightFileOperations;
using System.Reflection;
using System.Threading.Tasks;
using MediationModel;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using Quartz;
using LibraryExtensions;
using QuartzTelcobright;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;

namespace Process
{
    [Export("TelcobrightProcess", typeof(AbstractTelcobrightProcess))]
    public class CdrJobProcessor : AbstractTelcobrightProcess
    {
        public override string ToString()
        {
            return this.RuleName;
        }

        public override string RuleName => this.GetType().ToString();
        public override string HelpText => "Processes CDR";
        public override int ProcessId => 103;

        public override void Execute(IJobExecutionContext schedulerContext)
        {
            string operatorName = schedulerContext.JobDetail.JobDataMap.GetString("operatorName");
            TelcobrightConfig tbc = ConfigFactory.GetConfigFromSchedulerExecutionContext(
                schedulerContext, operatorName);
            CdrSetting cdrSetting = tbc.CdrSetting;
            string entityConStr = ConnectionManager.GetEntityConnectionStringByOperator(operatorName, tbc);
            PartnerEntities context = new PartnerEntities(entityConStr);

            try
            {

                context.Database.Connection.Open();
                var mediationContext = new MediationContext(tbc, context);
                tbc.GetPathIndependentApplicationDirectory();
                foreach (ne ne in mediationContext.Nes.Values)
                {
                    NeAdditionalSetting neAdditionalSetting = null;
                    cdrSetting.NeWiseAdditionalSettings.TryGetValue(ne.idSwitch, out neAdditionalSetting);
                    if (ne.UseIdCallAsBillId == 1 && ne.FilterDuplicateCdr == 1)
                    {
                        throw new Exception("Idcall cannot be used as uniquebillid when duplicate filtering is on.");
                    }
                    int minRowCountForBatchProcessing = neAdditionalSetting?.MinRowCountToStartBatchCdrProcessing ?? 1;
                    Dictionary<long, NewCdrWrappedJobForMerge> mergedJobsDic =
                        new Dictionary<long, NewCdrWrappedJobForMerge>(); //key=idJob
                    NewCdrWrappedJobForMerge headJobForMerge = null;
                    int rowCountSoFarForMerge = 0;
                    Action resetMergeJobStatus = () =>
                    {
                        mergedJobsDic = new Dictionary<long, NewCdrWrappedJobForMerge>(); //key=idJob
                        headJobForMerge = null;
                        rowCountSoFarForMerge = 0;
                    };
                    resetMergeJobStatus();
                    try
                    {
                        if (ne.SkipCdrDecoded == 1 || CheckIncompleteExists(context, mediationContext, ne) == false)
                            continue;
                        List<job> incompleteJobs = GetReProcessJobs(context, ne, ne.DecodingSpanCount);
                        incompleteJobs.AddRange(GetNewCdrJobs(tbc, context, ne, ne.DecodingSpanCount,
                            neAdditionalSetting)); //combine
                        //jobs with error to be processed as single job and add them to the first of the list, adding them to the last is a bit difficult to manage merge processing
                        List<job> jobsWithError = incompleteJobs.Where(j => !j.Error.IsNullOrEmptyOrWhiteSpace())
                            .ToList();
                        incompleteJobs = incompleteJobs.Where(ij => !jobsWithError.Select(ej => ej.id).Contains(ij.id))
                            .ToList();//without jobs with error, 
                        incompleteJobs = jobsWithError.Union(incompleteJobs).ToList();
                        CdrJobInputData cdrJobInputData = null;
                        ITelcobrightJob telcobrightJob = null;
                        using (DbCommand cmd = context.Database.Connection.CreateCommand())
                        {
                            foreach (job job in incompleteJobs
                            ) //for each job********************************************
                            {
                                Console.WriteLine("Processing CdrJob for Switch:" + ne.SwitchName + ", JobName:" +
                                                  job.JobName);
                                try
                                {
                                    closeDbConnection(
                                        cmd); //connection will be opened inside newcdr file job after creating daywise tables as table creation ddl asks for transaction restart
                                    mediationContext.MefJobContainer.DicExtensionsIdJobWise.TryGetValue(
                                        job.idjobdefinition.ToString(), out telcobrightJob);
                                    if (telcobrightJob == null)
                                        throw new Exception("JobRule not found in MEF collection.");
                                    cdrJobInputData =
                                        new CdrJobInputData(mediationContext, context, ne, job);
                                    if (job.idjobdefinition != 1
                                    ) //error process or re-process job, not merging, process as a single job*************
                                    {
                                        object retVal=telcobrightJob.Execute(cdrJobInputData); //EXECUTE
                                        telcobrightJob.PostprocessJob(retVal);
                                        cmd.ExecuteCommandText(" commit; ");
                                        closeDbConnection(cmd);
                                        continue;
                                    }
                                    if (neAdditionalSetting == null ||
                                        neAdditionalSetting?.ProcessMultipleCdrFilesInBatch == false
                                    ) //new cdr job, not merging, process as single job
                                    {
                                        object retVal=telcobrightJob.Execute(cdrJobInputData); //EXECUTE
                                        telcobrightJob.PostprocessJob(retVal);
                                        cmd.ExecuteCommandText(" commit; ");
                                        closeDbConnection(cmd);
                                        continue;
                                    }
                                    if (!job.Error.IsNullOrEmptyOrWhiteSpace()) //jobs with error, process as single job
                                    {
                                        object retVal = telcobrightJob.Execute(cdrJobInputData); //EXECUTE
                                        telcobrightJob.PostprocessJob(retVal);
                                        cmd.ExecuteCommandText(" commit; ");
                                        closeDbConnection(cmd);
                                        continue;
                                    }
                                    if (neAdditionalSetting?.ProcessMultipleCdrFilesInBatch == true
                                    ) //merge new cdr jobs for batch processing
                                    {
                                        var inputForPreprocess = new Dictionary<string, object>
                                        {
                                            {"cdrJobInputData", cdrJobInputData}
                                        };
                                        NewCdrPreProcessor preProcessor =
                                            (NewCdrPreProcessor) telcobrightJob.PreprocessJob(inputForPreprocess); //execute pre-processing
                                        if (headJobForMerge == null &&
                                            preProcessor.TxtCdrRows.Count >=
                                            minRowCountForBatchProcessing) //already large job, process as single
                                        {//but if merge in progress, do not process as single job, headjob for merge!=null means mergeInProgress
                                            object retVal= telcobrightJob.Execute(cdrJobInputData); //not merging, process as single job************
                                            telcobrightJob.PostprocessJob(retVal);
                                            cmd.ExecuteCommandText(" commit; ");
                                            closeDbConnection(cmd);
                                            continue;
                                        }
                                        //merge job for batch processing*******************
                                        NewCdrWrappedJobForMerge newWrappedJob =
                                            new NewCdrWrappedJobForMerge(job, preProcessor);
                                        if (headJobForMerge == null
                                        ) //empty list of merged job, add the first one (head)
                                        {
                                            headJobForMerge = newWrappedJob;
                                            mergedJobsDic.Add(headJobForMerge.TelcobrightJob.id, newWrappedJob);
                                            rowCountSoFarForMerge = newWrappedJob.OriginalRows.Count;
                                        }
                                        else //one of the tail jobs
                                        {
                                            mergedJobsDic.Add(newWrappedJob.TelcobrightJob.id, newWrappedJob);
                                            rowCountSoFarForMerge =
                                                headJobForMerge
                                                    .AppendTailJobRows(newWrappedJob); //apend head+new tail jobs rows
                                            if (rowCountSoFarForMerge >= minRowCountForBatchProcessing
                                            ) //enough jobs have been merged for batch processing 
                                            {
                                                cdrJobInputData.MergedJobsDic = mergedJobsDic;
                                                object retVal =telcobrightJob.Execute(cdrJobInputData); //Execute as merged job**
                                                telcobrightJob.PostprocessJob(retVal);
                                                cmd.ExecuteCommandText(" commit; ");
                                                closeDbConnection(cmd);
                                                resetMergeJobStatus();
                                            }
                                        }
                                    }
                                    else throw new Exception("Job must be processed as single or in batch (merged).");
                                }
                                catch (Exception e)
                                {
                                    try
                                    {
                                        List<CdrMergedJobError> mergedJobErrors = new List<CdrMergedJobError>();
                                        foreach (var mergedJobError in e.Data.Values)
                                        {
                                            mergedJobErrors.Add((CdrMergedJobError)mergedJobError);
                                        }
                                        resetMergeJobStatus();
                                        if (cmd.Connection.State != ConnectionState.Open) cmd.Connection.Open();
                                        cmd.ExecuteCommandText(" rollback; ");
                                        bool cacheLimitExceeded =
                                            RateCacheCleaner.CheckAndClearRateCache(mediationContext, e);
                                        if (cacheLimitExceeded) continue;
                                        cacheLimitExceeded = RateCacheCleaner.ClearTempRateTable(e, cmd);
                                        if (cacheLimitExceeded) continue;
                                        PrintErrorMessageToConsole(ne, job, e);
                                        ErrorWriter.WriteError(e, "ProcessCdr", job,
                                            "CdrJob processing error.", tbc.Telcobrightpartner.CustomerName, context);
                                        try
                                        {
                                            if (!mergedJobErrors.Any())
                                            {
                                                UpdateJobWithErrorInfo(cmd, job, e);
                                            }
                                            else
                                            {
                                                foreach (var mergedJobError in mergedJobErrors)
                                                {
                                                    UpdateJobWithErrorInfo(cmd, mergedJobError.Job, e);
                                                }
                                            }
                                            closeDbConnection(cmd);
                                        }
                                        catch (Exception e2)
                                        {
                                            resetMergeJobStatus();
                                            closeDbConnection(cmd);
                                            ErrorWriter.WriteError(e2, "ProcessCdr", job,
                                                "Exception within catch block.",
                                                tbc.Telcobrightpartner.CustomerName, context);
                                            continue;
                                        }
                                        continue; //with next cdr or job
                                    }
                                    catch (Exception)
                                    {
                                        resetMergeJobStatus();
                                        try
                                        {
                                            context.Database.Connection
                                                .Close(); ///////////reaching here would be database problem
                                            continue;
                                        }
                                        catch (Exception exception)
                                        {
                                            context.Database.Connection.Dispose();
                                            Console.WriteLine(exception);
                                            continue;
                                        }
                                    }
                                } //end catch
                            } //for each job
                            if (headJobForMerge != null) //mergedjob row count didn't hit maxvalue, process them when 
                            {
                                //there are no more jobs
                                cdrJobInputData.MergedJobsDic = mergedJobsDic;
                                object retVal= telcobrightJob.Execute(cdrJobInputData); //process as merged job************************
                                telcobrightJob.PostprocessJob(retVal);
                                cmd.ExecuteCommandText(" commit; ");
                                closeDbConnection(cmd);
                                resetMergeJobStatus();
                            }
                        } //using mysql command
                    } //try for each NE

                    catch (Exception e1)
                    {
                        Console.WriteLine(e1);
                        ErrorWriter.WriteError(e1, "ProcessCdr", null, "NE:" + ne.idSwitch,
                            tbc.Telcobrightpartner.CustomerName, context);
                        continue; //with next switch
                    }
                } //for each NE
            } //try
            catch (Exception e1)
            {
                Console.WriteLine(e1);
                ErrorWriter.WriteError(e1, "ProcessCdr", null, "", operatorName, context);
            }
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
            cmd.CommandText = " update job set `Error`= '" +
                              e.Message.Replace("'", "") +
                              Environment.NewLine + (e.InnerException?.ToString().Replace("'", "") ?? "")
                              + "' " + " where id=" + telcobrightJob.id + ";commit;";
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
                                && c.Status == jobStatusToFetch && c.idjobdefinition == 1
                                && c.JobState != "paused") //downloaded & new cdr
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
                                && c.Status == jobStatusToFetch && c.idjobdefinition == 1
                                && c.JobState != "paused") //downloaded & new cdr
                    .Include(c => c.ne.enumcdrformat)
                    .Include(c => c.ne.telcobrightpartner)
                    .OrderBy(c => c.JobName)
                    .Take(Convert.ToInt32(decodingSpan)).ToList();
            }
            return jobs;
        }
    }
}


