using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions.ConfigHelper;
using Quartz;
using QuartzTelcobright;
using QuartzTelcobright.PropertyGen;
using TelcobrightInfra;
using TelcobrightMediation;

namespace InstallConfig
{
    public class TelcoBillingQuartzWriter
    {
        private TelcobrightConfig tbc;

        public TelcoBillingQuartzWriter(TelcobrightConfig tbc)
        {
            this.tbc = tbc;
        }

        public void configureQuartzJobStore()
        {
            DatabaseSetting databaseSetting = tbc.DatabaseSetting;
            DbUtil.CreateOrOverwriteQuartzTables(databaseSetting);
            //read quartz config part for ALL configured operator (mef)
            int tcpPortNumber = tbc.TcpPortNoForRemoteScheduler;
            QuartzPropGenRemoteSchedulerAdoJobStore quartzPropGenRemoteSchedulerAdoJobStore =
                new QuartzPropGenRemoteSchedulerAdoJobStore(tcpPortNumber,databaseSetting);
            QuartzPropertyFactory quartzPropertyFactory =
                new QuartzPropertyFactory(quartzPropGenRemoteSchedulerAdoJobStore);
            NameValueCollection schedulerProperties = quartzPropertyFactory.GetProperties();
            IScheduler scheduler = null;
            var factory = new QuartzSchedulerFactory(schedulerProperties);
            scheduler = factory.CreateSchedulerInstance();
            QuartzTelcobrightManager quartzManager = new QuartzTelcobrightManager(scheduler);
            quartzManager.ClearJobs(); //reset job store
            quartzManager.CreateJobs<QuartzTelcobrightProcessWrapper>(tbc.SchedulerDaemonConfigs);
        }
    }
}
