using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;
using MySql.Data.MySqlClient;

namespace TelcobrightMediation
{
    public class FileOperationJobInputData : ITelcobrightJobInput
    {
        public job Job { get; }
        public PartnerEntities Context { get; }
        public TelcobrightConfig Tbc { get; }

        public FileOperationJobInputData(job telcobrightJob, TelcobrightConfig tbc)
        {
            this.Job = telcobrightJob;
            this.Tbc = tbc;
            this.Context = null;
        }
    }
}
