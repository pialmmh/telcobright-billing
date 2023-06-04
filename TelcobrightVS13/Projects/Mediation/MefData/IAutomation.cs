using System;
using System.Collections.Generic;
using MediationModel;
using TelcobrightMediation.Cdr;

namespace TelcobrightMediation
{
    public interface IAutomation
    {
        string RuleName { get; }
        string HelpText { get; }
        void connect(Object automationData);
        void execute(Object executionData);
    }
}