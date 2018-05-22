using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TelcobrightTester;
using LibraryExtensions.CommonHelper;
using Spring.Expressions;
using Utils.Testers;
using Utils.Testers.Employees;

namespace Utils
{
    class Program
    {
     
        static void Main(string[] args)
        {
            //var tester = new FluentTester();
            //tester.Test();
            //tester.PerformanceTest();
            Console.WriteLine("Press a key:");
            Console.WriteLine("1=Test Entity Renaming");
            Console.WriteLine("2=Entity Framework Batch Renaming");
            Console.WriteLine("7=jobqueue File Copy...");
            Console.WriteLine("8=Delete Completed CDR files from Vault.");
            Console.WriteLine("C=Generate Crud Statements");
            Console.WriteLine("p=Process Payment");
            Console.WriteLine("R=Generate PartialCdrFiles");

            ConsoleKeyInfo ki = new ConsoleKeyInfo();
            ki = Console.ReadKey(true);
            char CmdName = Convert.ToChar(ki.Key);

            switch (CmdName)
            {
                case '1':
                    Console.WriteLine("Test for entity renaming...");
                    EntityRenameTester entityTester = new EntityRenameTester();
                    entityTester.Test();

                    Console.WriteLine("Cdrjob Creation Complete...");

                    Console.WriteLine("Processing Complete...");
                    break;

                case '2':
                    Console.WriteLine("");

                    break;
                case '3':
                    Console.WriteLine("Processing 3=Create Cdr Job...");

                    Console.WriteLine("Creating Cdr job Complete...");
                    break;
                case '4':
                    Console.Write("Enter jobid:");
                    string jobId = Console.ReadLine();
                    int idJob = -1;
                    if (int.TryParse(jobId, out idJob))
                    {
                    }
                    else
                    {
                        Console.WriteLine("Invalid job id");
                        break;
                    }
                    Console.WriteLine("Processing 4=Process 1 Job in JobQueCdr...");

                    Console.WriteLine("Processing Complete...");
                    break;
                case '5':
                    Console.WriteLine("Processing 5=Process JobQueCdr...");

                    Console.WriteLine("Processing Complete...");
                    break;
                case '6':
                    Console.WriteLine("Dirctory Sync...");

                    //tb.UpdatePartitionInfo("cdrloaded");
                    //tb.UpdatePartitionInfo("cdrsummary");
                    Console.WriteLine("Processing Complete...");
                    break;
                case '7':
                    Console.WriteLine("jobQueue File Copy...");

                    //tb.UpdatePartitionInfo("cdrloaded");
                    //tb.UpdatePartitionInfo("cdrsummary");
                    Console.WriteLine("Processing Complete...");
                    break;
                case '8':
                    Console.WriteLine("Deleting local completed CDR files from Vault");
                    break;
                case 'c':
                case 'C':
                    List<string>
                        tableForCruds = new List<string>();//todo
                            //null; // new List<string>() { "acc_balance","acc_billable","acc_ledger","acc_ledger_summary","acc_tmp_credit","account" };
                    List<string> namespacesToInclude = new List<string>()
                    {
                        "System",
                        "System.Collections.Generic",
                        "System.Linq",
                        "System.Text",
                        "System.Threading.Tasks",
                        "LibraryExtensions"
                    };
                    string currentPath = System.IO.Path
                        .GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)
                        .Substring(6);
                    DirectoryInfo di = new DirectoryInfo(currentPath);
                    string targetDir = ((di.Parent).Parent).Parent.FullName + Path.DirectorySeparatorChar
                                       + "Models_Mediation" + Path.DirectorySeparatorChar + "EntityExtensions" +
                                       Path.DirectorySeparatorChar +
                                       "Crud";
                    List<string> classestoRewritePropertiesAsOverride = new List<string>()
                    {
                        "sum_voice_hr_01",
                        "sum_voice_hr_02",
                        "sum_voice_hr_03",
                        "sum_voice_day_01",
                        "sum_voice_day_02",
                        "sum_voice_day_03"
                    };
                    List<string> classestoOverrideMethods = new List<string>()
                    {
                        "sum_voice_hr_01",
                        "sum_voice_hr_02",
                        "sum_voice_hr_03",
                        "sum_voice_day_01",
                        "sum_voice_day_02",
                        "sum_voice_day_03"
                    };
                    List<string> classesToExcludeICacheble = new List<string>()
                    {
                        "sum_voice_hr_01",
                        "sum_voice_hr_02",
                        "sum_voice_hr_03",
                        "sum_voice_day_01",
                        "sum_voice_day_02",
                        "sum_voice_day_03"
                    };
                    List<string> classesToGenerateFluentValidationRules = new List<string>()
                    {
                        "cdr"
                    };
                    CrudGenerator crudGenerator = new CrudGenerator("MediationModel", true, tableForCruds,
                        namespacesToInclude, classestoRewritePropertiesAsOverride,
                        classestoOverrideMethods, classesToExcludeICacheble,classesToGenerateFluentValidationRules);
                    crudGenerator.GenerateCrudForAllClasses(targetDir);

                    break;

                case 's':
                case 'S':
                    
                        tableForCruds = new List<string>();//todo
                    //null; // new List<string>() { "acc_balance","acc_billable","acc_ledger","acc_ledger_summary","acc_tmp_credit","account" };
                    namespacesToInclude = new List<string>()
                    {
                        "System",
                        "System.Collections.Generic",
                        "System.Linq",
                        "System.Text",
                        "System.Threading.Tasks",
                        "LibraryExtensions"
                    };
                    targetDir = $@"c:/temp/java_autogenerated_classes/";

                    classestoRewritePropertiesAsOverride = new List<string>();

                    classestoOverrideMethods = new List<string>();

                    classesToExcludeICacheble = new List<string>();

                    classesToGenerateFluentValidationRules = new List<string>();
                    
                    CrudGeneratorRoo crudGeneratorRoo = new CrudGeneratorRoo("MediationModel", true, tableForCruds,
                        namespacesToInclude, classestoRewritePropertiesAsOverride,
                        classestoOverrideMethods, classesToExcludeICacheble, classesToGenerateFluentValidationRules);
                    crudGeneratorRoo.GenerateCrudForAllClasses(targetDir);

                    break;

                case 'P':
                case 'p':
                    Console.WriteLine("Add prepaid amount for wholesale voice.");
                    Console.WriteLine("Enter Payment Amount, partnerid then externalPaymentId, separated by comma ");

                    break;
                case 'r':
                case 'R':

                default:
                    break;
            }
        }

        
        static void TestEntityContext()
        {
            //using (MySqlConnection con =
            //    new MySqlConnection(ConfigurationManager.ConnectionStrings["Partner"].ConnectionString))
            {
                //con.Open();
                using (PartnerEntities model = new PartnerEntities())
                {
                    model.Database.Connection.Open();
                    var cmd = model.Database.Connection.CreateCommand();
                    cmd.CommandText = ("set autocommit=0;insert into enumvatrule values(-1,'test','testdesc')");
                    cmd.ExecuteNonQuery();


                    var cmd2 = model.Database.Connection.CreateCommand();
                    cmd2.CommandText = ("set autocommit=0;insert into enumvatrule values(-10,'test','should not be seen')");
                    cmd2.ExecuteNonQuery();


                    cmd.CommandText = ("commit;");
                    cmd.ExecuteNonQuery();

                    

                    List<enumvatrule> vatRules = model.enumvatrules.ToList();
                }
            }

        }

