using System.Collections.Generic;
using System.Text;
using LibraryExtensions;
using Newtonsoft.Json;

namespace MediationModel
{
    public partial class lcr
    {
        public LcrExpression LcrExpression { get; set; }
        public double CustomerRate { get; set; }
        public lcr()
        {
            this.LcrExpression = new LcrExpression(this);
        }

        public string GetExtendedInsertSql()
        {
            if (this.LcrCurrent != "")
                return new StringBuilder().Append("(")
                    .Append(this.Prefix.EncloseWith("'")).Append(",")
                    .Append(this.idrateplan.ToString()).Append(",")
                    .Append(this.startdate.ToString("yyyy-MM-dd HH:mm:ss").EncloseWith("'")).Append(",")
                    .Append(this.LcrCurrent.EncloseWith("'")).Append(")").ToString();
            else
                return "";
        }

        public LcrDisplayClass GetDisplayClass(Dictionary<string, route> dicRoutes, Dictionary<string, string> dicPrefixDescription)
        {
            Dictionary<string, List<RouteWithSupplierPrefix>> dicCostEntities = null;//string=cost,each int is the idRoute in route table
            dicCostEntities = JsonConvert.DeserializeObject<Dictionary<string, List<RouteWithSupplierPrefix>>>(this.LcrCurrent);
            var dicRouteCostEntities = GetDicRouteCostPair(dicRoutes, dicCostEntities);

            string prefixDescription = null;
            dicPrefixDescription.TryGetValue(this.Prefix, out prefixDescription);
            return new LcrDisplayClass(this, prefixDescription, dicRouteCostEntities);
        }
        private Dictionary<string, List<RoutePartnerPair>> GetDicRouteCostPair(Dictionary<string, route> dicRoutes,
            Dictionary<string, List<RouteWithSupplierPrefix>> dicCostEntities)
        {
            double cost = 0;
            var dicDisplayEntities = new Dictionary<string, List<RoutePartnerPair>>();
            foreach (var kv in dicCostEntities)
            {
                double.TryParse(kv.Key, out cost);
                var lstRouteCarrier = new List<RoutePartnerPair>();
                dicDisplayEntities.Add(kv.Key, lstRouteCarrier);
                foreach (var rwp in kv.Value)
                {
                    route thisRoute = null;
                    dicRoutes.TryGetValue(rwp.IdRoute.ToString(), out thisRoute);
                    if (thisRoute != null)
                        lstRouteCarrier.Add(new RoutePartnerPair(thisRoute.SwitchId, thisRoute.RouteName, thisRoute.partner.PartnerName, rwp.MatchedPrefixSupplier));
                    else//if route not found
                        lstRouteCarrier.Add(new RoutePartnerPair(-1, "<not found>", "<not found>", ""));
                }
            }
            return dicDisplayEntities;
        }

    }
}