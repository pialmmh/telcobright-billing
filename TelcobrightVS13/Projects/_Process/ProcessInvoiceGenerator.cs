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
    [Export("TelcobrightProcess", typeof(AbstractTelcobrightProcess))]
    public class ProcessInvoiceGenerator : AbstractTelcobrightProcess
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public override string RuleName => this.GetType().ToString();
        public override string HelpText => "Invoice Generator";
        public override int ProcessId => 108;
        public override void Execute(IJobExecutionContext schedulerContext)
        {
            string operatorName = schedulerContext.JobDetail.JobDataMap.GetString("operatorName");
            try
            {
                TelcobrightConfig tbc = ConfigFactory.GetConfigFromSchedulerExecutionContext(
                    schedulerContext, operatorName);
                string entityConStr = ConnectionManager.GetEntityConnectionStringByOperator(operatorName,tbc);
                MefJobComposer mefJobComposer = new MefJobComposer();
                mefJobComposer.Compose();
                ITelcobrightJob mefInvoicingJob = mefJobComposer.Jobs.Single(c => c.RuleName == "MefCdrInvoicingJob");
                using (PartnerEntities context = new PartnerEntities(entityConStr))
                {
                    context.Database.Connection.Open();
                    List<int> idJobDefs = context.enumjobdefinitions.Where(c => c.JobQueue == this.ProcessId)
                        .Select(c => c.id).ToList();
                    while (CheckIncomplete(context))
                    {
                        tbc.GetPathIndependentApplicationDirectory();
                        List<job> incompleteJobs = context.jobs //jobs other than newcdr
                            .Where(c => c.Status!=1 && c.CompletionTime == null && idJobDefs.Contains(c.idjobdefinition))
                            .OrderBy(c => c.priority).ToList();
                        using (DbCommand cmd = context.Database.Connection.CreateCommand())
                        {
                            foreach (job telcobrightJob in incompleteJobs)
                            {
                                try
                                {
                                    cmd.ExecuteCommandText("set autocommit=0;");
                                    InvoiceGenerationInputData invoiceGenerationInputData =
                                        new InvoiceGenerationInputData(tbc, context, telcobrightJob,
                                            GetInvoiceGenerationConfigs(tbc), ComposeInvoiceGenerationRules(),
                                            ComposeServiceGroups(), ComposeInvoiceSectionGenerators());

                                    mefInvoicingJob.Execute(invoiceGenerationInputData); //EXECUTE

                                    cmd.CommandText = $"update job set CompletionTime={DateTime.Now.ToMySqlField()}, " +
                                                      $"status=1, NoOfSteps=1," +
                                                      $"progress=1," +
                                                      $"Error=null where id={telcobrightJob.id}";
                                    cmd.ExecuteNonQuery();
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
                                            ErrorWriter wr2 = new ErrorWriter(e2, "ProcessInvoiceGenerator",
                                                telcobrightJob,
                                                "Exception within catch block.",
                                                tbc.Telcobrightpartner.CustomerName);
                                        }
                                        continue; //with next cdr or job
                                    }
                                    catch (Exception)
                                    {
                                        //reaching here would be database problem
                                        context.Database.Connection.Close();
                                    }
                                }
                            } //for each job
                        } //using mysql command
                    } //while
                }
            } //try
            catch (Exception e1)
            {
                Console.WriteLine(e1);
                ErrorWriter wr = new ErrorWriter(e1, "ProcessInvoiceGenerator", null, "", operatorName);
            }
        }

        private static Dictionary<int,InvoiceGenerationConfig> GetInvoiceGenerationConfigs(TelcobrightConfig tbc)
        {
            return tbc.CdrSetting.ServiceGroupConfigurations
                .ToDictionary(kv => kv.Key, kv => kv.Value.InvoiceGenerationConfig);
        }

        private Dictionary<int, IServiceGroup> ComposeServiceGroups()
        {
            ServiceGroupComposer serviceGroupComposer = new ServiceGroupComposer();
            serviceGroupComposer.Compose();
            return serviceGroupComposer.ServiceGroups.ToDictionary(c => c.Id);
        }
        private Dictionary<string,IInvoiceGenerationRule> ComposeInvoiceGenerationRules()
        {
            InvoiceGenerationRuleComposer invoiceGenerationRuleComposer =
                                                    new InvoiceGenerationRuleComposer();
            invoiceGenerationRuleComposer.Compose();
            return invoiceGenerationRuleComposer.InvoiceGenerationRules.ToDictionary(c => c.RuleName);
        }
        private Dictionary<string, IInvoiceSectionGenerator> ComposeInvoiceSectionGenerators()
        {
            InvoiceSectionGeneratorComposer composer = new InvoiceSectionGeneratorComposer();
            composer.Compose();
            return composer.InvoiceSectionGenerators.ToDictionary(c => c.RuleName);
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