        static void DateChangerInICDRFileForTestCases()
        {
            foreach (string file in Directory.GetFiles("C:\\Dropbox\\partial testing csvs with fake cdr\\icdrformat"))
            {
                List<string[]> lines = ParseTextFileToList(file, ';', 0);
                foreach (string[] line in lines)
                {
                    string globalCallId = line[3];
                    if (globalCallId == "AAAAEVbVTzSBcAABEtSs8w" || globalCallId == "BAAAEVbVTzSBcAABEtSs8w")
                    {
                        ChangeMonthToMay(line);
                    }
                }
                File.WriteAllLines(file,lines.Select(c=>string.Join(";",c)));
            }
        }

        static void ChangeMonthToMay(string[] line)
        {
            for (var i = 0; i < line.Length; i++)
            {
                string col = line[i];
                if (col.StartsWith("2016-03"))
                    line[i] = col.Replace("2016-03", "2016-05");
            }
        }
        static void TestEntityFrameworkCastingToAbstractClass()
        {
            Console.WriteLine("Testing Entity Framework Casting to Abstract Classes...");
            List<TestAbstractClass> lstAbstractClasses=new List<TestAbstractClass>();
            
            using (PartnerEntities model = new PartnerEntities())
            {
                var vatrules = model.Database.SqlQuery<vatrule>
                (
                    "select * from enumvatrule"
                ).ToList();
                foreach (var erule in vatrules)
                {
                    lstAbstractClasses.Add((TestAbstractClass)erule);
                }
            }
        }


