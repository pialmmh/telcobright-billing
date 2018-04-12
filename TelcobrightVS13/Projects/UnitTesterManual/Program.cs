using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelcobrightMediation;

namespace UnitTesterManual
{
    class Program
    {
        static void Main(string[] args)
        {
            string operatorName = "platinum";
            Console.WriteLine("Select a mock cdr process for operator:"+operatorName+".");
            Console.WriteLine("1=NewCdr");
            Console.WriteLine("2=ErrorCdr");
            Console.WriteLine("3=CdrReprocess");
            Console.WriteLine("4=CdrEraser");
            Console.WriteLine("q=Quit");
            ConsoleKeyInfo ki = new ConsoleKeyInfo();
            ki = Console.ReadKey(true);
            char cmdName = Convert.ToChar(ki.Key);
            switch (cmdName)
            {
                case '1':
                    Console.WriteLine("Executing NewCdr for "+operatorName+".");
                    new MockNewCdrProcessor().Execute(operatorName);
                    break;
                case '2':
                    Console.WriteLine("Executing ErrorCdr for " + operatorName + ".");
                    new MockCdrReProcessor().Execute(operatorName);
                    break;
                case '3':
                    Console.WriteLine("Executing ReprocessCdr for " + operatorName + ".");
                    new MockCdrReProcessor().Execute(operatorName);
                    break;
                case '4':
                    Console.WriteLine("Executing CdrEraser for " + operatorName + ".");
                    var mockProcessor = new MockCdrReProcessor { Job = new MockCdrEraserJob() };
                    mockProcessor.Execute(operatorName);
                    break;
                case 'q':
                case 'Q':
                    return;
            }
            
                    
        }
    }
}
