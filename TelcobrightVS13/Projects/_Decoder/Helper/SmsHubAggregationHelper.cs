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
        public static EventAggregationResult Aggregate(NewAndOldEventsWrapper<string[]> newAndOldEventsWrapper)
        {

            SmsType aggregationType = SmsType.None;

            List<string[]> newUnAggInstances = newAndOldEventsWrapper.NewUnAggInstances;
            List<string[]> oldUnAggInstances = newAndOldEventsWrapper.OldUnAggInstances;
            List<string[]> allUnaggregatedInstances = newUnAggInstances.Concat(oldUnAggInstances)
                .OrderBy(row => row[Sn.StartTime]).ToList();
            //if (newUnAggInstances.Count > 2 || oldUnAggInstances.Count > 2 || allUnaggregatedInstances.Count > 2)
            //    throw new Exception("allUnaggregatedInstances better than 2 rows  cannot be aggregated.");
              
            var groupedByBillId = allUnaggregatedInstances.GroupBy(row => row[Sn.UniqueBillId]).ToDictionary(g => g.Key);
            //if (groupedByBillId.Count > 1)
            //    throw new Exception("Rows with multiple bill ids cannot be aggregated.");
            string uniqueBillId = groupedByBillId.Keys.First();
            List<string[]> newRowsToBeDiscardedAfterAggregation = new List<string[]>();
            List<string[]> oldRowsToBeDiscardedAfterAggregation = new List<string[]>();


            //Dictionary<DateTime, List<string[]>> startimeWiseRows = allUnaggregatedInstances
            //    .GroupBy(r => Convert.ToDateTime(r[Sn.StartTime]))
            //    .ToDictionary(g => g.Key, g => g.ToList());
        

            List<string[]> sriReqInstance = allUnaggregatedInstances.Where(r => r[Sn.SmsType] == "1")
                .ToList();
            List<string[]> sriResInstance = allUnaggregatedInstances.Where(r => r[Sn.SmsType] == "2")
                .ToList();

            List<string[]> mtReqInstance = allUnaggregatedInstances.Where(r => r[Sn.SmsType] == "3")
                .ToList();
            List<string[]> mtResInstance = allUnaggregatedInstances.Where(r => r[Sn.SmsType] == "4")
                .ToList();

            List<string[]> retErrInstance = allUnaggregatedInstances.Where(r => r[Sn.SmsType] == "5")
                .ToList();

            List<string[]> unknownInstance = allUnaggregatedInstances.Where(r => r[Sn.SmsType] ==null || r[Sn.SmsType] == "")
                .ToList();

            List<string[]> tempRows = null;

            // only for sri smstype
            if ((sriReqInstance.Any() && sriResInstance.Any()))
            {
                aggregationType = SmsType.Sri;
                tempRows = sriReqInstance;
            } 

            if ((mtReqInstance.Any() && mtResInstance.Any()))
            {
                aggregationType = SmsType.Mt;
                tempRows = mtReqInstance;
            }
            if ((mtReqInstance.Any() && retErrInstance.Any()))
            {
                aggregationType = SmsType.ReturnError;
                tempRows = mtReqInstance;
            }


            bool aggregationComplete = false;
            
            if (tempRows == null || !tempRows.Any())
            {
                return createResultForAggregationNotPossible(uniqueBillId, allUnaggregatedInstances,
                    newUnAggInstances, oldUnAggInstances);
            }

            string[] aggregatedRow = tempRows.Last();

            //main aggregation
            if (aggregationType == SmsType.Sri)
            {
                aggregatedRow[Sn.Imsi] = sriResInstance.Last()[Sn.Imsi];
                aggregatedRow[Sn.Endtime] = sriResInstance.Last()[Sn.StartTime];
                aggregatedRow[Sn.ChargingStatus] = "1";
                aggregationComplete = !aggregatedRow[Sn.Imsi].IsNullOrEmptyOrWhiteSpace();
            }

            if (aggregationType == SmsType.Mt)
            {
                aggregatedRow[Sn.ChargingStatus] = "1";
                aggregatedRow[Sn.Endtime] = mtResInstance.Last()[Sn.StartTime];
                aggregationComplete = !aggregatedRow[Sn.Imsi].IsNullOrEmptyOrWhiteSpace();
            }

            if (aggregationType == SmsType.ReturnError)
            {
                aggregatedRow[Sn.Endtime] = retErrInstance.Last()[Sn.StartTime];
                aggregationComplete = !aggregatedRow[Sn.Imsi].IsNullOrEmptyOrWhiteSpace();
            }

            if (aggregationComplete)
            {
                aggregatedRow[Sn.Partialflag] = "0";
                foreach (string[] row in newUnAggInstances)
                {
                    if (row[Sn.IdCall] != aggregatedRow[Sn.IdCall])
                    {
                        newRowsToBeDiscardedAfterAggregation.Add(row);
                    }
                }
                foreach (string[] row in oldUnAggInstances)
                {
                    if (row[Sn.IdCall] != aggregatedRow[Sn.IdCall])
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
                return createResultForAggregationNotPossible(uniqueBillId, allUnaggregatedInstances, newUnAggInstances, oldUnAggInstances);
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
