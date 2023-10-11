using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Itenso.TimePeriod;
using LibraryExtensions;
using MediationModel;
using MySql.Data.MySqlClient;
using TelcobrightInfra;
using TelcobrightMediation.Config;

namespace TelcobrightMediation
{
    public class DayWiseEventCollector
    {
        public CdrCollectorInputData CollectorInput { get;}
        public DbCommand DbCmd { get;}
        public IFileDecoder Decoder { get; }
        public List<string[]> DecodedCdrRows { get; }
        public Dictionary<string, string[]> DecodedEventsAsTupDic { get; } = new Dictionary<string, string[]>();
        public Dictionary<DateTime, Dictionary<DateTime, HourlyEventData<string[]>>> DayAndHourWiseEvents { get; }
        public List<string> ExistingEvents = new List<string>();
        public string SourceTablePrefix { get; set; }
        public DayWiseEventCollector(CdrCollectorInputData collectorInput, DbCommand dbCmd,
            IFileDecoder decoder, List<string[]> decodedCdrRows, string sourceTablePrefix)
        {
            this.SourceTablePrefix = sourceTablePrefix;
            CollectorInput = collectorInput;
            DbCmd = dbCmd;
            this.Decoder = decoder;
            this.DecodedCdrRows = decodedCdrRows;
            CdrSetting cdrSetting = this.CollectorInput.CdrSetting;
            var pastHoursToSeekForCollection = cdrSetting.HoursToAddBeforeForSafePartialCollection;
            var nextHoursToSeekForCollection = cdrSetting.HoursToAddAfterForSafePartialCollection;
            //this.DayAndHourWiseEvents = 
            var x = decodedCdrRows.SelectMany(row =>
                {
                    int timeFieldNo = getTimeFieldNo(cdrSetting, row);
                    DateTime dateTime = row[timeFieldNo].ConvertToDateTimeFromMySqlFormat();
                    int hour = dateTime.Hour;
                    DateTime hourOfTheDay = dateTime.AddHours(hour);
                    List<DateTime> pastHoursToScan = Enumerable.Range(1, pastHoursToSeekForCollection)
                        .Select(num => hourOfTheDay.AddHours((-1) * num)).ToList();
                    List<DateTime> nextHoursToScan = Enumerable.Range(1, nextHoursToSeekForCollection)
                        .Select(num => hourOfTheDay.AddHours(num)).ToList();
                    List<DateTime> hoursInvolved = pastHoursToScan;
                    hoursInvolved.Add(hourOfTheDay);
                    hoursInvolved.AddRange(nextHoursToScan);

                    return hoursInvolved.Select(h => new //item
                    {
                        Date = h.Date,
                        HourOftheDay = h,
                        Row = row
                    });
                }).GroupBy(a => a.Date)
                .ToDictionary(g => g.Key, 
                                   g=>g.GroupBy(b=>b.HourOftheDay)
                                   .ToDictionary(g2=>g2.Key, g2=> new HourlyEventData<string[]>(g2.Select(i => i.Row).ToList(),g2.Key)));

            //.ToDictionary(g => g.Date, g =>
            //{
            //    return g.Rows.ToDictionary(r => r.HourOftheDay,);
            //});

        }
        public void collectExistingEvents(IFileDecoder decoder)
        {
            //List<DateTime> daysInvolved = this.DayWiseHourlyEvents.Keys.ToList();
            foreach (var kv in this.DayAndHourWiseEvents)
            {
                DateTime date = kv.Key;
                //HourlyEventData<string[]> hourlyData = kv.Value;

                //hourlyData.

                string tableName = this.SourceTablePrefix + date.ToMySqlFormatDateOnlyWithoutTimeAndQuote();
                //string selectExpression =
                //    $@"{decoder.getSelectExpressionForUniqueEvent(this.CollectorInput)} from {tableName} 
                //        where {decoder.getWhereForHourWiseUniqueEventCollection(this.CollectorInput), } ";
                string sql="";
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
                this.ExistingEvents = existingEvents;
            }

        }
        public void createNonExistingTables()
        {
            List<DateTime> daysInvolved = this.DayAndHourWiseEvents.Keys.ToList();
            //List<DateTime> hoursInvolved = this.DayAndHourWiseEvents.Select(dw => dw.Value.HourOfTheDay).ToList();
            Dictionary<string, DateTime> requiredTableNamesPerDay = daysInvolved //daywise new table, partition by hour
                .Select(day => new
                {
                    Day = day,
                    TableName = this.SourceTablePrefix + "_" + day.ToMySqlFormatDateOnlyWithoutTimeAndQuote()
                }).ToDictionary(a => a.TableName, a => a.Day);
            List<string> existingTables = getExistingTables(requiredTableNamesPerDay);
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
                    partitionByHour: true, engine: tableStorageEngine, partitionColName: "starttime");
                con.Close();
            }
        }

        private List<string> getExistingTables(Dictionary<string, DateTime> requiredTableNamesPerDay)
        {
            string databaseName = this.CollectorInput.CdrJobInputData.Tbc.DatabaseSetting.DatabaseName;
            this.DbCmd.CommandText = $"show tables from {databaseName} where tables_in_{databaseName} in (" +
                                     $" {string.Join(",", requiredTableNamesPerDay.Select(t => $"'{t}'"))};";
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

            return existingTables;
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
