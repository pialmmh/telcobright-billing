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
    public class LcrJobInputData : ITelcobrightJobInput
    {
        public TelcobrightConfig Tbc { get; }
        public job TelcobrightJob { get; }
        public ne Ne { get; }
        public LcrJobInputData(TelcobrightConfig tbc, job telcobrightJob)
        {
            this.Tbc = tbc;
            this.TelcobrightJob = telcobrightJob;
        }
    }
}
