using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using QuartzTelcobright;
using QuartzTelcobright.PropertyGen;
using TelcobrightMediation;

namespace InstallConfig
{
    public class QuartzWriter
    {
        private TelcobrightConfig tbc;

        public QuartzWriter(TelcobrightConfig tbc)
        {
            this.tbc = tbc;
        }

        public void writeQuartzJobStore()
        {
            //reset job store
            Console.WriteLine($"Reset QuartzJob Store for {tbc.Telcobrightpartner.databasename} (Y/N)? this will clear all job data.");
            ConsoleKeyInfo keyInfo = Console.ReadKey();
            if (keyInfo.KeyChar == 'Y' || keyInfo.KeyChar == 'y')
            {
                ConfigureQuartzJobStore(this.tbc); //configure job store for all opeartors
                Console.WriteLine();
                Console.WriteLine("Job store has been reset successfully.");
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Job store was not reset.");
            }
        }
        void ConfigureQuartzJobStore(TelcobrightConfig tbc)
        {
            DbUtil.CreateOrOverwriteQuartzTables(tbc.DatabaseSetting);
            //read quartz config part for ALL configured operator (mef)
            int tcpPortNumber = tbc.TcpPortNoForRemoteScheduler;
            QuartzPropGenRemoteSchedulerAdoJobStore quartzPropGenRemoteSchedulerAdoJobStore =
                new QuartzPropGenRemoteSchedulerAdoJobStore(tcpPortNumber: tcpPortNumber);
            quartzPropGenRemoteSchedulerAdoJobStore.DatabaseSetting = tbc.DatabaseSetting;
            QuartzPropertyFactory quartzPropertyFactory =
                new QuartzPropertyFactory(quartzPropGenRemoteSchedulerAdoJobStore);
            NameValueCollection schedulerProperties = quartzPropertyFactory.GetProperties();
            IScheduler scheduler = QuartzSchedulerFactory.CreateSchedulerInstance(schedulerProperties);
            QuartzTelcobrightManager quartzManager = new QuartzTelcobrightManager(scheduler);
            quartzManager.ClearJobs(); //reset job store
            quartzManager.CreateJobs<QuartzTelcobrightProcessWrapper>(tbc.SchedulerDaemonConfigs);
        }
    }
}
