using MediationModel;

namespace TelcobrightMediation
{
    public interface IInvoiceGenerationRule
    {
        string RuleName { get; }
        string HelpText { get; }
        int Id { get; }
        void Execute(object data);
    }
}