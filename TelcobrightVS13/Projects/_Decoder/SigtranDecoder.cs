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
using System.Text;
using LibraryExtensions;
using TelcobrightInfra.PerformanceAndOptimization;
using TelcobrightMediation.Cdr.Collection.PreProcessors;

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
            Dictionary<string, partnerprefix> ansPrefixes = input.MediationContext.AnsPrefixes880;
            inconsistentCdrs = new List<cdrinconsistent>();
            List<string[]> decodeRows = new List<string[]>();
            SigtranPacketDecoder decoder = new SigtranPacketDecoder(filePath,ansPrefixes);
            List<SigtranPacket> packets = decoder.GetPackets();
            decodeRows = decoder.CdrRecords(packets);
            return decodeRows;
        }

        public override string getTupleExpression(Object data)
        {
            Dictionary<string, object> dataAsDic = (Dictionary<string, object>)data;
            CdrCollectorInputData collectorInput = (CdrCollectorInputData)dataAsDic["collectorInput"];
            CdrSetting cdrSetting = collectorInput.CdrSetting;
            string[] row = (string[])dataAsDic["row"];
            int switchId = collectorInput.Ne.idSwitch;
            DateTime startTime = getEventDatetime(new Dictionary<string, object>
            {
                {"cdrSetting", cdrSetting},
                {"row", row}
            });
            //23:00 hours eventid to be rounded up as 00:00 next hour in uniqueEventTupleId                        
            //aggregation logic checks cdr for +-1 hour, so collection and aggregation will be possible            
            if (startTime.Hour == 23)
            {
                startTime = startTime.Date.AddDays(1);
            }
            else
            {
                //startTime = startTime.Date.AddHours(startTime.Hour); prev logic
                startTime = startTime.Date; //new logic
            }
            string sessionId = row[Fn.UniqueBillId];
            string separator = "/";
            return new StringBuilder(switchId.ToString()).Append(separator)
                .Append(startTime.ToMySqlFormatDateOnlyWithoutTimeAndQuote()).Append(separator)
                .Append(sessionId).ToString();
        }


        public override EventAggregationResult Aggregate(object data)
        {
            return SmsHubAggregationHelper.Aggregate((NewAndOldEventsWrapper<string[]>)data);
        }

        //public string getCreateTableSqlForUniqueEvent(CdrCollectorInputData decoderInputData)
        //{
        //    throw new NotImplementedException();
        //}

        //public string getSelectExpressionForUniqueEvent(CdrCollectorInputData decoderInputData)
        //{
        //    throw new NotImplementedException();
        //}

        //public string getWhereForHourWiseUniqueEventCollection(CdrCollectorInputData decoderInputData, DateTime hourOfDay)
        //{
        //    throw new NotImplementedException();
        //}

        //public string getSelectExpressionForPartialCollection(CdrCollectorInputData decoderInputData)
        //{
        //    throw new NotImplementedException();
        //}

        //public string getWhereForHourWisePartialCollection(CdrCollectorInputData decoderInputData, DateTime hourOfDay)
        //{
        //    throw new NotImplementedException();
        //}

        //public override string getCreateTableSqlForUniqueEvent(object data)
        //{
        //    throw new NotImplementedException();
        //}

        //public override string getSelectExpressionForUniqueEvent(object data)
        //{
        //    throw new NotImplementedException();
        //}

        //public override string getWhereForHourWiseCollection(Object data)
        //{
        //    throw new NotImplementedException();
        //}

        //public override string getSelectExpressionForPartialCollection(Object data)
        //{
        //    throw new NotImplementedException();
        //}

        //public override DateTime getEventDatetime(Object data)
        //{
        //    throw new NotImplementedException();
        //}


    }
}


