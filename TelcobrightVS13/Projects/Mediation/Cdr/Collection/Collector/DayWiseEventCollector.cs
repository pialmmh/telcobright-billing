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
        Dictionary<DateAndHour, List<T>> DateHourWiseEvents { get; }
        public List<T> ExistingEventsInDb = new List<T>();
        public string SourceTablePrefix { get; set; }
        public NeAdditionalSetting NeAdditionalSetting { get; set; }
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
            this.NeAdditionalSetting = collectorInput.CdrJobInputData.NeAdditionalSetting;
            var secondsBeforeForOldEventScan = cdrSetting.SecondsBeforeForOldEventScan;
            var secondsAfterForOldEventScan = cdrSetting.SecondsAfterForOldEventScan;
            
            this.TupleWiseDecodedEvents = inputEvents.Select(row => new
            {
                Tuple = decoder.getGeneratedUniqueEventId(row),
                Event = row
            }).GroupBy(a=>a.Tuple).ToDictionary(g => g.Key, g=>g.Select(groupEvents=>groupEvents.Event).ToList());

            this.DateHourWiseEvents=
                    inputEvents.SelectMany(row =>
                    {
                        DateTime dateTime = this.Decoder.getEventDatetime(new Dictionary<string, object>
                        {
                            {"cdrSetting",cdrSetting },
                            {"row",row }
                        });
                        DateTime date = dateTime.Date;
                        int hour = dateTime.Hour;
                        //DateTime hourOfTheDay = date.AddHours(hour);

                        var beginTimeToSearchForEvent = dateTime.AddSeconds(-1 * secondsBeforeForOldEventScan);
                        var endTimeToSearchForEvent = dateTime.AddSeconds(secondsAfterForOldEventScan);
                        var range = new DateRange(beginTimeToSearchForEvent, endTimeToSearchForEvent);
                        List<DateTime> hoursInvolved = range.GetInvolvedHours();
                        return hoursInvolved.Select(hi => new
                        {
                            DateAndHour = new DateAndHour(hi.Date, hi.Hour),
                            Row = row
                        });
                    }).GroupBy(a => a.DateAndHour).ToDictionary(g => g.Key, g => g.Select(a=>a.Row).ToList());
        }

        //public void collectTupleWiseExistingEvents(AbstractCdrDecoder decoder)
        //{
        //    //List<DateTime> daysInvolved = this.DayWiseHourlyEvents.Keys.ToList();
        //    using (MySqlConnection con = new MySqlConnection(this.ConStr))
        //    {
        //        con.Open();
        //        using (MySqlCommand cmd = new MySqlCommand("", con))
        //        {
        //            foreach (var kv in this.DayAndHourWiseEvents)
        //            {
        //                DateTime date = kv.Key;
        //                string tableName = this.SourceTablePrefix + "_" + date.ToMySqlFormatDateOnlyWithoutTimeAndQuote().Replace("-", "");

        //                Dictionary<DateTime, HourlyEventData<T>> hourlyDic = kv.Value;
        //                string selectExpression = this.UniqueEventsOnly? decoder.getSelectExpressionForUniqueEvent(this.CollectorInput)
        //                    :decoder.getSelectExpressionForPartialCollection(null);
        //                string sql = $@"{selectExpression} from {tableName} where {Environment.NewLine}";

        //                List<string> whereClausesByHour = hourlyDic.Select(kvHour =>
        //                {
        //                    HourlyEventData<T> hourWiseData = kvHour.Value;
        //                    var data = new Dictionary<string, object>
        //                    {
        //                        {"collectorInput", this.CollectorInput},
        //                        {"hourWiseData", hourWiseData}
        //                    };
        //                    return this.Decoder.getWhereForHourWiseCollection(data);
        //                }).ToList();
        //                sql += string.Join(" or " + Environment.NewLine, whereClausesByHour);
        //                if (this.UniqueEventsOnly == false)
        //                {
        //                    sql = sql.Replace("tuple in", "uniquebillid in").Replace("tuple =", "uniquebillid =");
        //                }

        //                List<T> existingEvents = new List<T>();
        //                if (whereClausesByHour.Any())
        //                {
        //                    if (UniqueEventsOnly) //collect unique events only
        //                    {
        //                        cmd.CommandText = sql;
        //                        cmd.CommandType = CommandType.Text;
        //                        DbDataReader reader = cmd.ExecuteReader();
        //                        try
        //                        {
        //                            while (reader.Read())
        //                            {
        //                                existingEvents.Add(
        //                                    (T) decoder
        //                                        .convertDbReaderRowToUniqueEventTuple(reader)); //collect uniqueevent
        //                            }
        //                            reader.Close();
        //                        }
        //                        catch (Exception e)
        //                        {
        //                            Console.WriteLine(e);
        //                            reader.Close();
        //                            throw;
        //                        }
        //                        finally
        //                        {
        //                            reader.Close();
        //                        }
        //                    }
        //                    else //collect partial events
        //                    {
        //                        CdrRowCollector<cdrerror> dbRowCollector =
        //                            new CdrRowCollector<cdrerror>(this.CollectorInput.CdrJobInputData, sql);
        //                        existingEvents = (List<T>) dbRowCollector.CollectAsTxtRows(cmd);
        //                    }
        //                }
        //                this.ExistingEventsInDb = existingEvents;
        //                if (this.UniqueEventsOnly == true)
        //                {
        //                    checkForDuplicatesAndThrow();
        //                }
        //            }
        //        }
        //        con.Close();
        //    }
        //}
        public void collectTupleWiseExistingEvents(AbstractCdrDecoder decoder)
        {
            //List<DateTime> daysInvolved = this.DayWiseHourlyEvents.Keys.ToList();
            using (MySqlConnection con = new MySqlConnection(this.ConStr))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand("", con))
                {
                    foreach (var kv in this.DateHourWiseEvents)
                    {
                        DateTime date = kv.Key.Date;
                        int hour = kv.Key.Hour;
                        List<T> singleHourRows = kv.Value;

                        string tableName = this.SourceTablePrefix + "_" + date.ToMySqlFormatDateOnlyWithoutTimeAndQuote().Replace("-", "") +
                            " partition (p" + (hour + 1) + ")";

                        string selectExpression = this.UniqueEventsOnly ? decoder.getSelectExpressionForUniqueEvent(this.CollectorInput)
                            : decoder.getSelectExpressionForPartialCollection(null);
                        string sqlForThisHour = $@"{selectExpression} from {tableName} where {Environment.NewLine}";
                        Dictionary<string, object> input = new Dictionary<string, object>
                        {
                            { "singleHourRows", singleHourRows},
                        };
                        string whereClausesByBillId = decoder.getWhereForHourWiseCollection(input);
                        sqlForThisHour += string.Join(" or " + Environment.NewLine, whereClausesByBillId);
                        if (this.UniqueEventsOnly == false)
                        {
                            sqlForThisHour = sqlForThisHour.Replace("tuple in", "uniquebillid in").Replace("tuple =", "uniquebillid =");
                        }
                        List<T> existingEvents = fetchOldEventsByHour(decoder, cmd, sqlForThisHour, whereClausesByBillId);
                        this.ExistingEventsInDb.AddRange(existingEvents);
                        if (this.UniqueEventsOnly == true)
                        {
                            checkForDuplicatesAndThrow();
                        }
                    }
                }
                con.Close();
            }
        }

        private List<T> fetchOldEventsByHour(AbstractCdrDecoder decoder, MySqlCommand cmd, string sqlForThisHour, string whereClausesByBillId)
        {
            List<T> existingEvents = new List<T>();
            if (whereClausesByBillId.Any())
            {
                if (UniqueEventsOnly) //collect unique events only
                {
                    cmd.CommandText = sqlForThisHour;
                    cmd.CommandType = CommandType.Text;
                    DbDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        while (reader.Read())
                        {
                            existingEvents.Add(
                                (T)decoder
                                    .convertDbReaderRowToUniqueEventTuple(reader)); //collect uniqueevent
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
                else //collect partial events
                {
                    CdrRowCollector<cdrerror> dbRowCollector =
                        new CdrRowCollector<cdrerror>(this.CollectorInput.CdrJobInputData, sqlForThisHour);
                    existingEvents = (List<T>)dbRowCollector.CollectAsTxtRows(cmd);
                }
            }
            return existingEvents;
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
            //List<DateTime> daysInvolved = this.DayAndHourWiseEvents.Keys.ToList();
            List<DateTime> daysInvolved = this.DateHourWiseEvents.Keys.Select(dh => dh.Date).Distinct().ToList();
            // List<DateTime> hoursInvolved = this.DateHourWiseEvents.Select(dw => dw.Key.Date).ToList();
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
                List<string> existingTables = requiredTableNamesPerDay.Any()?
                    DaywiseTableManager.getExistingTableNames(this.DatabaseName, requiredTableNamesPerDay.Keys, con)
                    :new List<string>();
                List<string> newTablesToBeCreated = requiredTableNamesPerDay.Keys
                    .Where(t => existingTables.Contains(t) == false)
                    .ToList();
                List<DateTime> tableDatesToBeCreated =
                    newTablesToBeCreated.Select(t => requiredTableNamesPerDay[t]).ToList();
                string templateSql = this.UniqueEventsOnly?this.Decoder.getCreateTableSqlForUniqueEvent(this.CollectorInput)
                    :this.Decoder.getCreateTableSqlForPartialEvent(null);
                string tablePrefix = this.UniqueEventsOnly?this.Decoder.UniqueEventTablePrefix
                    :this.Decoder.PartialTablePrefix;
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
