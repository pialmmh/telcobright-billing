using System;
using System.Collections.Generic;
using MediationModel;
using TelcobrightMediation.Cdr;

namespace TelcobrightMediation
{
    public interface ICdrRule
    {
        string RuleName { get; }
        string HelpText { get; }
        int Id { get; }
        bool CheckIfTrue(cdr cdr);
    }
}