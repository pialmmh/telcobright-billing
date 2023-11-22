using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightInfra.PerformanceAndOptimization
{
    public static class GarbageCollectionHelper
    {
        public static void CompactGCNowForOnce()
        {
            Console.WriteLine("WARNING!!!!!!!! MANUAL GARBAGE COLLECTION AND COMPACTION OF LOH.");
            System.Runtime.GCSettings.LargeObjectHeapCompactionMode = System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
        }
    }
}
