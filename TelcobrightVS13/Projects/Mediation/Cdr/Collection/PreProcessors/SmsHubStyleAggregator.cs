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
    public class SmsHubStyleAggregator<T>
    {
        private DayWiseEventCollector<T> EventCollector { get; }
        public CdrCollectorInputData CollectorInput { get; set; }
        private Dictionary<string, T> AggregatedEvents { get; } = new Dictionary<string, T>();
        private List<T> OldUnAggregatedEventsFromDb { get; }
        private List<T> NewUnAggregatedEventsNotInDb { get; }
        private AbstractCdrDecoder Decoder { get; }
        public SmsHubStyleAggregator(DayWiseEventCollector<T> eventCollector)
        {
            this.EventCollector = eventCollector;
            this.CollectorInput = eventCollector.CollectorInput;
            this.NewUnAggregatedEventsNotInDb = eventCollector.InputEvents;
            this.OldUnAggregatedEventsFromDb = eventCollector.ExistingEventsInDb;
            this.Decoder = eventCollector.Decoder;
        }
        public List<NewAndOldEventsWrapper<string[]>> aggregateCdrs(out List<NewAndOldEventsWrapper<string[]>> failedList)
        {
            Dictionary<string, NewAndOldEventsWrapper<T>> billIdWiseNewAndOldWrappers = mergeAndGroupNewAndOldEvents();
            var wrappers = billIdWiseNewAndOldWrappers.Values;
            //List<string[]> rowsToAggregate = wrappers.SelectMany(wr => wr.OldUnAggInstances.Select(s => s as string[]))
            //    .ToList()
            //    .Concat(wrappers.SelectMany(wr => wr.NewUnAggInstances.Select(s => s as string[]))).ToList();
            failedList =new List<NewAndOldEventsWrapper<string[]>>();
            //NewCdrPreProcessor preProcessor = new NewCdrPreProcessor(rowsToAggregate, new List<cdrinconsistent>(),
            //    this.CollectorInput);
            //preProcessor.Decoder = this.Decoder;
            List<NewAndOldEventsWrapper<string[]>> unGroupedNewAndOldEventsWrappers = new List<NewAndOldEventsWrapper<string[]>>();
            foreach (var kv in billIdWiseNewAndOldWrappers)
            {
                string billId = kv.Key;
                NewAndOldEventsWrapper<T> newAndOldEventsWrapper = kv.Value;
                List<NewAndOldEventsWrapper<string[]>> tempfailedList;
                List<NewAndOldEventsWrapper<string[]>> unGroupedNewAndOldEventsWrapper =this.Decoder.PreAggregateL2(newAndOldEventsWrapper, out tempfailedList);
                failedList.AddRange(tempfailedList);
                unGroupedNewAndOldEventsWrappers.AddRange(unGroupedNewAndOldEventsWrapper);
            }
            //Dictionary<string, EventAggregationResult> aggregationResults =
            //List <NewAndOldEventsWrapper<string[]>> aggregationResults =
            //    this.Decoder.PreAggregateL2(, out failedList);
            return unGroupedNewAndOldEventsWrappers;
        }

        private Dictionary<string, NewAndOldEventsWrapper<T>> mergeAndGroupNewAndOldEvents()
        {
            Dictionary<string, NewAndOldEventsWrapper<T>> newAndOldInstanceWrappers =
                this.NewUnAggregatedEventsNotInDb
                    .GroupBy(r => Decoder.getGeneratedUniqueEventId(r))
                    .Select(g => new NewAndOldEventsWrapper<T>
                    {
                        UniqueBillId = g.Key,
                        NewUnAggInstances = g.ToList(),
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
