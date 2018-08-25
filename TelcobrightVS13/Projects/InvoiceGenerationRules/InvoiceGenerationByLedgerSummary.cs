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
    public class InvoiceGenerationByLedgerSummary : IInvoiceGenerationRule
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => GetType().Name;
        public string HelpText => "Generate invoice from ledger summary.";
        public int Id => 2;

        public void Execute(object data)
        {
            InvoiceGenerationInputData input = (InvoiceGenerationInputData) data;
            InvoiceGenerator invoiceGenerator=new InvoiceGenerator(input,
                input.ServiceGroup.ExecInvoicePreProcessing,input.ServiceGroup.ExecInvoicePostProcessing);
            invoiceGenerator.GenerateInvoice();
        }
    }
}
