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
using System.IO;
using LibraryExtensions;
using TelcobrightMediation.Config;
using LibraryExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TelcobrightMediation.Cdr;

namespace UnitTesterManual
{
    public class MockNewCdrProcessor
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => "ProcessCDR";
        public string HelpText => "Processes CDR";
        public int ProcessId => 103;
        public string OperatorName { get; set; }
        public IEventCollector EventCollector { get; set; }
        public bool DecodeAndDumpOnly { get; set; } = false;
        public bool ProcessInReverseOrder{ get; set; }
        public MockNewCdrProcessor(string operatorName, IEventCollector eventCollector)
        {
            this.OperatorName = operatorName;
            this.EventCollector = eventCollector;
        }

        public void ExecuteJobsWithTests(ITelcobrightJob job,CdrJobInputData cdrJobInputData)
        {
            job.Execute(cdrJobInputData);
        }
        public void Execute()
        {
            if (this.OperatorName == string.Empty) throw new NoNullAllowedException("OperatorName must be set.");
            try
            {
                string configFileName = new DirectoryInfo(FileAndPathHelper.GetBinPath()).Parent.Parent.Parent.FullName +
                                             Path.DirectorySeparatorChar + "WS_Topshelf_Quartz" +
                                             Path.DirectorySeparatorChar +
                                             "bin" + Path.DirectorySeparatorChar + "config" +
                                             Path.DirectorySeparatorChar + $@"{this.OperatorName}.conf";
                TelcobrightConfig tbc = ConfigFactory.GetConfigFromFile(configFileName);
                string entityConStr = ConnectionManager.GetEntityConnectionStringByOperator(this.OperatorName);
                using (PartnerEntities context = new PartnerEntities(entityConStr))
                {
                    context.Database.Connection.Open();
                    var mediationContext = new MediationContext(tbc, context);
                    tbc.GetPathIndependentApplicationDirectory();
                    ne ne = context.nes.Include(n => n.telcobrightpartner)
                        .Where(n => n.telcobrightpartner.databasename == tbc.DatabaseSetting.DatabaseName &&
                                    (n.SkipCdrDecoded == null || n.SkipCdrDecoded != 1)).ToList().FirstOrDefault();
                    if (ne == null)
                    {
                        Console.WriteLine("No Ne found configured for CdrProcessing, exiting...");
                        return;
                    }
                    List<job> incompleteJobs = context.jobs
                        .Where(c => c.idjobdefinition == 1 && c.Status == 7 && c.CompletionTime == null
                                    && c.idNE == ne.idSwitch).ToList();
                    if (this.ProcessInReverseOrder)
                        incompleteJobs = incompleteJobs.OrderByDescending(c => c.JobName).ToList();
                    using (DbCommand cmd = context.Database.Connection.CreateCommand())
                    {
                        foreach (job telcobrightJob in incompleteJobs)
                        {
                            string jobPurposeIndicator = this.DecodeAndDumpOnly == false
                                ? "Processing CdrJob"
                                : "Decoding & Dumping raw cdr";
                            Console.WriteLine(
                                $"{jobPurposeIndicator} for Switch:{ne.SwitchName}, JobName:{telcobrightJob.JobName}");
                            cmd.ExecuteCommandText("set autocommit=0;"); //transaction started
                            try
                            {
                                ITelcobrightJob iJob = null;
                                if (this.DecodeAndDumpOnly == false)
                                {
                                    iJob = new MockNewCdrFileJob(this.EventCollector, this.OperatorName);
                                }
                                var cdrJobInputData =
                                    new CdrJobInputData(mediationContext, context, ne, telcobrightJob);
                                iJob.Execute(cdrJobInputData); //execute job, this includes commit if successful,
                                cmd.CommandText = " commit; ";
                                cmd.ExecuteNonQuery();
                            } 
                            catch (Exception e)
                            {
                                try
                                {
                                    bool rateCacheSizeExceeded =
                                        HandleOufOfMemoryExceptionForRateCache(mediationContext, e);
                                    if (rateCacheSizeExceeded) continue;
                                    Console.WriteLine("xxxErrorxxx Processing CdrJob for Switch:" +
                                                      ne.SwitchName + ", JobName:" +
                                                      telcobrightJob.JobName);
                                    Console.WriteLine(e.Message);
                                    context.Database.Connection.CreateCommand().ExecuteCommandText(" rollback; ");
                                    ErrorWriter wr = new ErrorWriter(e, "ProcessCdr", telcobrightJob,
                                        "CdrJob processing error.", tbc.DatabaseSetting.DatabaseName);

                                    //also save the error information within the job
                                    //use try catch in case DB is not accesible
                                    try
                                    {
                                        context.Database.Connection.CreateCommand().CommandText =
                                            " update job set `Error`= '" +
                                            e.Message.Replace("'", "") +
                                            Environment.NewLine +
                                            (e.InnerException?.ToString().Replace("'", "") ??
                                             "") + "' " + " where id=" + telcobrightJob.id;
                                        context.Database.Connection.CreateCommand().ExecuteNonQuery();
                                    }
                                    catch (Exception e2)
                                    {
                                        ErrorWriter wr2 = new ErrorWriter(e2, "ProcessCdr", telcobrightJob,
                                            "Exception within catch block.", tbc.DatabaseSetting.DatabaseName);
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
                    }
                }
            } //try
            catch (Exception e1)
            {
                Console.WriteLine(e1);
                ErrorWriter wr = new ErrorWriter(e1, "ProcessCdr", null, "", "mockNewCdrProcessor");
            }
        }

        bool HandleOufOfMemoryExceptionForRateCache(MediationContext mediationContext, Exception e)
        {
            bool rateCacheSizeExceeded = false;
            try
            {
                if (e.Message.Contains("OutOfMemory")) //ratecache too big and exceeds c#'s limit
                {
                    mediationContext.MefServiceFamilyContainer.RateCache
                        .ClearRateCache(); //involves GC as well to freeup memory instantly
                    rateCacheSizeExceeded = true;
                }
            }
            catch (Exception e1)
            {
                Console.WriteLine(e);
                throw;
            }
            return rateCacheSizeExceeded;
        }
        bool CheckIncomplete(PartnerEntities context, MediationContext mediationContext)
        {
            List<int> idJobDefs = context.enumjobdefinitions.Where(c => c.JobQueue == this.ProcessId).Select(c => c.id).ToList();
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
                                && c.Status == 7 && c.idjobdefinition == 1) //downloaded & new cdr
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
                                && c.Status == 7 && c.idjobdefinition == 1) //downloaded & new cdr
                    .Include(c => c.ne.enumcdrformat)
                    .Include(c => c.ne.telcobrightpartner)
                    .OrderBy(c => c.JobName)
                    .Take(Convert.ToInt32(decodingSpan)).ToList();
            }
            return jobs;
        }
    }





}


