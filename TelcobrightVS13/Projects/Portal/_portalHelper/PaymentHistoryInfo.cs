using System;
using System.Collections.Generic;

namespace PortalApp._portalHelper
{
    public class PaymentHistoryInfo
    {
        public long AccountId { get; set; }
        public int PartnerId { get; set; }
        public string PartnerName { get; set; }
        public string Service { get; set; }
        public string Date { get; set; }
        public decimal? PaymentAmount { get; set; }
        public string Type { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public string TransactionDetails { get; set; }
        public string Reference { get; set; }


        public List<PaymentHistoryInfo> populatePaymentHistory()
        {

            List<PaymentHistoryInfo> lstPaymentDetails = new List<PaymentHistoryInfo>() {
            new PaymentHistoryInfo(){PartnerId = 1, PartnerName = "Onetel", Service = "Voice",Date = "2017-11-13",PaymentAmount = 500, Type = "Topup",BalanceBefore=0,BalanceAfter=500 },
            new PaymentHistoryInfo(){PartnerId=2,PartnerName="Carrier-2",Service="Voice",Date = "2017-11-13",PaymentAmount = 500, Type = "Topup",BalanceBefore=1000,BalanceAfter=150 },
            new PaymentHistoryInfo(){ PartnerId = 3,PartnerName = "GP",Service = "Domestic",Date = "2017-11-13",PaymentAmount = 200, Type = "Topup",BalanceBefore=100,BalanceAfter=300}
    };
            return lstPaymentDetails;
        }

    }
}