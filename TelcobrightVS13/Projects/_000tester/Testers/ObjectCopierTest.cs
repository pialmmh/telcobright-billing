using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using Spring.Expressions;

namespace Utils.Testers
{
    public class ObjectCopierTest
    {
        private cdr OldObject { get; set; }
        private cdr NewObject { get; set; }
        private List<string> SpringExpressions { get; set; }
        private int _iteration = 100000;
        public ObjectCopierTest(cdr newObject)
        {
            NewObject = newObject;
            SpringExpressions = new List<string>()
            {"NewObject.SwitchId=OldObject.SwitchId",
             "NewObject.SequenceNumber=OldObject.SequenceNumber",
             "NewObject.CustomerID=OldObject.CustomerID",
             "NewObject.StartTime=OldObject.StartTime",
             "NewObject.CustomerCost = OldObject.CustomerCost"
            };
        }

        public void Test()
        {
            OldObject = new cdr()
            {
                SwitchId = 1,
                SequenceNumber = 1001,
                InPartnerId = 99,
                StartTime = DateTime.Now,
                InPartnerCost = Convert.ToDecimal(".2133333333333333333")
            };
            
            TestExecTimeByNativeCode();
            TestExecTimeByExpressionParsing();
            
        }

        private void TestExecTimeByNativeCode()
        {
            var start = DateTime.Now;
            for (int i = 0; i < _iteration; i++)
            {
                NewObject.SwitchId = OldObject.SwitchId;
                NewObject.SequenceNumber = OldObject.SequenceNumber;
                NewObject.InPartnerId = OldObject.InPartnerId;
                NewObject.StartTime = OldObject.StartTime;
                NewObject.InPartnerCost = OldObject.InPartnerCost;
            }
            var end = DateTime.Now;
            var elapsedTime = (end - start).TotalMilliseconds;
            Console.WriteLine("Time Required by Native Code:" + elapsedTime.ToString() + " mili seconds");
        }

        private void TestExecTimeByExpressionParsing()
        {
            List<IExpression> parsedExpressions = SpringExpressions.Select(c => Expression.Parse(c)).ToList();
            var start = DateTime.Now;
            for (int i = 0; i < _iteration; i++)
            {
                parsedExpressions.ForEach(c => c.GetValue(this, null));
            }
            var end = DateTime.Now;
            var elapsedTime = (end - start).TotalMilliseconds;
            Console.WriteLine("Time Required by Expression Parsing:" + elapsedTime.ToString() + "mili seconds");
        }
        
    }
}
