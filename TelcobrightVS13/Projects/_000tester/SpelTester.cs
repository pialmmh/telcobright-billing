using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using FlexValidation;
using LibraryExtensions;
using Spring.Context;
using Spring.Context.Support;
using Spring.Core.TypeResolution;
using Spring.Expressions;
using Expression = Spring.Expressions.Expression;

namespace Utils
{
    
    public class SpelTester
    {
        private Inventor tesla;
        private Inventor pupin;
        private Society ieee;

        
        public SpelTester()
        {
            //Func<Expression, bool> dateTimeCreator = () => "2008-01-01 00:00:00".ConvertToDateTimeFromMySqlFormat();
            //this.Vars.Add(
                
            tesla = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");
            tesla.Inventions = new string[]
            {
                "Telephone repeater", "Rotating magnetic field principle",
                "Polyphase alternating-current system", "Induction motor",
                "Alternating-current power transmission", "Tesla coil transformer",
                "Wireless communication", "Radio", "Fluorescent lights"
            };
            tesla.PlaceOfBirth.City = "Smiljan";


            pupin = new Inventor("Mihajlo Pupin", new DateTime(1854, 10, 9), "Serbian");
            pupin.Inventions = new string[] { "Long distance telephony & telegraphy", "Secondary X-Ray radiation", "Sonar" };
            pupin.PlaceOfBirth.City = "Idvor";
            pupin.PlaceOfBirth.Country = "Serbia";


            ieee = new Society();
            ieee.Members.Add(tesla);
            ieee.Members.Add(pupin);
            ieee.Officers["president"] = pupin;
            ieee.Officers["advisors"] = new Inventor[] {tesla, pupin};
        }

        public static Dictionary<K, V> HashtableToDictionary<K, V>(Hashtable table)
        {
            return table
                .Cast<DictionaryEntry>()
                .ToDictionary(kvp => (K)kvp.Key, kvp => (V)kvp.Value);
        }
        public void Test()
        {
            TestName();

            string[] row = new string[] { "1", "2", "3", "2008-01-01 00:00:00" };
            

            

            //TypeRegistry.RegisterType("Convert", "System.Convert");

            IExpression exp = Expression.Parse("DateParsers['stringToDateConverterFromMySqlFormat'].Invoke(obj[3])");
            //var d = (DateTime)exp.GetValue(validatableRow);
            

            IDictionary vars=new Hashtable();
            
            
            //TypeRegistry.
            //TypeRegistry.RegisterType("StringExtensions", "LibraryExtensions.StringExtensions");
            //TypeRegistry.RegisterType("",);
            bool isDateTime= (bool)ExpressionEvaluator.GetValue(row, "[3].IsMySqlDateTime()");

            

            DateTime dt1 = (DateTime)ExpressionEvaluator.GetValue(this,"dateTimeCreator.Invoke()" );

            //DateTime dt2 = (DateTime)ExpressionEvaluator.GetValue(this, "date('" + tempDate.ToString("yyyy-MM-dd") + "')");




            TypeRegistry.RegisterType("Convert", "System.Convert");
            bool tf = (bool) ExpressionEvaluator.GetValue(row, "Convert.ToInt32([2]) > 0");

            
            Bar b=new Bar();
            string val = (string) ExpressionEvaluator.GetValue(b, "[1]");

            
            int num= (int)ExpressionEvaluator.GetValue(b, "Convert.ToInt32([0])");
            string result = (string)ExpressionEvaluator.GetValue(b,
                "String.Concat([0],[1],[2])"); // evaluated to 2
            //TestLiteralExpressions();
            //TestPropertiesArrayListsEtc();
            //TestMethods();
            //TestTypes();
            RelationalOperators();
        }

