using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using com.google.common.@base;

namespace TelcobrightMediation.Accounting
{
    public class InvoiceGenerationHelper
    {
        private Func<InvoiceGenerationInputData,InvoiceGenerationInputData> InvoicePreProcessingAction { get; set; }
        private Func<InvoicePostProcessingData,InvoicePostProcessingData> InvoicePostProcessingAction { get; set; }
        private DbCommand Cmd { get; set; }
        private IServiceGroup ServiceGroup { get; set; }
        private TelcobrightConfig Tbc { get; set; }

        public InvoiceGenerationHelper(
            Func<InvoiceGenerationInputData, InvoiceGenerationInputData>invoicePreProcessingAction,
            Func<InvoicePostProcessingData, InvoicePostProcessingData> invoicePostProcessingAction)
        {
            this.InvoicePreProcessingAction = invoicePreProcessingAction;
            this.InvoicePostProcessingAction = invoicePostProcessingAction;
        }

        public InvoicePostProcessingData GenerateInvoice(InvoiceGenerationInputData invoiceGenerationInputData)
        {
            int idServiceGroup = this.ServiceGroup.Id;
            ServiceGroupConfiguration serviceGroupConfiguration = null;
            this.Tbc.CdrSetting.ServiceGroupConfigurations.TryGetValue(idServiceGroup, out serviceGroupConfiguration);
            if(serviceGroupConfiguration == null) throw new Exception("serviceGroupConfiguration cannot be null.");
            string configuredInvoiceGenerationRuleName = serviceGroupConfiguration.InvoiceGenerationRuleName;
            IInvoiceGenerationRule invoiceGenerationRule =
                invoiceGenerationInputData.InvoiceGenerationRules[configuredInvoiceGenerationRuleName];
            InvoicePostProcessingData invoicePostProcessingData= invoiceGenerationRule.Execute(invoiceGenerationInputData);
            return invoicePostProcessingData;
        }
        public InvoiceGenerationInputData ExecInvoicePreProcessing(InvoiceGenerationInputData invoiceGenerationInputData)
        {
            this.InvoicePreProcessingAction.Invoke(invoiceGenerationInputData);
            return invoiceGenerationInputData;
        }
        public InvoicePostProcessingData ExecInvoicePostProcessing(InvoicePostProcessingData invoicePostProcessingData)
        {
            this.InvoicePostProcessingAction.Invoke(invoicePostProcessingData);
            return invoicePostProcessingData;
        }
    }
}
