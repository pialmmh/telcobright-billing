using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;

namespace TelcobrightMediation.Accounting
{
    public class AccBalanceThresholdAction
    {
        public int ThresholdNumber { get; }
        public AccBalanceThresholdAction(int thresholdNumber)
        {
            this.ThresholdNumber = thresholdNumber;
        }

        public void CreateAutomationJobForReachingThreshold(Action methodToCreateJob)
        {
            methodToCreateJob();//and this job will be executed by a telcobright process
        }
    }
}
