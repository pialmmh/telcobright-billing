using TelcobrightMediation.Accounting;
using System.Collections.Generic;
namespace TelcobrightMediation
{
    public abstract class AbstractInvoiceSectionGenerator : IInvoiceSectionGenerator
    {
        public abstract string RuleName { get; }
        public abstract InvoiceSection GetInvoiceSection(InvoiceSectionGeneratorData invoiceSectionGeneratorData);
        protected virtual InvoiceSection GetInvoiceSection<T>(InvoiceSectionGeneratorData invoiceSectionGeneratorData
            , string sql)
        {
            InvoiceSectionWithDataRowCreator<T>
                invoiceSectionCreator = new InvoiceSectionWithDataRowCreator<T>(
                    invoicePostProcessingData: invoiceSectionGeneratorData.InvoicePostProcessingData,
                    sectionNumber: invoiceSectionGeneratorData.SectionNumber,
                    templateName: invoiceSectionGeneratorData.TemplateName);
            return invoiceSectionCreator.CreateInvoiceSection(sql);
        }
        protected virtual List<T>  GetInvoiceSectionDataRowsOnly<T>(InvoiceSectionGeneratorData invoiceSectionGeneratorData
            , string sql)
        {
            InvoiceSectionWithDataRowCreator<T>
                invoiceSectionCreator = new InvoiceSectionWithDataRowCreator<T>(
                    invoicePostProcessingData: invoiceSectionGeneratorData.InvoicePostProcessingData,
                    sectionNumber: invoiceSectionGeneratorData.SectionNumber,
                    templateName: invoiceSectionGeneratorData.TemplateName);
            return invoiceSectionCreator.CreateInvoiceSectionRowDataOnly(sql);
        }
    }
}