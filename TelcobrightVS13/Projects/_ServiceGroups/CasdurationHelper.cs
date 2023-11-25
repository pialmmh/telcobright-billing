using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation
{
    class CasDurationHelper
    {

        public static decimal getDomesticDur(decimal actualDurationSec)
        {
            decimal finalDuration;
            if (actualDurationSec == 0) return 0;

            decimal minDurationSec = 0.1M;

            if (minDurationSec < 0M)//no rounding, use actual duration
            {
                return actualDurationSec;
            }
            else if (minDurationSec > 0M)//e.g. minimum .1 sec (100 ms) required for rounding up
            {
                //the code below works upto 11 digits e.g. 3538.099999999994 if the last digit >4 or there is more decimal then only rounds up
                decimal floorDuration = Math.Floor(actualDurationSec);
                decimal miliSecPart = actualDurationSec - floorDuration;
                if (miliSecPart >= minDurationSec)
                {
                    actualDurationSec = Math.Ceiling(actualDurationSec);
                }
                else
                {
                    actualDurationSec = Math.Floor(actualDurationSec);
                }
            }
            else//always round up
            {
                actualDurationSec = Math.Ceiling(actualDurationSec);
            }

            finalDuration = Convert.ToInt64(actualDurationSec);
            return finalDuration;
        }
        public static decimal getIntlOutDur(decimal actualDurationSec)
        {
            decimal duration100ms = getDomesticDur(actualDurationSec);
            int Resolution = 15;
            long finalDuration = Convert.ToInt64(duration100ms);


            long lngResolution = Convert.ToInt64(Resolution);
            if (finalDuration % lngResolution > 0)
            {
                finalDuration = ((finalDuration / lngResolution) + 1) * lngResolution;
            }
            else
            {
                finalDuration = ((finalDuration / lngResolution)) * lngResolution;
            }
            return finalDuration;
        }
    }
}
