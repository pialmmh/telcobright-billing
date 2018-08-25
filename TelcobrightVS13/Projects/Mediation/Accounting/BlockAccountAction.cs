using MediationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.Accounting
{
    public class BlockAccountAction : AccountAction
    {
        public BlockAccountAction()
        {
            this.Id = 3;
            this.ActionName = "Block Account";
        }

        public override bool execute(partner partner)
        {
            throw new NotImplementedException();
        }
    }
}
