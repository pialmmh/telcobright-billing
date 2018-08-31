using MediationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.Accounting
{
    public class SMSAccountAction : IAutomationAction
    {
        public int Id { get; set; }
        public string ActionName { get; set; }
        public TelcobrightConfig Tbc { get; set; }
        public SMSAccountAction()
        {
            this.Id = 2;
            this.ActionName = "Send SMS";
        }

        public void Execute(object data)
        {
            throw new NotImplementedException();
        }
    }
}
