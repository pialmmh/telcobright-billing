using TelcobrightMediation.Accounting;

namespace TelcobrightMediation
{
    public abstract class AbstractInvoiceSectionGenerator : IInvoiceSectionGenerator
    {
        public abstract string RuleName { get; }
        public abstract InvoiceSection GetInvoiceSection(InvoiceSectionGeneratorData invoiceSectionGeneratorData);
        protected InvoiceSection GetInvoiceSection<T>(InvoiceSectionGeneratorData invoiceSectionGeneratorData
            , string sql)
        {
            InvoiceSectionWithDataRowCreator<T>
                invoiceSectionCreator = new InvoiceSectionWithDataRowCreator<T>(
                    invoicePostProcessingData: invoiceSectionGeneratorData.InvoicePostProcessingData,
                    sectionNumber: invoiceSectionGeneratorData.SectionNumber,
                    templateName: invoiceSectionGeneratorData.TemplateName);
            return invoiceSectionCreator.CreateInvoiceSection(sql);
        }
    }
}