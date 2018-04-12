using System;
using System.Collections.Generic;

namespace MediationModel
{
    public partial class LcrJobData
    {
        public int RefRatePlan { get; set; }
        public DateTime StartDate { get; set; }
        public List<string> LstPrefix { get; set; }
        //public LCREffectiveTimeWisePrefixes LcrTimeAndPrefix { get; set; }
        public LcrJobData(int refRatePlan)
        {
            this.RefRatePlan = refRatePlan;
            this.LstPrefix = new List<string>();
        }
    }
}