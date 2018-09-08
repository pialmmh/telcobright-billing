using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.Accounting
{
    public class InvoiceGenerationConfig
    {
        public string InvoiceGenerationRuleName { get; set; }
        public Dictionary<string, string> SectionGeneratorVsTemplateNames { get; set; }
    }
}
