using TelcobrightMediation;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TelcobrightFileOperations;
using System.Threading.Tasks;
using System.Reflection;
using MediationModel;
using Quartz;
using QuartzTelcobright;
using TelcobrightMediation.Config;

namespace Process
{

    [Export("TelcobrightProcess", typeof(ITelcobrightProcess))]
    public class ProcessFileCopy : ITelcobrightProcess
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => "ProcessFilecopy";
        public string HelpText => "Copies Files between SyncPairs";
        public int ProcessId => 104;
        public void Execute(IJobExecutionContext schedulerContext)
        {
            string operatorName = schedulerContext.JobDetail.JobDataMap.GetString("operatorName");
            TelcobrightConfig tbc = ConfigFactory.GetConfigFromSchedulerExecutionContext(
                schedulerContext, operatorName);
            JobDataMap jobDataMap = schedulerContext.JobDetail.JobDataMap;
            SyncPair syncPair = tbc.DirectorySettings.SyncPairs[jobDataMap.GetString("SyncPair")];
            if (syncPair.SkipCopyingToDestination == true) return;
            
            using (MySqlConnection conPartner = new MySqlConnection(ConfigurationManager.ConnectionStrings["Partner"].ConnectionString))
            {
                conPartner.Open();
                using (PartnerEntities contextTb = new PartnerEntities(conPartner, false))
                {
                    string jobRootDirectory = tbc.GetPathIndependentApplicationDirectory();
                    List<int> jobDefsForThisQueue =
                        contextTb.enumjobdefinitions.Where(c => c.JobQueue == this.ProcessId).Select(c => c.id).ToList();
                    bool incompleteExists =
                        contextTb.jobs.Any(c => c.Status != 1 && jobDefsForThisQueue.Contains(c.idjobdefinition)
                                                && c.JobParameter.StartsWith(
                                                    "{\"syncPairName\":\"" + syncPair.Name + "\""));
                    if (incompleteExists == false) return; //there is no job, just exit.
                    List<job> lstIncomplete = contextTb.jobs
                        .Where(c => c.Status != 1 && jobDefsForThisQueue.Contains(c.idjobdefinition)
                                    && c.JobParameter.StartsWith("{\"syncPairName\":\"" + syncPair.Name + "\""))
                        .ToList();
                    if (tbc.CdrSetting.DescendingOrderWhileListingFiles == true)
                    {
                        lstIncomplete = lstIncomplete.OrderBy(c => c.priority).ThenByDescending(c => c.JobName)
                            .ToList();
                    }
                    else lstIncomplete = lstIncomplete.OrderBy(c => c.priority).ToList();

                    ////****end debug
                    MefJobContainer jobData = new MefJobContainer();
                    jobData.CmpJob.Compose();
                    foreach (ITelcobrightJob ext in jobData.CmpJob.Jobs)
                    {
                        jobData.DicExtensions.Add(ext.Id.ToString(), ext);
                    }
                    string sql = "";
                    if (conPartner.State != ConnectionState.Open) conPartner.Open();
                    using (MySqlCommand cmd = new MySqlCommand("", conPartner))
                    {
                        foreach (job thisJob in lstIncomplete)
                        {
                            try
                            {
                                //this console out makes cdrjob processing ipossible
                                //Console.WriteLine("Processing Non-CDR job: " + ThisJob.JobName + ", type: " + ThisJob.idjobdefinition);
                                ITelcobrightJob iJob = null;
                                jobData.DicExtensions.TryGetValue(thisJob.idjobdefinition.ToString(), out iJob);
                                if (iJob != null)
                                {
                                    FileCopyJobInputData fileCopyJobInputData=new FileCopyJobInputData(tbc,thisJob);
                                    JobCompletionStatus jobStatus = iJob.Execute(fileCopyJobInputData);
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
                                try
                                {
                                    sql = " rollback; ";
                                    cmd.CommandText = sql;
                                    cmd.ExecuteNonQuery();

                                    ErrorWriter wr = new ErrorWriter(e1, "ProcessFileCopy", thisJob, "",operatorName);

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
                                        ErrorWriter wr2 = new ErrorWriter(e2, "ProcessFileCopy", null, "",operatorName);
                                    }
                                    continue; //with next cdr or job
                                }
                                catch (Exception e3)
                                {
                                    //reaching here would be database problem
                                    conPartner.Close();
                                }
                            } //end catch
                        }
                    }
                }
            }
        }

    }
}
