using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation
{
    public class InvoiceSection
    {
        public string SectionName { get; set; }
        public string TemplateName { get; set; }
        public String SerializedData { get; set; }

        public InvoiceSection(string sectionName, string templateName, string serializedData)
        {
            SectionName = sectionName;
            TemplateName = templateName;
            SerializedData = serializedData;
        }
    }
}
