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
using TelcobrightMediation.Config;

namespace Process
{
    [DisallowConcurrentExecution]
    [Export("TelcobrightProcess", typeof(AbstractTelcobrightProcess))]
    public class AccountingHelper : AbstractTelcobrightProcess
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public override string RuleName => this.GetType().ToString();
        public override string HelpText => "Perform invoice generation & other accounting automation tasks";
        public override int ProcessId => 117;
        public override void Execute(IJobExecutionContext schedulerContext)
        {
            string operatorName = schedulerContext.JobDetail.JobDataMap.GetString("operatorName");
            TelcobrightConfig tbc = ConfigFactory.GetConfigFromSchedulerExecutionContext(
                schedulerContext, operatorName);
            string entityConStr = ConnectionManager.GetEntityConnectionStringByOperator(operatorName, tbc);
            PartnerEntities context = new PartnerEntities(entityConStr);
            try
            {
                context.Database.Connection.Open();
                var mediationContext = new MediationContext(tbc, context);
                while (CheckIncomplete(context, mediationContext))
                {
                    List<job> incompleteJobs = context.jobs //jobs other than newcdr
                        .Where(c => c.CompletionTime == null && c.idjobdefinition == 1)
                        .Where(c => c.enumjobdefinition.JobQueue == this.ProcessId)
                        .OrderBy(c => c.priority).Take(Convert.ToInt32(100)).ToList();
                    using (DbCommand cmd = context.Database.Connection.CreateCommand())
                    {
                        foreach (job telcobrightJob in incompleteJobs)
                        {
                            Console.WriteLine("Processing Accounting Job, JobName: " + telcobrightJob.JobName);
                            try
                            {
                                cmd.ExecuteCommandText("set autocommit=0;");
                                ITelcobrightJob iJob = null;
                                mediationContext.MefJobContainer.DicExtensionsIdJobWise.TryGetValue(
                                    telcobrightJob.idjobdefinition.ToString(), out iJob);
                                if (iJob == null)
                                    throw new Exception("JobRule not found in MEF collection.");
                                var cdrJobInputData =
                                    new CdrJobInputData(mediationContext, context, null, telcobrightJob);
                                iJob.Execute(cdrJobInputData); //EXECUTE
                                cmd.ExecuteCommandText(" commit; ");
                            }
                            catch (Exception e)
                            {
                                try
                                {
                                    Console.WriteLine("xxxErrorxxx Processing Accounting Job:" +
                                                      " JobName:" + telcobrightJob.JobName);
                                    Console.WriteLine(e.Message);
                                    cmd.ExecuteCommandText(" rollback; ");
                                    ErrorWriter.WriteError(e, "AccountingHelper", telcobrightJob,
                                        "Accounting job error.", tbc.Telcobrightpartner.CustomerName, context);
                                    try
                                    {
                                        cmd.CommandText = " update job set `Error`= '" +
                                                          e.Message.Replace("'", "") +
                                                          Environment.NewLine +
                                                          (e.InnerException?.ToString().Replace("'", "") ??
                                                           "")
                                                          + "' " +
                                                          " where id=" + telcobrightJob.id;
                                        cmd.ExecuteNonQuery();
                                    }
                                    catch (Exception e2)
                                    {
                                        ErrorWriter.WriteError(e2, "Accounting Helper", telcobrightJob,
                                            "Exception within catch block.", tbc.Telcobrightpartner.CustomerName,
                                            context);
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
                } //while
            } //try
            catch (Exception e1)
            {
                Console.WriteLine(e1);
                ErrorWriter.WriteError(e1, "AccountingHelper", null, "", operatorName,context);
            }
        }

        bool CheckIncomplete(PartnerEntities context, MediationContext mediationContext)
        {
            List<int> idJobDefs = context.enumjobdefinitions.Where(c => c.JobQueue == this.ProcessId).Select(c => c.id)
                .ToList();
            return context.jobs.Any(c => c.CompletionTime == null && idJobDefs.Contains(c.idjobdefinition));
        }
    }
}


