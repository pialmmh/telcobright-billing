using MediationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.Accounting
{
    public class AccActionEx : acc_action
    {
        public AccountActionRule Rule { get; set; }

        public String RuleDescription
        {
            get {
                if (Rule.IsPercent)
                    return string.Format("{0}%", Rule.Amount);
                else if (Rule.IsFormulaBased)
                    return string.Format("ACR:{0}$, ACD:{1}Sec, Minute:{2}, Ports: {3}", Rule.ACR, Rule.ACD, Rule.Amount, Rule.NoOfPorts);
                else
                    return "Fixed Amount";
            }
        }
    }
}
