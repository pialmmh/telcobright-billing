using MediationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.Accounting
{
    public interface IAutomationAction
    {
        int Id { get; set; }
        String ActionName { get; set; }
        void Execute(object automationData);
    }
}
