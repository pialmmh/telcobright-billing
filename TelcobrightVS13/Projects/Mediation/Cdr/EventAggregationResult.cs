using System;
using System.Collections.Generic;
using LibraryExtensions;
using MediationModel;

namespace TelcobrightMediation
{
    public class EventAggregationResult
    {
        public string UniqueEventId { get; }
        public List<string[]> AllUnaggregatedInstances { get; }
        public string[] AggregatedInstance { get; }
        public List<string[]> NewInstancesCouldNotBeAggregated { get; }
        public List<string[]> OldInstancesCouldNotBeAggregated { get; }
        public List<string[]> InstancesToBeDiscardedAfterAggregation { get; }
        public EventAggregationResult(string uniqueEventId, List<string[]> allUnaggregatedInstances,
            string[] aggregatedInstance, List<string[]> newInstancesCouldNotBeAggregated,
            List<string[]> oldInstancesCouldNotBeAggregated, List<string[]> instancesToBeDiscardedAfterAggregation)
        {
            UniqueEventId = uniqueEventId;
            this.AllUnaggregatedInstances = allUnaggregatedInstances;
            AggregatedInstance = aggregatedInstance;
            this.NewInstancesCouldNotBeAggregated = newInstancesCouldNotBeAggregated;
            this.OldInstancesCouldNotBeAggregated = oldInstancesCouldNotBeAggregated;
            InstancesToBeDiscardedAfterAggregation = instancesToBeDiscardedAfterAggregation;
            if (this.AllUnaggregatedInstances.Count > 0)
            {
                if(this.UniqueEventId.IsNullOrEmptyOrWhiteSpace())
                    throw new Exception("Unique event id cannot be empty");

                if (this.AggregatedInstance != null) // successfully aggregated 
                {
                    if (aggregatedInstance[Fn.IdCall].IsNullOrEmptyOrWhiteSpace())
                        throw new Exception("Idcall not set for aggregated instance.");
                    if (this.InstancesToBeDiscardedAfterAggregation.Count + 1 != this.AllUnaggregatedInstances.Count )
                    {
                        throw new Exception("InstancesToBeDiscarded+1 must be equal to originalinstances when aggregation is successful.");
                    }
                    if (NewInstancesCouldNotBeAggregated.Count > 0 || NewInstancesCouldNotBeAggregated.Count > 0)
                    {
                        throw new Exception("RowsCouldNotBeAggregated has to be empty when aggregation is successful.");
                    }
                }
                else//aggregation failed
                {
                    if (instancesToBeDiscardedAfterAggregation.Count > 0)
                    {
                        throw new Exception("Instances cannot be discarded when aggregation is unsuccessful.");
                    }
                    if (this.AllUnaggregatedInstances.Count != this.NewInstancesCouldNotBeAggregated.Count+
                                                                    this.OldInstancesCouldNotBeAggregated.Count)
                        throw new Exception("AllUnAggregatedInstances count != newInstancesCouldNotBeAggregated+OldInstancesCouldNotBeAggregated.");
                }
            }
            else
            {
                throw new Exception("Original unaggregated instances must be > 0");
            }
        }
    }
}