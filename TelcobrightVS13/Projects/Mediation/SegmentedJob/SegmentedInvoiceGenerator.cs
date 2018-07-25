using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;
using Newtonsoft.Json;
using TelcobrightMediation;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;

namespace TelcobrightMediation
{
    public class SegmentedInvoiceGenerator : AbstractRowBasedSegmentedJobProcessor
    {
        private CdrCollectorInputData CdrCollectorInput { get; }
        public SegmentedInvoiceGenerator(CdrCollectorInputData cdrCollectorInput,int batchSizeWhenPreparingLargeSqlJob, 
            string indexedColumnName, string dateColumnName)
            : base(cdrCollectorInput.TelcobrightJob, cdrCollectorInput.Context, batchSizeWhenPreparingLargeSqlJob,
                indexedColumnName, dateColumnName)
        {
            this.CdrCollectorInput = cdrCollectorInput;
        }

        public override ISegmentedJob CreateJobSegmentInstance(jobsegment jobSegment)
        {
            DayWiseRowIdsCollection dayWiseRowsIdsCollection = base.DeserializeDayWiseRowIdsCollection(jobSegment);
            string selectSql = dayWiseRowsIdsCollection.GetSelectSql();
            DbRowCollector<acc_transaction> dbRowCollector =
                new DbRowCollector<acc_transaction>(this.CdrCollectorInput.CdrJobInputData, selectSql);
            List<acc_transaction> transactions = dbRowCollector.Collect();
            var con = this.CdrCollectorInput.Context.Database.Connection;
            if(con.State!=ConnectionState.Open) con.Open();
            var cmd = con.CreateCommand();
            cmd.CommandText = $"select jobstate from job where id=" +
                              $"{this.CdrCollectorInput.TelcobrightJob.id}";
            string jobStateJson = (string)cmd.ExecuteScalar();
            Dictionary<string, string> jobStateMap = null;
            if (jobStateJson.IsNullOrEmptyOrWhiteSpace()==false)
            {
                jobStateMap = JsonConvert.DeserializeObject<Dictionary<string,string>>(jobStateJson);
            }
            decimal invoicedAmountAfterLastSegment = 0;
            if (jobStateMap!=null & jobStateMap.Any())
            {
                invoicedAmountAfterLastSegment = Convert.ToDecimal(jobStateMap["invoicedAmountAfterLastSegment"]);
            }
            CdrInvoicingJob cdrInvoicingJob=new CdrInvoicingJob(this.CdrCollectorInput,transactions,invoicedAmountAfterLastSegment);
            return cdrInvoicingJob;
        }
        public override void FinishJob(List<jobsegment> jobsegments, Action<object> additionalJobFinalizingTask)
        {
            using (DbCommand cmd = ConnectionManager.CreateCommandFromDbContext(this.Context))
            {
                additionalJobFinalizingTask?.Invoke(cmd);
                if (jobsegments.Any(c => c.status != 1) == false) //no incomplete segment
                {
                    cmd.ExecuteCommandText(" set autocommit=0; ");
                    cmd.ExecuteCommandText(" update job set completiontime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                                           " status=9, Error='' " +//status 9=ready for posting
                                           " where id=" + this.TelcobrightJob.id);
                    //delete job segments which can hold large amount of data
                    cmd.ExecuteCommandText(" delete from jobsegment where idjob=" + this.TelcobrightJob.id);
                    cmd.ExecuteCommandText(" commit; ");
                }
            }
        }
    }
}
