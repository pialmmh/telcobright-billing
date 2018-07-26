﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;
using MySql.Data.MySqlClient;

namespace TelcobrightMediation
{
    public class FileCopyJobInputData : ITelcobrightJobInput
    {
        public TelcobrightConfig Tbc { get; }
        
        public job TelcobrightJob { get; }
        public PartnerEntities Context { get; }

        public FileCopyJobInputData(TelcobrightConfig tbc,job telcobrightJob)
        {
            this.Tbc = tbc;
            this.TelcobrightJob = telcobrightJob;
            this.Context = null;
        }
    }
}
