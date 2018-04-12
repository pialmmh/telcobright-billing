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
    public class FlexValidatorTester
    {
        public void Test()
        {
            string[] row = new string[] {"1", "2", "3", "2008-01-01 00:00:00"};
            TypeRegistry.RegisterType("Convert", typeof(System.Convert));

            IExpression exp = Expression.Parse("DateParsers['stringToDateConverterFromMySqlFormat'].Invoke(obj[3])");
            //var d = (DateTime)exp.GetValue(validatableRow);
            var validator = GetBoxedValidatorInstance();
            List<long> elapsedtimes = new List<long>();
            int executionCount = 0;
            Console.Clear();
            Console.WriteLine("Press ESC to stop");
            int iterationCount = 500000;
            do
            {
                while (!Console.KeyAvailable)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    Console.WriteLine($@"Execution {++executionCount}: Entering test loop...");
                    for (int i = 0; i < iterationCount; i++)
                    {
                        ValidationResult validationResult = validator.Validate(row);
                    }
                    stopwatch.Stop();
                    elapsedtimes.Add(stopwatch.ElapsedMilliseconds);
                    Console.WriteLine("Time required for execution: " + stopwatch.ElapsedMilliseconds + " milliseconds.");
                    Console.WriteLine("Average execution time: " + elapsedtimes.Average() + " milliseconds.");
                    Console.WriteLine("Average execution time for Single Iteration: " + elapsedtimes.Average()/iterationCount + " milliseconds.");
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);

            Console.Read();
        }

        private FlexValidator<string[]> GetBoxedValidatorInstance()
        {
            DateTime dt = new DateTime(2008, 1, 1);
            var dic = new Dictionary<string, string>()
            {
                { $@"Validator.BooleanParsers['isNumericChecker'].Invoke(obj[2]) == true 
                     and Validator.IntParsers['intConverterProxy'].Invoke(obj[2]) > 0", "SequenceNumber must be > 0"
                },//public const int Sequencenumber = 2;
                {
                    "!String.IsNullOrEmpty(obj[1]) and !String.IsNullOrWhiteSpace(obj[1])",
                    "OriginatingCalledNumber cannot be empty"
                }, //public const int Originatingcallednumber = 9;
                {
                    "!String.IsNullOrEmpty(obj[2]) and Validator.DoubleParsers['doubleConverterProxy'].Invoke(obj[2]) >= 0",
                    "durationsec must be >= 0"
                },
                {$@"Validator.BooleanParsers['isDateTimeChecker'].Invoke(obj[3]) == true and
                    Validator.DateParsers['strToMySqlDtConverter'].Invoke(obj[3]) > date('"+ dt.ToString("yyyy-MM-dd") +"')",
                    "StartTime must be > "+dt.ToString("yyyy-MM-dd") },//public const int Starttime = 29;
                
            };
            return CreateNewValidatorInstance(dic);
        }
        
        private FlexValidator<string[]> CreateNewValidatorInstance(Dictionary<string, string> dicExpressions)
        {
            FlexValidator<string[]> flexValidator = new FlexValidator<string[]>(
                continueOnError: false,
                throwExceptionOnFirstError: false,
                validationExpressionsWithErrorMessage: dicExpressions);
            flexValidator.DateParsers.Add( "strToMySqlDtConverter", str => str.ConvertToDateTimeFromMySqlFormat());
            flexValidator.DoubleParsers.Add("doubleConverterProxy", str => Convert.ToDouble(str));
            flexValidator.IntParsers.Add("intConverterProxy", str => Convert.ToInt32(str));
            flexValidator.BooleanParsers.Add("isDateTimeChecker", str => str.IsDateTime(StringExtensions.MySqlDateTimeFormat));
            flexValidator.BooleanParsers.Add("isNumericChecker", str => str.IsNumeric());
            return flexValidator;
        }

    }
}
