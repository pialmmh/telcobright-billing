using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;

namespace TelcobrightMediation.Accounting
{
    public abstract class AccountAction : IAccountAction
    {
        public int Id { get; set; }
        public String ActionName { get; set; }

        public abstract bool execute(partner partner);
    }
}
