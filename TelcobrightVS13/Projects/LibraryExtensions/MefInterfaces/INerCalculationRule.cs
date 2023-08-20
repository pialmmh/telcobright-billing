using TelcobrightMediation.Cdr;
namespace LibraryExtensions
{
    public interface INerCalculationRule
    {
        string RuleName { get; }
        string HelpText { get; }
        int Id { get; }
        void ExecuteNerRule(CdrProcessor cdrProcessor, CdrExt cdrExt);
    }
}