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
        public TelcobrightConfig Tbc { get; set; }

        public ActionBlockingAutomation()
        {
            Id = 3;
            ActionName = "Block Account";
        }

        public void Execute(object automationData)
        {
            throw new NotImplementedException();
        }
    }
}
