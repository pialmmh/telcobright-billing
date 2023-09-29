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

namespace TelcobrightMediation.Cdr.Collection.PreProcessors
{
    public class DuplicaterEventFilter
    {
        private DayAndHourWiseCollector Collector { get; set; }
        public CdrCollectorInputData CollectorInput { get; set; }
        public DbCommand DbCmd { get; set; }
        private Dictionary<string, string[]> FinalNonDuplicateEvents { get; set; } = new Dictionary<string, string[]>();
        public DuplicaterEventFilter(DayAndHourWiseCollector collector)
        {
            this.Collector = collector;
            this.CollectorInput = collector.CollectorInput;
            this.DbCmd = collector.DbCmd;
        }
        public Dictionary<string, string[]> filterDuplicateCdrs()
        {
            List<string> existingEvents = this.Collector.collectExistingEvents();
            Dictionary<string, string> alreadyConsideredEvents = existingEvents.ToDictionary(e => e);
            foreach (var kv in Collector.DecodedEventsAsTupDic)
            {
                string tuple = kv.Key;
                string[] decodedRow = kv.Value;
                if (alreadyConsideredEvents.ContainsKey(tuple) == false)
                {
                    FinalNonDuplicateEvents.Add(tuple, decodedRow);
                    alreadyConsideredEvents.Add(tuple, tuple); //it's just used like hashmap
                }
                else
                {
                    //cdr is skipped
                    Console.WriteLine("Skipped duplicate cdrs");
                }
            }

            //create uniqueevent tables for each date
            List<DateTime> datesToCreateTable = Collector.DayWiseNewTuples.Keys.ToList();
            string engine = "innodb";
            string partitionColName = "starttime";
            string tablePrefix = "zz_uniqueevent";
            string templateSql = $@"CREATE if not exists TABLE <{tablePrefix}> (tuple varchar(200) COLLATE utf8mb4_bin NOT NULL,
						  {partitionColName} datetime NOT NULL,
						  description varchar(50) COLLATE utf8mb4_bin DEFAULT NULL,
						  UNIQUE KEY ind_tuple (tuple)) 
                          ENGINE= {engine} DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin";

            var databaseSetting = this.CollectorInput.CdrJobInputData.Tbc.DatabaseSetting;
            string conStr = DbUtil.getDbConStrWithDatabase(databaseSetting);
            //ddl statement may auto commit all transactions
            //so use a different db connection 
            using (MySqlConnection con = new MySqlConnection(conStr))
            {
                con.Open();
                DaywiseTableManager.CreateTables(tablePrefix: tablePrefix,
                    templateSql: templateSql,
                    dateTimes: datesToCreateTable,
                    con: con,
                    partitionByHour: true, engine: engine, partitionColName: partitionColName);
                con.Close();
            }
            return FinalNonDuplicateEvents;
        }

        

        
    }
}
