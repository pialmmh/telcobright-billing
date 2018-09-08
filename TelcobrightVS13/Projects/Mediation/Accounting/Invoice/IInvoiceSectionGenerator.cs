using TelcobrightMediation.Accounting;

namespace TelcobrightMediation
{
    public interface IInvoiceSectionGenerator
    {
        string RuleName { get; }
        InvoiceSection GetInvoiceSection(InvoiceSectionGeneratorData invoiceSectionGeneratorData);
    }
}