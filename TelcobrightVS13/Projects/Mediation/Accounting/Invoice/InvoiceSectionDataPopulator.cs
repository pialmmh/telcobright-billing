using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;
using Newtonsoft.Json;

namespace TelcobrightMediation.Accounting.Invoice
{
    public class InvoiceSectionDataPopulator
    {
        public InvoicePostProcessingData InvoicePostProcessingData { get; }
        private string CdrOrSummaryTableName { get; }
        public InvoiceSectionDataPopulator(InvoicePostProcessingData invoicePostProcessingData,
            string cdrOrSummaryTableName)
        {
            InvoicePostProcessingData = invoicePostProcessingData;
            this.CdrOrSummaryTableName = cdrOrSummaryTableName;
        }

        public Dictionary<string,string> AppendSectionToJsonDetail(Dictionary<string, string> jsonDetail,
            List<InvoiceSection> invoiceSections)
        {
            invoiceSections.ForEach(invoiceSection =>
            {
                jsonDetail.Add(invoiceSection.SectionName + ", " + "Template-" +
                               invoiceSection.TemplateName, invoiceSection.SerializedData);
            });
            return jsonDetail;
        }

        public IEnumerable<InvoiceSection> Populate()
        {
            int idServiceGroup = Convert.ToInt32(
                this.InvoicePostProcessingData.InvoiceGenerationInputData.InvoiceJsonDetail["idServiceGroup"]);
            InvoiceGenerationConfig invoiceGenerationConfig = null;
            var invoiceGenerationInputData = this.InvoicePostProcessingData.InvoiceGenerationInputData;
            invoiceGenerationInputData
                .ServiceGroupWiseInvoiceGenerationConfigs.TryGetValue(idServiceGroup, out invoiceGenerationConfig);
            if (invoiceGenerationConfig == null)
                throw new Exception("Invoice generation config not found.");
            int sectionCount = 0;
            foreach (KeyValuePair<string, string> kv in invoiceGenerationConfig.SectionGeneratorVsTemplateNames)
            {
                string sectionGeneratorName = kv.Key;
                string templateName = kv.Value;
                IInvoiceSectionGenerator invoiceSectionGenerator =
                    invoiceGenerationInputData.InvoiceSectionGenerators[sectionGeneratorName];
                InvoiceSectionGeneratorData invoiceSectionGeneratorInput
                    = new InvoiceSectionGeneratorData(invoicePostProcessingData: this.InvoicePostProcessingData,
                        sectionNumber: ++sectionCount, templateName: templateName,
                        cdrOrSummaryTableName: this.CdrOrSummaryTableName);
                yield return invoiceSectionGenerator.GetInvoiceSection(invoiceSectionGeneratorInput);
            }
        }
    }
}
