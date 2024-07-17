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
        //public override string UniqueEventTablePrefix { get; }
        //public override string PartialTableStorageEngine { get; }
        //public override string partialTablePartitionColName { get; }
        protected virtual CdrCollectorInputData Input { get; set; }
     
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
            List<SigtranPacket> packets = decoder.DecodePackets();
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

        public override string getCreateTableSqlForPartialEvent(object data)
        {
            return $@"CREATE TABLE if not exists <{this.PartialTablePrefix}> (
                  SwitchId varchar(100)  NOT NULL,
                  IdCall bigint(20) NOT NULL,
                  SequenceNumber varchar(100)  DEFAULT NULL,
                  FileName varchar(100)  NOT NULL,
                  ServiceGroup varchar(100)  DEFAULT NULL,
                  IncomingRoute varchar(100)  DEFAULT NULL,
                  originatingip varchar(100)  DEFAULT NULL,
                  OPC varchar(100)  DEFAULT NULL,
                  OriginatingCIC varchar(100)  DEFAULT NULL,
                  OriginatingCalledNumber varchar(100)  DEFAULT NULL,
                  TerminatingCalledNumber varchar(100)  DEFAULT NULL,
                  OriginatingCallingNumber varchar(100)  DEFAULT NULL,
                  TerminatingCallingNumber varchar(100)  DEFAULT NULL,
                  PrePaid varchar(100)  DEFAULT NULL,
                  DurationSec varchar(100)  DEFAULT NULL,
                  EndTime varchar(100)  DEFAULT NULL,
                  ConnectTime varchar(100)  DEFAULT NULL,
                  AnswerTime varchar(100)  DEFAULT NULL,
                  ChargingStatus varchar(100)  DEFAULT NULL,
                  PDD varchar(100)  DEFAULT NULL,
                  CountryCode varchar(100)  DEFAULT NULL,
                  AreaCodeOrLata varchar(100)  DEFAULT NULL,
                  ReleaseDirection varchar(100)  DEFAULT NULL,
                  ReleaseCauseSystem varchar(100)  DEFAULT NULL,
                  ReleaseCauseEgress varchar(100)  DEFAULT NULL,
                  OutgoingRoute varchar(100)  DEFAULT NULL,
                  terminatingip varchar(100)  DEFAULT NULL,
                  DPC varchar(100)  DEFAULT NULL,
                  TerminatingCIC varchar(100)  DEFAULT NULL,
                  StartTime datetime DEFAULT NULL,
                  InPartnerId varchar(100)  DEFAULT '0',
                  CustomerRate varchar(100)  DEFAULT NULL,
                  OutPartnerId varchar(100)  DEFAULT '0',
                  SupplierRate varchar(100)  DEFAULT NULL,
                  MatchedPrefixY varchar(100)  DEFAULT NULL,
                  UsdRateY varchar(100)  DEFAULT NULL,
                  MatchedPrefixCustomer varchar(100)  DEFAULT NULL,
                  MatchedPrefixSupplier varchar(100)  DEFAULT NULL,
                  InPartnerCost varchar(100)  DEFAULT NULL,
                  OutPartnerCost varchar(100)  DEFAULT NULL,
                  CostAnsIn varchar(100)  DEFAULT NULL,
                  CostIcxIn varchar(100)  DEFAULT NULL,
                  Tax1 varchar(100)  DEFAULT '0.00000000',
                  IgwRevenueIn varchar(100)  DEFAULT NULL,
                  RevenueAnsOut varchar(100)  DEFAULT NULL,
                  RevenueIgwOut varchar(100)  DEFAULT NULL,
                  RevenueIcxOut varchar(100)  DEFAULT NULL,
                  Tax2 varchar(100)  DEFAULT '0.00000000',
                  XAmount varchar(100)  DEFAULT NULL,
                  YAmount varchar(100)  DEFAULT NULL,
                  AnsPrefixOrig varchar(100)  DEFAULT NULL,
                  AnsIdOrig varchar(100)  DEFAULT NULL,
                  AnsPrefixTerm varchar(100)  DEFAULT NULL,
                  AnsIdTerm varchar(100)  DEFAULT NULL,
                  ValidFlag varchar(100)  DEFAULT NULL,
                  PartialFlag varchar(100)  DEFAULT NULL,
                  ReleaseCauseIngress varchar(100)  DEFAULT NULL,
                  InRoamingOpId varchar(100)  DEFAULT NULL,
                  OutRoamingOpId varchar(100)  DEFAULT NULL,
                  CalledPartyNOA varchar(100)  DEFAULT NULL,
                  CallingPartyNOA varchar(100)  DEFAULT NULL,
                  AdditionalSystemCodes varchar(100)  DEFAULT NULL,
                  AdditionalPartyNumber varchar(100)  DEFAULT NULL,
                  ResellerIds varchar(100)  DEFAULT NULL,
                  ZAmount varchar(100)  DEFAULT NULL,
                  PreviousRoutes varchar(100)  DEFAULT NULL,
                  E1Id varchar(100)  DEFAULT NULL,
                  MediaIp1 varchar(100)  DEFAULT NULL,
                  MediaIp2 varchar(100)  DEFAULT NULL,
                  MediaIp3 varchar(100)  DEFAULT NULL,
                  MediaIp4 varchar(100)  DEFAULT NULL,
                  CallReleaseDuration varchar(100)  DEFAULT NULL,
                  E1IdOut varchar(100)  DEFAULT NULL,
                  InTrunkAdditionalInfo varchar(100)  DEFAULT NULL,
                  OutTrunkAdditionalInfo varchar(100)  DEFAULT NULL,
                  InMgwId varchar(100)  DEFAULT NULL,
                  OutMgwId varchar(100)  DEFAULT NULL,
                  MediationComplete varchar(100)  DEFAULT '0',
                  Codec varchar(100)  DEFAULT NULL,
                  ConnectedNumberType varchar(100)  DEFAULT NULL,
                  RedirectingNumber varchar(100)  DEFAULT NULL,
                  CallForwardOrRoamingType varchar(100)  DEFAULT NULL,
                  OtherDate varchar(100)  DEFAULT NULL,
                  SummaryMetaTotal varchar(100)  DEFAULT NULL,
                  TransactionMetaTotal varchar(100)  DEFAULT NULL,
                  ChargeableMetaTotal varchar(100)  DEFAULT NULL,
                  ErrorCode varchar(100)  DEFAULT NULL,
                  NERSuccess varchar(100)  DEFAULT NULL,
                  RoundedDuration varchar(100)  DEFAULT NULL,
                  PartialDuration varchar(100)  DEFAULT NULL,
                  PartialAnswerTime varchar(100)  DEFAULT NULL,
                  PartialEndTime varchar(100)  DEFAULT NULL,
                  FinalRecord varchar(100)  DEFAULT NULL,
                  Duration1 varchar(100)  DEFAULT NULL,
                  Duration2 varchar(100)  DEFAULT NULL,
                  Duration3 varchar(100)  DEFAULT NULL,
                  Duration4 varchar(100)  DEFAULT NULL,
                  PreviousPeriodCdr varchar(100)  DEFAULT NULL,
                  UniqueBillId varchar(100)  DEFAULT NULL,
                  AdditionalMetaData varchar(500)  DEFAULT NULL,
                  Category varchar(100)  DEFAULT NULL,
                  SubCategory varchar(100)  DEFAULT NULL,
                  ChangedByJobId varchar(100)  DEFAULT NULL,
                  SignalingStartTime varchar(100)  DEFAULT NULL,
                  KEY ind_Unique_Bill (UniqueBillId)
                  ) ENGINE=InnoDB ";
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


