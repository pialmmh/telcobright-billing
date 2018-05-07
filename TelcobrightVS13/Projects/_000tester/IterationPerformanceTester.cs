using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FlexValidation;
using LibraryExtensions;
using Spring.Core.TypeResolution;
using Spring.Expressions;
using ValidationResult = FlexValidation.ValidationResult;

namespace Utils
{
    public class IterationPerformanceTester
    {
        public void Test()
        {
            List<cdrfieldlist> cdrfieldlists = null;
            using (PartnerEntities context = new PartnerEntities())
            {
                cdrfieldlists = context.cdrfieldlists.ToList();
            }
            Console.Clear();
            int iterationCount = 1000000;
            Stopwatch stopwatch = new Stopwatch();

            Console.WriteLine($@"Entering single loop...");
            stopwatch.Start();
            for (int i = 0; i < iterationCount; i++)
            {
                UpdateInSingleLoop(cdrfieldlists);
            }
            stopwatch.Stop();
            Console.WriteLine("Time required for single loop: " + stopwatch.ElapsedMilliseconds + " milliseconds.");

            stopwatch.Reset();
            Console.WriteLine($@"Entering multiple loop...");
            stopwatch.Start();
            for (int i = 0; i < iterationCount; i++)
            {
                UpdateInMultipleLoop(cdrfieldlists);
            }
            stopwatch.Stop();
            Console.WriteLine("Time required for multiple loop: " + stopwatch.ElapsedMilliseconds + " milliseconds.");


            stopwatch.Reset();
            Console.WriteLine($@"Testing by updating single entity in separate function...");
            stopwatch.Start();
            for (int i = 0; i < iterationCount; i++)
            {
                cdrfieldlists.ForEach(c => UpdateSingleEntity(c));
            }
            stopwatch.Stop();
            Console.WriteLine("Time required for single update in method: " + stopwatch.ElapsedMilliseconds + " milliseconds.");

            Console.Read();
        }

        void UpdateInSingleLoop(List<cdrfieldlist>cdrFieldlists)
        {
            cdrFieldlists.ForEach(c =>
            {
                c.FieldName = "adfadf";
                c.fieldnumber = 100;
                c.IsNumeric = 0;
            });
        }

        void UpdateInMultipleLoop(List<cdrfieldlist> cdrFieldlists)
        {
            cdrFieldlists.ForEach(c => c.FieldName = "adfadf");
            cdrFieldlists.ForEach(c => c.fieldnumber = 100);
            cdrFieldlists.ForEach(c => c.IsNumeric = 0);
        }
        void UpdateSingleEntity(cdrfieldlist cdrFieldlists)
        {
            cdrFieldlists.FieldName = "adfadf";
            cdrFieldlists.fieldnumber = 100;
            cdrFieldlists.IsNumeric = 0;
        }
    }
}
