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
            if(typeof(T)!=typeof(string[]))
                throw new NotImplementedException("Duplicate filtering is so far only allowed for string[] type");
            this.EventCollector = eventCollector;
            this.CollectorInput = eventCollector.CollectorInput;
        }
        public Dictionary<string, T> filterDuplicateCdrs(out List<T> excludedDuplicateEvents, 
            out HashSet<string> existingUniqueEventInstancesFromDB)
        {
            excludedDuplicateEvents= new List<T>();
            Dictionary<string, string> alreadyConsideredEvents = this.EventCollector.ExistingEventsInDb.Select(e=>
                {
                    var strings = e as string[];
                    if (strings != null) return strings[0];
                    throw new NotImplementedException("Duplicate filtering is so far only allowed for string[] type");
                })
                .GroupBy(e=>e)
                .Select(g=> new
                {
                    Key=g.Key,
                    Item= g.First()
                }).ToDictionary(a => a.Key, a=>a.Item);
            existingUniqueEventInstancesFromDB= new HashSet<string>();
            foreach (var kv in alreadyConsideredEvents)
            {
                existingUniqueEventInstancesFromDB.Add(kv.Key);
            }
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
