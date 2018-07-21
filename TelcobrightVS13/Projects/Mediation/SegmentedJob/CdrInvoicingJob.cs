using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using javax.transaction;
using LibraryExtensions;
using MediationModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TelcobrightMediation.Accounting;

namespace TelcobrightMediation.Cdr
{
    public class CdrInvoicingJob : ISegmentedJob
    {
        private AccountingContext AccountingContext { get; }
        private int GlAccountId { get; }
        private account GlAccount { get; }
        private List<acc_transaction> Transactions { get; }
        public int ActualStepsCount => this.Transactions.Count;
        private CdrCollectorInputData CdrCollectorInputData { get; }
        private MediationContext MediationContext => this.CdrCollectorInputData.CdrJobInputData.MediationContext;
        private CdrSetting CdrSetting => this.MediationContext.Tbc.CdrSetting;
        private decimal InvoicedAmountAfterLastSegment { get; }
        bool segmentProcessed = false;
        private decimal ProcessedInvoicedAmountSoFar
        {
            get
            {
                if (segmentProcessed==false)
                {
                    throw new Exception("Property ProcessedInvoicedAmount is not accessible until this " +
                                        "job segment is processed");
                }
                return this.ProcessedInvoicedAmountSoFar;
            }
        }
        public CdrInvoicingJob(CdrCollectorInputData cdrCollectorInputData, List<acc_transaction> transactions,
            decimal invoicedAmountAfterLastSegment)
        {
            job telcobrightJob = cdrCollectorInputData.TelcobrightJob;

            //this.GlAccountId = glAccountId;
            this.CdrCollectorInputData = cdrCollectorInputData;
            this.Transactions = transactions;
            this.InvoicedAmountAfterLastSegment = invoicedAmountAfterLastSegment;
        }
        public void Execute()
        {
            decimal invoiceableAmount = 0;
            foreach (var transaction in this.Transactions)
            {
                if (transaction.isBillable !=null  || transaction.isBilled == 1)
                {
                    throw new Exception("At least a billed or non-billable transaction has been found.");
                }
                invoiceableAmount += (-1)*transaction.amount;
                transaction.isBilled = 1;
            }
        }
    }
}
