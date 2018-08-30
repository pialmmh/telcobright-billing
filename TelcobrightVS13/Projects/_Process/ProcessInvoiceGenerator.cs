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
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;

namespace Process
{
    [Export("TelcobrightProcess", typeof(ITelcobrightProcess))]
    public class ProcessInvoiceGenerator : ITelcobrightProcess
    {
        public override string ToString()
        {
            return this.RuleName;
        }

        public string RuleName => this.GetType().ToString();
        public string HelpText => "Invoice Generator";
        public int ProcessId => 108;

        public void Execute(IJobExecutionContext schedulerContext)
        {
            string operatorName = schedulerContext.JobDetail.JobDataMap.GetString("operatorName");
            try
            {
                TelcobrightConfig tbc = ConfigFactory.GetConfigFromSchedulerExecutionContext(
                    schedulerContext, operatorName);
                string entityConStr = ConnectionManager.GetEntityConnectionStringByOperator(operatorName);
                MefJobComposer mefJobComposer=new MefJobComposer();
                mefJobComposer.Compose();
                ITelcobrightJob mefInvoicingJob = mefJobComposer.Jobs.Single(c => c.RuleName == "InvoicingJob");
                using (PartnerEntities context = new PartnerEntities(entityConStr))
                {
                    context.Database.Connection.Open();
                    List<int> idJobDefs = context.enumjobdefinitions.Where(c => c.JobQueue == this.ProcessId)
                        .Select(c => c.id).ToList();
                    while (CheckIncomplete(context))
                    {
                        tbc.GetPathIndependentApplicationDirectory();
                        try
                        {
                            List<job> incompleteJobs = context.jobs //jobs other than newcdr
                                .Where(c => c.CompletionTime == null && idJobDefs.Contains(c.idjobdefinition))
                                .OrderBy(c => c.priority).ToList();
                            using (DbCommand cmd = context.Database.Connection.CreateCommand())
                            {
                                foreach (job telcobrightJob in incompleteJobs)
                                {
                                    try
                                    {
                                        cmd.ExecuteCommandText("set autocommit=0;");
                                        InvoiceGenerationInputData invoiceGenerationInputData =
                                            new InvoiceGenerationInputData(tbc,context,telcobrightJob);
                                        mefInvoicingJob.Execute(invoiceGenerationInputData); //EXECUTE
                                        cmd.ExecuteCommandText(" commit; ");
                                    }
                                    catch (Exception e)
                                    {
                                        try
                                        {
                                            cmd.ExecuteCommandText(" rollback; ");
                                            try
                                            {
                                                UpdateJobWithErrorInfo(cmd, telcobrightJob, e);
                                            }
                                            catch (Exception e2)
                                            {
                                                ErrorWriter wr2 = new ErrorWriter(e2, "ProcessInvoiceGenerator", telcobrightJob,
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
                            ErrorWriter wr = new ErrorWriter(e1, "ProcessInvoiceGenerator", null,null,
                                tbc.DatabaseSetting.DatabaseName);
                            continue; //with next switch
                        }
                    } //while
                }
            } //try
            catch (Exception e1)
            {
                Console.WriteLine(e1);
                ErrorWriter wr = new ErrorWriter(e1, "ProcessInvoiceGenerator", null, "", operatorName);
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
                              Environment.NewLine + (e.InnerException?.ToString().Replace("'", "") ?? "")
                              + "' " + " where id=" + telcobrightJob.id;
            cmd.ExecuteNonQuery();
        }

        bool CheckIncomplete(PartnerEntities context, MediationContext mediationContext)
        {
            List<int> idJobDefs = context.enumjobdefinitions.Where(c => c.JobQueue == this.ProcessId).Select(c => c.id)
                .ToList();
            return context.jobs.Any(c => c.CompletionTime == null && idJobDefs.Contains(c.idjobdefinition));
        }

        bool CheckIncomplete(PartnerEntities context)
        {
            List<int> idJobDefs = context.enumjobdefinitions.Where(c => c.JobQueue == this.ProcessId).Select(c => c.id)
                .ToList();
            return context.jobs.Any(c => c.CompletionTime == null && idJobDefs.Contains(c.idjobdefinition));
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
               
    }
}


