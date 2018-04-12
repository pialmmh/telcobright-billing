namespace TelcobrightMediation
{
    public interface IFileNameMatchingRule
    {
        string RuleName { get; }
        string HelpText { get; }
        int Id { get; }
        void GetFileInfo(string directoryString, string fileName);

    }
}