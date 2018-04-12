using Quartz;

namespace CrystalQuarts.Samples.Common
{
    public class QuartzTelcobrightProcessInstance
    {
        public string Identity { get; set; }
        public string Group { get; set; }
        public bool FireOnceIfMissFired { get; set; }//because quartz.net only allows to fire once after missfire
        public JobDataMap JobDataMap { get; set; }
        public string CronExpression { get; set; }
        public QuartzTelcobrightProcessInstance()
        {
            this.JobDataMap = new JobDataMap();
        }
    }
}