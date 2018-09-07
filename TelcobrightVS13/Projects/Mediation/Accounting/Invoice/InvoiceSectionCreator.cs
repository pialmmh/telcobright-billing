using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;
using TelcobrightMediation.Accounting;

namespace TelcobrightMediation
{
    public class InvoiceSectionCreator<T>
    {
        private InvoicePostProcessingData InvoicePostProcessingData { get; set; }
        private PartnerEntities Context { get; }
        private DateTime StartDate { get; }
        private DateTime EndDate { get; }
        private string SectionName { get; }
        private string TemplateName { get; }
        public InvoiceSectionCreator(InvoicePostProcessingData invoicePostProcessingData,
            string templateName, int sectionNumber)
        {
            this.Context = invoicePostProcessingData.InvoiceGenerationInputData.Context;
            this.StartDate = invoicePostProcessingData.StartDate;
            this.EndDate = invoicePostProcessingData.EndDate;
            this.SectionName = "Section-" + sectionNumber.ToString();
            this.TemplateName = templateName;
        }
        public InvoiceSection CreateInvoiceSection(string sql)
        {
            List<T> data = this.Context.Database.SqlQuery<T>(sql).ToList();
            return new InvoiceSection(sectionName: this.SectionName,
                templateName: this.TemplateName,
                serializedData: new JsonCompressor<List<T>>().SerializeToCompressedBase64(data));
        }
    }
}
