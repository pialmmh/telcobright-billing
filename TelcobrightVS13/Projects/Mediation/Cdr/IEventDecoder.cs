using System;
using System.Collections.Generic;
using MediationModel;
using LibraryExtensions;
namespace TelcobrightMediation
{
    public interface IEventDecoder
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
        string getCreateTableSqlForUniqueEvent(Object data);
        string getSelectExpressionForUniqueEvent(Object data);
        string getWhereForHourWiseCollection(Object data);
        string getSelectExpressionForPartialCollection(Object data);
        DateTime getEventDatetime(Object data);
    }
    
}