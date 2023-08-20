using System;

namespace LibraryExtensions
{
    public interface ITelcobrightJob//each iJob corresponding to one enumjobdefinition
    {
        string RuleName { get; }
        string HelpText { get; }
        int Id { get; }
        JobCompletionStatus Execute(ITelcobrightJobInput jobInputData);
    }
}