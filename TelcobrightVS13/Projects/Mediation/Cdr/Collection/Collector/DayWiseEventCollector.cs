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
    public class DayWiseEventCollector<T>
    {
        public bool UniqueEventsOnly { get; set; }
        public CdrCollectorInputData CollectorInput { get;}
        public DbCommand DbCmd { get;}
        public AbstractCdrDecoder Decoder { get; }
        public List<T> DecodedEvents { get; }
        public Dictionary<string, List<T>> TupleWiseDecodedEvents { get; } 
        public Dictionary<DateTime, Dictionary<DateTime, HourlyEventData<T>>> DayAndHourWiseEvents { get; }
        public List<string> ExistingTuples = new List<string>();
        public string SourceTablePrefix { get; set; }

        public DayWiseEventCollector(bool uniqueEventsOnly, CdrCollectorInputData collectorInput, DbCommand dbCmd,
            AbstractCdrDecoder decoder, List<T> decodedEvents, string sourceTablePrefix)
        {
            this.UniqueEventsOnly = uniqueEventsOnly;
            this.SourceTablePrefix = sourceTablePrefix;
            CollectorInput = collectorInput;
            DbCmd = dbCmd;
            this.Decoder = decoder;
            this.DecodedEvents = decodedEvents;
            CdrSetting cdrSetting = this.CollectorInput.CdrSetting;
            var pastHoursToSeekForCollection = cdrSetting.HoursToAddBeforeForSafePartialCollection;
            var nextHoursToSeekForCollection = cdrSetting.HoursToAddAfterForSafePartialCollection;
            this.TupleWiseDecodedEvents = decodedEvents.Select(e =>
            {
                var data = new Dictionary<string, object>
                {
                    {"collectorInput", this.CollectorInput},
                    {"row", e}
                };
                return new
                {
                    Tuple = decoder.getTupleExpression(data),
                    Event = e
                };
            }).GroupBy(a=>a.Tuple).ToDictionary(g => g.Key, g=>g.Select(groupEvents=>groupEvents.Event).ToList());
            this.DayAndHourWiseEvents = decodedEvents.SelectMany(row =>
            {
                DateTime dateTime= this.Decoder.getEventDatetime(new Dictionary<string,object>
                {
                    {"cdrSetting",cdrSetting },
                    {"row",row }
                });
                DateTime date = dateTime.Date;
                int hour = dateTime.Hour;
                DateTime hourOfTheDay = date.AddHours(hour);
                List<DateTime> pastHoursToScan = Enumerable.Range(1, pastHoursToSeekForCollection)
                    .Select(num => hourOfTheDay.AddHours((-1) * num)).ToList();
                List<DateTime> nextHoursToScan = Enumerable.Range(1, nextHoursToSeekForCollection)
                    .Select(num => hourOfTheDay.AddHours(num)).ToList();
                List<DateTime> hoursInvolved = pastHoursToScan;
                hoursInvolved.Add(hourOfTheDay);
                hoursInvolved.AddRange(nextHoursToScan);

                int min = date.Minute;
                int sec = date.Second;
                if (min != 0 || sec != 0) throw new Exception("Hour, minute and second parts must be zero in date value for daywise collection. ");

                return hoursInvolved.Select(h => new //item
                {
                    Date = h.Date,
                    HourOftheDay = h,
                    Row = row
                });
            }).GroupBy(a => a.Date).ToDictionary(g => g.Key,
                    g => g.GroupBy(b => b.HourOftheDay).ToDictionary(g2 => g2.Key,
                            g2 => new HourlyEventData<T>(g2.Select(i => i.Row).ToList(), g2.Key)));
        }

        public void collectTupleWiseExistingEvents(AbstractCdrDecoder decoder)
        {
            //List<DateTime> daysInvolved = this.DayWiseHourlyEvents.Keys.ToList();
            foreach (var kv in this.DayAndHourWiseEvents)
            {
                DateTime date = kv.Key;
                string tableName = this.SourceTablePrefix + "_" + date.ToMySqlFormatDateOnlyWithoutTimeAndQuote().Replace("-", "");

                Dictionary<DateTime, HourlyEventData<T>> hourlyDic = kv.Value;
                string sql = $@"{decoder.getSelectExpressionForUniqueEvent(this.CollectorInput)} from {tableName} where {Environment.NewLine}";
                List<string> whereClausesByHour= hourlyDic.Select(kvHour =>
                {
                    HourlyEventData<T> hourWiseData = kvHour.Value;
                    var data = new Dictionary<string, object>
                    {
                        {"collectorInput", this.CollectorInput},
                        {"hourWiseData", hourWiseData}
                    };
                    return this.Decoder.getWhereForHourWiseCollection(data);
                }).ToList();
                sql += string.Join(" or " +Environment.NewLine, whereClausesByHour);

                List<string> existingEvents = new List<string>();
                if (whereClausesByHour.Any())
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
                this.ExistingTuples = existingEvents;
                if (this.UniqueEventsOnly==true)
                {
                    Dictionary<string, int> tupleWiseCount = this.ExistingTuples.GroupBy(s => s)
                        .Select(g => new
                        {
                            Tuple = g.Key,
                            Count = g.Count()
                        }).ToDictionary(a => a.Tuple, a => a.Count);
                    foreach (var tupVsCount in tupleWiseCount)
                    {
                        string tuple = tupVsCount.Key;
                        int count = tupVsCount.Value;
                        if(count>1) throw new Exception($"tuple {tuple} has more than one previous instances ({count})");
                    }
                }
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
                    TableName = this.SourceTablePrefix + "_" + day.ToMySqlFormatDateOnlyWithoutTimeAndQuote().Replace("-","")
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
            //ddl statement may auto commit all transactions, so use a different db connection 
            using (MySqlConnection con = new MySqlConnection(conStr))
            {
                con.Open();
                DaywiseTableManager.CreateTables(tablePrefix: tablePrefix,
                    templateSql: templateSql,
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
                                     $" {string.Join(",", requiredTableNamesPerDay.Keys.Select(t => $"'{t}'"))});";
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

      
    }
}
