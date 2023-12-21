using TelcobrightMediation;
using Newtonsoft.Json;
using System.ComponentModel.Composition;
using System.IO;
using TelcobrightFileOperations;
using System.Linq;
using System;
using LibraryExtensions.ConfigHelper;
using MediationModel;
using TelcobrightInfra;
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
        public object Execute(ITelcobrightJobInput jobInputData)
        {
            //returning corrrect jobCompletion status is important, because file may not be deleted due to pre-requisite
            //in that case the job must not be marked as "complete" in database
            OptimizerJobInputData input = (OptimizerJobInputData)jobInputData;
            string entityConStr = DbUtil.GetEntityConnectionString(input.Tbc.DatabaseSetting);
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
                    Console.WriteLine("Processing Optimizer: " + input.Job.JobName + ", type: File Delete");
                    File.Delete(delParam.FileLocation.ServerIp.Replace("/", Path.DirectorySeparatorChar.ToString()) + Path.DirectorySeparatorChar.ToString() + delParam.FileName);
                    return JobCompletionStatus.Complete;
                }
                else if (delParam.FileLocation.LocationType == "vault")
                {
                    Console.WriteLine("Processing Optimizer: " + input.Job.JobName + ", type: File Delete");
                    string fileToDelete = delParam.FileLocation.StartingPath.Replace("/", Path.DirectorySeparatorChar.ToString()) + Path.DirectorySeparatorChar + delParam.FileName;
                    if (File.Exists(fileToDelete))
                    {
                        File.Delete(fileToDelete);
                    }
                    return JobCompletionStatus.Complete;
                }
            }
            return JobCompletionStatus.Incomplete;
        }

        public object PreprocessJob(object data)
        {
            throw new NotImplementedException();
        }

        public object PostprocessJob(object data)
        {
            throw new NotImplementedException();
        }

        public ITelcobrightJob createNewNonSingletonInstance()
        {
            throw new NotImplementedException();
        }

        private static JobParamFileDelete GetJobParamByHandlingDeserializeErrorFromBackslash
            (OptimizerJobInputData input)
        {
            JobParamFileDelete delParam = new JobParamFileDelete();
            string jobParameter = input.Job.JobParameter;
            if (jobParameter.Contains("\"PathSeparator\":\"\\"))
            {
                jobParameter = jobParameter.Replace("\"PathSeparator\":\"\\", "\"PathSeparator\":\"`");
                jobParameter = jobParameter.Replace("unsplit\\", "unsplit`");
                delParam = JsonConvert.DeserializeObject<JobParamFileDelete>(jobParameter);
                delParam.FileLocation.PathSeparator = delParam.FileLocation.PathSeparator.Replace("`", "\\");
                delParam.FileName = delParam.FileName.Replace("`", "\\");

                return delParam;
            }
            delParam = JsonConvert.DeserializeObject<JobParamFileDelete>(jobParameter);
            return delParam;
        }
    }
}
