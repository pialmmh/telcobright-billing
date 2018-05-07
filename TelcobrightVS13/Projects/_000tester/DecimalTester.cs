using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    class DecimalTester
    {
        Random rnd=new Random(1024);
        private int totalRecords = 10000;
        List<decimal_test> dts=new List<decimal_test>();
        List<decimal_test> negativeDts = new List<decimal_test>();
        testEntities context=new testEntities();
        public void Test()
        {
            DeletePrevRecords();
            InitNumbers();
            WriteToDb();
            this.dts = null;
            this.dts= RePopulateNumbersFromDb();
            NegateNumbers();
            SumDoubles();
            SumDecimals();
            UpdateDecimalTestsInDb();
            this.dts = null;
            this.dts = RePopulateNumbersFromDb();
            Console.WriteLine("Double sum="+this.dts.Sum(dt=>dt.double1));
            Console.WriteLine("Decimal sum=" + this.dts.Sum(dt => dt.decimal1));
            Console.ReadLine();
        }

        void DeletePrevRecords()
        {
            this.context.Database.ExecuteSqlCommand("delete from decimal_test");
        }

        void InitNumbers()
        {
            foreach (var i in Enumerable.Range(1,this.totalRecords).ToArray())
            {
                double rndNumber = this.rnd.NextDouble()*(22/7);
                this.dts.Add(new decimal_test()
                {
                    double1 = rndNumber,
                    decimal1 = Convert.ToDecimal(rndNumber)
                });
            }
        }

        void WriteToDb()
        {
            this.context.decimal_test.AddRange(this.dts);
            this.context.SaveChanges();
        }

        List<decimal_test> RePopulateNumbersFromDb()
        {
            return this.context.decimal_test.ToList();
        }

        void NegateNumbers()
        {
            this.dts.ForEach(dt =>
            {
                dt.double2 = (-1) * dt.double1;
                dt.decimal2 = (-1)* dt.decimal1;
            });
        }

        void SumDoubles()
        {
            this.dts.ForEach(dt=>dt.doubletotal=dt.double1+dt.double2);
        }

        void SumDecimals()
        {
            this.dts.ForEach(dt=>dt.decimalTotal=dt.decimal1+dt.decimal2);
        }

        void UpdateDecimalTestsInDb()
        {
            this.DeletePrevRecords();
            this.context.decimal_test.AddRange(this.dts);
            this.context.SaveChanges();
        }
    }
}
