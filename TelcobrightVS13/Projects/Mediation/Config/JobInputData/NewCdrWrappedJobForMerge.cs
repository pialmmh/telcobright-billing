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
        public int NewAndInconsistentCount => this.OriginalRows.Count + this.OriginalCdrinconsistents.Count;
        public int AppendTailJobRows(NewCdrWrappedJobForMerge tailJob)
        {
            this.PreProcessor.TxtCdrRows.AddRange(tailJob.PreProcessor.TxtCdrRows);
            foreach (cdrinconsistent inconsistentCdr in tailJob.PreProcessor.InconsistentCdrs)//blocking collection, can't addRange
            {
                this.PreProcessor.InconsistentCdrs.Add(inconsistentCdr);
            }
            int mergedCount = this.PreProcessor.TxtCdrRows.Count+ this.PreProcessor.InconsistentCdrs.Count;
            if (this.OriginalRows.Count + this.OriginalCdrinconsistents.Count> mergedCount)
                throw new Exception("Sum of Original rows and inconistent must be < mergedCount");
            return mergedCount;
        }
    }
}
