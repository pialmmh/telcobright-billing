using MediationModel;

namespace TelcobrightMediation
{
    public interface IAutoCreateJob
    {
        string RuleName { get; }
        string HelpText { get; }
        int Id { get; }
        void Execute(ne thisSwitch);
    }
}