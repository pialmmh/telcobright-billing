using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Itenso.TimePeriod;
using LibraryExtensions;
using LibraryExtensions.ConfigHelper;
using MediationModel;
using MySql.Data.MySqlClient;
using TelcobrightInfra;
using TelcobrightMediation.Config;

namespace TelcobrightMediation
{
    public class DayWiseEventCollector<T>
    {
        private readonly object _synchronouslockWhileExecutingDdl = new object();
        public bool UniqueEventsOnly { get; set; }
        public CdrCollectorInputData CollectorInput { get;}
        private DatabaseSetting DatabaseSetting { get; set; }
        private string DatabaseName { get; set; }
        private string ConStr { get; set; }
        public AbstractCdrDecoder Decoder { get; }
        public List<T> InputEvents { get; }
        public Dictionary<string, List<T>> TupleWiseDecodedEvents { get; } 
        public Dictionary<DateTime, Dictionary<DateTime, HourlyEventData<T>>> DayAndHourWiseEvents { get; }
        public List<T> ExistingEventsInDb = new List<T>();
        public string SourceTablePrefix { get; set; }

        public DayWiseEventCollector(bool uniqueEventsOnly, CdrCollectorInputData collectorInput, DbCommand dbCmd,
            AbstractCdrDecoder decoder, List<T> inputEvents, string sourceTablePrefix)
        {
            this.UniqueEventsOnly = uniqueEventsOnly;
            this.SourceTablePrefix = sourceTablePrefix;
            CollectorInput = collectorInput;
            this.DatabaseSetting = this.CollectorInput.CdrJobInputData.Tbc.DatabaseSetting;
            this.DatabaseName = this.DatabaseSetting.DatabaseName;
            this.ConStr = DbUtil.getDbConStrWithDatabase(this.DatabaseSetting);
            this.Decoder = decoder;
            this.InputEvents = inputEvents;
            CdrSetting cdrSetting = this.CollectorInput.CdrSetting;
            var pastHoursToSeekForCollection = cdrSetting.HoursToAddBeforeForSafePartialCollection;
            var nextHoursToSeekForCollection = cdrSetting.HoursToAddAfterForSafePartialCollection;
            this.TupleWiseDecodedEvents = inputEvents.Select(row =>
            {
                var data = new Dictionary<string, object>
                {
                    {"collectorInput", this.CollectorInput},
                    {"row", row}
                };
                return new
                {
                    Tuple = decoder.getTupleExpression(data),
                    Event = row
                };
            }).GroupBy(a=>a.Tuple).ToDictionary(g => g.Key, g=>g.Select(groupEvents=>groupEvents.Event).ToList());

            this.DayAndHourWiseEvents = inputEvents.SelectMany(row =>
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
            using (MySqlConnection con = new MySqlConnection(this.ConStr))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand("", con))
                {
                    foreach (var kv in this.DayAndHourWiseEvents)
                    {
                        DateTime date = kv.Key;
                        string tableName = this.SourceTablePrefix + "_" + date.ToMySqlFormatDateOnlyWithoutTimeAndQuote().Replace("-", "");

                        Dictionary<DateTime, HourlyEventData<T>> hourlyDic = kv.Value;
                        string sql = $@"{decoder.getSelectExpressionForUniqueEvent(this.CollectorInput)} from {tableName} where {Environment.NewLine}";
                        List<string> whereClausesByHour = hourlyDic.Select(kvHour =>
                        {
                            HourlyEventData<T> hourWiseData = kvHour.Value;
                            var data = new Dictionary<string, object>
                            {
                                {"collectorInput", this.CollectorInput},
                                {"hourWiseData", hourWiseData}
                            };
                            return this.Decoder.getWhereForHourWiseCollection(data);
                        }).ToList();
                        sql += string.Join(" or " + Environment.NewLine, whereClausesByHour);

                        List<T> existingEvents = new List<T>();
                        if (whereClausesByHour.Any())
                        {
                            cmd.CommandText = sql;
                            cmd.CommandType = CommandType.Text;
                            DbDataReader reader = cmd.ExecuteReader();
                            try
                            {
                                while (reader.Read())
                                {
                                    if (UniqueEventsOnly)
                                    {
                                        existingEvents.Add(
                                            (T)decoder
                                                .convertDbReaderRowToUniqueEventTuple(reader)); //collect uniqueevent
                                    }
                                    else
                                    {
                                        existingEvents.Add(
                                            (T)decoder
                                                .convertDbReaderRowToUniqueEventTuple(reader)); //collect full event e.g. cdr as string[]
                                    }
                                }
                                reader.Close();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                reader.Close();
                                throw;
                            }
                            finally
                            {
                                reader.Close();
                            }
                        }
                        this.ExistingEventsInDb = existingEvents;
                        if (this.UniqueEventsOnly == true)
                        {
                            checkForDuplicatesAndThrow();
                        }
                    }
                }
                con.Close();
            }
        }

        private void checkForDuplicatesAndThrow()
        {
            Dictionary<T, int> tupleWiseCount = this.ExistingEventsInDb.GroupBy(s => s)
                                    .Select(g => new
                                    {
                                        Tuple = g.Key,
                                        Count = g.Count()
                                    }).ToDictionary(a => a.Tuple, a => a.Count);
            foreach (var tupVsCount in tupleWiseCount)
            {
                T tuple = tupVsCount.Key;
                int count = tupVsCount.Value;
                if (count > 1)
                {
                    string tupleVal = tuple.ToString();
                    if (typeof(T) == typeof(string))
                    {
                        tupleVal = tuple.ToString();
                    }
                    else if (typeof(T) == typeof(string[]))
                    {
                        tupleVal = string.Join(",", tuple as string[]);
                    }
                    throw new Exception($"tuple {tupleVal} has more than one previous instances ({count})");
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
                    TableName = this.SourceTablePrefix + "_" +
                                day.ToMySqlFormatDateOnlyWithoutTimeAndQuote().Replace("-", "")
                }).ToDictionary(a => a.TableName, a => a.Day);
            //ddl statement may auto commit all transactions, so use a different db connection 
            using (MySqlConnection con = new MySqlConnection(this.ConStr))
            {
                con.Open();
                List<string> existingTables =
                    DaywiseTableManager.getExistingTableNames(this.DatabaseName, requiredTableNamesPerDay.Keys, con);
                List<string> newTablesToBeCreated = requiredTableNamesPerDay.Keys
                    .Where(t => existingTables.Contains(t) == false)
                    .ToList();
                List<DateTime> tableDatesToBeCreated =
                    newTablesToBeCreated.Select(t => requiredTableNamesPerDay[t]).ToList();
                string templateSql = this.Decoder.getCreateTableSqlForUniqueEvent(this.CollectorInput);
                string tablePrefix = this.Decoder.UniqueEventTablePrefix;
                string tableStorageEngine = this.Decoder.PartialTableStorageEngine;

                lock (_synchronouslockWhileExecutingDdl)
                {
                    if (tableDatesToBeCreated.Any())
                    {
                        DaywiseTableManager.CreateTables(tablePrefix: tablePrefix,
                            templateSql: templateSql,
                            dateTimes: tableDatesToBeCreated,
                            con: con,
                            partitionByHour: true, engine: tableStorageEngine, partitionColName: "starttime");
                    }
                }
                con.Close();
            }
        }
    }
}
