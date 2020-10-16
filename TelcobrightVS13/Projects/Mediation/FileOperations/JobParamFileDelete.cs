using TelcobrightMediation;

namespace TelcobrightFileOperations
{
    public class JobParamFileDelete
    {
        public string FileName { get; set; }
        public FileLocation FileLocation { get; set; }
        public JobPreRequisite JobPrerequisite { get; set; }
        public JobParamFileDelete()//default , for json
        {
            this.JobPrerequisite = new JobPreRequisite();
        }
        public JobParamFileDelete(string filename, FileLocation filelocation)
        {
            this.FileName = filename;
            this.FileLocation = filelocation;
            this.JobPrerequisite = new JobPreRequisite();
        }
    }
}