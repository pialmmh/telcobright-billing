using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using LibraryExtensions;
using MediationModel;
using Quartz;
using QuartzTelcobright;
using TelcobrightFileOperations;
using TelcobrightMediation;
using TelcobrightMediation.Config;

//not system namespace, telcobright's namespace

namespace Process
{
    [DisallowConcurrentExecution]
    [Export("TelcobrightProcess", typeof(AbstractTelcobrightProcess))]
    public class DownloadMarker : AbstractTelcobrightProcess
    {
        public override string ToString()
        {
            return this.RuleName;
        }

        public override string RuleName => "ProcessFileLister";

        public override string HelpText =>
            "Sync one source local or remote directory to multiple local or remote directories";

        public override int ProcessId => 114;

        public class FileSyncPair
        {
            public FileLocation SrcLocation { get; set; }
            public FileLocation DstLocation { get; set; }
        }

        public override void Execute(IJobExecutionContext schedulerContext)
        {
            var processes = System.Diagnostics.Process.GetProcesses().ToList();
            foreach (var process in processes.Where(p => p.ProcessName.ToLower().Contains("werfault")))
            {
                process.Kill();
            }
            JobDataMap jobDataMap = schedulerContext.JobDetail.JobDataMap;
            string operatorName = schedulerContext.JobDetail.JobDataMap.GetString("operatorName");
            string maxMarkingForDownlaod = schedulerContext.JobDetail.JobDataMap.GetString("maxMarkingForDownlaod");
            TelcobrightConfig tbc = ConfigFactory.GetConfigFromSchedulerExecutionContext(
                schedulerContext, operatorName);
            string entityConStr = ConnectionManager.GetEntityConnectionStringByOperator(operatorName, tbc);
            PartnerEntities context = new PartnerEntities(entityConStr);

            try
            {
                var connection = context.Database.Connection;
                connection.Open();
                int max = Convert.ToInt32(maxMarkingForDownlaod);

                int alreadyMarkedJobCount = getMarkedJobCount(context);
                if (alreadyMarkedJobCount > 0)
                {
                    max = (max - alreadyMarkedJobCount);
                }

                if (max <= 0)
                {
                    return;
                }
                List<long> jobnamesNeedToMark;
                if (tbc.CdrSetting.DescendingOrderWhileListingFiles)
                {
                    jobnamesNeedToMark = context
                        .Database
                        .SqlQuery<long>(
                            $@"select id from job 
                    where idjobdefinition=6 
                    and status=6
                    and creationtime < NOW() - INTERVAL 10 MINUTE
                    order by right(jobname,27) desc limit 0,{max}").ToList();
                }
                else
                {
                    jobnamesNeedToMark = context
                        .Database
                        .SqlQuery<long>(
                            $@"select id from job 
                    where idjobdefinition=6 
                    and status=6
                    and creationtime < NOW() - INTERVAL 10 MINUTE
                    order by right(jobname,27) limit 0,{max}").ToList();
                }

                jobnamesNeedToMark.ForEach(i => context
                    .Database
                    .ExecuteSqlCommand(
                        $@"update job set status=4
                    where idjobdefinition=6 
                    and status=6 and id={i}"));

            }
            catch (Exception e1)
            {
                Console.WriteLine(e1);
                ErrorWriter.WriteError(e1, "FileLister", null, "", operatorName, context);
            }
        }

        private static int getMarkedJobCount(PartnerEntities context)
        {
                int count= context.Database.SqlQuery<int>(
                    $@"select count(*) from job 
                    where idjobdefinition=6 
                    and status=4").ToList().First();
            return count ;
        }
    }
}

