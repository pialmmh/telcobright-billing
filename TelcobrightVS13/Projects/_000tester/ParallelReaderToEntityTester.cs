using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;

namespace Utils
{
    class ParallelReaderToEntityTester
    {
        public void Test()
        {
            using (PartnerEntities context=new PartnerEntities())
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                var objects = context.Database.SqlQuery<rate>("select * from rate limit 0,200000")
                    .AsParallel().ToList();
                stopwatch.Stop();
                Console.WriteLine("EXEC TIME Parallel METHOD {0}", stopwatch.ElapsedMilliseconds);

                stopwatch = new Stopwatch();
                stopwatch.Start();
                var rates = context.Database.SqlQuery<rate>("select * from rate limit 0,200000") .ToList();
                stopwatch.Stop();
                Console.WriteLine("EXEC TIME non parallel METHOD {0}", stopwatch.ElapsedMilliseconds);

                Console.Read();
            }
        }
    }
}
