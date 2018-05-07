using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Decoders;
using TelcobrightMediation;
using LibraryExtensions;
using MediationModel;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;

namespace UnitTesterManual
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo projectDirectory= new DirectoryInfo(FileAndPathHelper.GetBinPath()).Parent.Parent.Parent;
            string appConfigPathOfUtilInstallConfig = projectDirectory.GetDirectories()
                .First(d => d.Name == "UtilInstallConfig").GetFiles().First(f => f.Name == "App.config")
                .FullName;
            string operatorName = File.ReadLines(appConfigPathOfUtilInstallConfig)
                .First(line => line.Contains("JsonConfigFileNameForPortalCopyForSingleOperator"))
                .Split('=')[2].Split('"')[1].Split('_')[1];

            int idCdrFormat = 0;
            using (PartnerEntities context =
                new PartnerEntities(ConnectionManager.GetEntityConnectionStringByOperator(operatorName)))
            {
                idCdrFormat = context.nes.Where(c => c.idCustomer ==
                                                     context.telcobrightpartners.Where(
                                                             p => p.databasename == operatorName)
                                                         .Select(p => p.idCustomer).ToList().FirstOrDefault())
                    .Select(n => n.idcdrformat).First();

                DirectoryInfo mefExtensionDirectory = projectDirectory.GetDirectories()
                    .First(d => d.Name == "WS_Topshelf_Quartz").GetDirectories().First(dir => dir.Name == "Extensions");
                //DecoderComposer decoderComposer = new DecoderComposer();
                //decoderComposer.ComposeFromPath(mefExtensionDirectory.FullName);
                //var decoders = decoderComposer.Decoders.ToDictionary(d => d.Id.ToString());
                //IFileDecoder cdrDecoder = decoders[idCdrFormat.ToString()];
                //IEventCollector eventCollector = new RawTextCdrCollectorFromDb(operatorName, "mockcdr", context);
                IEventCollector eventCollector = new FileBasedTextCdrCollector(new CdrCollectorInputData());
                Console.WriteLine("*********Running for Operator:" + operatorName + "**********");
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
                        var processor= new MockNewCdrProcessor(operatorName, eventCollector)
                        {
                            ProcessInReverseOrder = false
                        };
                        processor.Execute();
                        break;
                    case '2':
                        Console.WriteLine("Executing ErrorCdr for " + operatorName + ".");
                        var mockErrorProcessor = new MockCdrReProcessor { Job = new MockCdrErrorProcessorJob() };
                        mockErrorProcessor.Execute(operatorName);
                        break;
                    case '3':
                        Console.WriteLine("Executing ReprocessCdr for " + operatorName + ".");
                        new MockCdrReProcessor().Execute(operatorName);
                        break;
                    case '4':
                        Console.WriteLine("Executing CdrEraser for " + operatorName + ".");
                        var mockProcessor = new MockCdrReProcessor {Job = new MockCdrEraserJob()};
                        mockProcessor.Execute(operatorName);
                        break;
                    case '5':
                        Console.WriteLine("Executing RawCdrDecoder & Dumper for " + operatorName + ".");
                        MockNewCdrProcessor dumpProcessor = new MockNewCdrProcessor(operatorName, eventCollector);
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
}
