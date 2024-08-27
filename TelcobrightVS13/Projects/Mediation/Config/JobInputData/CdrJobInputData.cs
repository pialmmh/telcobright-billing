using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;
using MySql.Data.MySqlClient;

namespace TelcobrightMediation
{
    public class CdrJobOutput
    {
        public List<job> Jobs { get; set; }
        public List<FileInfo> FilesToCleanUp { get; set; }= new List<FileInfo>();
    }
    public class CdrJobInputData : ITelcobrightJobInput
    {
        public TelcobrightConfig Tbc => this.MediationContext.Tbc;
        public CdrSetting CdrSetting => this.Tbc.CdrSetting;
        public MediationContext MediationContext { get; }
        public AutoIncrementManager AutoIncrementManager => this.MediationContext.AutoIncrementManager;
        public PartnerEntities Context { get; }
        public ne Ne { get; }
        public NeAdditionalSetting NeAdditionalSetting { get; }
        public job Job { get; }
        public Dictionary<long, NewCdrWrappedJobForMerge> MergedJobsDic { get; set; }= new Dictionary<long, NewCdrWrappedJobForMerge>();
        public bool IsBatchJob => this.MergedJobsDic.Any();

        public CdrJobInputData(MediationContext mediationContext, PartnerEntities context, ne ne, 
            job job)
        {
            this.MediationContext = mediationContext;
            this.Context = context;
            this.Ne = ne;
            this.Job = job;
            Dictionary<int, NeAdditionalSetting> neWiseAdditionalSettings = this.CdrSetting.NeWiseAdditionalSettings;
            NeAdditionalSetting neAdditionalSetting = null;
            neWiseAdditionalSettings.TryGetValue(ne.idSwitch, out neAdditionalSetting);
            this.NeAdditionalSetting = neAdditionalSetting;

        }
    }
}
