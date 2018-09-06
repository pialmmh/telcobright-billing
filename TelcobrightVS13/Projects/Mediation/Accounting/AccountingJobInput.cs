using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;

namespace TelcobrightMediation.Accounting
{
    public class AccountingJobInputData : ITelcobrightJobInput
    {
        public TelcobrightConfig Tbc { get; }
        public PartnerEntities Context { get; }
        public job TelcobrightJob { get; }
        public AccountingJobInputData(TelcobrightConfig tbc, PartnerEntities context, job telcobrightJob)
        {
            this.Tbc = tbc;
            this.Context = context;
            this.TelcobrightJob = telcobrightJob;
        }
    }
}
