using TelcobrightMediation.Accounting;

namespace TelcobrightMediation
{
    public interface IInvoiceSectionGenerator
    {
        string SectionType { get; }
        InvoiceSection GetInvoiceSection(InvoiceSectionGeneratorData invoiceSectionGeneratorData);
    }
}