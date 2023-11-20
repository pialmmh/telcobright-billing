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
        private AbstractCdrDecoder Decoder { get; }

        public TelcobridgeStyleAggregator(DayWiseEventCollector<T> eventCollector)
        {
            this.EventCollector = eventCollector;
            this.CollectorInput = eventCollector.CollectorInput;
            this.NewUnAggregatedEventsNotInDb = eventCollector.InputEvents;
            this.OldUnAggregatedEventsFromDb = eventCollector.ExistingEventsInDb;
            this.Decoder = eventCollector.Decoder;
        }
        public Dictionary<string, T> aggregateCdrs(out List<T> eventsRemainedUnaggreagated, out List<T> eventsToBeDiscardedAfterAggregation)
        {
            var allUnAggregatedEvents = this.OldUnAggregatedEventsFromDb.Concat(this.NewUnAggregatedEventsNotInDb)
                .GroupBy(e => Decoder.getTupleExpression(e))
                .Select(g => new
                {
                    Key = g.Key,
                    Events = g.ToList()
                }).ToDictionary(a => a.Key, a => a.Events);
            Dictionary<string,T> finalAggregatedEvents= new Dictionary<string, T>();
            eventsRemainedUnaggreagated= new List<T>();
            foreach (var kv in allUnAggregatedEvents)
            {
                string billId = kv.Key;
                List<T> unAggregatedInstances = kv.Value;
                //object unaggregatedInstancesRorThisTupleOrBillId = null;
                //T aggregatedInstance = (T)this.Decoder.Aggregate(unAggregatedInstances, out unaggregatedInstancesRorThisTupleOrBillId);
                //finalAggregatedEvents.Add(billId,aggregatedInstance);
            }
            eventsToBeDiscardedAfterAggregation = new List<T>();
            return finalAggregatedEvents;
        }
    }
}
