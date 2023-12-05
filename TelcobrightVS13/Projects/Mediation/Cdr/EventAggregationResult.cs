using System;
using System.Collections.Generic;
using LibraryExtensions;
using MediationModel;

namespace TelcobrightMediation
{
    public class EventAggregationResult
    {
        public string UniqueEventId { get; }
        public List<string[]> OriginalUnaggregatedInstances { get; }
        public string[] AggregatedInstance { get; }
        public List<string[]> InstancesCouldNotBeAggregated { get; }
        public List<string[]> InstancesToBeDiscardedAfterAggregation { get; }
        public EventAggregationResult(string uniqueEventId, List<string[]> originalUnaggregatedInstances,
            string[] aggregatedInstance, List<string[]> instancesCouldNotBeAggregated, List<string[]> instancesToBeDiscardedAfterAggregation)
        {
            UniqueEventId = uniqueEventId;
            this.OriginalUnaggregatedInstances = originalUnaggregatedInstances;
            AggregatedInstance = aggregatedInstance;
            InstancesCouldNotBeAggregated = instancesCouldNotBeAggregated;
            InstancesToBeDiscardedAfterAggregation = instancesToBeDiscardedAfterAggregation;
            if (this.OriginalUnaggregatedInstances.Count > 0)
            {
                if(this.UniqueEventId.IsNullOrEmptyOrWhiteSpace())
                    throw new Exception("Unique event id cannot be empty");

                if (this.AggregatedInstance != null) // successfully aggregated 
                {
                    if (aggregatedInstance[Fn.IdCall].IsNullOrEmptyOrWhiteSpace())
                        throw new Exception("Idcall not set for aggregated instance.");
                    if (this.InstancesToBeDiscardedAfterAggregation.Count + 1 != this.OriginalUnaggregatedInstances.Count )
                    {
                        throw new Exception("InstancesToBeDiscarded+1 must be equal to originalinstances when aggregation is successful.");
                    }
                    if (instancesCouldNotBeAggregated.Count == 0)
                    {
                        throw new Exception("InstancesCouldNotBeAggregated has to be empty when aggregation is successful.");
                    }
                }
                else
                {
                    if (instancesToBeDiscardedAfterAggregation.Count == 0)
                    {
                        throw new Exception("Instances cannot be discarded when aggregation is unsuccessful.");
                    }
                    if (this.OriginalUnaggregatedInstances.Count != this.InstancesCouldNotBeAggregated.Count)
                        throw new Exception("instancesCouldNotBeAggregated cannot be empty when aggregation is unsucessful.");
                }
            }
            else
            {
                throw new Exception("Original unaggregated instances must be > 0");
            }
        }
    }
}