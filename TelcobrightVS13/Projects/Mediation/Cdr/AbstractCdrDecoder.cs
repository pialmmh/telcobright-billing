using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;
using TelcobrightMediation;

namespace TelcobrightMediation
{
    public abstract class AbstractCdrDecoder : IEventDecoder
    {
        public abstract string RuleName { get; }
        public abstract int Id { get; }
        public abstract string HelpText { get; }
        public abstract CompressionType CompressionType { get; set; }
        public virtual string UniqueEventTablePrefix { get; } = "zz_uniqueevent";
        public virtual string PartialTablePrefix { get; } = "zz_zz_partialevent";
        public virtual string PartialTableStorageEngine { get; } = "innodb";
        public virtual string partialTablePartitionColName { get; } = "starttime";

        public abstract List<string[]> DecodeFile(CdrCollectorInputData decoderInputData,
            out List<cdrinconsistent> inconsistentCdrs);

       public virtual string getWhereForHourWiseCollection(Object data)
        {
            Dictionary<string, object> dataAsDic = (Dictionary<string, object>) data;
            HourlyEventData<string[]> hourwiseData = (HourlyEventData<string[]>) dataAsDic["hourWiseData"];
            CdrCollectorInputData collectorInput = (CdrCollectorInputData) dataAsDic["collectorInput"];
            DateTime hourOfDay = hourwiseData.HourOfTheDay;
            int minute = hourOfDay.Minute;
            int second = hourOfDay.Second;
            if (minute != 0 || second != 0)
                throw new Exception("Hour of the day must be 0-23 and can't contain minutes or seconds parts.");

            string tuples = string.Join(",", hourwiseData.Events.Select(r =>
            {
                //Dictionary<string, object> tupGenInput = new Dictionary<string, object>()
                //{
                //    {"collectorInput", collectorInput},
                //    {"row", r}
                //};
                //string tupleExpression = getTupleExpression(tupGenInput);
                string tupleExpression = r[Fn.UniqueBillId];
                return new StringBuilder("'")
                    .Append(tupleExpression)
                    .Append("'");
            }));
            return $@"(tuple in ({tuples}) and {hourOfDay.GetSqlWhereExpressionForHourlyCollection("starttime")}) ";
        }

        public virtual string getTupleExpression(Object data)
        {
            Dictionary<string, object> dataAsDic = (Dictionary<string, object>) data;
            CdrCollectorInputData collectorInput = (CdrCollectorInputData) dataAsDic["collectorInput"];
            CdrSetting cdrSetting = collectorInput.CdrSetting;
            string[] row = (string[]) dataAsDic["row"];
            int switchId = collectorInput.Ne.idSwitch;
            DateTime startTime = getEventDatetime(new Dictionary<string, object>
            {
                {"cdrSetting", cdrSetting},
                {"row", row}
            });
            string sessionId = getSessionId(row);
            string separator = "/";
            return new StringBuilder(switchId.ToString()).Append(separator)
                .Append(startTime.ToMySqlFormatWithoutQuote()).Append(separator)
                .Append(sessionId).ToString();
        }

        public string getGeneratedUniqueEventId(Object data)
        {
            string[] row = (string[])data;
            return row[Fn.UniqueBillId];
        }

        public virtual string getSelectExpressionForUniqueEvent(Object data)
        {
            return @"select tuple ";
        }

        public virtual string getSelectExpressionForPartialCollection(Object data)
        {
            return @"select * ";
        }

        public virtual DateTime getEventDatetime(Object data)
        {
            Dictionary<string, object> dataAsDic = (Dictionary<string, object>) data;
            CdrSetting cdrSetting = (CdrSetting) dataAsDic["cdrSetting"];
            string[] row = (string[]) dataAsDic["row"];
            int timeFieldNo = EventDateTimeHelper.getTimeFieldNo(cdrSetting, row);
            DateTime dateTime = row[timeFieldNo].ConvertToDateTimeFromMySqlFormat();
            return dateTime;
        }

        public object convertDbReaderRowToUniqueEventTuple(object data)
        {
            DbDataReader reader = (DbDataReader) data;
            return new string[] {reader[0].ToString()};
        }

        public object convertDbReaderRowToObject(object data)
        {
            DbDataReader reader = (DbDataReader) data;
            string[] row = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
            {
                object value = reader.GetValue(i);
                row[i] = value != DBNull.Value ? Convert.ToString(value) : "";
            }
            return row;
        }

        public virtual IEventDecoder createNewNonSingletonInstance()
        {
            Type t = this.GetType();
            return (IEventDecoder) Activator.CreateInstance(t);
        }

        public virtual EventAggregationResult Aggregate(object data)
        {
            throw new NotImplementedException();
        }

        private static string getSessionId(string[] row)
        {
            string sessionId = row[Fn.UniqueBillId];
            long sessionIdNum = 0;
            if (sessionId.IsNullOrEmptyOrWhiteSpace() || Int64.TryParse(sessionId, out sessionIdNum) == false)
            {
                throw new Exception("UniquebillId is not in correct format.");
            }
            return sessionId;
        }

        public virtual string getCreateTableSqlForUniqueEvent(Object data)
        {
            return $@"CREATE table if not exists <{this.UniqueEventTablePrefix}> 
                          (tuple varchar(200) COLLATE utf8mb4_bin NOT NULL,
						  starttime datetime NOT NULL,
						  description varchar(50) COLLATE utf8mb4_bin DEFAULT NULL) 
                          ENGINE= innodb DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin ";
        }

        public virtual string getCreateTableSqlForPartialEvent(object data)
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
                  AdditionalMetaData varchar(100)  DEFAULT NULL,
                  Category varchar(100)  DEFAULT NULL,
                  SubCategory varchar(100)  DEFAULT NULL,
                  ChangedByJobId varchar(100)  DEFAULT NULL,
                  SignalingStartTime varchar(100)  DEFAULT NULL,
                  KEY ind_Unique_Bill (UniqueBillId)
                  ) ENGINE=InnoDB ";
        }
    }
}
