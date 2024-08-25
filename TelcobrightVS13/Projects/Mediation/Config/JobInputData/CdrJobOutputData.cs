using System.Collections.Generic;
using System.IO;
using MediationModel;

namespace TelcobrightMediation
{
    public class CdrJobOutputData
    {
        public job Job { get; set; }
        public List<FileInfo> FilesToCleanUp { get; set; }= new List<FileInfo>();
    }
}