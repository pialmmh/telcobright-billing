using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using LibraryExtensions;
using MediationModel;
using MySql.Data.MySqlClient;
using TelcobrightFileOperations;
using TelcobrightInfra;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;
using TelcobrightMediation.Mediation.Cdr;

namespace TelcobrightMediation
{
    public class FileBasedTextCdrCollector : IEventCollector
    {
        public CdrCollectorInputData CollectorInput { get; set; }
        public Dictionary<string, object> Params { get; set; }
        private DbCommand DbCmd;

        public FileBasedTextCdrCollector(CdrCollectorInputData collectorInput)
        {
            this.CollectorInput = collectorInput;
            this.DbCmd = ConnectionManager.CreateCommandFromDbContext(this.CollectorInput.Context);
            this.DbCmd.CommandType = CommandType.Text;
        }
        public object Collect()
        {
            IFileDecoder decoder = null;
            this.CollectorInput.MefDecodersData.DicExtensions.TryGetValue(this.CollectorInput.Ne.idcdrformat,
                out decoder);
            if (decoder == null)
            {
                throw new Exception("No suitable file decoder found for cdrformat: " +
                                    this.CollectorInput.Ne.idcdrformat
                                    + " and Ne:" + this.CollectorInput.Ne.idSwitch);
            }
            List<cdrinconsistent> cdrinconsistents = new List<cdrinconsistent>();
            List<string[]> decodedCdrRows = decoder.DecodeFile(this.CollectorInput, out cdrinconsistents); //collect
            Dictionary<string, string[]> decodedEventsAsTupDic= new Dictionary<string, string[]>();
            Dictionary<string, string[]> finalNonDuplicateEvents = new Dictionary<string, string[]>();
            if (CollectorInput.Tbc.CdrSetting.FilterDuplicates == true && decodedCdrRows.Count>0)
            {
                filterDuplicateCdrs(decoder, decodedCdrRows, decodedEventsAsTupDic, finalNonDuplicateEvents);
            }
            NewCdrPreProcessor textCdrCollectionPreProcessor =
                new NewCdrPreProcessor(finalNonDuplicateEvents.Values.ToList(), cdrinconsistents, this.CollectorInput);
            textCdrCollectionPreProcessor.FinalNonDuplicateEvents = finalNonDuplicateEvents;
            return textCdrCollectionPreProcessor;
        }

        private void filterDuplicateCdrs(IFileDecoder decoder, List<string[]> decodedCdrRows, Dictionary<string, string[]> decodedEventsAsTupDic, Dictionary<string, string[]> finalNonDuplicateEvents)
        {
            Dictionary<DateTime, List<string>> dayWiseNewTuples = new Dictionary<DateTime, List<string>>();
            foreach (string[] row in decodedCdrRows)
            {
                CdrSetting cdrSetting = this.CollectorInput.CdrSetting;
                int timeFieldNo = getTimeFieldNo(cdrSetting, row);
                DateTime thisDate = row[timeFieldNo].ConvertToDateTimeFromMySqlFormat().Date;
                List<string> tuplesOfTheDay;//cdrs expressed as tuple like string expressions
                if (dayWiseNewTuples.TryGetValue(thisDate, out tuplesOfTheDay) == false)
                {
                    tuplesOfTheDay = new List<string>();
                    dayWiseNewTuples.Add(thisDate, tuplesOfTheDay);
                }
                string tupleExpressionForRow = decoder.getTupleExpression(CollectorInput, row);
                decodedEventsAsTupDic.Add(tupleExpressionForRow, row);
                tuplesOfTheDay.Add(tupleExpressionForRow);
            }
            Func<DateTime, List<string>, string> getSqlPerDay
                = (day, tuples) => $" select tuple from uniqueevent where tuple " +
                                   $" in ({string.Join(",", tuples.Select(t=>new StringBuilder("'").Append(t).Append("'")))}) " +
                                   $" and {decoder.getSqlWhereClauseForDayWiseSafeCollection(this.CollectorInput, day)}";

            string sql = string.Join(" union all ", dayWiseNewTuples.Select(kv => getSqlPerDay(kv.Key, kv.Value)));
            List<string> newEvents = dayWiseNewTuples.SelectMany(kv => kv.Value).ToList();
            List<string> existingEvents = new List<string>();
            this.DbCmd.CommandText = sql;
            this.DbCmd.CommandType = CommandType.Text;
            DbDataReader reader = this.DbCmd.ExecuteReader();
            while (reader.Read())
            {
                existingEvents.Add(reader[0].ToString());
            }
            reader.Close();
            Dictionary<string, string> alreadyConsideredEvents = existingEvents.ToDictionary(e => e);
            foreach (var kv in decodedEventsAsTupDic)
            {
                string tuple = kv.Key;
                string[] decodedRow = kv.Value;
                if (alreadyConsideredEvents.ContainsKey(kv.Key) == false)
                {
                    finalNonDuplicateEvents.Add(tuple, decodedRow);
                    alreadyConsideredEvents.Add(tuple, tuple);//it's just used like hashmap
                }
            }
        }

        private static int getTimeFieldNo(CdrSetting cdrSettings, string[] row)
        {
            int timeFieldNo = -1;
            switch (cdrSettings.SummaryTimeField)
            {
                case SummaryTimeFieldEnum.StartTime:
                    timeFieldNo = Fn.StartTime;
                    break;
                case SummaryTimeFieldEnum.AnswerTime:
                    timeFieldNo = Fn.AnswerTime;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return timeFieldNo;
        }

    }
}
