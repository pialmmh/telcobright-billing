using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.Cdr
{
    public interface IEventCollector
    {
        object Collect();
    }
}
