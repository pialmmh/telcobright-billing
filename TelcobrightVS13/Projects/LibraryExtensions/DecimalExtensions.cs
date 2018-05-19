using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryExtensions
{
    public static class DecimalExtensions
    {
        public static decimal RoundFractionsUpTo(this decimal val, int maxDecimalPrecision)
        {
            return decimal.Round(val, maxDecimalPrecision);
        }
    }
}
