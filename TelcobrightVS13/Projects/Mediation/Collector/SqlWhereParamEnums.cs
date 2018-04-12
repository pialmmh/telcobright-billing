using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation
{
    public enum SqlWhereParamType
    {
        Datetime,
        Numeric,
        Text,
        Null
    }

    public enum SqlWhereAndOrType
    {
        FirstBeforeAndOr,
        And,
        Or
    }
}
