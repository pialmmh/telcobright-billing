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
    public static class SmsHubAggregationHelper
    {
        static string[] responseTypes = new[] { "2", "4" };
        public static EventAggregationResult Aggregate(NewAndOldEventsWrapper<string[]> newAndOldEventsWrapper)
        {
            //MsuType aggregationType = MsuType.None;

            List<string[]> newUnAggInstances = newAndOldEventsWrapper.NewUnAggInstances;
            List<string[]> oldUnAggInstances = newAndOldEventsWrapper.OldUnAggInstances;

            List<string[]> allUnaggregatedInstances = newUnAggInstances.Concat(oldUnAggInstances)
                .OrderBy(row => row[Sn.StartTime]).ToList();
            //if (newUnAggInstances.Count > 2 || oldUnAggInstances.Count > 1 || allUnaggregatedInstances.Count > 2)
            //    throw new Exception("allUnaggregatedInstances > 2 rows  cannot be aggregated.");

            if (newUnAggInstances.Count > 2 || allUnaggregatedInstances.Count > 2)
                throw new Exception("allUnaggregatedInstances > 2 rows  cannot be aggregated.");

            var groupedByBillId = allUnaggregatedInstances.GroupBy(row => row[Sn.UniqueBillId]).ToDictionary(g => g.Key);
            if (groupedByBillId.Count > 1)
                throw new Exception("Rows with multiple bill ids cannot be aggregated.");
            string uniqueBillId = groupedByBillId.Keys.First();

            List<string[]> newRowsToBeDiscardedAfterAggregation = new List<string[]>();
            List<string[]> oldRowsToBeDiscardedAfterAggregation = new List<string[]>();
            var aggregationComplete = false;
            string[] aggregatedRow = allUnaggregatedInstances.First();
            if (allUnaggregatedInstances.Count == 2)//agg success
            {
                var smsType = aggregatedRow[Sn.SmsType];
                if (smsType == "3")
                {
                    aggregatedRow[Sn.Endtime] = allUnaggregatedInstances.Last()[Sn.StartTime];
                    aggregatedRow[Sn.AggregationInfo] = allUnaggregatedInstances.Last()[Sn.Filename]
                                                        + ","
                                                        + allUnaggregatedInstances.Last()[Sn.PacketFrameTime];
                    aggregatedRow[Sn.ChargingStatus] = "1";
                    //aggregationComplete = !aggregatedRow[Sn.Imsi].IsNullOrEmptyOrWhiteSpace();
                    aggregatedRow[Sn.Partialflag] = "0";
                }
                else if (smsType == "1")
                {
                    aggregatedRow[Sn.Imsi] = allUnaggregatedInstances.Last()[Sn.Imsi];
                    aggregatedRow[Sn.Endtime] = allUnaggregatedInstances.Last()[Sn.StartTime];
                    aggregatedRow[Sn.AggregationInfo] = allUnaggregatedInstances.Last()[Sn.Filename]
                                                        + ","
                                                        + allUnaggregatedInstances.Last()[Sn.PacketFrameTime];
                    aggregatedRow[Sn.ChargingStatus] = "1";
                    //aggregationComplete = !aggregatedRow[Sn.Imsi].IsNullOrEmptyOrWhiteSpace();
                    aggregatedRow[Sn.Partialflag] = "0";
                }
                else
                {
                    throw new Exception("Unsupported sigtran message type for aggregation.");
                }
            }
            else if (allUnaggregatedInstances.Count == 1)// failedagg
            {
                return createResultForAggregationNotPossible(uniqueBillId, allUnaggregatedInstances,
                    newUnAggInstances, oldUnAggInstances);
            }
            else
            {
                throw new Exception("Instance count must be 1 or 2.");
            }

            //if (tempRows == null || !tempRows.Any())
            //{
            //    return createResultForAggregationNotPossible(uniqueBillId, allUnaggregatedInstances,
            //        newUnAggInstances, oldUnAggInstances);
            //}
            //main aggregation

            //if (aggregationType == MsuType.Sri)
            //{
            //    aggregationComplete = AggregateSri(sris, sriResps, unknownInstances, aggregatedRow);
            //}

            //if (aggregationType == MsuType.Mt)
            //{
            //    aggregationComplete = AggregateMt(mts, mtResps, unknownInstances, aggregatedRow);
            //}

            //if (aggregationType == MsuType.ReturnError)
            //{
            //    aggregatedRow[Sn.Endtime] = retErrInstances.Last()[Sn.StartTime];
            //    aggregatedRow[Sn.AggregationInfo] = retErrInstances.Last()[Sn.Filename]
            //                                        + ","
            //                                        + retErrInstances.Last()[Sn.PacketFrameTime];
            //    aggregationComplete = !aggregatedRow[Sn.Imsi].IsNullOrEmptyOrWhiteSpace();
            //}


            

            var response = allUnaggregatedInstances.Last();

            if (responseTypes.Contains(response[Sn.SmsType]))
            {
                if (newUnAggInstances.Any(n => n[Sn.IdCall] == response[Sn.IdCall]))
                {
                    newRowsToBeDiscardedAfterAggregation.Add(response);
                }
                else
                {
                    if (oldUnAggInstances.All(o => o[Sn.IdCall] != response[Sn.IdCall]))
                    {
                        throw new Exception("Partial instances must belong either new or old list");
                    }
                    oldRowsToBeDiscardedAfterAggregation.Add(response);
                }
            }
            else
                throw new Exception("Second partial instance should be of response type.");
            //i++;
            //if (aggregationComplete)
            //{
            //foreach (string[] row in newUnAggInstances)
            //{
            //    if (row[Sn.IdCall] != aggregatedRow[Sn.IdCall])
            //    {
            //        newRowsToBeDiscardedAfterAggregation.Add(row);
            //    }
            //}
            //newRowsToBeDiscardedAfterAggregation.Add(newUnAggInstances.Last());
            //foreach (string[] row in oldUnAggInstances)
            //{
            //    if (row[Sn.IdCall] != aggregatedRow[Sn.IdCall])
            //    {
            //        oldRowsToBeDiscardedAfterAggregation.Add(row);
            //    }
            //}
            //if (oldUnAggInstances.Any()) oldRowsToBeDiscardedAfterAggregation.Add(oldUnAggInstances.Last());

            //if (!newRowsToBeDiscardedAfterAggregation.Any())
            //{
            //    Console.WriteLine("corner case");
            //}
            //if (aggregatedRow == null)
            //{
            //    Console.WriteLine("corner case");
            //}
            var result = new EventAggregationResult//aggregation successful
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
            return result;
            //}
            //else
            //{
            //    return createResultForAggregationNotPossible(uniqueBillId, allUnaggregatedInstances, newUnAggInstances, oldUnAggInstances);
            //}
        }
        private static bool AggregateMt(List<string[]> mts, List<string[]> mtResps, List<string[]> unknowns, string[] aggregatedRow)
        {
            if (mts.Any() && unknowns.Any() && !mtResps.Any())
            {
                aggregatedRow[Sn.Endtime] = unknowns.Last()[Sn.StartTime];
                aggregatedRow[Sn.AggregationInfo] = unknowns.Last()[Sn.Filename]
                                                    + ","
                                                    + unknowns.Last()[Sn.PacketFrameTime];
            }
            else
            {
                aggregatedRow[Sn.Endtime] = mtResps.Last()[Sn.StartTime];
                aggregatedRow[Sn.AggregationInfo] = mtResps.Last()[Sn.Filename]
                                                    + ","
                                                    + mtResps.Last()[Sn.PacketFrameTime];
            }

            aggregatedRow[Sn.ChargingStatus] = "1";
            bool aggregationComplete = !aggregatedRow[Sn.Imsi].IsNullOrEmptyOrWhiteSpace();
            return aggregationComplete;
        }

        private static bool AggregateSri(List<string[]> sriReqInstance, List<string[]> sriResInstance, List<string[]> unknownInstance, string[] aggregatedRow)
        {
            if (sriReqInstance.Any() && unknownInstance.Any() && !sriResInstance.Any())
            {
                aggregatedRow[Sn.Imsi] = unknownInstance.Last()[Sn.Imsi];
                aggregatedRow[Sn.Endtime] = unknownInstance.Last()[Sn.StartTime];
                aggregatedRow[Sn.AggregationInfo] = unknownInstance.Last()[Sn.Filename]
                                                    + ","
                                                    + unknownInstance.Last()[Sn.PacketFrameTime];
            }
            else
            {
                aggregatedRow[Sn.Imsi] = sriResInstance.Last()[Sn.Imsi];
                aggregatedRow[Sn.Endtime] = sriResInstance.Last()[Sn.StartTime];
                aggregatedRow[Sn.AggregationInfo] = sriResInstance.Last()[Sn.Filename]
                                                    + ","
                                                    + sriResInstance.Last()[Sn.PacketFrameTime];
            }
            aggregatedRow[Sn.ChargingStatus] = "1";
            bool aggregationComplete = !aggregatedRow[Sn.Imsi].IsNullOrEmptyOrWhiteSpace();
            return aggregationComplete;
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
