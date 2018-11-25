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
        object Data { get;}
        void Prepare(object input);
        bool IsPrepared { get; }
        bool CheckIfTrue(cdr cdr);
    }
}