        class EntityRenameTester
        {
            public void Test()
            {
                //using (PartnerEntities context = new PartnerEntities())
                //{
                //    //context.accounts.Add(new Account()
                //    //{
                //    //    id = 1,
                //    //    accountName = "test",
                //    //});
                //    //context.SaveChanges();
                //}
                
            }
        }


        static void ProcessPayment(string accountName, double amount)
        {

            //PaymentProcessor paymentPrcessor = new PaymentProcessor(accContext);
        }

        
        static void SubstringTest()
        {
            string PhoneNumber = "91901783564";
            DateTime starttime = DateTime.Now;
            string prefix = "";
            long LoopCount = 10000000;
            for (long m = 1; m <= LoopCount; m++)
            {
                for (int i = PhoneNumber.Length; i > 0; i--)
                {
                    prefix = Dummy(PhoneNumber.Substring(0, i));
                }
            }
            DateTime endtime = DateTime.Now;
            TimeSpan Duration = endtime - starttime;
            double TotalSec = Duration.TotalSeconds;

            starttime = DateTime.Now;
            List<string> lstStrs = new List<string>();
            for (int i = PhoneNumber.Length; i > 0; i--)
            {
                lstStrs.Add(PhoneNumber.Substring(0, i));
            }
            for (long m = 1; m <= LoopCount; m++)
            {
                foreach (string str in lstStrs)
                {
                    prefix = Dummy(str);
                }
            }
            endtime = DateTime.Now;
            Duration = endtime - starttime;
            double TotalSecList = Duration.TotalSeconds;

        }
        static string Dummy(string Prefix)
        {
            return Prefix;
        }

        class TupleTest
        {
            ValueTuple<string, string, string> nestedTuple =
                new ValueTuple<string, string, string>("very", "big", "city");
            Dictionary<ValueTuple<string, string, string, string, string, string, string,
                ValueTuple<string, string, string>>, string> dic = new Dictionary<ValueTuple<string, string, string, string, string, string, string, ValueTuple<string, string, string>>, string>();
            public void test()
            {
                var combinedTup = new ValueTuple<string, string, string, string, string, string, string, ValueTuple<string, string, string>>
                    ("bangladesh", "is", "a", "country", "dhaka", "is", "a", nestedTuple);
                dic.Add(combinedTup, "sentence");
                var start = DateTime.UtcNow;
                for (int i = 0; i < 1000000; i++)
                {
                    var anotherTup = new ValueTuple<string, string, string, string, string, string, string, ValueTuple<string, string, string>>
                    ("bangladesh", "is", "a", "country", "dhaka", "is", "a",
                    new ValueTuple<string, string, string>("very", "big", "city"));
                    string val = "";
                    dic.TryGetValue(anotherTup, out val);
                }
                var end = DateTime.UtcNow;
                long elapsedMS = (long)(end - start).TotalMilliseconds;

            }
        }
        public static List<string[]> ParseTextFileToList(string path, char separator, int linesToSkipBefore)
        {
            List<string[]> parsedData = new List<string[]>();
            using (StreamReader readFile = new StreamReader(path))
            {
                string line;
                string[] row;
                int thisLine = 1;
                while ((line = readFile.ReadLine()) != null)
                {
                    if (thisLine <= linesToSkipBefore)
                    {
                        thisLine += 1;
                        continue;
                    }
                    else if (line.Trim() == "" || line.Contains(separator) == false)//skip blanks and not having separator char
                    {
                        continue;
                    }
                    row = line.Split(separator);
                    parsedData.Add(row);
                    thisLine += 1;
                }
            }
            return parsedData;
        }
    }
    abstract class TestAbstractClass
    {
        public abstract int id { get; set; }
        public abstract string Type { get; set; }
        public abstract string description { get; set; }
    }

    class vatrule : TestAbstractClass
    {
        public override int id { get; set; }
        public override string Type { get; set; }
        public override string description { get; set; }
    }
    

}
