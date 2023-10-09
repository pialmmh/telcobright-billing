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
        private DayWiseEventCollector EventCollector { get; set; }
        public CdrCollectorInputData CollectorInput { get; set; }
        public DbCommand DbCmd { get; set; }
        private Dictionary<string, string[]> FinalNonDuplicateEvents { get; set; } = new Dictionary<string, string[]>();
        public DuplicaterEventFilter(DayWiseEventCollector eventCollector)
        {
            this.EventCollector = eventCollector;
            this.CollectorInput = eventCollector.CollectorInput;
            this.DbCmd = eventCollector.DbCmd;
        }
        public Dictionary<string, string[]> filterDuplicateCdrs()
        {
            Dictionary<string, string> alreadyConsideredEvents = this.EventCollector.ExistingEvents.ToDictionary(e => e);
            foreach (var kv in EventCollector.DecodedEventsAsTupDic)
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
            return FinalNonDuplicateEvents;
        }

        

        
    }
}
