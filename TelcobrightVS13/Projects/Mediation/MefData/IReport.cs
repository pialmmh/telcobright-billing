namespace TelcobrightMediation
{
    public interface IReport
    {
        string RuleName { get; }
        string HelpText { get; }
        int Id { get; }
    }
}