using System;
using System.Collections.Generic;
using MediationModel;
using LibraryExtensions;
namespace TelcobrightMediation
{
    public interface IFileDecoder
    {
        string RuleName { get; }
        int Id { get; }
        string HelpText { get; }
        CompressionType CompressionType { get; set; }
        string PartialTablePrefix { get; }
        string PartialTableStorageEngine { get; }
        string partialTablePartitionColName { get; }
        List<string[]> DecodeFile(CdrCollectorInputData decoderInputData,out List<cdrinconsistent> inconsistentCdrs);
        string getTupleExpression(CdrCollectorInputData decoderInputData, string[] row);
        string getSqlWhereClauseForHourWiseSafeCollection(CdrCollectorInputData decoderInputData,DateTime day);
        string getCreateTableSqlForUniqueEvent(CdrCollectorInputData decoderInputData);
        string getDuplicateCollectionSql(CdrCollectorInputData decoderInputData,DateTime hourOfTheDay, List<string> tuples);
        string getPartialCollectionSql(CdrCollectorInputData decoderInputData, DateTime hourOfTheDay, List<string> tuples);
    }
}