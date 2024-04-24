using TelcobrightMediation;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;
using Quartz;
using QuartzTelcobright;
using TelcobrightFileOperations;
using TelcobrightInfra.PerformanceAndOptimization;
using TelcobrightMediation.Config;

namespace Process
{

    [DisallowConcurrentExecution]
    [Export("TelcobrightProcess", typeof(AbstractTelcobrightProcess))]
    public class ProcessOptimizer : AbstractTelcobrightProcess
    {
        public override string ToString()
        {
            return this.RuleName;
        }

        public override string RuleName => "ProcessOptimizer";
        public override string HelpText => "Peforms clean up by deleting file or data";
        public override int ProcessId => 107;

        public override void Execute(IJobExecutionContext schedulerContext)
        {
            string operatorName = schedulerContext.JobDetail.JobDataMap.GetString("operatorName");
            TelcobrightConfig tbc = ConfigFactory.GetConfigFromSchedulerExecutionContext(
                schedulerContext, operatorName);
            string entityConStr = ConnectionManager.GetEntityConnectionStringByOperator(operatorName, tbc);
            PartnerEntities context = new PartnerEntities(entityConStr);
            try
            {
                if (tbc.CdrSetting.isTableDelete == true)
                {
                    TableOptimizer optimize = TableOptimizer.creatDeleteTableObject();
                    optimize.deleteTables(context);
                }
                MefJobContainer jobData = new MefJobContainer();
                jobData.CmpJob.Compose();
                foreach (ITelcobrightJob ext in jobData.CmpJob.Jobs)
                {
                    jobData.DicExtensions.Add(ext.Id.ToString(), ext);
                }

                var con = context.Database.Connection;
                if (con.State != ConnectionState.Open) con.Open();
                List<job> incompleteJobs= new List<job>();
                if (tbc.CdrSetting.DescendingOrderWhileProcessingListedFiles == true)
                {
                    incompleteJobs = context.Database
                        .SqlQuery<job>($@"select * from job where idjobdefinition = 8 and status !=1 
                                order by JobName desc limit 0,10000;").ToList();
                    //incompleteJobs = context.jobs.Where(j=>j.Status!=1 && j.idjobdefinition==8)
                    //    .OrderByDescending(j => j.JobName).Take(10000).ToList();
                }
                else
                {
                    incompleteJobs = context.Database
                        .SqlQuery<job>($@"select * from job where idjobdefinition = 8 and status !=1 
                                order by JobName  limit 0,10000;").ToList();
                    //incompleteJobs = context.jobs.Where(j => j.Status != 1 && j.idjobdefinition == 8)
                    //    .OrderBy(j => j.JobName).Take(10000).ToList();
                }
                string sql = "";
                using (DbCommand cmd = ConnectionManager.CreateCommandFromDbContext(context))
                {
                    foreach (job thisJob in incompleteJobs)
                    {
                        //GarbageCollectionHelper.CompactGCNowForOnce();
                        try
                        {
                            sql = "set autocommit=0;";
                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();
                            //dont use a console.out here because it doesn't not guarantee that the job is being executed
                            //e.g. when a delete job has pre-requisite, it will actually be not executed, showing "executing"
                            //in console could be misleading.
                            ITelcobrightJob iJob = null;
                            jobData.DicExtensions.TryGetValue(thisJob.idjobdefinition.ToString(), out iJob);
                            if (iJob != null)
                            {
                                OptimizerJobInputData optimizerJobInputData =
                                    new OptimizerJobInputData(tbc, thisJob);

                                JobCompletionStatus jobStatus =
                                    (JobCompletionStatus) iJob.Execute(optimizerJobInputData);
                                if (jobStatus == JobCompletionStatus.Complete)
                                {
                                    sql = " update job set CompletionTime='" +
                                          DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'," +
                                          " Status=1, " +
                                          " Error='' " +
                                          " where id=" + thisJob.id;
                                    cmd.CommandText = sql;
                                    cmd.ExecuteNonQuery();

                                    sql = " commit; ";
                                    cmd.CommandText = sql;
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        catch (Exception e1)
                        {
                            Console.WriteLine(e1);
                            sql = " rollback; ";
                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();

                            cmd.CommandText =
                                "set autocommit=1;"; //must change back immediately, even if rollback occurs
                            cmd.ExecuteNonQuery();

                            ErrorWriter.WriteError(e1, "Optimizer", thisJob, "", operatorName, context);
                            //also save the error information within the job
                            //use try catch in case DB is not accesible
                            try
                            {
                                sql = " update job set `Error`= '" +
                                      e1.Message.Replace("'", "") + Environment.NewLine +
                                      (e1.InnerException != null
                                          ? e1.InnerException.ToString().Replace("'", "")
                                          : "")
                                      + "' " +
                                      " where id=" + thisJob.id;
                                cmd.CommandText = sql;
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception e2)
                            {
                                ErrorWriter.WriteError(e2, "Optimizer", thisJob, "", operatorName, context);
                            }
                            continue; //with next cdr or job
                        } //end catch
                    }
                }
            } //try
            catch (Exception e1)
            {
                Console.WriteLine(e1);
                ErrorWriter.WriteError(e1, "Optimizer", null, "", operatorName, context);
            }
        }


    }
}
