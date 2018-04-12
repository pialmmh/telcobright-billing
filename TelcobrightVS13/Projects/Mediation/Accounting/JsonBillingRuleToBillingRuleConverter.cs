using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;
using Newtonsoft.Json;
namespace TelcobrightMediation.Accounting
{
    public static class JsonBillingRuleToBillingRuleConverter
    {
        public static BillingRule Convert(jsonbillingrule jsonbillingrule)
        {
            return JsonConvert.DeserializeObject<BillingRule>(jsonbillingrule.JsonExpression);
        }
    }
}
