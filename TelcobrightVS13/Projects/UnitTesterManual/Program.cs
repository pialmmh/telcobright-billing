using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Decoders;
using TelcobrightMediation;

namespace UnitTesterManual
{
    class Program
    {
        static void Main(string[] args)
        {
            string operatorName = "jsl";//change here...
            IFileDecoder cdrDecoder=new ZteTdmDecoder();//change here...
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
                    Console.WriteLine("Dump rawCdrs? (Y/N)?");
                    var keyInfo = Console.ReadKey();
                    Console.WriteLine();
                    bool dumpRawCdr = false;
                    if (new List<char>() {'y', 'Y'}.Contains(keyInfo.KeyChar))
                    {
                        dumpRawCdr = true;
                        Console.WriteLine("Executing NewCdr for " + operatorName + ". Raw cdr dumping is enabled.");
                    }
                    else
                    {
                        Console.WriteLine("Executing NewCdr for " + operatorName + ". Raw cdr dumping is disabled.");
                    }
                    new MockNewCdrProcessor(operatorName,cdrDecoder,dumpRawCdr).Execute();
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
