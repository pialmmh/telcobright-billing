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
    public class InvoiceGenerationFromRawTransaction : IInvoiceGenerationRule
    {
        public override string ToString()
        {
            return this.RuleName;
        }

        public string RuleName => GetType().Name;
        public string HelpText => "Generate invoice from raw transactions in batches.";
        public int Id => 1;

        public InvoicePostProcessingData Execute(object data) //incomplete
        {
            InvoiceGenerationInputData input = (InvoiceGenerationInputData) data;
            //InvoiceDataCollector invoiceDataCollector = input.InvoiceDataCollector;
            PartnerEntities context = input.Context;
            int batchSizeForJobSegments = input.BatchSizeForJobSegment;
            SegmentedCdrInvoicingJobProcessor segmentedInvoiceProcessor =
                new SegmentedCdrInvoicingJobProcessor(input, "id", "transactiontime");
            if (input.TelcobrightJob.Status != 2) //prepare job if not prepared already
                segmentedInvoiceProcessor.PrepareSegments();
            List<jobsegment> jobsegments =
                segmentedInvoiceProcessor.ExecuteIncompleteSegments();
            segmentedInvoiceProcessor.FinishJob(jobsegments, null); //mark job as complete
            return null;//incomplete, finish later
        }
    }
}
