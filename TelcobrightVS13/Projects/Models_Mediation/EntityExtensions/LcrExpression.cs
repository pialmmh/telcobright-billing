using System.Collections.Generic;
using Newtonsoft.Json;

namespace MediationModel
{
    public class LcrExpression
    {
        private lcr _parentLcr;

        public Dictionary<string, List<RouteWithSupplierPrefix>> DicCostEntities { get; set; }//each int is the idRoute in route table
        public LcrExpression(lcr parentLcr)
        {
            this.DicCostEntities = new Dictionary<string, List<RouteWithSupplierPrefix>>();
            this._parentLcr = parentLcr;
        }
        public void AppendListOfRouteCost(List<RouteVsCost> lstRouteCost)
        {
            List<RouteWithSupplierPrefix> thisList = null;
            foreach (var rvc in lstRouteCost)
            {
                this.DicCostEntities.TryGetValue(rvc.Cost.ToString(), out thisList);
                if (thisList == null)
                {
                    thisList = new List<RouteWithSupplierPrefix>();
                    this.DicCostEntities.Add(rvc.Cost.ToString(), thisList);
                }
                //otherwise, routes for this cost already exists
                thisList.Add(new RouteWithSupplierPrefix() { IdRoute = rvc.Route.idroute, MatchedPrefixSupplier = rvc.SupplierPrefix });
                //ThisList.Add(rvc.Route.idroute);
            }
            //now write json representation to lcr.LCRCurrent field
            this._parentLcr.LcrCurrent = JsonConvert.SerializeObject(this.DicCostEntities);
        }
    }
}