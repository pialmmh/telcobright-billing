using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions.ConfigHelper;

namespace QuartzTelcobright.PropertyGen
{
    public class QuartzPropGenRemoteSchedulerAdoJobStore : AbstractQuartzPropertyGenerator
    {
        public DatabaseSetting DatabaseSetting { get;}
        private int TcpPortNumber { get; set; }

        public QuartzPropGenRemoteSchedulerAdoJobStore(int tcpPortNumber,DatabaseSetting databaseSetting)
        {
            this.TcpPortNumber = tcpPortNumber;
            this.DatabaseSetting = databaseSetting;
        }

        protected override NameValueCollection GenerateSchedulerProperties()
        {
            string connectionString = GetConnectionStringForJobStore();
            return QuartzPropertyTemplate.GetAdoJobStoreTemplate(this.TcpPortNumber, connectionString);

        }

        private string GetConnectionStringForJobStore()
        {
            //connection strings are not available at this moment in app.conf
            return $@"server = {this.DatabaseSetting.ServerName}; 
                    User Id = {this.DatabaseSetting.WriteUserNameForApplication}; 
                    password = {this.DatabaseSetting.WritePasswordForApplication}; Persist Security Info = True;
                    default command timeout = 3600; 
                    database = {this.DatabaseSetting.DatabaseName}";

        }
    }
}
