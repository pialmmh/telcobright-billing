using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.Sansay
{
    enum SansayWsReturnCodes
    {
        Ok = 200,
        FinishedWithError = 300,
        Fail = 400,
        UserNotAuthorized = 401,
        InvalidOperation = 402
    }
}
