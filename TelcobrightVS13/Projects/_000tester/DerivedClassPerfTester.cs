using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utils
{
    static class CdrFieldListExtFactory
    {
        public static CdrFieldListExt CreateCdrFieldListExt(cdrfieldlist c)
        {
            return new CdrFieldListExt()
            {
                fieldnumber = c.fieldnumber,
                FieldName = c.FieldName,
                IsNumeric = c.IsNumeric,
                IsDateTime = c.IsDateTime
            };
        }
    }

    class CdrFieldListExt : cdrfieldlist
    {

    }

    class CdrFieldListContainer
    {
        public cdrfieldlist Cdrfieldlist { get; }

        public CdrFieldListContainer(cdrfieldlist cdrfieldlist)
        {
            this.Cdrfieldlist = cdrfieldlist;
        }
    }

    public class DerivedClassPerfTester
    {
        private List<cdrfieldlist> CdrFieldLists { get; }
        private List<CdrFieldListExt> CdrFieldListExts { get; }= new List<CdrFieldListExt>();
        private List<CdrFieldListContainer> CdrFieldListContainers { get; } = new List<CdrFieldListContainer>();
        public DerivedClassPerfTester()
        {
            PartnerEntities context=new PartnerEntities();
            this.CdrFieldLists = context.cdrfieldlists.ToList();
            this.CdrFieldLists.ForEach(c => this.CdrFieldListExts.Add(CdrFieldListExtFactory.CreateCdrFieldListExt(c)));
            this.CdrFieldLists.ForEach(c => this.CdrFieldListContainers.Add(new CdrFieldListContainer(c)));
        }

        public void Test()
        {
            while (true)
            {
                int iteration = 50000;
                int andResult = 200;
                Stopwatch stopwatch = new Stopwatch();
                Console.WriteLine("starting derived style");
                using (MD5 md5 = MD5.Create())
                {
                    stopwatch.Start();
                    for (int i = 0; i < iteration - 1; i++)
                    {
                        this.CdrFieldListExts.ForEach(c =>
                        {
                            var sb = new StringBuilder(c.fieldnumber).Append(c.FieldName).Append(c.IsNumeric)
                                .Append(c.IsDateTime).Append(md5.ToString());
                            andResult = andResult & sb.ToString().Length;
                        });
                    }
                }
                stopwatch.Stop();
                Console.WriteLine("andResult=" + andResult);
                Console.WriteLine("Milliseconds Required for ext: " + stopwatch.ElapsedMilliseconds);

                stopwatch.Reset();
                Console.WriteLine("starting conainter style...");
                using (MD5 md5 = MD5.Create())
                {
                    stopwatch.Start();
                    for (int i = 0; i < iteration - 1; i++)
                    {
                        this.CdrFieldListContainers.ForEach(c =>
                        {
                            var sb = new StringBuilder(c.Cdrfieldlist.fieldnumber).Append(c.Cdrfieldlist.FieldName).Append(c.Cdrfieldlist.IsNumeric)
                                .Append(c.Cdrfieldlist.IsDateTime).Append(md5.ToString());
                            andResult = andResult & sb.ToString().Length;
                        });
                    }
                }
                stopwatch.Stop();
                Console.WriteLine("andResult=" + andResult);
                Console.WriteLine("Milliseconds Required for Container: " + stopwatch.ElapsedMilliseconds);
                Thread.Sleep(100);
            }
            
        }
    }
}
