using MediationModel;
using System.Collections.Generic;
namespace TelcobrightMediation
{
    public interface IStringExpressionGenerator
    {
        string RuleName { get; }
        string HelpText { get; }
        int Id { get; }
        object Data { get; }
        void Prepare();
        bool IsPrepared { get; }
        string GetStringExpression(Dictionary<string,object> input);
    }
}