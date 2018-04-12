using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using MediationModel;
using System.IO;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Data.Entity;
using System.Reflection;
using MySql.Data.Entity;
using Utils.Models;

namespace Utils
{
    public class Program
    {
        public static void Main(string[] args)
        {

            //DbConfiguration.SetConfiguration(new MySqlEFConfiguration());
            //TupleTest tupTest = new TupleTest();
            //tupTest.test();
            Console.WriteLine("Press a key:");
            Console.WriteLine("C=Generate Crud Statements");
            
            ConsoleKeyInfo ki = new ConsoleKeyInfo();
            ki = Console.ReadKey(true);
            char CmdName = Convert.ToChar(ki.Key);
            switch (CmdName)
            {
                case 'c':
                case 'C':
                    List<string> tableForCruds = null;// new List<string>() { "acc_balance","acc_billable","acc_ledger","acc_ledger_summary","acc_tmp_credit","account" };
                    List<string> namespacesToInclude = new List<string>() { "System",
                                                                            "System.Collections.Generic",
                                                                            "System.Linq",
                                                                            "System.Text",
                                                                            "System.Threading.Tasks",
                                                                            "LibraryExtensions"
                                                                           };
                    string connectionString = "server=localhost;database=platinum;uid=root;password=Takay1#$ane";
                    using (MySqlConnection con=new MySqlConnection(connectionString))
                    {
                        con.Open();
                        using (PartnerEntities context = new PartnerEntities(con,false))
                        {
                            string currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Substring(6);
                            DirectoryInfo di = new DirectoryInfo(currentPath);
                            string targetDir = ((di.Parent).Parent).Parent.FullName + Path.DirectorySeparatorChar
                                               + "Models_Mediation" + Path.DirectorySeparatorChar + "EntityExtensions" + Path.DirectorySeparatorChar +
                                               "Crud";
                            CrudGenerator crudGenerator = new CrudGenerator("MediationModel", true, tableForCruds, namespacesToInclude, context);
                            crudGenerator.GenerateCrudForAllClasses(targetDir);
                        }
                    }
                    break;
                case 'P':
                case 'p':
                    Console.WriteLine("Add prepaid amount for wholesale voice.");
                    Console.WriteLine("Enter Payment Amount, partnerid then externalPaymentId, separated by comma ");
                    var arr = Console.ReadLine().Split(',');
                    double amount = Convert.ToDouble(arr[0]);
                    int idPartner = Convert.ToInt32(arr[1]);
                    int externalId = Convert.ToInt32(arr[2]);
                    string creditAccount = "d1/sg4/p" + idPartner + "/sf1/pd0/servicebank/uomUSD";
                    Dictionary<string, string> paramDeferredTmpPayment = new Dictionary<string, string>()
                    {
                        { "debitAccount","d1/virtualcash/uomUSD"},
                        { "creditAccount",creditAccount}
                    };
                    acc_tmp_credit tmpCreditProcessor = new acc_tmp_credit()
                    {
                        externalId = externalId,
                        idpartner = idPartner,
                        jobExecutionPriority = 1,
                        //jsonDetail = JsonConvert.SerializeObject(paramDeferredTmpPayment),
                        TargetAccountName = creditAccount,
                        quantity = amount,
                        starttime = DateTime.Now,
                    };
                    using (PartnerEntities context = new PartnerEntities())
                    {
                        context.acc_tmp_credit.Add(tmpCreditProcessor);
                        context.SaveChanges();
                    }
                    break;
                case 'r':
                case 'R':
                    Console.WriteLine("CDR format string e.g. dialogic then Source folder, separated by ,");
                    arr = Console.ReadLine().Split(',');
                    PartialCdrGenerator pCdrGen = new PartialCdrGenerator(arr[0], arr[1]);
                    pCdrGen.Generate();
                    break;
                default:
                    break;
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
    }
}
