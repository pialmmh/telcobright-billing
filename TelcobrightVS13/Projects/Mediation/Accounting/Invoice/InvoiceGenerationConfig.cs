using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelcobrightFileOperations;

namespace TelcobrightMediation.Accounting
{
    public class InvoiceGenerationConfig
    {
        public string InvoiceGenerationRuleName { get; set; }
        public Dictionary<string, string> SectionGeneratorVsTemplateNames { get; set; }
        public Dictionary<string, string> OtherParams { get; set; }
        public List<string> SectionNamesOfInvoiceForExport { get; set; }
        //public IStringExpressionGenerator InvoiceRefNoExpressionGenerator { get; set; }
    }
}
