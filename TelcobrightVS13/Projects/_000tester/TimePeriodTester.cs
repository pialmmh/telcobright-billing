using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace Utils
{
    public class TimePeriodTester
    {
        public void Test()
        {
            DateTime dt=new DateTime(2018,01,01,18,59,59);
            DateTime dtDay = dt.RoundDownToDay();
            DateTime dtHour = dt.RoundDownToHour();
        }
    }
}
