using System;

namespace TelcobrightMediation
{
    public interface ITelcobrightJob//each iJob corresponding to one enumjobdefinition
    {
        string RuleName { get; }
        string HelpText { get; }
        int Id { get; }
        object Execute(ITelcobrightJobInput jobInputData);
        object PreprocessJob(ITelcobrightJobInput jobInputData);
        object PostprocessJob(ITelcobrightJobInput jobInputData);
    }
}