using MediationModel;

namespace TelcobrightMediation
{
    public interface IServiceGroupPreProcessor
    {
        string RuleName { get; }
        string HelpText { get; }
        int Id { get; }
        void Execute(cdr thisCdr, CdrProcessor cdrProcessor);
    }
}