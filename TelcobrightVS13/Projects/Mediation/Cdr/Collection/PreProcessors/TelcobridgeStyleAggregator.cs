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
// ReSharper disable All

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
        public Dictionary<string, EventAggregationResult> aggregateCdrs()
        {
            Dictionary<string, NewAndOldEventsWrapper<T>> billIdWiseNewAndOldWrappers = mergeAndGroupNewAndOldEvents();
            Dictionary<string, EventAggregationResult> aggregationResults = new Dictionary<string, EventAggregationResult>();
            foreach (var kv in billIdWiseNewAndOldWrappers)
            {
                string billId = kv.Key;
                NewAndOldEventsWrapper<T> newAndOldEventsWrapper = kv.Value;
                EventAggregationResult aggregationResult = this.Decoder.Aggregate(newAndOldEventsWrapper);
                aggregationResults.Add(billId, aggregationResult);
            }
            return aggregationResults;
        }

        private Dictionary<string, NewAndOldEventsWrapper<T>> mergeAndGroupNewAndOldEvents()
        {
            Dictionary<string, NewAndOldEventsWrapper<T>> newAndOldInstanceWrappers = 
                this.NewUnAggregatedEventsNotInDb
                .GroupBy(r => Decoder.getGeneratedUniqueEventId(r))
                .Select(g => new NewAndOldEventsWrapper<T>
                {
                    UniqueBillId = g.Key,
                    NewUnAggInstances = g.ToList()
                }).ToDictionary(wrapper => wrapper.UniqueBillId);

            Dictionary<string, List<T>> billIdWiseOldInstances = this.OldUnAggregatedEventsFromDb
                .GroupBy(r => Decoder.getGeneratedUniqueEventId(r))
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var kv in billIdWiseOldInstances)
            {
                var billId = kv.Key;
                List<T> oldUnAggInstances = kv.Value;
                NewAndOldEventsWrapper<T> targetWrapper = null;
                newAndOldInstanceWrappers.TryGetValue(billId, out targetWrapper);
                if (targetWrapper == null)
                {
                    throw new Exception($"Could not find new wrapper instance for billid: {billId}");
                }
                targetWrapper.OldUnAggInstances = oldUnAggInstances;
            }
            return newAndOldInstanceWrappers;
        }
    }
}
