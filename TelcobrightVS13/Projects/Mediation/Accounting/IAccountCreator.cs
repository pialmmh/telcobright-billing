using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.Accounting
{
    public interface IAccountCreator
    {
        AccountingContext AccountingContext { get;}
        AccountFactory AccountFactory { get; }
    }
}
