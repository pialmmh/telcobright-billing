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
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Config;
using CdrInvoicingJob = TelcobrightMediation.Cdr.CdrInvoicingJob;

namespace TelcobrightMediation
{
    public class SegmentedInvoiceGenerator : AbstractRowBasedSegmentedJobProcessor
    {
        private AccountingJobInputData AccountingJobInputData { get; }
        public SegmentedInvoiceGenerator(AccountingJobInputData accountingJobInputData, 
            string indexedColumnName, string dateColumnName)
            : base(accountingJobInputData.TelcobrightJob, accountingJobInputData.Context, 
                  accountingJobInputData.CdrSetting.BatchSizeWhenPreparingLargeSqlJob,
                  indexedColumnName, dateColumnName)
        {
            this.AccountingJobInputData = accountingJobInputData;
        }

        public override ISegmentedJob CreateJobSegmentInstance(jobsegment jobSegment)
        {
            RowIdsCollectionForSingleDay dayWiseRowsIdsCollection = base.DeserializeDayWiseRowIdsCollection(jobSegment);
            string selectSql = dayWiseRowsIdsCollection.GetSelectSql();
            DbRowCollector<acc_transaction> dbRowCollector =
                new DbRowCollector<acc_transaction>(this.AccountingJobInputData, selectSql);
            List<acc_transaction> transactions = dbRowCollector.Collect();
            var con = this.AccountingJobInputData.Context.Database.Connection;
            if(con.State!=ConnectionState.Open) con.Open();
            var cmd = con.CreateCommand();
            cmd.CommandText = $"select jobstate from job where id=" +
                              $"{this.AccountingJobInputData.TelcobrightJob.id}";
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
            CdrInvoicingJob cdrInvoicingJob=new CdrInvoicingJob(this.AccountingJobInputData,transactions,invoicedAmountAfterLastSegment);
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
                    CreateTransactionPostingJob();
                    //delete job segments which can hold large amount of data
                    cmd.ExecuteCommandText(" delete from jobsegment where idjob=" + this.TelcobrightJob.id);
                    cmd.ExecuteCommandText(" commit; ");
                }
            }
        }

        private void CreateTransactionPostingJob()
        {
            throw new NotImplementedException();
        }
    }
}
