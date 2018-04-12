using System.Collections.Generic;

namespace MediationModel
{
    public class RouteWiseCdrsCollection
    {
        public route Route { get; set; }
        public List<cdr> Cdrs { get;}
        public RouteWiseCdrsCollection(route pRoute)
        {
            this.Cdrs = new List<cdr>();
            this.Route = pRoute;
        }
        public string GetIdentifier()
        {
            return this.Route.idroute.ToString();
        }
    }
}