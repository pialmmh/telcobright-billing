using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Decoders;
using TelcobrightMediation;
using LibraryExtensions;
namespace UnitTesterManual
{
    class Program
    {
        static void Main(string[] args)
        {
            string appConfigPathOfUtilInstallConfig = new DirectoryInfo(FileAndPathHelper.GetBinPath()).Parent.Parent.Parent
                .GetDirectories()
                .First(d => d.Name == "UtilInstallConfig").GetFiles().First(f => f.Name == "App.config")
                .FullName;
            string operatorName = File.ReadLines(appConfigPathOfUtilInstallConfig)
                .First(line => line.Contains("JsonConfigFileNameForPortalCopyForSingleOperator"))
                .Split('=')[2].Split('"')[1].Split('_')[1];
            IFileDecoder cdrDecoder=new ZteTdmDecoder();//change here...
            //IFileDecoder cdrDecoder = new DialogicControlSwitchDecoder();//change here...
            Console.WriteLine("*********Running for Operator:"+operatorName+"**********");
            Console.WriteLine("Select a mock process:");
            Console.WriteLine("1=NewCdr");
            Console.WriteLine("2=ErrorCdr");
            Console.WriteLine("3=CdrReprocess");
            Console.WriteLine("4=CdrEraser");
            Console.WriteLine("5=RawCdrDecoderAndDumper");
            Console.WriteLine("q=Quit");
            ConsoleKeyInfo ki = new ConsoleKeyInfo();
            ki = Console.ReadKey(true);
            char cmdName = Convert.ToChar(ki.Key);
            switch (cmdName)
            {
                case '1':
                    new MockNewCdrProcessor(operatorName,cdrDecoder).Execute();
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
                case '5':
                    Console.WriteLine("Executing RawCdrDecoder & Dumper for " + operatorName + ".");
                    MockNewCdrProcessor dumpProcessor= new MockNewCdrProcessor(operatorName, cdrDecoder);
                    dumpProcessor.DecodeAndDumpOnly = true;
                    dumpProcessor.Execute();
                    break;
                case 'q':
                case 'Q':
                    return;
            }
            
                    
        }
    }
}
