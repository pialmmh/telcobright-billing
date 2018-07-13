using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;
using Wintellect.PowerCollections;
namespace Utils
{
    public class LargelistTester
    {
        public void Test()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            List<rate> rates = GetRatesByRatePlan(37,20000);

            stopwatch.Stop();
            Console.WriteLine("EXEC TIME {0}", stopwatch.ElapsedMilliseconds);

            

            long itemCount = 80000000;
            //long segmentSize = itemCount/5;

            //UseSingleList(itemCount);
            //UseChunkedList(itemCount,segmentSize);
            UseBigList(itemCount);
        }

        private static List<rate> GetRatesByRatePlan(int idRatePlan, int segmentSize)
        {
            List<rate> rates=new List<rate>();
            using (PartnerEntities context = new PartnerEntities())
            {
                int startLimit = 0;
                bool rateExists = true;
                while (rateExists)
                {
                    GC.TryStartNoGCRegion(250 * 1000 * 1000);
                    var sql = $@"select * from rate where idrateplan={idRatePlan}
                         order by id limit {startLimit},{segmentSize}";
                    var rateSegment = context.Database.SqlQuery<rate>
                    (sql).ToList();
                    if (rateSegment.Any())
                    {
                        rates.AddRange(rateSegment);
                        startLimit += segmentSize;
                    }
                    else
                    {
                        rateExists = false;
                    }
                    GC.EndNoGCRegion();
                }
            }
            return rates;
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
        private void UseBigList(long itemCount)
        {

            BigList<long> llist = new BigList<long>();
            for (long i = 0; i < itemCount; i++)
            {
                llist.Add(i);
            }
        }
    }
}
