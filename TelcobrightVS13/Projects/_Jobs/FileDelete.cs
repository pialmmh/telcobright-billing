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
    public class TaskFileDelete : ITelcobrightJob
    {
        public override string ToString() => this.RuleName;
        public string RuleName => "JobDeleteFile";
        public string HelpText => "Delete File in a location";
        public int Id => 8;
        public JobCompletionStatus Execute(ITelcobrightJobInput jobInputData)
        {
            //returning corrrect jobCompletion status is important, because file may not be deleted due to pre-requisite
            //in that case the job must not be marked as "complete" in database
            OptimizerJobInputData input = (OptimizerJobInputData)jobInputData;
            string entityConStr =
                ConnectionManager.GetEntityConnectionStringByOperator(input.Tbc.DatabaseSetting.DatabaseName);
            JobParamFileDelete delParam = null;
            delParam = GetJobParamByHandlingDeserializeErrorFromBackslash(input);
            //check if prereq jobs have been finished or not
            using (PartnerEntities context = new PartnerEntities(entityConStr))
            {
                if (delParam.JobPrerequisite.CheckComplete(context) == false)
                {
                    return JobCompletionStatus.Incomplete;
                }
                if (delParam.FileLocation.LocationType == "local")
                {
                    Console.WriteLine("Processing Optimizer: " + input.TelcobrightJob.JobName + ", type: File Delete");
                    File.Delete(delParam.FileLocation.ServerIp.Replace("/", Path.DirectorySeparatorChar.ToString()) + Path.DirectorySeparatorChar.ToString() + delParam.FileName);
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

        private static JobParamFileDelete GetJobParamByHandlingDeserializeErrorFromBackslash
            (OptimizerJobInputData input)
        {
            JobParamFileDelete delParam = null;
            var jobParameter = input.TelcobrightJob.JobParameter;
            if (jobParameter.Contains("\"PathSeparator\":\"\\"))
            {
                jobParameter = jobParameter.Replace("\"PathSeparator\":\"\\", "\"PathSeparator\":\"`");
                delParam = JsonConvert.DeserializeObject<JobParamFileDelete>(jobParameter);
                delParam.FileLocation.PathSeparator = delParam.FileLocation.PathSeparator.Replace("`", "\\");
                return delParam;
            }
            delParam = JsonConvert.DeserializeObject<JobParamFileDelete>(jobParameter);
            return delParam;
        }
    }
}
