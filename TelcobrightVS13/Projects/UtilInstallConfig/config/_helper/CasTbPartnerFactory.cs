using MediationModel;

namespace InstallConfig.config._helper
{
    public class CasTbPartnerFactory
    {
        public static telcobrightpartner GetTemplatePartner(int idCustomer, string customerName,string databaseName)
        {
            return new telcobrightpartner
            {
                idCustomer = idCustomer,
                CustomerName = customerName,
                idOperatorType = 2,
                databasename = databaseName,
                databasetype = "",
                user = null,
                pass = null,
                ServerNameOrIP = null,
                IBServerNameOrIP = null,
                IBdatabasename = null,
                IBdatabasetype = null,
                IBuser = null,
                IBpass = null,
                TransactionSizeForCDRLoading = null,
                NativeTimeZone = 3251,
                IgwPrefix = null,
                RateDictionaryMaxRecords = 3000000,
                MinMSForIntlOut = 100,
                RawCdrKeepDurationDays = 90,
                SummaryKeepDurationDays = 730,
                AutoDeleteOldData = 1,
                AutoDeleteStartHour = 4,
                AutoDeleteEndHour = 6
            };
        }
    }
}