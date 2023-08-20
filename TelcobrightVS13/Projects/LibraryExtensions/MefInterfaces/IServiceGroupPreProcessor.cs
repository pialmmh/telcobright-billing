using MediationModel;

namespace LibraryExtensions
{
    public interface IServiceGroupPreProcessor
    { 
        string RuleName { get; }
        string HelpText { get; }
        int Id { get; }
        void Execute(cdr thisCdr, CdrProcessor cdrProcessor);
    }
}