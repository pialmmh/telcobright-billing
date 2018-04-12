using System;
using MediationModel;

namespace TelcobrightMediation
{
    public interface ITelcobrightJobInput
    {
        job TelcobrightJob { get; }
    }

    public interface ITelcobrightJob//each iJob corresponding to one enumjobdefinition
    {
        string RuleName { get; }
        string HelpText { get; }
        int Id { get; }
        JobCompletionStatus Execute(ITelcobrightJobInput jobInputData);
    }
}