using System.Collections.Generic;
using System.Linq;
using MediationModel;

namespace TelcobrightMediation
{
    public class JobPreRequisite
    {
        public List<long> ExecuteAfterJobs { get; set; }//these idjobs have to be completed first
        public JobPreRequisite()
        {
            this.ExecuteAfterJobs = new List<long>();
        }
        public bool CheckComplete(PartnerEntities context)
        {
            return !context.jobs.Where(c => this.ExecuteAfterJobs.Contains(c.id)).Any(c => c.Status != 1);
        }
    }
}