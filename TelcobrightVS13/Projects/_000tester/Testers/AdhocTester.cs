using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions.CommonHelper;
using Spring.Expressions;
using Utils.Testers;

namespace Utils
{
    public class AdhocTester
    {
        public void StartTesting()
        {
            

            List<string> fieldNames = new ClassTypeOrPropertyHelper().GetSimplePropertyNames(typeof(cdr));
            File.WriteAllText("c:/temp/entityProperties.txt", String.Join(Environment.NewLine, fieldNames));
            //new JsonDeserializeTestWithNonEmptyConstructor().Test();
            var start = DateTime.Now;
            //for (int i = 0; i < 10000; i++)
            {
                FlexValidationTest();
            }
            var end = DateTime.Now;
            Console.WriteLine("Total time:" + (end - start).TotalSeconds);
            cdrfieldlist cf = null;
            using (jslEntities context = new jslEntities())
            {
                cf = context.cdrfieldlists.Take(1).ToList().First();
                //cf = context.cdrfieldlists.Where(c=>c.FieldName=="PDD").Take(1).First();
            }
            //Conditions.When("",cf);
            IExpression exp = Expression.Parse("1 == 1");
            IDictionary vars = new Hashtable();
            exp = Expression.Parse($@"fieldname !='PDD'
                             ?isdatetime > 0
                             :1=1");
            bool result = (bool)exp.GetValue(cf);

            exp = Expression.Parse($@"When(fieldname !='PDD'
                             ?isdatetime > 0
                             :1 == 1");
            result = (bool)exp.GetValue(cf);

            //SpelTester spelTester = new SpelTester();
            //spelTester.Test();

            var fluentTester = new TestToCompareRuleVsRuleSet();
            fluentTester.TestAndCompare();
            //TestEntityContext();
            //TestEntityFrameworkCastingToAbstractClass();
            //DateChangerInICDRFileForTestCases();
        }

        private static void FlexValidationTest()
        {
            FlexValidatorTester validatorTester = new FlexValidatorTester();
            validatorTester.Test();
        }
    }
}
