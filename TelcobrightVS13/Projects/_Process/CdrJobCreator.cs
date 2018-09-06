using TelcobrightMediation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using TelcobrightFileOperations;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediationModel;
using MySql.Data.MySqlClient;
using Quartz;
using QuartzTelcobright;
using TelcobrightMediation.Config;

namespace Process
{

    [Export("TelcobrightProcess", typeof(ITelcobrightProcess))]
    public class CdrJobCreator : ITelcobrightProcess
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => this.GetType().ToString();
        public string HelpText => "Method to Create Cdr Job";
        public int ProcessId => 101;
        
        public void Execute(IJobExecutionContext schedulerContext)
        {
            //todo: remove temp code
            Console.WriteLine("Going to sleep");
            Thread.Sleep(10000);
            Console.WriteLine("Sleep complete");
            return;
            //
            string operatorName = schedulerContext.JobDetail.JobDataMap.GetString("operatorName");
            try
            {
                TelcobrightConfig tbc = ConfigFactory.GetConfigFromSchedulerExecutionContext(
                    schedulerContext, operatorName);
                string entityConStr = ConnectionManager.GetEntityConnectionStringByOperator(operatorName);
                using (PartnerEntities context = new PartnerEntities(entityConStr))
                {
                    int idOprator = context.telcobrightpartners
                        .Where(c => c.databasename == tbc.DatabaseSetting.DatabaseName).Select(c => c.idCustomer)
                        .First();
                    foreach (ne thisSwitch in context.nes.Where(c => c.idCustomer == idOprator).ToList())
                    {
                        try
                        {
                            if (thisSwitch.SkipCdrListed == 1) continue;
                            Console.WriteLine("Listing Files in Switch:" + thisSwitch.SwitchName);
                            string vaultName = thisSwitch.SourceFileLocations;
                            Vault vault = tbc.DirectorySettings.Vaults.First(c => c.Name == vaultName);
                            var fileNames = vault.GetFileList();
                            if (tbc.CdrSetting.DescendingOrderWhileListingFiles == true)
                                fileNames = fileNames.OrderByDescending(c => c).ToList();
                            foreach (string fileName in fileNames)
                            {
                                if (fileName.EndsWith(".tmp") || fileName.Contains(".filepart")
                                ) //make sure when copying to vault always .tmp ext used
                                {
                                    continue;
                                }
                                //check if that filename already exists
                                job newCdr = new job();
                                newCdr.JobName = fileName; //.Split(null)[1];//16
                                bool exists = context.jobs.Any(c => c.JobName == newCdr.JobName
                                                                    && c.idNE == thisSwitch.idSwitch);
                                if (exists == false) //File Name Does not exist
                                {
                                    int priority = context.enumjobdefinitions.First(c => c.id == 1).Priority;
                                    newCdr.idNE = thisSwitch.idSwitch;
                                    newCdr.CreationTime = DateTime.Now;
                                    newCdr.Status = 7; //local, so downloaded in local switch directory
                                    newCdr.priority = priority;
                                    newCdr.idjobdefinition = 1; //new cdr
                                    context.jobs.Add(newCdr);
                                    context.SaveChanges();
                                }
                            }
                        } //try
                        catch (Exception e1)
                        {
                            Console.WriteLine(e1);
                            ErrorWriter wr =
                                new ErrorWriter(e1, "CdrJobCreator/SwitchId:" + thisSwitch.idSwitch, null, "",
                                    operatorName);
                        } //catch
                    } //for each customerswitchinfo
                }
            }
            catch (Exception e1)
            {
                Console.WriteLine(e1);
                ErrorWriter wr = new ErrorWriter(e1, "CdrJobCreator", null, "", operatorName);
            }
        }
    }
}
