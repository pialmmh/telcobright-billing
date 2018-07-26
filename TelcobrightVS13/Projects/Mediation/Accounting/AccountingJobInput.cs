using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;

namespace TelcobrightMediation.Accounting
{
    public class AccountingJobInputData : ITelcobrightJobInput
    {
        public TelcobrightConfig Tbc { get; }
        public CdrSetting CdrSetting => this.Tbc.CdrSetting;
        private AccountingContext AccountingContext { get; }
        public PartnerEntities Context { get; }
        public ne Ne { get; }
        public job TelcobrightJob { get; }
        public List<DateTime> DatesInvolved { get; }
        public AccountingJobInputData(TelcobrightConfig tbc, PartnerEntities context, job telcobrightJob,
            List<DateTime> datesInvolved)
        {
            this.Tbc = tbc;
            this.Context = context;
            this.TelcobrightJob = telcobrightJob;
            this.DatesInvolved = datesInvolved;
            this.AccountingContext = new AccountingContext(this.Context, 0, null, datesInvolved,
                CdrSetting.SegmentSizeForDbWrite);
        }
    }
}
