using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryExtensions
{
    public class CollectionSegmenter<T>
    {
        //convert enumerable to list to save problems associated with enumerable.skip() with parallel collections
        public List<T> Enumerable { get; }
        private int SkipFromStart { get; set; }
        public CollectionSegmenter(IEnumerable<T> enumerable, int startAtZeroBasedIndex)
        {
            if (enumerable is ParallelQuery)
            {
                throw new Exception("Parallel queries are not supported");
            }
            this.Enumerable = enumerable.ToList();
            this.SkipFromStart = startAtZeroBasedIndex;
        }

        public void ExecuteMethodInSegments(int segmentSize, Action<IEnumerable<T>> method)
        {
            IEnumerable<T> segment;
            while ((segment = GetNextSegment(segmentSize)).Any())
            {
                method.Invoke(segment);
            }
        }

        public List<TOut> ExecuteMethodInSegmentsWithRetval<TOut>(int segmentSize, Func<IEnumerable<T>, List<TOut>> method)
        {
            IEnumerable<T> segment;
            List<TOut> retVal= new List<TOut>();
            while ((segment = GetNextSegment(segmentSize)).Any())
            {
                retVal.AddRange(method.Invoke(segment));
            }
            return retVal;
        }

        public IEnumerable<T> GetNextSegment(int segmentSize)
        {
            if (segmentSize <= 0) throw new Exception("Segment size must be >=0");
            var segment = this.Enumerable.Skip(this.SkipFromStart).Take(segmentSize).AsEnumerable();
            this.SkipFromStart += segmentSize;
            return segment;
        }
    }
}
