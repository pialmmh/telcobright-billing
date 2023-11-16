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
    public class TelcobridgeStyleAggregator<T>
    {
        private DayWiseEventCollector<T> EventCollector { get; }
        public CdrCollectorInputData CollectorInput { get; set; }
        
        private Dictionary<string, T> AggregatedEvents { get; } = new Dictionary<string, T>();
        private List<T> OldUnAggregatedEventsFromDb { get; }
        private List<T> NewUnAggregatedEventsNotInDb { get; }
        private Dictionary<string,List<T>> AllUnAggregatedEvents { get; set; }
        private AbstractCdrDecoder Decoder { get; }

        public TelcobridgeStyleAggregator(DayWiseEventCollector<T> eventCollector)
        {
            this.EventCollector = eventCollector;
            this.CollectorInput = eventCollector.CollectorInput;
            this.OldUnAggregatedEventsFromDb = eventCollector.ExistingEvents;
            this.NewUnAggregatedEventsNotInDb = eventCollector.DecodedEvents;
            this.Decoder = eventCollector.Decoder;
        }
        public Dictionary<string, T> aggregateCdrs(out List<T> finalUnaggregatedEvents)
        {
            this.AllUnAggregatedEvents = this.OldUnAggregatedEventsFromDb.Concat(this.NewUnAggregatedEventsNotInDb)
                .GroupBy(e => Decoder.getTupleExpression(e))
                .Select(g => new
                {
                    Key = g.Key,
                    Events = g.ToList()
                }).ToDictionary(a => a.Key, a => a.Events);
            Dictionary<string,T> finalAggregatedEvents= new Dictionary<string, T>();
            finalUnaggregatedEvents= new List<T>();
            foreach (var kv in this.AllUnAggregatedEvents)
            {
                string billId = kv.Key;
                List<T> unAggregatedInstances = kv.Value;
                object rowsRemainedUnAggregated = null;
                T aggregatedInstance = (T)this.Decoder.Aggregate(unAggregatedInstances, out rowsRemainedUnAggregated);
                finalAggregatedEvents.Add(billId,aggregatedInstance);
            }
                
            //foreach (var kv in EventCollector.TupleWiseDecodedEvents)
            //{
            //    string tuple = kv.Key;
            //    List<T> eventsForThisTuple = kv.Value;
            //    if (billIdWiseEvents.ContainsKey(tuple) == false)
            //    {
            //        T head = eventsForThisTuple.First();
            //        List<T> tail = eventsForThisTuple.Skip(1).ToList();
            //        AggregatedEvents.Add(tuple, head);
            //        billIdWiseEvents.Add(tuple, tuple); //it's just used like hashmap
            //        finalUnaggregatedEvents.AddRange(tail);
            //    }
            //    else
            //    {
            //        finalUnaggregatedEvents.AddRange(eventsForThisTuple); //dup events are skipped
            //    }
            //}
            return finalAggregatedEvents;
        }
    }
}
