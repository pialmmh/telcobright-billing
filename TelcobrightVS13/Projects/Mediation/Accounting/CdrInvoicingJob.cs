using System.Collections.Generic;
using MediationModel;

namespace TelcobrightMediation.Accounting
{
    public class CdrInvoicingJob : ISegmentedJob
    {
        public int ActualStepsCount { get; }
        private List<acc_transaction> billableTransactions { get; }=new List<acc_transaction>();
        private CdrJobInputData CdrjobInputData { get; set; }  
        private MediationContext MediationContext => this.CdrjobInputData.MediationContext;
        private CdrSetting CdrSetting => this.MediationContext.Tbc.CdrSetting;

        public CdrInvoicingJob(int actualStepsCount)
        {
            this.ActualStepsCount = actualStepsCount;
        }
        public void Execute()
        {
            
        }

        
    }
}
