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
            List<string[]> allRowsToAggregate = ((List<string[]>)data).OrderBy(row => row[Fn.StartTime]).ToList();
            List<string[]> rowsToBeDiscardedAfterAggregation = new List<string[]>();
            List<string[]> ingressLegs = allRowsToAggregate.Where(r => r[Fn.InTrunkAdditionalInfo] == "originate")
                .ToList();
            List<string[]> egressLegs = allRowsToAggregate.Where(r => r[Fn.InTrunkAdditionalInfo] == "answer")
                .ToList();

            if (ingressLegs.Any() == false || egressLegs.Any() == false)
            {
                return createResutlForAggregationNotPossible(allRowsToAggregate);
            }
            Dictionary<decimal, List<string[]>> durationWiseEgressRows =
                egressLegs.GroupBy(r => Convert.ToDecimal(r[Fn.DurationSec]))
                    .ToDictionary(g => g.Key, g => g.ToList());
            bool aggregationComplete = false;
            decimal maxDuration = durationWiseEgressRows.Keys.Max();
            List<string[]> tempRows = null;
            durationWiseEgressRows.TryGetValue(maxDuration, out tempRows);
            if (tempRows == null || !tempRows.Any())
            {
                return createResutlForAggregationNotPossible(allRowsToAggregate);
            }
            string[] aggregatedRow = tempRows.Last();
            aggregatedRow[Fn.IncomingRoute] = ingressLegs.Last()[Fn.IncomingRoute];//main aggregation

            aggregationComplete = !aggregatedRow[Fn.IncomingRoute].IsNullOrEmptyOrWhiteSpace() &&
                                  !aggregatedRow[Fn.OutgoingRoute].IsNullOrEmptyOrWhiteSpace() &&
                                  (aggregatedRow[Fn.IncomingRoute] != aggregatedRow[Fn.OutgoingRoute]);

            if (aggregationComplete)
            {
                foreach (string[] row in allRowsToAggregate)
                {
                    if (row[Fn.IdCall] != aggregatedRow[Fn.IdCall])
                    {
                        rowsToBeDiscardedAfterAggregation.Add(row);
                    }
                }
                return new EventAggregationResult()//aggregation successful
                {
                    AggregatedInstance = aggregatedRow,
                    InstancesCouldNotBeAggregated = new List<string[]>(),
                    InstancesToBeDiscardedAfterAggregation = rowsToBeDiscardedAfterAggregation
                };
            }
            else
            {
                return createResutlForAggregationNotPossible(allRowsToAggregate);
            }
        }
        private static EventAggregationResult createResutlForAggregationNotPossible(List<string[]> allRowsToAggregate)
        {
            return new EventAggregationResult()
            {
                AggregatedInstance = null,
                InstancesCouldNotBeAggregated = allRowsToAggregate,
                InstancesToBeDiscardedAfterAggregation = new List<string[]>()
            };
        }
    }
}
