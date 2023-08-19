using System;
using System.Collections.Generic;
namespace LibraryExtensions
{
    public interface IAutomation
    {
        string RuleName { get; }
        string HelpText { get; }
        void connect(Object automationData);
        void execute(Object executionData);
    }
}