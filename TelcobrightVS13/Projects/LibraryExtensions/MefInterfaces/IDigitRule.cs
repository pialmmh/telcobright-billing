using MediationModel;

namespace LibraryExtensions
{
    public interface IDigitRule
    {
        string RuleName { get; }
        string HelpText { get; }
        int Id { get; }
        string GetModifiedDigits(DigitRulesData digitRulesData,string phoneNumber);
    }
}