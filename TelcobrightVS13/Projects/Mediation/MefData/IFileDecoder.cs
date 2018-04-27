using System.Collections.Generic;
using MediationModel;
namespace TelcobrightMediation
{
    public interface IFileDecoder
    {
        string RuleName { get; }
        int Id { get; }
        string HelpText { get; }
        List<string[]> DecodeFile(CdrCollectorInputData decoderInputData,out List<cdrinconsistent> inconsistentCdrs);
    }
}