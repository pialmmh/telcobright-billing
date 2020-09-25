using System.Collections.Generic;
using MediationModel;
namespace TelcobrightMediation
{
    public interface IExceptionalCdrPreProcessor
    {
        string RuleName { get; }
        int Id { get; }
        string HelpText { get; }
        cdr Process(cdr c);
    }
}