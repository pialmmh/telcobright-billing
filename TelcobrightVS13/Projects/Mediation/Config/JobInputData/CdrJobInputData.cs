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
    public class CdrJobInputData : ITelcobrightJobInput
    {
        public TelcobrightConfig Tbc => this.MediationContext.Tbc;
        public CdrSetting CdrSetting => this.Tbc.CdrSetting;
        public MediationContext MediationContext { get; }
        public AutoIncrementManager AutoIncrementManager => this.MediationContext.AutoIncrementManager;
        public PartnerEntities Context { get; }
        public ne Ne { get; }
        public job TelcobrightJob { get; }

        public CdrJobInputData(MediationContext mediationContext, PartnerEntities context, ne ne, job telcobrightJob)
        {
            this.MediationContext = mediationContext;
            this.Context = context;
            this.Ne = ne;
            this.TelcobrightJob = telcobrightJob;
        }
    }
}
