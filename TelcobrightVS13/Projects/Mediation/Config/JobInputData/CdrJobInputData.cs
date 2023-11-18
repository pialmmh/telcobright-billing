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
    public class NewCdrWrappedJobForMerge {
        public job Job { get; }
        public NewCdrPreProcessor PreProcessor { get; }
        public List<string[]> OriginalRows { get; }= new List<string[]>();
        public List<cdrinconsistent> OriginalCdrinconsistents { get; }= new List<cdrinconsistent>();
        public NewCdrWrappedJobForMerge(job job, NewCdrPreProcessor preProcessor)
        {
            Job = job;
            this.PreProcessor = preProcessor;
            foreach (string[] row in preProcessor.TxtCdrRows)
            {
                this.OriginalRows.Add(row);
            }
            foreach (var inconsistentCdr in preProcessor.InconsistentCdrs)
            {
                this.OriginalCdrinconsistents.Add(inconsistentCdr);
            }
        }
        public int AppendTailJobRows(NewCdrWrappedJobForMerge tailJob)
        {
            this.PreProcessor.TxtCdrRows.AddRange(tailJob.PreProcessor.TxtCdrRows);
            foreach (cdrinconsistent inconsistentCdr in tailJob.PreProcessor.InconsistentCdrs)
            {
                this.PreProcessor.InconsistentCdrs.Add(inconsistentCdr);
            }
            int mergedCount = this.PreProcessor.TxtCdrRows.Count+ this.PreProcessor.InconsistentCdrs.Count;
            if (this.OriginalRows.Count + this.OriginalCdrinconsistents.Count> mergedCount)
                throw new Exception("Sum of Original rows and inconistent must be < mergedCount");
            return mergedCount;
        }
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
