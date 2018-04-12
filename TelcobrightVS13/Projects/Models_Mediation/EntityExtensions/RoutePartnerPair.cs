namespace MediationModel
{
    public class RoutePartnerPair
    {
        public int SwitchId { get; set; }
        public string RouteName { get; set; }
        
        public string MRouteCarrierCombined;
        public string PartnerName { get; set; }
        public string SupplierPrefix { get; set; }
        public string RouteCarrierCombined => "[" + this.PartnerName + ":" + this.SupplierPrefix + ", Route: " + this.SwitchId + "-" + this.RouteName + "]";

        public RoutePartnerPair(int switchid, string routename, string partnername, string matchedPrefixSupplier)
        {
            this.SwitchId = switchid;
            this.RouteName = routename;
            this.PartnerName = partnername;
            this.SupplierPrefix = matchedPrefixSupplier;
        }
    }
}