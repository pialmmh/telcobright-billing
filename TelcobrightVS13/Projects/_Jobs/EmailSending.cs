using TelcobrightMediation;
using Newtonsoft.Json;
using System.ComponentModel.Composition;
using System.IO;
using TelcobrightFileOperations;
using System.Linq;
using System;
using MediationModel;
using TelcobrightMediation.Config;

namespace Jobs
{

    [Export("Job", typeof(ITelcobrightJob))]
    public class EmailSending : ITelcobrightJob
    {
        public override string ToString() => this.RuleName;
        public string RuleName => this.GetType().Name;
        public string HelpText => "Sends email notification";
        public int Id => 20;

        public JobCompletionStatus Execute(ITelcobrightJobInput jobInputData)
        {
            //returning corrrect jobCompletion status is important, because file may not be deleted due to pre-requisite
            //in that case the job must not be marked as "complete" in database
            StandardTelcobrightJobInput input = (StandardTelcobrightJobInput) jobInputData;
            string entityConStr =
                ConnectionManager.GetEntityConnectionStringByOperator(input.Tbc.DatabaseSetting.GetOperatorName);
            JobParamFileDelete delParam = null;
            //check if prereq jobs have been finished or not
            job telcobrightJob = input.TelcobrightJob;
            using (PartnerEntities context = new PartnerEntities(entityConStr))
            {
                if (delParam.JobPrerequisite.CheckComplete(context) == false)
                {
                    return JobCompletionStatus.Incomplete;
                }
                if (delParam.FileLocation.LocationType == "local")
                {
                    Console.WriteLine("Processing Optimizer: " + input.TelcobrightJob.JobName + ", type: File Delete");
                    File.Delete(delParam.FileLocation.ServerIp.Replace("/", Path.DirectorySeparatorChar.ToString()) +
                                Path.DirectorySeparatorChar.ToString() + delParam.FileName);
                    return JobCompletionStatus.Complete;
                }
                else if (delParam.FileLocation.LocationType == "vault")
                {
                    Vault vault = input.Tbc.DirectorySettings.Vaults.First(c => c.Name == delParam.FileLocation.Name);
                    if (vault.DeleteSingleFile(delParam.FileName) == false) return JobCompletionStatus.Incomplete;
                }
            }
            return JobCompletionStatus.Complete;
        }
    }
}
