using MediationModel;

namespace TelcobrightMediation
{
    public interface IInvoiceGenerationRule
    {
        string RuleName { get; }
        string HelpText { get; }
        int Id { get; }
        invoice Execute(object data);
    }
}