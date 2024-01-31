using System;

namespace PortalApp._myCodes
{
    public class TopUpInfo
    {
        public int PartnerID { get; set; }
        
        public string Date { get; set; }
        public string Type { get; set; }
        public Double Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentReference { get; set; }
        public string Comment { get; set; }

    }
}