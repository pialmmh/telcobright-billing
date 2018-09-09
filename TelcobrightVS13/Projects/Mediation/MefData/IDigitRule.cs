using MediationModel;

namespace TelcobrightMediation
{
    public interface IDigitRule
    {
        string RuleName { get; }
        string HelpText { get; }
        int Id { get; }
        string GetModifiedDigits(DigitRulesData digitRulesData,string phoneNumber);
    }
}