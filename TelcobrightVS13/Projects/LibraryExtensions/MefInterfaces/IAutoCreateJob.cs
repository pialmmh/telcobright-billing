using MediationModel;

namespace LibraryExtensions
{
    public interface IAutoCreateJob
    {
        string RuleName { get; }
        string HelpText { get; }
        int Id { get; }
        void Execute(ne thisSwitch);
    }
}