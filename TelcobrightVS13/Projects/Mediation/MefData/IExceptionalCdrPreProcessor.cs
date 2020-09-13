using System.Collections.Generic;
using MediationModel;
namespace TelcobrightMediation
{
    public interface IExceptionalCdrPreProcessor
    {
        string RuleName { get; }
        int Id { get; }
        string HelpText { get; }
        string[] Process(string[] cdr);
    }
}