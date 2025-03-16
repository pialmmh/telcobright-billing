using TelcobrightMediation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.IO;
using System.Linq;
using TelcobrightFileOperations;
using LibraryExtensions;
using LibraryExtensions.LibraryExtensions;
using MediationModel;
using Quartz;
using QuartzTelcobright;
using TelcobrightMediation.Config;
using Process.helper;

namespace Process
{

    [DisallowConcurrentExecution]
    [Export("TelcobrightProcess", typeof(AbstractTelcobrightProcess))]
    public class RamDiskMounter : AbstractTelcobrightProcess
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public override string RuleName => this.GetType().ToString();
        public override string HelpText => "Method to Create Cdr Job";
        public override int ProcessId => 112;

        public override void Execute(IJobExecutionContext schedulerContext)
        {
            string operatorName = schedulerContext.JobDetail.JobDataMap.GetString("operatorName");
            TelcobrightConfig tbc = ConfigFactory.GetConfigFromSchedulerExecutionContext(
                schedulerContext, operatorName);
            CdrSetting cdrSetting = tbc.CdrSetting;
            string entityConStr = ConnectionManager.GetEntityConnectionStringByOperator(operatorName, tbc);
            PartnerEntities context = new PartnerEntities(entityConStr);
            Console.WriteLine($"RamDiskMounter {operatorName}");
            try
            {
                int idOprator = context.telcobrightpartners
                   .Where(c => c.databasename == tbc.Telcobrightpartner.databasename).Select(c => c.idCustomer)
                   .First();
                foreach (ne thisSwitch in context.nes.Where(c => c.idCustomer == idOprator).ToList())
                {
                    string vaultName = thisSwitch.SourceFileLocations;
                    //Vault vault = tbc.DirectorySettings.Vaults.First(c => c.Name == vaultName);
                    FileLocation fileLocation = tbc.DirectorySettings.FileLocations[vaultName];
                    string vaultPath = fileLocation.StartingPath;
                    if (!CheckIfRamDiskDirExists(vaultPath))
                    {
                        char[] mountingPoint = vaultPath.Split(':')[0].ToCharArray();
                        RamDiskHelper ramdisk = new RamDiskHelper(6144, mountingPoint[0]);
                        ramdisk.Create();
                        foreach (KeyValuePair<string, FileLocation> fileloc in tbc.DirectorySettings.FileLocations.Where(f => f.Key != vaultName))
                        {
                            string startingPath = fileloc.Value.StartingPath;
                            if (!CheckIfRamDiskDirExists(startingPath))
                            {
                                Directory.CreateDirectory(startingPath);
                            }
                        }
                    };
                }
            }
            catch (Exception e1)
            {
                Console.WriteLine(e1);
                ErrorWriter.WriteError(e1, "RamDiskMounter", null, "", operatorName, context);
            }
        }

        private bool CheckIfRamDiskDirExists(string mountingPoint)
        {
            return Directory.Exists(mountingPoint);
        }
    }
}
