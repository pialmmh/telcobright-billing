using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation
{
    public class XyzInvoiceDataRow
    {
        public int tup_outpartnerid { get; set; }
        public long successfulcalls { get; set; }
        public decimal roundedduration { get; set; }
        public decimal longDecimalAmount1 { get; set; }//x
        public decimal longDecimalAmount2 { get; set; }//y
        public decimal longDecimalAmount3 { get; set; }//z
        public decimal customercost { get; set; }//revenueigw
    }
}
