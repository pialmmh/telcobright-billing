using MediationModel;
using TelcobrightMediation.Accounting;

namespace TelcobrightMediation
{
    public interface IInvoiceGenerationRule
    {
        string RuleName { get; }
        string HelpText { get; }
        int Id { get; }
        InvoicePostProcessingData Execute(object data);
    }
}