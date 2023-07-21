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
        //private string SchedulerDatabaseName { get; set; }
        private int TcpPortNumber { get; set; }
        public string ConStr { get; }

        public QuartzPropGenRemoteSchedulerAdoRuntime(int tcpPortNumber,
            string conStr)
        {
            this.TcpPortNumber = tcpPortNumber;
            this.ConStr = conStr;
            //this.SchedulerDatabaseName = schedulerDatabaseName;
        }
        protected override NameValueCollection GenerateSchedulerProperties()
        {
            string connectionString =this.ConStr;
            return QuartzPropertyTemplate.GetAdoJobStoreTemplate(this.TcpPortNumber, connectionString);
        }
    }
}
