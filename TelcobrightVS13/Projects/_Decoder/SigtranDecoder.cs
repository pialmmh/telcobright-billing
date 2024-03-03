using TelcobrightMediation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using TelcobrightFileOperations;
using MediationModel;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Mediation.Cdr;
using System.Linq;
using System.Globalization;
using System.IO;
using LibraryExtensions;
using TelcobrightInfra.PerformanceAndOptimization;

namespace Decoders
{

    [Export("Decoder", typeof(AbstractCdrDecoder))]
    public class SigtranDecoder : AbstractCdrDecoder
    {
        public override string ToString() => this.RuleName;
        public override string RuleName => GetType().Name;
        public override int Id =>78;
        public override string HelpText => "Sigtarn Decoder";
        public override CompressionType CompressionType { get; set; }
        public override string UniqueEventTablePrefix { get; }
        public override string PartialTableStorageEngine { get; }
        public override string partialTablePartitionColName { get; }
        protected CdrCollectorInputData Input { get; set; }
     
        public override List<string[]> DecodeFile(CdrCollectorInputData decoderInputData, out List<cdrinconsistent> inconsistentCdrs)
        {
            this.Input = decoderInputData;
            string fileName = this.Input.FullPath;

            return decodeLine(decoderInputData, out inconsistentCdrs, fileName, decoderInputData.TelcobrightJob.JobName);
        }
        public List<string[]> decodeLine(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs, string filePath, string jobName)
        {
            Dictionary<string, partnerprefix> ansPrefixes = input.MediationContext.AnsPrefixes;
            inconsistentCdrs = new List<cdrinconsistent>();
            List<string[]> decodeRows = new List<string[]>();
            SigtranPacketDecoder decoder = new SigtranPacketDecoder(filePath,ansPrefixes);
            decoder.populatePrefix();
            List<SigtranPacket> packets = decoder.GetPackets();
            decodeRows = decoder.CdrRecords(packets);
            return decodeRows;
        }

        public string getTupleExpression(CdrCollectorInputData decoderInputData, string[] row)
        {
            throw new NotImplementedException();
        }

        public string getCreateTableSqlForUniqueEvent(CdrCollectorInputData decoderInputData)
        {
            throw new NotImplementedException();
        }

        public string getSelectExpressionForUniqueEvent(CdrCollectorInputData decoderInputData)
        {
            throw new NotImplementedException();
        }

        public string getWhereForHourWiseUniqueEventCollection(CdrCollectorInputData decoderInputData, DateTime hourOfDay)
        {
            throw new NotImplementedException();
        }

        public string getSelectExpressionForPartialCollection(CdrCollectorInputData decoderInputData)
        {
            throw new NotImplementedException();
        }

        public string getWhereForHourWisePartialCollection(CdrCollectorInputData decoderInputData, DateTime hourOfDay)
        {
            throw new NotImplementedException();
        }

        public override string getTupleExpression(Object data)
        {
            throw new NotImplementedException();
        }

        public override string getCreateTableSqlForUniqueEvent(object data)
        {
            throw new NotImplementedException();
        }

        public override string getSelectExpressionForUniqueEvent(object data)
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

    }
}


