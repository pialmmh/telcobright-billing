using System.Collections.Generic;

namespace TelcobrightMediation.Cdr.Collection.PreProcessors
{
    public class NewAndOldEventsWrapper<T>
    {
        public string UniqueBillId { get; set; }
        public List<T> NewUnAggInstances { get; set; }
        public List<T> OldUnAggInstances { get; set; }= new List<T>();

    }
}