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
    public class QuartzPropGenRemoteSchedulerAdoRuntime:AbstractQuartzPropertyGenerator
    {
        private string SchedulerDatabaseName { get; set; }
        private int TcpPortNumber { get; set; }
        public QuartzPropGenRemoteSchedulerAdoRuntime(int tcpPortNumber)
        {
            this.TcpPortNumber = tcpPortNumber;
        }
        protected override NameValueCollection GenerateSchedulerProperties()
        {
            string connectionString = GetConnectionStringToRunScheduler(this.SchedulerDatabaseName);
            return QuartzPropertyTemplate.GetAdoJobStoreTemplate(this.TcpPortNumber, connectionString);
        }
        private static string GetConnectionStringToRunScheduler(string schedulerDatabaseName)
        {
            //connection strings are available in app.conf
            return ConfigurationManager.ConnectionStrings["Scheduler"]
                .ConnectionString.Replace("#DatabaseName#", schedulerDatabaseName);
        }
    }
}
