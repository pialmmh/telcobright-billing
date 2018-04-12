using TelcobrightMediation.Cdr;
namespace TelcobrightMediation
{
    public interface INerCalculationRule
    {
        string RuleName { get; }
        string HelpText { get; }
        int Id { get; }
        void ExecuteNerRule(CdrProcessor cdrProcessor, CdrExt cdrExt);
    }
}