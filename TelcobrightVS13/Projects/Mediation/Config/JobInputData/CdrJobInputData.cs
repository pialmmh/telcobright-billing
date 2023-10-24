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
        public job TelcobrightJob { get; }
        public NewCdrPreProcessor PreProcessor { get; }
        public List<string[]> OriginalRows= new List<string[]>();
        public NewCdrWrappedJobForMerge(job telcobrightJob, NewCdrPreProcessor preProcessor)
        {
            TelcobrightJob = telcobrightJob;
            this.PreProcessor = preProcessor;
            foreach (string[] row in preProcessor.TxtCdrRows)
            {
                this.OriginalRows.Add(row);
            }
        }
        public int AppendTailJobRows(NewCdrWrappedJobForMerge tailJob)
        {
            this.PreProcessor.TxtCdrRows.AddRange(tailJob.PreProcessor.TxtCdrRows);
            return this.PreProcessor.TxtCdrRows.Count;
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
        public job TelcobrightJob { get; }
        public Dictionary<long, NewCdrWrappedJobForMerge> MergedJobsDic { get; set; }= new Dictionary<long, NewCdrWrappedJobForMerge>();
        public bool IsBatchJob => this.MergedJobsDic.Any();
        public CdrJobInputData(MediationContext mediationContext, PartnerEntities context, ne ne, job telcobrightJob)
        {
            this.MediationContext = mediationContext;
            this.Context = context;
            this.Ne = ne;
            this.TelcobrightJob = telcobrightJob;
        }
    }
}
