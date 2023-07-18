using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PortalApp._myCodes
{
    public class PaymentHistoryInfo
    {
        public int PartnerID { get; set; }
        public string PartnerName { get; set; }
        public string Service { get; set; }
        public string Date { get; set; }
        public Double PaymentAmount { get; set; }
        public string Type { get; set; }
        public Double BalanceBefore { get; set; }
        public Double BalanceAfter { get; set; }


        public List<PaymentHistoryInfo> populatePaymentHistory()
        {

            List<PaymentHistoryInfo> lstPaymentDetails = new List<PaymentHistoryInfo>() {
            new PaymentHistoryInfo(){PartnerID = 1, PartnerName = "Onetel", Service = "Voice",Date = "2017-11-13",PaymentAmount = 500, Type = "Topup",BalanceBefore=0,BalanceAfter=500 },
            new PaymentHistoryInfo(){PartnerID=2,PartnerName="Carrier-2",Service="Voice",Date = "2017-11-13",PaymentAmount = 500, Type = "Topup",BalanceBefore=1000,BalanceAfter=150 },
            new PaymentHistoryInfo(){ PartnerID = 3,PartnerName = "GP",Service = "Domestic",Date = "2017-11-13",PaymentAmount = 200, Type = "Topup",BalanceBefore=100,BalanceAfter=300}
    };
            return lstPaymentDetails;
        }

    }
}