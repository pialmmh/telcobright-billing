using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
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
using TelcobrightInfra;
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
            Dictionary<string, string[]> decodedEventsAsTupDic = new Dictionary<string, string[]>();
            NewCdrPreProcessor textCdrCollectionPreProcessor = null;
            if (CollectorInput.Ne.FilterDuplicateCdr == 1 && decodedCdrRows.Count > 0) //filter duplicates
            {
                Dictionary<string, string[]> finalNonDuplicateEvents =
                    filterDuplicateCdrs(decoder, decodedCdrRows, decodedEventsAsTupDic);
                textCdrCollectionPreProcessor =
                    new NewCdrPreProcessor(finalNonDuplicateEvents.Values.ToList(), cdrinconsistents, this.CollectorInput);
                textCdrCollectionPreProcessor.FinalNonDuplicateEvents = finalNonDuplicateEvents;
            }
            else//duplicate check not required
            {
                textCdrCollectionPreProcessor =
                    new NewCdrPreProcessor(decodedCdrRows, cdrinconsistents, this.CollectorInput);
            }
            if (textCdrCollectionPreProcessor == null)
            {
                throw new Exception("textCdrCollectionPreProcessor cannot be null");
            }
            return textCdrCollectionPreProcessor;
        }

        private Dictionary<string, string[]> filterDuplicateCdrs(IFileDecoder decoder, List<string[]> decodedCdrRows,
            Dictionary<string, string[]> decodedEventsAsTupDic)
        {
            Dictionary<DateTime, List<string>> dayWiseNewTuples = new Dictionary<DateTime, List<string>>();
            CdrSetting cdrSetting = this.CollectorInput.CdrSetting;
            Dictionary<string, string[]> finalNonDuplicateEvents = new Dictionary<string, string[]>();
            foreach (string[] row in decodedCdrRows)
            {
                int timeFieldNo = getTimeFieldNo(cdrSetting, row);
                DateTime thisDate = row[timeFieldNo].ConvertToDateTimeFromMySqlFormat().Date;
                List<string> tuplesOfTheDay; //cdrs expressed as tuple like string expressions
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
                = (day, tuples) =>
                {
                    bool tableExists = false;
                    string tableName = "uniqueevent" + day.Date.ToString("yyyyMMdd");
                    string databaseName = this.CollectorInput.CdrJobInputData.Tbc.DatabaseSetting.DatabaseName;
                    this.DbCmd.CommandText = $"show tables from {databaseName} where tables_in_{databaseName}='{tableName}';";
                    this.DbCmd.CommandType = CommandType.Text;
                    DbDataReader reader1 = this.DbCmd.ExecuteReader();
                    string existingTable = "";
                    while (reader1.Read())
                    {
                        //existingEvents.Add(reader[0].ToString());
                        existingTable = reader1[0].ToString();
                        tableExists = tableName == existingTable;
                    }
                    reader1.Close();
                    if (tableExists == false)
                    {
                        return "tableDoesNotExist";
                    }
                    return
                        $" select tuple from {tableName} where tuple " +
                        $" in ({string.Join(",", tuples.Select(t => new StringBuilder("'").Append(t).Append("'")))}) " +
                        $" and {decoder.getSqlWhereClauseForDayWiseSafeCollection(this.CollectorInput, day)}";
                };

            string sql = string.Join(" union all ", dayWiseNewTuples.Select(kv => getSqlPerDay(kv.Key, kv.Value))
                .Where(sqlPerDay=>sqlPerDay!= "tableDoesNotExist"));
            List<string> newEvents = dayWiseNewTuples.SelectMany(kv => kv.Value).ToList();
            List<string> existingEvents = new List<string>();
            if (sql!="")
            {
                this.DbCmd.CommandText = sql;
                this.DbCmd.CommandType = CommandType.Text;
                DbDataReader reader = this.DbCmd.ExecuteReader();
                while (reader.Read())
                {
                    existingEvents.Add(reader[0].ToString());
                }
                reader.Close();
            }


            Dictionary<string, string> alreadyConsideredEvents = existingEvents.ToDictionary(e => e);
            foreach (var kv in decodedEventsAsTupDic)
            {
                string tuple = kv.Key;
                string[] decodedRow = kv.Value;
                if (alreadyConsideredEvents.ContainsKey(tuple) == false)
                {
                    finalNonDuplicateEvents.Add(tuple, decodedRow);
                    alreadyConsideredEvents.Add(tuple, tuple); //it's just used like hashmap
                }
                else
                {
                    //cdr is skipped
                    Console.WriteLine("Skipped duplicate cdrs");
                }
            }

            //create uniqueevent tables for each date
            List<DateTime> datesToCreateTable = dayWiseNewTuples.Keys.ToList();
            string templateSql = @"CREATE TABLE uniqueevent (
                                      tuple varchar(200) COLLATE utf8mb4_bin NOT NULL,
                                      StartTime datetime NOT NULL,
                                      description varchar(50) COLLATE utf8mb4_bin DEFAULT NULL,
                                      UNIQUE KEY ind_tuple (tuple)
                                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;";
            var databaseSetting = this.CollectorInput.CdrJobInputData.Tbc.DatabaseSetting;
            string conStr = DbUtil.getDbConStrWithDatabase(databaseSetting);
            //ddl statement may auto commit all transactions
            //so use a different db connection 
            using (MySqlConnection con= new MySqlConnection(conStr))
            {
                con.Open();
                DaywiseTableManager.CreateTables("uniqueevent", templateSql, datesToCreateTable, con);
                con.Close();
            }
            return finalNonDuplicateEvents;
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
