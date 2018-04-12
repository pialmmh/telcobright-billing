namespace MediationModel
{
    public class RouteVsCost
    {

        public route Route { get; set; }
        public double Cost { get; set; }
        public string SupplierPrefix { get; set; }

        public RouteVsCost(route pRoute, double cost, string SupplierPrefix)
        {
            this.Route = pRoute;
            this.Cost = cost;
            this.SupplierPrefix = SupplierPrefix;
        }
        public string GetIdentifier()
        {
            return this.Route.idroute.ToString();
        }
    }
}