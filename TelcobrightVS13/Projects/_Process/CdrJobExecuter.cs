﻿using TelcobrightMediation;
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
    [Export("TelcobrightProcess", typeof(ITelcobrightProcess))]
    public class CdrJobExecuter : ITelcobrightProcess
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => this.GetType().ToString();
        public string HelpText => "Processes CDR";
        public int ProcessId => 103;
        public void Execute(IJobExecutionContext schedulerContext)
        {
            string operatorName = schedulerContext.JobDetail.JobDataMap.GetString("operatorName");
            try
            {
                TelcobrightConfig tbc = ConfigFactory.GetConfigFromSchedulerExecutionContext(
                    schedulerContext, operatorName);
                string entityConStr = ConnectionManager.GetEntityConnectionStringByOperator(operatorName);
                using (PartnerEntities context = new PartnerEntities(entityConStr))
                {
                    context.Database.Connection.Open();
                    var mediationContext = new MediationContext(tbc, context);
                    tbc.GetPathIndependentApplicationDirectory();
                    foreach (ne ne in mediationContext.Nes.Values)
                    {
                        try
                        {
                            if (ne.SkipCdrDecoded == 1 || CheckIncomplete(context, mediationContext, ne) == false)
                                continue;
                            List<job> incompleteJobs = GetReProcessJobs(context, ne, ne.DecodingSpanCount);
                            incompleteJobs.AddRange(GetNewCdrJobs(tbc, context, ne, ne.DecodingSpanCount)); //combine
                            using (DbCommand cmd = context.Database.Connection.CreateCommand())
                            {
                                foreach (job telcobrightJob in incompleteJobs)
                                {
                                    Console.WriteLine("Processing CdrJob for Switch:" + ne.SwitchName + ", JobName:" +
                                                      telcobrightJob.JobName);
                                    try
                                    {
                                        if (cmd.Connection.State != ConnectionState.Open) cmd.Connection.Open();
                                        cmd.ExecuteCommandText("set autocommit=0;");
                                        ITelcobrightJob iJob = null;
                                        mediationContext.MefJobContainer.DicExtensionsIdJobWise.TryGetValue(
                                            telcobrightJob.idjobdefinition.ToString(), out iJob);
                                        if (iJob == null)
                                            throw new Exception("JobRule not found in MEF collection.");
                                        var cdrJobInputData =
                                            new CdrJobInputData(mediationContext, context, ne, telcobrightJob);
                                        iJob.Execute(cdrJobInputData); //EXECUTE
                                        cmd.ExecuteCommandText(" commit; ");
                                    }
                                    catch (Exception e)
                                    {
                                        try
                                        {
                                            if (cmd.Connection.State != ConnectionState.Open) cmd.Connection.Open();
                                            cmd.ExecuteCommandText(" rollback; ");
                                            bool cacheLimitExceeded =
                                                RateCacheCleaner.CheckAndClearRateCache(mediationContext, e);
                                            if (cacheLimitExceeded) continue;
                                            cacheLimitExceeded = RateCacheCleaner.ClearTempRateTable(e, cmd);
                                            if (cacheLimitExceeded) continue;
                                            PrintErrorMessageToConsole(ne, telcobrightJob, e);
                                            ErrorWriter wr = new ErrorWriter(e, "ProcessCdr", telcobrightJob,
                                                "CdrJob processing error.", tbc.DatabaseSetting.DatabaseName);
                                            try
                                            {
                                                UpdateJobWithErrorInfo(cmd, telcobrightJob, e);
                                            }
                                            catch (Exception e2)
                                            {
                                                ErrorWriter wr2 = new ErrorWriter(e2, "ProcessCdr", telcobrightJob,
                                                    "Exception within catch block.",
                                                    tbc.DatabaseSetting.DatabaseName);
                                            }
                                            continue; //with next cdr or job
                                        }
                                        catch (Exception)
                                        {
                                            //reaching here would be database problem
                                            context.Database.Connection.Close();
                                        }
                                    } //end catch
                                } //for each job
                            } //using mysql command
                        } //try for each NE

                        catch (Exception e1)
                        {
                            Console.WriteLine(e1);
                            ErrorWriter wr = new ErrorWriter(e1, "ProcessCdr", null, "NE:" + ne.idSwitch,
                                tbc.DatabaseSetting.DatabaseName);
                            continue; //with next switch
                        }
                    } //for each NE
                }
            } //try
            catch (Exception e1)
            {
                Console.WriteLine(e1);
                ErrorWriter wr = new ErrorWriter(e1,"ProcessCdr",null,"",operatorName);
            }
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
                                Environment.NewLine +(e.InnerException?.ToString().Replace("'", "") ??"")
                                + "' " +" where id=" + telcobrightJob.id+";commit;";
            cmd.ExecuteNonQuery();
        }
        bool CheckIncomplete(PartnerEntities context, MediationContext mediationContext)
        {
            List<int> idJobDefs = context.enumjobdefinitions.Where(c => c.JobQueue == this.ProcessId).Select(c => c.id)
                .ToList();
            return context.jobs.Any(c => c.CompletionTime == null && idJobDefs.Contains(c.idjobdefinition));
        }

        bool CheckIncomplete(PartnerEntities context, MediationContext mediationContext, ne ne)
        {
            List<int> idJobDefs = context.enumjobdefinitions.Where(c => c.JobQueue == this.ProcessId).Select(c => c.id).ToList();
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
            var jobs = contextTb.jobs//jobs other than newcdr
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
        public List<job> GetNewCdrJobs(TelcobrightConfig tbc,PartnerEntities contextTb, ne thisSwitch, int? decodingSpan)
        {
            List<job> jobs = null;
            if (tbc.CdrSetting.DescendingOrderWhileProcessingListedFiles == true)
            {
                jobs=contextTb.jobs
                    .Where(c => c.CompletionTime == null
                                && c.idNE == thisSwitch.idSwitch
                                && c.Status == 7 && c.idjobdefinition == 1
                                && c.JobState != "paused") //downloaded & new cdr
                    .Include(c => c.ne.enumcdrformat)
                    .Include(c => c.ne.telcobrightpartner)
                    .OrderByDescending(c => c.JobName)
                    .Take(Convert.ToInt32(decodingSpan)).ToList();
            }
            else
            {
                jobs=contextTb.jobs
                    .Where(c => c.CompletionTime == null
                                && c.idNE == thisSwitch.idSwitch
                                && c.Status == 7 && c.idjobdefinition == 1
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


