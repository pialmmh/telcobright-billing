using MediationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.Accounting
{
    public class ActionBlockingAutomation : IAutomationAction
    {
        public int Id { get; set; }
        public string ActionName { get; set; }
        public void Execute(object automationData)
        {
            throw new NotImplementedException();
        }
    }
}
