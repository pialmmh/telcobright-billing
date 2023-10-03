using TelcobrightMediation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using LibraryExtensions;
using TelcobrightFileOperations;
using MediationModel;
using TelcobrightMediation.Mediation.Cdr;


namespace Decoders
{

    [Export("Decoder", typeof(IFileDecoder))]
    public class ZteTdmDecoder:IFileDecoder
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public int Id => 17;
        public string HelpText => "Decodes ZTE TDM CDR.";
        public CompressionType CompressionType { get; set; }
        public string PartialTablePrefix { get; }
        public string PartialTableStorageEngine { get; }
        public string partialTablePartitionColName { get; }

        public List<string[]> DecodeFile(CdrCollectorInputData input,out List<cdrinconsistent> inconsistentCdrs)
        {
            inconsistentCdrs = new List<cdrinconsistent>();
            List<string[]> decodedRows = ZteDecoderHelper.DecodeFile(input.Ne.idcdrformat, input, out inconsistentCdrs);
            decodedRows.ForEach(row =>
            {
                row[Fn.IncomingRoute] = row[Fn.InTrunkAdditionalInfo];
                row[Fn.OutgoingRoute] = row[Fn.OutTrunkAdditionalInfo];
            });
            return decodedRows;
        }

        public string getTupleExpression(CdrCollectorInputData decoderInputData, string[] row)
        {
            throw new NotImplementedException();
        }

        public string getSqlWhereClauseForHourWiseSafeCollection(CdrCollectorInputData decoderInputData,DateTime hourOfDay, int minusHoursForSafeCollection, int plusHoursForSafeCollection)
        {
            throw new NotImplementedException();
        }

        public string getCreateTableSqlForUniqueEvent(CdrCollectorInputData decoderInputData)
        {
            throw new NotImplementedException();
        }

        public string getDuplicateCollectionSql(CdrCollectorInputData decoderInputData, DateTime hourOfTheDay, List<string> tuples)
        {
            throw new NotImplementedException();
        }

        public string getPartialCollectionSql(CdrCollectorInputData decoderInputData, DateTime hourOfTheDay, List<string> tuples)
        {
            throw new NotImplementedException();
        }
    }
}
