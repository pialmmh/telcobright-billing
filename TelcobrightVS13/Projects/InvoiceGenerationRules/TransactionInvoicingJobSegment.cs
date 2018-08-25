using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;
using Newtonsoft.Json;
using TelcobrightMediation.Accounting;

namespace TelcobrightMediation.Cdr
{
    public class TransactionInvoicingJobSegment : ISegmentedJob
    {
        private int JobSegmentNumber { get; }
        private AccountingContext AccountingContext { get; }
        private List<acc_transaction> Transactions { get; }
        public int ActualStepsCount => this.Transactions.Count;
        private InvoiceGenerationInputData InvoiceGenerationInputData { get; }
        private CdrSetting CdrSetting => this.InvoiceGenerationInputData.Tbc.CdrSetting;
        bool segmentProcessed = false;
        private decimal processedInvoicedAmountSoFar;
        private PartnerEntities Context { get; }

        private decimal ProcessedInvoicedAmountSoFar
        {
            get
            {
                if (segmentProcessed == false)
                {
                    throw new Exception("Property ProcessedInvoicedAmount is not accessible until this " +
                                        "job segment is processed");
                }
                return this.processedInvoicedAmountSoFar;
            }
            set { this.processedInvoicedAmountSoFar = value; }
        }

        public TransactionInvoicingJobSegment(InvoiceGenerationInputData invoiceGenerationInputData,
            List<acc_transaction> transactions,
            int jobSegmentNumber, decimal invoicedAmountAfterLastSegment)
        {
            if (jobSegmentNumber <= 0)
                throw new Exception("Segment number must be > 0.");
            this.InvoiceGenerationInputData = invoiceGenerationInputData;
            this.Context = this.InvoiceGenerationInputData.Context;
            this.JobSegmentNumber = jobSegmentNumber;
            this.AccountingContext = this.InvoiceGenerationInputData.AccountingContext;
            this.Transactions = transactions;
            this.ProcessedInvoicedAmountSoFar = invoicedAmountAfterLastSegment;
        }

        public void Execute()
        {
            decimal invoiceableAmount = 0;
            foreach (var transaction in this.Transactions)
            {
                if (transaction.isBillable != null || transaction.isBilled == 1)
                {
                    throw new Exception("At least a billed or non-billable transaction has been found.");
                }
                invoiceableAmount += (-1) * transaction.amount;
                transaction.isBilled = 1;
            }
            this.ProcessedInvoicedAmountSoFar += invoiceableAmount;
            this.segmentProcessed = true;
            UpdateJobStateAfterSegment();
            this.AccountingContext.WriteAllChanges();
        }

        public void UpdateJobStateAfterSegment()
        {
            Dictionary<string, string> newJobStateAsMap = new Dictionary<string, string>();
            var cmd = this.Context.Database.Connection.CreateCommand();
            newJobStateAsMap.Add("lastProcessedSegmentNumber", this.JobSegmentNumber.ToString());
            newJobStateAsMap.Add("invoicedAmountAfterLastSegment", this.ProcessedInvoicedAmountSoFar.ToString());
            newJobStateAsMap.Add("lastSegmentExecutedOn", DateTime.Now.ToMySqlStyleDateTimeStrWithoutQuote());
            cmd.CommandText = $" update job set jobstate='{JsonConvert.SerializeObject(newJobStateAsMap)}'" +
                              $" where id={this.InvoiceGenerationInputData.TelcobrightJob.id}";
            cmd.ExecuteNonQuery();
        }
    }
}
