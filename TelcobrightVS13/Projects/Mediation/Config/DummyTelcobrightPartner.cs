using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;

namespace TelcobrightMediation.Config
{
    public static class DummyTelcobrightPartner
    {
        public static telcobrightpartner getDummyTelcobrightPartner()
        {
            return new telcobrightpartner
            {
                idCustomer = 0,
                CustomerName = "Dummy",
                idOperatorType = 4,
                databasename = "dummy",
                NativeTimeZone = 3251,
                IgwPrefix = null,
                RateDictionaryMaxRecords = 3000000,
                MinMSForIntlOut = 100,
                RawCdrKeepDurationDays = 180,
                SummaryKeepDurationDays = 730,
                AutoDeleteOldData = 1,
                AutoDeleteStartHour = 4,
                AutoDeleteEndHour = 6
            };
        }
    }
}
