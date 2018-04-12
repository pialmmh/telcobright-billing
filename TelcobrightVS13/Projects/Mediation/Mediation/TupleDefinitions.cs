using System.Collections.Generic;
using System.Linq;
using MediationModel;
using TelcobrightMediation.Accounting;
namespace TelcobrightMediation
{
    public class TupleDefinitions
    {
        public Dictionary<string, List<rateplanassignmenttuple>> DicRouteTuplesIncludingBillingRuleAssignment { get; set; }
        public Dictionary<string, List<rateplanassignmenttuple>> DicPartnerTuplesIncludingBillingRuleAssignment { get; set; }
        public Dictionary<string, List<rateplanassignmenttuple>> DicServiceTuplesIncludingBillingRuleAssignment { get; set; }
        public TupleDefinitions(List<rateplanassignmenttuple> rateplanassignmenttuplesIncludingBillingRuleAssignment)
        {
            //route
            List<rateplanassignmenttuple> shortList = rateplanassignmenttuplesIncludingBillingRuleAssignment.Where(
                c => c.route != null && c.route > 0).ToList();
            if (shortList.Count > 0)
            {
                this.DicRouteTuplesIncludingBillingRuleAssignment = CreateDic('r', shortList);
            }
            else this.DicRouteTuplesIncludingBillingRuleAssignment = new Dictionary<string, List<rateplanassignmenttuple>>();
            //partner
            shortList = rateplanassignmenttuplesIncludingBillingRuleAssignment.Where(
                c => c.idpartner != null && c.idpartner > 0).ToList();
            if (shortList.Count > 0)
            {
                this.DicPartnerTuplesIncludingBillingRuleAssignment = CreateDic('p', shortList);
            }
            else this.DicPartnerTuplesIncludingBillingRuleAssignment = new Dictionary<string, List<rateplanassignmenttuple>>();
            //service only
            shortList = rateplanassignmenttuplesIncludingBillingRuleAssignment.Where(
                c => c.AssignDirection == null || c.AssignDirection < 1).ToList();
            if (shortList.Count > 0)
            {
                this.DicServiceTuplesIncludingBillingRuleAssignment = CreateDic('s', shortList);
            }
            else this.DicServiceTuplesIncludingBillingRuleAssignment = new Dictionary<string, List<rateplanassignmenttuple>>();
        }
        private Dictionary<string, List<rateplanassignmenttuple>> CreateDic
            (char tupleType, List<rateplanassignmenttuple> rateplanassignmenttuples)
        {
            Dictionary<string, List<rateplanassignmenttuple>> x = null;
            switch (tupleType)
            {
                case 'r':
                    x = this.DicRouteTuplesIncludingBillingRuleAssignment = new Dictionary<string, List<rateplanassignmenttuple>>();
                    break;
                case 'p':
                    x = this.DicPartnerTuplesIncludingBillingRuleAssignment = new Dictionary<string, List<rateplanassignmenttuple>>();
                    break;
                default:
                    x = this.DicServiceTuplesIncludingBillingRuleAssignment = new Dictionary<string, List<rateplanassignmenttuple>>();
                    break;
            }
            foreach(rateplanassignmenttuple thisTuple in rateplanassignmenttuples)
            {
                AppendToDic(x, thisTuple);
            }
            return x;
        }
        private void AppendToDic(Dictionary<string,List<rateplanassignmenttuple>> dic, rateplanassignmenttuple thisTuple)
        {
            string key = thisTuple.GetTuple();
            List<rateplanassignmenttuple> lstTuples = null;
            //if (dic.ContainsKey(Key) == false)
            //{
            //    dic.Add(Key, ThisTuple);
            //}
            dic.TryGetValue(key, out lstTuples);
            if(lstTuples==null)
            {
                lstTuples = new List<rateplanassignmenttuple>();
                dic.Add(key, lstTuples);
            }
            lstTuples.Add(thisTuple);
        }

    }
}