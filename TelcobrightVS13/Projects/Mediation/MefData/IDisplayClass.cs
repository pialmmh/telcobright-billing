namespace TelcobrightMediation
{
    public interface IDisplayClass//each iJob corresponding to one enumjobdefinition
    {
        //string RuleName { get; }
        string HelpText { get; }
        int Id { get; }
        //enumjobdefinition jobdef;
        //CdrJob cdrJob;
        object GetDisplayClass(object oneEntity);
    }
}