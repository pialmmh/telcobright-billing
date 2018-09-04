using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryExtensions
{
    public static class NumberFormatter
    {
        public static string RoundToWholeIfFractionPartIsZero(double myNumber)
        {
            var s = string.Format("{0:0.00}", myNumber);
            if (s.EndsWith("00"))
            {
                return ((int)myNumber).ToString();
            }
            else
            {
                return s;
            }
        }
    }
}
