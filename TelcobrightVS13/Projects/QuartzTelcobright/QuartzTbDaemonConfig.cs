using System.Collections.Generic;
using LibraryExtensions.ConfigHelper;
using Quartz;

namespace TelcobrightMediation.Scheduler.Quartz
{
    public class QuartzTbDaemonConfig
    {
        public string Identity { get; }
        public string Group { get;}
        public string OperatorName { get; }
        public bool FireOnceIfMissFired { get;}//becuase quartz.net only allows to fire once after missfire
        public JobDataMap JobDataMap { get; }
        public string CronExpression { get;}
        public QuartzTbDaemonConfig()//default constructor required for json deserializing
        {
            this.JobDataMap = new JobDataMap();
        }
        //constructor
        public QuartzTbDaemonConfig(string operatorName, 
            string identity, string @group, 
            bool fireOnceIfMissFired, string cronExpression,Dictionary<string,string> jobDataMap)
        {//taking a dictionary for jobDataMap allows more readable constructor using object initializer syntax
            this.OperatorName = operatorName;
            this.Identity = identity;//databasename indicates telcobrightPartnername
            this.Group = @group;
            this.FireOnceIfMissFired = fireOnceIfMissFired;
            this.CronExpression = cronExpression;
            this.JobDataMap = new JobDataMap();
            foreach (var kv in jobDataMap)
            {
                this.JobDataMap.Put(kv.Key, kv.Value);
                //put identity,group in jobDataMap along with telcobrightProcessId, which is already in the collection during declaration in util install config
                this.JobDataMap.Put("identity", this.Identity);
                this.JobDataMap.Put("group", this.Group);
            }
        }
    }
}