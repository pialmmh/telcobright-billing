using MediationModel;

namespace TelcobrightMediation
{
    public interface IPartnerRule
    {
        string RuleName { get; }
        string HelpText { get; }
        int Id { get; }
        void Execute(cdr thisCdr, MefPartnerRulesContainer pData);
    }
}