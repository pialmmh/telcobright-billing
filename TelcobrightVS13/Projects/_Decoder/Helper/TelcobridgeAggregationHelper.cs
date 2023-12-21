using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;
using TelcobrightMediation;
using TelcobrightMediation.Cdr.Collection.PreProcessors;

namespace Decoders
{
    public static class TelcobridgeAggregationHelper
    {
        public static EventAggregationResult Aggregate(NewAndOldEventsWrapper<string[]> newAndOldEventsWrapper)
        {
            List<string[]> newUnAggInstances = newAndOldEventsWrapper.NewUnAggInstances;
            List<string[]> oldUnAggInstances = newAndOldEventsWrapper.OldUnAggInstances;
            List<string[]> allUnaggregatedInstances = newUnAggInstances.Concat(oldUnAggInstances)
                .OrderBy(row => row[Fn.StartTime]).ToList();
            var groupedByBillId = allUnaggregatedInstances.GroupBy(row => row[Fn.UniqueBillId]).ToDictionary(g => g.Key);
            if(groupedByBillId.Count>1)    
                throw new Exception("Rows with multiple bill ids cannot be aggregated.");
            string uniqueBillId = groupedByBillId.Keys.First();
            List<string[]> newRowsToBeDiscardedAfterAggregation = new List<string[]>();
            List<string[]> oldRowsToBeDiscardedAfterAggregation = new List<string[]>();
            List<string[]> ingressLegs = allUnaggregatedInstances.Where(r => r[Fn.InTrunkAdditionalInfo] == "originate")
                .ToList();
            List<string[]> egressLegs = allUnaggregatedInstances.Where(r => r[Fn.InTrunkAdditionalInfo] == "answer")
                .ToList();
            List<string[]> egressLegsWithEndFlag = egressLegs.Where(r => r[Fn.OutTrunkAdditionalInfo].ToLower() == "end").ToList();

            if (ingressLegs.Any() == false || egressLegs.Any() == false || egressLegsWithEndFlag.Any()==false)
            {
                return createResultForAggregationNotPossible(uniqueBillId, allUnaggregatedInstances,
                    newUnAggInstances, oldUnAggInstances);
            }
            Dictionary<decimal, List<string[]>> durationWiseEgressRows = egressLegsWithEndFlag
                .GroupBy(r => Convert.ToDecimal(r[Fn.DurationSec]))
                    .ToDictionary(g => g.Key, g => g.ToList());
            bool aggregationComplete = false;
            decimal maxDuration = durationWiseEgressRows.Keys.Max();
            List<string[]> tempRows = null;
            durationWiseEgressRows.TryGetValue(maxDuration, out tempRows);
            if (tempRows == null || !tempRows.Any())
            {
                return createResultForAggregationNotPossible(uniqueBillId, allUnaggregatedInstances,
                    newUnAggInstances, oldUnAggInstances);
            }
            string[] aggregatedRow = tempRows.Last();
            aggregatedRow[Fn.IncomingRoute] = ingressLegs.Last()[Fn.IncomingRoute];//main aggregation
            aggregationComplete = !aggregatedRow[Fn.IncomingRoute].IsNullOrEmptyOrWhiteSpace() &&
                                  !aggregatedRow[Fn.OutgoingRoute].IsNullOrEmptyOrWhiteSpace() &&
                                  (aggregatedRow[Fn.IncomingRoute] != aggregatedRow[Fn.OutgoingRoute]);

            if (aggregationComplete)
            {
                aggregatedRow[Fn.Partialflag] = "0";
                foreach (string[] row in newUnAggInstances)
                {
                    if (row[Fn.IdCall] != aggregatedRow[Fn.IdCall])
                    {
                        newRowsToBeDiscardedAfterAggregation.Add(row);
                    }
                }
                foreach (string[] row in oldUnAggInstances)
                {
                    if (row[Fn.IdCall] != aggregatedRow[Fn.IdCall])
                    {
                        oldRowsToBeDiscardedAfterAggregation.Add(row);
                    }
                }
                return new EventAggregationResult//aggregation successful
                (
                    uniqueEventId: uniqueBillId,
                    allUnaggregatedInstances: allUnaggregatedInstances,
                    aggregatedInstance: aggregatedRow,
                    newInstancesCouldNotBeAggregated: new List<string[]>(),
                    oldInstancesCouldNotBeAggregated: new List<string[]>(), 
                    newInstancesToBeDiscardedAfterAggregation: newRowsToBeDiscardedAfterAggregation,
                    oldInstancesToBeDiscardedAfterAggregation: oldRowsToBeDiscardedAfterAggregation,
                    oldPartialInstancesFromDB: oldUnAggInstances
                );
            }
            else
            {
                return createResultForAggregationNotPossible(uniqueBillId,allUnaggregatedInstances,newUnAggInstances,oldUnAggInstances);
            }
        }
        private static EventAggregationResult createResultForAggregationNotPossible(string uniqueEventId,
            List<string[]> originalUnAggregatedInstances, 
            List<string[]> newUnAggInstances,
            List<string[]> oldUnAggInstances)
        {
            return new EventAggregationResult
            (
                uniqueEventId: uniqueEventId,
                allUnaggregatedInstances: originalUnAggregatedInstances,
                aggregatedInstance: null,
                newInstancesCouldNotBeAggregated: newUnAggInstances,
                oldInstancesCouldNotBeAggregated: oldUnAggInstances,
                newInstancesToBeDiscardedAfterAggregation: new List<string[]>(),
                oldInstancesToBeDiscardedAfterAggregation: new List<string[]>(),
                oldPartialInstancesFromDB: oldUnAggInstances
            );
        }
    }
}
