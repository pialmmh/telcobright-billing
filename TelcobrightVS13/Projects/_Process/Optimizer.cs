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
using TelcobrightMediation.Config;

namespace Process
{

    [Export("TelcobrightProcess", typeof(ITelcobrightProcess))]
    public class ProcessOptimizer : ITelcobrightProcess
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => "ProcessOptimizer";
        public string HelpText => "Peforms clean up by deleting file or data";
        public int ProcessId => 107;

        public void Execute(IJobExecutionContext schedulerContext)
        {
            string operatorName = schedulerContext.JobDetail.JobDataMap.GetString("operatorName");
            try
            {
                TelcobrightConfig tbc = ConfigFactory.GetConfigFromSchedulerExecutionContext(
                    schedulerContext, operatorName);
                bool incompleteExists = true;
                MefJobContainer jobData = new MefJobContainer();
                jobData.CmpJob.Compose();
                foreach (ITelcobrightJob ext in jobData.CmpJob.Jobs)
                {
                    jobData.DicExtensions.Add(ext.Id.ToString(), ext);
                }
                while (incompleteExists)
                {
                    string entityConStr = ConnectionManager.GetEntityConnectionStringByOperator(operatorName);
                    using (PartnerEntities context = new PartnerEntities(entityConStr))
                    {
                        var con = context.Database.Connection;
                        if (con.State != ConnectionState.Open) con.Open();
                        List<int> jobDefsForThisQueue =
                            context.enumjobdefinitions.Where(c => c.JobQueue == this.ProcessId).Select(c => c.id)
                                .ToList();
                        incompleteExists =
                            context.jobs.Any(
                                c => c.Status != 1 && jobDefsForThisQueue.Contains(c.idjobdefinition));
                        if (incompleteExists == false) break; //there is no job, just exit.
                        List<job> lstIncomplete = context.jobs
                            .Where(c => c.Status != 1 && jobDefsForThisQueue.Contains(c.idjobdefinition)).ToList();
                        if (tbc.CdrSetting.DescendingOrderWhileListingFiles == true)
                        {
                            lstIncomplete = lstIncomplete.OrderBy(c => c.priority).ThenByDescending(c => c.JobName)
                                .ToList();
                        }
                        else lstIncomplete = lstIncomplete.OrderBy(c => c.priority).ToList();
                        //shuffle the list for almost equal distribution of delete files in different sources
                        lstIncomplete.Shuffle();

                        string sql = "";
                        using (DbCommand cmd = ConnectionManager.CreateCommandFromDbContext(context))
                        {
                            foreach (job thisJob in lstIncomplete)
                            {
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

                                        JobCompletionStatus jobStatus = iJob.Execute(optimizerJobInputData);
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


                                    ErrorWriter wr = new ErrorWriter(e1, "Optimizer", thisJob, "", operatorName);

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
                                        ErrorWriter wr2 =
                                            new ErrorWriter(e2, "Optimizer", thisJob, "", operatorName);
                                    }
                                    continue; //with next cdr or job
                                } //end catch
                            }
                        }
                    }
                } //while
            } //try
            catch (Exception e1)
            {
                Console.WriteLine(e1);
                ErrorWriter wr = new ErrorWriter(e1, "Optimizer", null, "", operatorName);
            }
        }


    }
}
