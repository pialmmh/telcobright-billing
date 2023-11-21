using System.Collections.Generic;

namespace TelcobrightMediation
{
    public class EventAggregationResult
    {
        public string[] AggregatedInstance { get; set; }=null;
        public List<string[]> InstancesCouldNotBeAggregated { get; set; }= new List<string[]>();
        public List<string[]> InstancesToBeDiscardedAfterAggregation { get; set; }= new List<string[]>();
    }
}