using System;
using System.Collections.Generic;

namespace PortalApp._portalHelper
{
    public class ServiceAcountStatus
    {
        public int PartnerID { get; set; }
        public string PartnerName { get; set; }
        public string Service { get; set; }
        public string PaymentMode { get; set; }
        public string Currency { get; set; }
        public Double CurrentBalance { get; set; }
        public Double MaxCreditLimit { get; set; }
        public Double LastCreditedAmount { get; set; }
        public string Date { get; set; }
        public string LastAmountType{ get; set; }
        public List<ServiceAcountStatus> popultateGrid()
        {

            List<ServiceAcountStatus> lstPaymentSrvice = new List<ServiceAcountStatus>()
        {
            new ServiceAcountStatus() {PartnerID=1,PartnerName="Onetel",Service="Voice",Currency="USD", PaymentMode="Prepaid",CurrentBalance=500,MaxCreditLimit=0,LastCreditedAmount=1000,Date="2017-11-13" ,LastAmountType="Topup" },
             new ServiceAcountStatus() {PartnerID=2,PartnerName="Carrier-2",Service="Voice",Currency="USD",PaymentMode="Prepaid",CurrentBalance=-1000,MaxCreditLimit=1000,LastCreditedAmount=1000,Date="2017-11-13",LastAmountType="Credit" },
              new ServiceAcountStatus() {PartnerID=3,PartnerName="GP",Service="Domestic",Currency="USD",PaymentMode="Postpaid",CurrentBalance=-10000,MaxCreditLimit=99999,LastCreditedAmount=6000,Date="-",LastAmountType="Invoice" }
    };


            return lstPaymentSrvice;
        }
    }

   
}