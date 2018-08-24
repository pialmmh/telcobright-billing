using MediationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.Accounting
{
    public class SendSMSAccountAction : AccountAction
    {
        public SendSMSAccountAction()
        {
            this.Id = 2;
            this.ActionName = "Send SMS";
        }

        public override bool execute(partner partner)
        {
            throw new NotImplementedException();
        }
    }
}
