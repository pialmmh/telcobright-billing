using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;
using Newtonsoft.Json;

namespace TelcobrightMediation.Accounting
{
    public class InvoiceGenerator
    {
        public IInvoiceGenerationRule InvoiceGenerationRule { get; set; }
        public InvoiceGenerationInputData InvoiceGenerationInputData { get; set; }
        public invoice GeneratedInvoice { get; private set; }
        public bool PreProcessingComplete { get; private set; }
        public InvoiceGenerator(IInvoiceGenerationRule invoiceGenerationRule
            ,InvoiceGenerationInputData invoiceGenerationInputData)
        {
            this.InvoiceGenerationRule = invoiceGenerationRule;
            this.InvoiceGenerationInputData = invoiceGenerationInputData;
        }
        public invoice GenerateInvoice()
        {
            this.GeneratedInvoice = this.InvoiceGenerationRule.Execute(this.InvoiceGenerationInputData);
            
            return this.GeneratedInvoice;
        }
        public void ExecInvoicePreProcessing(Action<InvoiceGenerationInputData> invoicePreProcessingAction)
        {
            invoicePreProcessingAction.Invoke(this.InvoiceGenerationInputData);
        }
        public void ExecInvoicePostProcessing(Action<InvoiceGenerationInputData> invoicePostProcessingAction)
        {
            invoicePostProcessingAction.Invoke(this.InvoiceGenerationInputData);
        }
    }
}
