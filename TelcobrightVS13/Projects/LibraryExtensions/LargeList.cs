using System;
using System.Collections.Generic;

namespace LibraryExtensions
{
    public class LargeList<T>
    {
        private long noOfInternalLists = 5;
        private long segmentSize;
        public long MaxCapacity => this.segmentSize * this.noOfInternalLists;
        public long Count => this.segment0.Count + this.segment1.Count +
                                 this.segment2.Count + this.segment3.Count + this.segment4.Count;
        private readonly List<T> segment0 = new List<T>();
        private readonly List<T> segment1 = new List<T>();
        private readonly List<T> segment2 = new List<T>();
        private readonly List<T> segment3 = new List<T>();
        private readonly List<T> segment4 = new List<T>();

        public LargeList(long segmentSize)
        {
            this.segmentSize = segmentSize;
        }

        public T this[long index]
        {
            get
            {
                if (index >= 0 && index < this.Count)
                {
                    var targetlist = GetListByListno(GetListNoByIndex(index));
                    return targetlist[Convert.ToInt32(index % segmentSize)];
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"Index must be >=0 & <{this.Count}");
                }
            }
        }

        public void Add(T item)
        {
            if (this.Count>=this.MaxCapacity)
            {
                throw new ArgumentOutOfRangeException($"Max capacity {this.MaxCapacity} has exceeded.");
            }
            long nextIndex = this.Count;
            var targetlist = GetListByListno(GetListNoByIndex(nextIndex));
            targetlist.Add(item);
        }

        private long GetListNoByIndex(long index)
        {
            return index / this.segmentSize;
        }
        private List<T> GetListByListno(long targetListNumber)
        {
            if (targetListNumber < 0 || targetListNumber > this.noOfInternalLists)
                throw new ArgumentOutOfRangeException($"Current list number must be >=0 & <" + this.noOfInternalLists);
            switch (targetListNumber)
            {
                case 0:
                    return this.segment0;
                case 1:
                    return this.segment1;
                case 2:
                    return this.segment2;
                case 3:
                    return this.segment3;
                case 4:
                    return this.segment4;
            }
            throw new ArgumentOutOfRangeException($"Current list number must be >=0 & <" + this.noOfInternalLists);
        }
    }
}
