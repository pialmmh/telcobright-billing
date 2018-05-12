using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Testers
{
    public class TestChargeable
    {
        public string Name { get; set; }
        public int Sg { get; set; }
        public int Sf { get; set; }
        public int Pd { get; set; }
        public int Uom { get; set; }
        public ValueTuple<int, int, int, int> Tuple { get; private set; }
        public void CreateTuple()
        {
            this.Tuple= new ValueTuple<int, int, int, int>(this.Sg, this.Sf, this.Pd, this.Uom);
        }
    }
    public class TestAcc
    {
        public string Name { get; set; }
        public int Sg { get; set; }
        public int Sf { get; set; }
        public int Pd { get; set; }
        public int Uom { get; set; }

        public ValueTuple<int, int, int, int> GeTuple()
        {
            return new ValueTuple<int, int, int, int>(this.Sg, this.Sf, this.Pd, this.Uom);
        }
    }
    public class StringVsTupPerfTester
    {
        Dictionary<ValueTuple<int,int,int,int>,TestAcc> accByTuple=new Dictionary<ValueTuple<int, int, int,int>, TestAcc>();
        private static Random random = new Random();
        private Dictionary<string,TestAcc> accbyName=new Dictionary<string, TestAcc>();
        List<TestChargeable> chargeables=new List<TestChargeable>();
        public static string RandomString(int length)
            {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public void Test()
        {
            CreateAccountsCollection();
            CreateDummyChargeables();
            Stopwatch st=new Stopwatch();
            st.Reset();
            st.Start();
            LookUpByAccountName();
            st.Stop();
            Console.WriteLine($@"Timereq for loopup by name: {st.ElapsedMilliseconds} secs.");

            st.Reset();
            st.Start();
            LookUpByValueTuple();
            st.Stop();
            Console.WriteLine($@"Timereq for loopup by valueTuple: {st.ElapsedMilliseconds} secs.");
            Console.Read();
        }

        private void LookUpByAccountName()
        {
            this.chargeables.ForEach(c =>

                {
                    TestAcc acc = null;
                    this.accbyName.TryGetValue(c.Name, out acc);
                }
            );
        }
        private void LookUpByValueTuple()
        {
            this.chargeables.ForEach(c =>

                {
                    TestAcc acc = null;
                    this.accByTuple.TryGetValue(c.Tuple, out acc);
                }
            );
        }
        private void CreateDummyChargeables()
        {
            for (int i = 0; i <= 1000000; i++)
            {
                var c = new TestChargeable()
                {
                    Name = RandomString(10),
                    Sg = random.Next(),
                    Sf = random.Next(),
                    Pd = random.Next(),
                    Uom = random.Next(),
                };
                c.CreateTuple();
                this.chargeables.Add(c);

            }
        }

        private void CreateAccountsCollection()
        {
            for (int i = 0; i <= 100; i++)
            {
                var newAcc = new TestAcc()
                {
                    Name = RandomString(10),
                    Sg = random.Next(),
                    Sf = random.Next(),
                    Pd = random.Next(),
                    Uom = random.Next(),
                };
                this.accbyName.Add(newAcc.Name,newAcc);
                this.accByTuple.Add(newAcc.GeTuple(),newAcc);

            }
        }
    }
}
