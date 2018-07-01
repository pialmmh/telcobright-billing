using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace Utils
{
    public class LargelistTester
    {
        public void Test()
        {
            long itemCount = 60000000;
            long segmentSize = itemCount/5;
            
            //UseSingleList(itemCount);
            UseChunkedList(itemCount,segmentSize);
        }

        private void UseSingleList(long itemCount)
        {
            List<long> numbers=new List<long>();
            for (long i = 0; i < itemCount; i++)
            {
                numbers.Add(i);
            }
        }

        private void UseChunkedList(long itemCount, long segmentSize)
        {
            
            LargeList<long> llist = new LargeList<long>(segmentSize);
            for (long i = 0; i < itemCount; i++)
            {
                llist.Add(i);
            }
        }
    }
}
