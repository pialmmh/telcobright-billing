using System;
using System.Collections.Generic;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using System.Linq;
using LibraryExtensions;
using MediationModel;
using Newtonsoft.Json;
using TelcobrightMediation.Accounting;

namespace InvoiceGenerationRules
{

    [Export("InvoiceGenerationRule", typeof(IInvoiceGenerationRule))]
    public class InvoiceGenerationByBatchJob : IInvoiceGenerationRule
    {
        public override string ToString()
        {
            return this.RuleName;
        }

        public string RuleName => GetType().Name;
        public string HelpText => "Generate invoice from raw transactions in batches.";
        public int Id => 1;

        public void Execute(object data)//incomplete
        {
            InvoiceGenerationInputData invoiceData = (InvoiceGenerationInputData) data;
            InvoiceDataCollector invoiceDataCollector = invoiceData.InvoiceDataCollector;
            PartnerEntities context = invoiceData.Context;
            int batchSizeForJobSegments = invoiceData.BatchSizeForJobSegment;

            SegmentedCdrInvoicingJobProcessor segmentedInvoiceProcessor =
                new SegmentedCdrInvoicingJobProcessor(this.Input, "id", "transactiontime");
            if (this.Input.TelcobrightJob.Status != 2) //prepare job if not prepared already
                segmentedInvoiceProcessor.PrepareSegments();
            List<jobsegment> jobsegments =
                segmentedInvoiceProcessor.ExecuteIncompleteSegments();
            segmentedInvoiceProcessor.FinishJob(jobsegments, null); //mark job as complete
            return JobCompletionStatus.Complete;


        }
    }
}
