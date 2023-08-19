using MediationModel;

namespace LibraryExtensions
{
    public interface IPartnerRule
    {
        string RuleName { get; }
        string HelpText { get; }
        int Id { get; }
        int Execute(cdr thisCdr, MefPartnerRulesContainer data);
    }
}