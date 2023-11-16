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

    [Export("Decoder", typeof(AbstractCdrDecoder))]
    public class ZteIpTdmDecoder : AbstractCdrDecoder
    {
        public override string ToString() => this.RuleName;
        public override string RuleName => GetType().Name;
        public override int Id => 18;
        public override string HelpText => "Decodes ZTE IP TDM CDR.";
        public override CompressionType CompressionType { get; set; }
        public override string UniqueEventTablePrefix { get; }
        public override string PartialTableStorageEngine { get; }
        public override string partialTablePartitionColName { get; }

        public override List<string[]> DecodeFile(CdrCollectorInputData input,out List<cdrinconsistent> inconsistentCdrs)
        {
            inconsistentCdrs = new List<cdrinconsistent>();
            //this.id = base.Id;
            List<string[]> decodedRows = ZteDecoderHelper.DecodeFile(input.Ne.idcdrformat,input, out inconsistentCdrs);
            decodedRows.ForEach(row =>
            {
                SetIncomingRoute(row);
                SetOutgoingRoute(row);
            });
            return decodedRows;
        }

        public override string getTupleExpression(Object data)
        {
            throw new NotImplementedException();
        }

        public override string getCreateTableSqlForUniqueEvent(Object data)
        {
            throw new NotImplementedException();
        }

        public override string getSelectExpressionForUniqueEvent(Object data)
        {
            throw new NotImplementedException();
        }

        public override string getWhereForHourWiseCollection(Object data)
        {
            throw new NotImplementedException();
        }

        public override string getSelectExpressionForPartialCollection(Object data)
        {
            throw new NotImplementedException();
        }

        public override DateTime getEventDatetime(Object data)
        {
            throw new NotImplementedException();
        }


        private static void SetIncomingRoute(string[] row)
        {
            if (row[Fn.InTrunkAdditionalInfo] == "0")
            {
                row[Fn.IncomingRoute] = row[Fn.InMgwId];
            }
            else if (Convert.ToInt32(row[Fn.InTrunkAdditionalInfo]) > 0 && row[Fn.InMgwId] == "1")
            {
                row[Fn.IncomingRoute] = row[Fn.InTrunkAdditionalInfo];
            }
            else throw new ArgumentOutOfRangeException($"Unexpected values of InMgw & InTrunkAdditionalInfo while setting incoming route.");
        }
        private static void SetOutgoingRoute(string[] row)
        {
            if (row[Fn.OutTrunkAdditionalInfo] == "0")
            {
                row[Fn.OutgoingRoute] = row[Fn.OutMgwId];
            }
            else if (Convert.ToInt32(row[Fn.OutTrunkAdditionalInfo]) > 0 && row[Fn.OutMgwId] == "1")
            {
                row[Fn.OutgoingRoute] = row[Fn.OutTrunkAdditionalInfo];
            }
            //else throw new ArgumentOutOfRangeException($"Unexpected values of OutMgw & OutTrunkAdditionalInfo while setting oucoming route.");
        }
    }
}
