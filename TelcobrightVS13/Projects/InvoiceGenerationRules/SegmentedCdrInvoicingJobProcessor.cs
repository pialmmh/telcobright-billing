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
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;

namespace TelcobrightMediation
{
    public class SegmentedCdrInvoicingJobProcessor : AbstractRowBasedSegmentedJobProcessor
    {
        private InvoiceGenerationInputData InvoiceGenerationInputData { get; }

        public SegmentedCdrInvoicingJobProcessor(InvoiceGenerationInputData invoiceGenerationInputData,
            string indexedColumnName, string dateColumnName)
            : base(invoiceGenerationInputData.Tbc.CdrSetting,invoiceGenerationInputData.TelcobrightJob, invoiceGenerationInputData.Context,
                invoiceGenerationInputData.Tbc.CdrSetting.BatchSizeWhenPreparingLargeSqlJob,
                indexedColumnName, dateColumnName)
        {
            this.InvoiceGenerationInputData = invoiceGenerationInputData;
        }

        public override ISegmentedJob CreateJobSegmentInstance(jobsegment jobSegment)
        {
            List<acc_transaction> transactions = CollectTransactionsForThisSegment(jobSegment);
            decimal invoicedAmountAfterLastSegment = GetInvoiceAmountAfterLastSegment();
            TransactionInvoicingJobSegment cdrInvoicingJob = 
                new TransactionInvoicingJobSegment(this.InvoiceGenerationInputData, transactions,
                jobSegment.segmentNumber,invoicedAmountAfterLastSegment);
            return cdrInvoicingJob;
        }

        private decimal GetInvoiceAmountAfterLastSegment()
        {
            var con = this.InvoiceGenerationInputData.Context.Database.Connection;
            if (con.State != ConnectionState.Open) con.Open();
            var cmd = con.CreateCommand();
            cmd.CommandText = $"select jobstate from job where id=" +
                              $"{this.InvoiceGenerationInputData.TelcobrightJob.id}";
            string jobStateJson = (string)cmd.ExecuteScalar();
            Dictionary<string, string> jobStateMap = null;
            if (jobStateJson.IsNullOrEmptyOrWhiteSpace() == false)
            {
                jobStateMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(jobStateJson);
            }
            decimal invoicedAmountAfterLastSegment = 0;
            if (jobStateMap != null & jobStateMap.Any())
            {
                invoicedAmountAfterLastSegment = Convert.ToDecimal(jobStateMap["invoicedAmountAfterLastSegment"]);
            }
            return invoicedAmountAfterLastSegment;
        }

        private List<acc_transaction> CollectTransactionsForThisSegment(jobsegment jobSegment)
        {
            RowIdsCollectionForSingleDay dayWiseRowsIdsCollection = base.DeserializeDayWiseRowIdsCollection(jobSegment);
            string selectSql = dayWiseRowsIdsCollection.GetSelectSql();
            DbRowCollector<acc_transaction> dbRowCollector =
                new DbRowCollector<acc_transaction>(this.InvoiceGenerationInputData, selectSql);
            List<acc_transaction> transactions = dbRowCollector.Collect();
            return transactions;
        }

        public override void FinishJob(List<jobsegment> jobsegments, Action<object> additionalJobFinalizingTask)
        {
            using (DbCommand cmd = ConnectionManager.CreateCommandFromDbContext(this.Context))
            {
                additionalJobFinalizingTask?.Invoke(cmd);
                if (jobsegments.Any(c => c.status != 1) == false) //no incomplete segment
                {
                    cmd.ExecuteCommandText(" set autocommit=0; ");
                    cmd.ExecuteCommandText(" update job set completiontime='" +
                                           DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                                           " status=9, Error='' " + //status 9=ready for posting
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
