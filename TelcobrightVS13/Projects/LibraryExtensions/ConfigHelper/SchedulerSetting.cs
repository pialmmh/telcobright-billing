using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryExtensions.ConfigHelper
{
    public class SchedulerSetting
    {
        public string SchedulerType { get; }
        public DatabaseSetting DatabaseSetting { get; }

        public SchedulerSetting(string schedulerType, DatabaseSetting databaseSetting)
        {
            this.SchedulerType = schedulerType;
            this.DatabaseSetting = databaseSetting;
        }
    }
}
