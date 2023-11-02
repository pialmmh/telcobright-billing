using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using LibraryExtensions.ConfigHelper;
using MediationModel;
using MySql.Data.MySqlClient;
using TelcobrightInfra;
using TelcobrightMediation.Config;

namespace TelcobrightMediation.Cdr.Collection.PreProcessors
{
    public class DuplicaterEventFilter<T>
    {
        private DayWiseEventCollector<T> EventCollector { get; set; }
        public CdrCollectorInputData CollectorInput { get; set; }
        
        private Dictionary<string, T> FinalNonDuplicateEvents { get; set; } = new Dictionary<string, T>();
        public DuplicaterEventFilter(DayWiseEventCollector<T> eventCollector)
        {
            this.EventCollector = eventCollector;
            this.CollectorInput = eventCollector.CollectorInput;
            
        }
        public Dictionary<string, T> filterDuplicateCdrs(out List<T> excludedDuplicateEvents)
        {
            excludedDuplicateEvents= new List<T>();
            Dictionary<string, string> alreadyConsideredEvents = this.EventCollector.ExistingEvents.Select(e=>e.ToString())
                .GroupBy(e=>e)
                .Select(g=> new
                {
                    Key=g.Key,
                    Item= g.First()
                })
                .ToDictionary(a => a.Key, a=>a.Item);
            foreach (var kv in EventCollector.TupleWiseDecodedEvents)
            {
                string tuple = kv.Key;
                List<T> eventsForThisTuple = kv.Value;
                if (alreadyConsideredEvents.ContainsKey(tuple) == false)
                {
                    T head = eventsForThisTuple.First();
                    List<T> tail = eventsForThisTuple.Skip(1).ToList();
                    FinalNonDuplicateEvents.Add(tuple, head);
                    alreadyConsideredEvents.Add(tuple, tuple); //it's just used like hashmap
                    excludedDuplicateEvents.AddRange(tail);
                }
                else
                {
                    excludedDuplicateEvents.AddRange(eventsForThisTuple); //dup events are skipped
                }
            }
            return FinalNonDuplicateEvents;
        }
    }
}
