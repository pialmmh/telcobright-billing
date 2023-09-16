using System;

namespace TelcobrightInfra
{
    public interface IScript
    {
        string RuleName { get; }
        string HelpText { get; }
        ScriptType ScriptType { get; }
        string ScriptDir { get; set; }
        string SrcTextFileName { get; set; }
        string GetScript(Object data);
    }
}