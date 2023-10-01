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
    public class DayAndHourWiseCollector
    {
        public CdrCollectorInputData CollectorInput { get;}
        public DbCommand DbCmd { get;}
        public IFileDecoder Decoder { get; }
        public List<string[]> DecodedCdrRows { get; }
        public Dictionary<string, string[]> DecodedEventsAsTupDic { get; } = new Dictionary<string, string[]>();
        public Dictionary<DateTime, List<string>> DayWiseNewTuples { get; }= new Dictionary<DateTime, List<string>>();
        public DayAndHourWiseCollector(CdrCollectorInputData collectorInput, DbCommand dbCmd,
            IFileDecoder decoder, List<string[]> decodedCdrRows)
        {
            CollectorInput = collectorInput;
            DbCmd = dbCmd;
            this.Decoder = decoder;
            this.DecodedCdrRows = decodedCdrRows;
            CdrSetting cdrSetting = this.CollectorInput.CdrSetting;
            foreach (string[] row in this.DecodedCdrRows)
            {
                int timeFieldNo = getTimeFieldNo(cdrSetting, row);
                DateTime thisDate = row[timeFieldNo].ConvertToDateTimeFromMySqlFormat().Date;
                List<string> tuplesOfTheDay; //cdrs expressed as tuple like string expressions
                if (this.DayWiseNewTuples.TryGetValue(thisDate, out tuplesOfTheDay) == false)
                {
                    tuplesOfTheDay = new List<string>();
                    this.DayWiseNewTuples.Add(thisDate, tuplesOfTheDay);
                }
                string tupleExpressionForRow = this.Decoder.getTupleExpression(CollectorInput, row);
                DecodedEventsAsTupDic.Add(tupleExpressionForRow, row);
                tuplesOfTheDay.Add(tupleExpressionForRow);
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
        public string getSqlPerDay(DateTime hourOfTheDay, List<string> tuples, IFileDecoder decoder)
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
                $" and {decoder.getSqlWhereClauseForDayWiseSafeCollection(this.CollectorInput, hourOfTheDay)}";
        }
        public List<string> collectExistingEvents()
        {
            string sql = string.Join(" union all ", this.DayWiseNewTuples.Select(kv => this.getSqlPerDay(kv.Key, kv.Value, this.Decoder))
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
