using System;

namespace TelcobrightInfra
{
    public interface IScript
    {
        string RuleName { get; }
        string HelpText { get; }
        ScriptType ScriptType { get; }
        string GetScript(Object data);
    }
}