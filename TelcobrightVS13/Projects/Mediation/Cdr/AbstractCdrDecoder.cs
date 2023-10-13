using System;
using System.Collections.Generic;
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
        public abstract string PartialTablePrefix { get; }
        public abstract string PartialTableStorageEngine { get; }
        public abstract string partialTablePartitionColName { get; }
        public abstract List<string[]> DecodeFile(CdrCollectorInputData decoderInputData, out List<cdrinconsistent> inconsistentCdrs);

        public virtual string getCreateTableSqlForUniqueEvent(Object data)
        {
            return $@"CREATE table if not exists <{this.PartialTablePrefix}> (tuple varchar(200) COLLATE utf8mb4_bin NOT NULL,
						  starttime datetime NOT NULL,
						  description varchar(50) COLLATE utf8mb4_bin DEFAULT NULL,
						  UNIQUE KEY ind_tuple (tuple)) 
                          ENGINE= innodb DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin ";
        }

        public virtual string getSelectExpressionForUniqueEvent(Object data)
        {
            return @"select tuple ";
        }

        public virtual string getWhereForHourWiseCollection(Object data)
        {
            HourlyEventData<string[]> hourwiseData = (HourlyEventData<string[]>)data;
            DateTime hourOfDay = hourwiseData.HourOfTheDay;
            int minute = hourOfDay.Minute;
            int second = hourOfDay.Second;
            if (minute != 0 || second != 0)
                throw new Exception("Hour of the day must be 0-23 and can't contain minutes or seconds parts.");
            string whereClauses = "";
            string tuples = string.Join(",", hourwiseData.Events.Select(r => r[Fn.UniqueBillId]));
            return $@"tuple in ({tuples}) and {hourOfDay.GetSqlWhereExpressionForHourlyCollection("starttime")}";
        }


        public abstract string getSelectExpressionForPartialCollection(Object data);

        public virtual DateTime getEventDatetime(Object data)
        {
            Dictionary<string, object> dataAsDic = (Dictionary<string, object>)data;
            CdrSetting cdrSetting = (CdrSetting)dataAsDic["cdrSetting"];
            string[] row = (string[])dataAsDic["row"];
            int timeFieldNo = EventDateTimeHelper.getTimeFieldNo(cdrSetting, row);
            DateTime dateTime = row[timeFieldNo].ConvertToDateTimeFromMySqlFormat();
            return dateTime;
        }

        public virtual string getTupleExpression(CdrCollectorInputData decoderInputData, string[] row)
        {
            CdrSetting cdrSetting = decoderInputData.CdrSetting;
            int switchId = decoderInputData.Ne.idSwitch;
            DateTime startTime = getEventDatetime(new Dictionary<string, object>
            {
                {"cdrSetting",cdrSetting },
                {"row",row }
            });
            string sessionId = getSessionId(row);
            string separator = "/";
            return new StringBuilder(switchId.ToString()).Append(separator)
                .Append(startTime.ToMySqlFormatWithoutQuote()).Append(separator)
                .Append(sessionId).ToString();
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
    }
}
