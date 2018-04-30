using System.Collections.Generic;
using System.Linq;

namespace MediationModel
{
    public class IdCallWiseListOfRouteVsCost
    {
        //main dic to hold prefix/IdCall wise call info [prefix hasbeen used as IdCall]
        private Dictionary<string, List<RouteVsCost>> _dicIdVsRouteCost;
        public IdCallWiseListOfRouteVsCost()
        {
            this._dicIdVsRouteCost = new Dictionary<string, List<RouteVsCost>>();
        }
        public void Append(long IdCall, RouteVsCost rc)
        {
            List<RouteVsCost> lstRouteVsCost = null;
            if (this._dicIdVsRouteCost.ContainsKey(IdCall.ToString()) == false)
            {
                lstRouteVsCost = new List<RouteVsCost>();
                this._dicIdVsRouteCost.Add(IdCall.ToString(), lstRouteVsCost);
            }
            else
            {
                lstRouteVsCost = this._dicIdVsRouteCost[IdCall.ToString()];
            }
            lstRouteVsCost.Add(rc);
        }

        public List<lcr> GetLcr(LcrJobData lData)
        {
            var lstLcr = new List<lcr>();
            var idRateplan = lData.RefRatePlan;
            var startdate = lData.StartDate;
            foreach (var kv in this._dicIdVsRouteCost)//per prefix
            {
                var sortedList = kv.Value.OrderBy(c => c.Cost).ToList();//sort by cost
                //create one lcr entity for each prefix
                var thisLcr = new lcr();
                var thisExp = new LcrExpression(thisLcr);
                thisLcr.idrateplan = idRateplan;
                thisLcr.startdate = startdate;
                thisLcr.Prefix = kv.Key;
                thisLcr.LcrExpression = thisExp;
                thisLcr.LcrExpression.AppendListOfRouteCost(sortedList);
                lstLcr.Add(thisLcr);
            }
            return lstLcr;
        }

    }
}