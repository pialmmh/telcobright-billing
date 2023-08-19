using System;
using System.Collections.Generic;
using MediationModel;
using LibraryExtensions;
namespace LibraryExtensions
{
    public interface IFileDecoder
    {
        string RuleName { get; }
        int Id { get; }
        string HelpText { get; }
        CompressionType CompressionType { get; set; }
        List<string[]> DecodeFile(CdrCollectorInputData decoderInputData,out List<cdrinconsistent> inconsistentCdrs);
        string getTupleExpression(CdrCollectorInputData decoderInputData, string[] row);
        string getSqlWhereClauseForDayWiseSafeCollection(CdrCollectorInputData decoderInputData,DateTime day);
    }
}