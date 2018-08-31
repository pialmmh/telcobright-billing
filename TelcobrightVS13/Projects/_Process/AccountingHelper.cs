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
    [Export("TelcobrightProcess", typeof(ITelcobrightProcess))]
    public class AccountingHelper : ITelcobrightProcess
    {
        public override string ToString()
        {
            return this.RuleName;
        }

        public string RuleName => this.GetType().ToString();
        public string HelpText => "Perform invoice generation & other accounting automation tasks";
        public int ProcessId => 117;
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
                                        ErrorWriter wr = new ErrorWriter(e, "AccountingHelper", telcobrightJob,
                                            "Accounting job error.", tbc.DatabaseSetting.DatabaseName);
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
                                            ErrorWriter wr2 = new ErrorWriter(e2, "Accounting Helper", telcobrightJob,
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
                        } //using mysql command
                    } //while
                }
            } //try
            catch (Exception e1)
            {
                Console.WriteLine(e1);
                ErrorWriter wr = new ErrorWriter(e1, "AccountingHelper", null, "", operatorName);
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


