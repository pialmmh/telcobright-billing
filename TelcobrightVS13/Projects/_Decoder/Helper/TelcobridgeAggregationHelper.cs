using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;
using TelcobrightMediation;

namespace Decoders
{
    public static class TelcobridgeAggregationHelper
    {
        public static EventAggregationResult Aggregate(object data)
        {
            //List<string[]> allRowsToAggregate = ((List<string[]>)data).OrderBy(row => row[Fn.StartTime]).ToList();
            List<string[]> originalUnAggregatedInstances = ((List<string[]>)data).OrderBy(row => row[Fn.StartTime]).ToList();
            var groupedByBillId = originalUnAggregatedInstances.GroupBy(row => row[Fn.UniqueBillId]).ToDictionary(g => g.Key);
            if(groupedByBillId.Count>1)    
                throw new Exception("Rows with multiple bill ids cannot be aggregated.");
            string uniqueBillId = groupedByBillId.Keys.First();
            List<string[]> rowsToBeDiscardedAfterAggregation = new List<string[]>();
            List<string[]> ingressLegs = originalUnAggregatedInstances.Where(r => r[Fn.InTrunkAdditionalInfo] == "originate")
                .ToList();
            List<string[]> egressLegs = originalUnAggregatedInstances.Where(r => r[Fn.InTrunkAdditionalInfo] == "answer")
                .ToList();
            List<string[]> egressLegsWithEndFlag = egressLegs.Where(r => r[Fn.OutTrunkAdditionalInfo].ToLower() == "end").ToList();

            if (ingressLegs.Any() == false || egressLegs.Any() == false || egressLegsWithEndFlag.Any()==false)
            {
                return createResultForAggregationNotPossible(uniqueBillId,originalUnAggregatedInstances);
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
                return createResultForAggregationNotPossible(uniqueBillId,originalUnAggregatedInstances);
            }
            string[] aggregatedRow = tempRows.Last();
            aggregatedRow[Fn.IncomingRoute] = ingressLegs.Last()[Fn.IncomingRoute];//main aggregation

            aggregationComplete = !aggregatedRow[Fn.IncomingRoute].IsNullOrEmptyOrWhiteSpace() &&
                                  !aggregatedRow[Fn.OutgoingRoute].IsNullOrEmptyOrWhiteSpace() &&
                                  (aggregatedRow[Fn.IncomingRoute] != aggregatedRow[Fn.OutgoingRoute]);

            if (aggregationComplete)
            {
                foreach (string[] row in originalUnAggregatedInstances)
                {
                    if (row[Fn.IdCall] != aggregatedRow[Fn.IdCall])
                    {
                        rowsToBeDiscardedAfterAggregation.Add(row);
                    }
                }
                return new EventAggregationResult//aggregation successful
                (
                    uniqueBillId,
                    originalUnAggregatedInstances,
                    aggregatedRow,
                    new List<string[]>(),
                    rowsToBeDiscardedAfterAggregation
                );
            }
            else
            {
                return createResultForAggregationNotPossible(uniqueBillId,originalUnAggregatedInstances);
            }
        }
        private static EventAggregationResult createResultForAggregationNotPossible(string uniqueEventId, List<string[]> originalUnAggregatedInstances)
        {
            return new EventAggregationResult
            (
                uniqueEventId,
                originalUnAggregatedInstances,
                null,
                originalUnAggregatedInstances,
                new List<string[]>()
            );
        }
    }
}
