using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    class LargeList<T>
    {
        private int noOfInternalLists = 5;
        private int segmentSize;
        public int MaxCapacity => this.segmentSize * this.noOfInternalLists;
        private int currentListNumber = 1;
        private List<T> currentList;
        private List<T> segment1=new List<T>();
        private List<T> segment2=new List<T>();
        private List<T> segment3=new List<T>();
        private List<T> segment4=new List<T>();
        private List<T> segment5=new List<T>();

        public LargeList(int segmentSize)
        {
            this.segmentSize = segmentSize;
            this.currentList = this.segment1;
        }

        public T this[int index]
        {
            get
            {
                if (index >= 0 && index <= this.MaxCapacity)
                {

                }
                else
                {
                    throw new ArgumentOutOfRangeException($"Argument out of range in property indexer.");
                }
            }
        }

        public void Add(T item)
        {
            if (segmentSize>)
            {
                
            }
        }

        private List<T> GetListByListno()
        {
            if (currentListNumber<1||currentListNumber>5)
                throw new ArgumentOutOfRangeException($"Current list number must be >=1 & <=5");
            
        }
    }
}
