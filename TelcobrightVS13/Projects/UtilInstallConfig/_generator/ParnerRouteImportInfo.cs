namespace InstallConfig
{
    internal class ParnerRouteImportInfo
    {
        public string PartnerName { get; set; }
        public int IdPartner { get; set; }
        public int SwitchId{get; set; }
        public string DomesticTGs { get; set; }
        public string InternationalTGs { get; set; }
        //public string Description { get; set; }//can't use description as route naes are commaseparated in excel row
        public int Status { get; set; }
    }
}