        void TestTypes()
        {
            ExpressionEvaluator.GetValue(null, "1 is int");

            ExpressionEvaluator.GetValue(null, "DateTime.Today");

            var x=ExpressionEvaluator.GetValue(null, "new string[] {'abc', 'efg'}");
            Type dateType = (Type) ExpressionEvaluator.GetValue(null, "T(System.DateTime)");
            Debug.Print(dateType.ToString());

            IDictionary vars= new Hashtable();
            Expression.RegisterFunction("func", "{|Inventor| $Inventor.Name}", vars);
            IDictionary<string,object> dic=new ConcurrentDictionary<string, object>();
            //Convert.ToInt32()
            foreach (DictionaryEntry kv in vars)
            {
                dic.Add(kv.Key.ToString(),kv.Value);
            }
            var val=ExpressionEvaluator.GetValue(tesla, "#func(this.tesla)", dic);  // 120

            Type evalType =
                (Type) ExpressionEvaluator.GetValue(null, "T(Spring.Expressions.ExpressionEvaluator, Spring.Core)");

            bool trueValue = (bool) ExpressionEvaluator.GetValue(tesla, "T(System.DateTime) == DOB.GetType()");
            TypeRegistry.RegisterType("Society", typeof(Society));

            Inventor pupin = (Inventor)ExpressionEvaluator.GetValue(ieee, "Officers[Society.President]");
        }
        void RelationalOperators()
        {
            cdrfieldlist cdrf = null;
            using (PartnerEntities context = new PartnerEntities())
            {
                cdrf = context.cdrfieldlists.Take(1).ToList().First();
            }
            Console.WriteLine(ExpressionEvaluator.GetValue(cdrf, "fieldnumber > 0").ToString()); 
            Console.WriteLine(ExpressionEvaluator.GetValue(cdrf, "fieldnumber == 0").ToString());
            Console.WriteLine(ExpressionEvaluator.GetValue(cdrf, "String.IsNullOrEmpty(fieldname)").ToString());
            Console.WriteLine(ExpressionEvaluator.GetValue(cdrf, "String.IsNullOrEmpty(fieldname)").ToString());
            Console.WriteLine(ExpressionEvaluator.GetValue(cdrf, "fieldnumber between {-1,1}").ToString());
            Console.WriteLine(ExpressionEvaluator.GetValue(tesla, "GetAge(date('2015-01-01'))").ToString());
            Console.WriteLine(ExpressionEvaluator.GetValue(tesla, "T(System.DateTime) == DOB.GetType()"));
            Console.WriteLine();

            Console.WriteLine(ExpressionEvaluator.GetValue(tesla, "Numbers.Sum()"));
            var x = new List<int>() {1, 1};
            Console.WriteLine(x.Sum());
            ExpressionEvaluator.GetValue(null, "2 == 2");  // true

            ExpressionEvaluator.GetValue(null, "date('1974-08-24') != DateTime.Today");  // true

            ExpressionEvaluator.GetValue(null, "2 < -5.0"); // false

            ExpressionEvaluator.GetValue(null, "DateTime.Today <= date('1974-08-24')"); // false

            ExpressionEvaluator.GetValue(null, "'Test' >= 'test'"); // true
        }

        private void TestPropertiesArrayListsEtc()
        {
            int year = (int)ExpressionEvaluator.GetValue(tesla, "DOB.Year");  // 1856
            IExpression exp = Expression.Parse("DOB.Year");
            year = (int) exp.GetValue(tesla, null); 
            string city = (string)ExpressionEvaluator.GetValue(pupin, "PlaCeOfBirTh.CiTy");  // "Idvor"   

            exp = Expression.Parse("Inventions[3]");
            var invention = (string) exp.GetValue(tesla, null);
        }


        private void TestMethods()
        {
            //string literal
            char[] chars = (char[]) ExpressionEvaluator.GetValue(null, "'test'.ToCharArray(1, 2)");  // 't','e'

//date literal
            int year = (int) ExpressionEvaluator.GetValue(null, "date('1974/08/24').AddYears(31).Year"); // 2005

// object usage, calculate age of tesla navigating from the IEEE society.

            ExpressionEvaluator.GetValue(ieee,
                "Members[0].GetAge(date('2005-01-01'))"); // 149 (eww..a big anniversary is coming up ;)
        }

        private void TestLiteralExpressions()
        {
            string helloWorld = (string)ExpressionEvaluator.GetValue(null, "'Hello World'"); // evals to "Hello World"

            string tonyPizza = (string)ExpressionEvaluator.GetValue(null, "'Tony\\'s Pizza'"); // evals to "Tony's Pizza"

            double avogadrosNumber = (double)ExpressionEvaluator.GetValue(null, "6.0221415E+23");

            int maxValue = (int)ExpressionEvaluator.GetValue(null, "0x7FFFFFFF");  // evals to 2147483647

            DateTime birthday = (DateTime)ExpressionEvaluator.GetValue(null, "date('1974/08/24')");

            DateTime exactBirthday =
                (DateTime)ExpressionEvaluator.GetValue(null, " date('19740824T131030', 'yyyyMMddTHHmmss')");

            bool trueValue = (bool)ExpressionEvaluator.GetValue(null, "true");

            object nullValue = ExpressionEvaluator.GetValue(null, "null");
        }
        private void TestName()
        {
            Inventor tesla = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");

            tesla.PlaceOfBirth.City = "Smiljan";

            string evaluatedName = (string)ExpressionEvaluator.GetValue(tesla, "Name");

            string evaluatedCity = (string)ExpressionEvaluator.GetValue(tesla, "PlaceOfBirth.City");
            IExpression exp = Expression.Parse("Name");

            evaluatedName = (string)exp.GetValue(tesla, null);
        }

        

        
    }
    public class Bar
    {
        private string[] numbers;

        public Bar()
        {
            numbers = new[]{ "1", "two", "three" };
        }
        public string this[int index]
        {
            get { return numbers[index]; }
            set { numbers[index] = value; }
        }
    }
}
