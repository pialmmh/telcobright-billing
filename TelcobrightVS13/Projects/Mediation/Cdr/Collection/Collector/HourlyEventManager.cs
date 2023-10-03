using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;
using MySql.Data.MySqlClient;
using TelcobrightInfra;
using TelcobrightMediation.Config;

namespace TelcobrightMediation
{
    public class HourlyEventManager
    {
        public CdrCollectorInputData CollectorInput { get;}
        public DbCommand DbCmd { get;}
        public IFileDecoder Decoder { get; }
        public List<string[]> DecodedCdrRows { get; }
        public Dictionary<string, string[]> DecodedEventsAsTupDic { get; } = new Dictionary<string, string[]>();
        public Dictionary<DateTime, List<string>> HourWiseNewTuples { get; }= new Dictionary<DateTime, List<string>>();
        public string SourceTablePrefix { get; set; }

        public HourlyEventManager(CdrCollectorInputData collectorInput, DbCommand dbCmd,
            IFileDecoder decoder, List<string[]> decodedCdrRows, string sourceTablePrefix)
        {
            this.SourceTablePrefix = sourceTablePrefix;
            CollectorInput = collectorInput;
            DbCmd = dbCmd;
            this.Decoder = decoder;
            this.DecodedCdrRows = decodedCdrRows;
            CdrSetting cdrSetting = this.CollectorInput.CdrSetting;
            foreach (string[] row in this.DecodedCdrRows)
            {
                int timeFieldNo = getTimeFieldNo(cdrSetting, row);
                DateTime datetime = row[timeFieldNo].ConvertToDateTimeFromMySqlFormat();
                DateTime day = datetime.Date;
                int hour = datetime.Hour;
                DateTime roundedHour = day.AddHours(hour);

                List<string> tuplesOfTheHr; //cdrs expressed as tuple like string expressions
                if (this.HourWiseNewTuples.TryGetValue(roundedHour, out tuplesOfTheHr) == false)
                {
                    tuplesOfTheHr = new List<string>();
                    this.HourWiseNewTuples.Add(roundedHour, tuplesOfTheHr);
                }
                string tupleExpressionForRow = this.Decoder.getTupleExpression(CollectorInput, row);
                DecodedEventsAsTupDic.Add(tupleExpressionForRow, row);
                tuplesOfTheHr.Add(tupleExpressionForRow);
            }
        }
        public void createNonExistingTables()
        {
            List<DateTime> hoursInvolved = this.HourWiseNewTuples.Keys.ToList();
            List<DateTime> daysInvolved = hoursInvolved.Select(dateAsHr => dateAsHr.Date).ToList();
            Dictionary<string, DateTime> requiredTableNamesPerDay = daysInvolved //daywise new table, partition by hour
                .Select(day => new
                {
                    Day = day,
                    TableName = this.SourceTablePrefix + "_" + day.ToString("yyyy-MM-dd")
                }).ToDictionary(a => a.TableName, a => a.Day);

            string databaseName = this.CollectorInput.CdrJobInputData.Tbc.DatabaseSetting.DatabaseName;
            this.DbCmd.CommandText = $"show tables from {databaseName} where tables_in_{databaseName} in (" +
                                     $" {string.Join(",",requiredTableNamesPerDay.Select(t=>$"'{t}'"))};";
            this.DbCmd.CommandType = CommandType.Text;
            DbDataReader reader1 = this.DbCmd.ExecuteReader();
            List<string> existingTables = new List<string>();
            try
            {
                while (reader1.Read())
                {
                    existingTables.Add(reader1[0].ToString());
                }
                reader1.Close();
            }
            catch (Exception e)
            {
                reader1.Close();
                Console.WriteLine(e);
                throw;
            }
            List<string> newTablesToBeCreated = requiredTableNamesPerDay.Keys.Where(t => existingTables.Contains(t) == false)
                .ToList();
            List<DateTime> tableDatesToBeCreated =
                newTablesToBeCreated.Select(t => requiredTableNamesPerDay[t]).ToList();
            string templateSql = this.Decoder.getCreateTableSqlForUniqueEvent(this.CollectorInput);
            string tablePrefix = this.Decoder.PartialTablePrefix;
            string tableStorageEngine = this.Decoder.PartialTableStorageEngine;
            var databaseSetting = this.CollectorInput.CdrJobInputData.Tbc.DatabaseSetting;
            string conStr = DbUtil.getDbConStrWithDatabase(databaseSetting);
            //ddl statement may auto commit all transactions
            //so use a different db connection 
            using (MySqlConnection con = new MySqlConnection(conStr))
            {
                con.Open();
                DaywiseTableManager.CreateTables(tablePrefix: tablePrefix,
                    sql: templateSql,
                    dateTimes: tableDatesToBeCreated,
                    con: con,
                    partitionByHour: true, engine: tableStorageEngine, partitionColName:"starttime" );
                con.Close();
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
        string getSqlForDuplicateCollection(DateTime hourOfTheDay, List<string> tuples, IFileDecoder decoder)
        {
            bool tableExists = false;
            string tableName = "zz_uniqueevent" + hourOfTheDay.Date.ToString("yyyyMMdd");
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
                $" and {decoder.getSqlWhereClauseForHourWiseSafeCollection(this.CollectorInput, hourOfTheDay,-1,1)}";
        }
        public List<string> collectExistingEvents()
        {
            string sql = string.Join(" union all ", this.HourWiseNewTuples.Select(kv => this.getSqlForDuplicateCollection(kv.Key, kv.Value, this.Decoder))
                .Where(sqlPerDay => sqlPerDay != "tableDoesNotExist"));
            List<string> existingEvents = new List<string>();
            if (sql != "")
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
            return existingEvents;
        }
    }
}
