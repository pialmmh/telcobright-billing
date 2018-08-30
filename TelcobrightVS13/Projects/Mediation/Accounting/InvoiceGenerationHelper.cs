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
        private InvoiceGenerationInputData InvoiceGenerationInputData { get; set; }
        private Action<InvoiceGenerationInputData> InvoicePreProcessingAction { get; set; }
        private Func<InvoicePostProcessingData,InvoicePostProcessingData> InvoicePostProcessingAction { get; set; }
        private DbCommand Cmd { get; set; }
        private IServiceGroup ServiceGroup { get; set; }
        private TelcobrightConfig Tbc { get; set; }
        public InvoiceGenerationHelper(InvoiceGenerationInputData invoiceGenerationInputData,
            Action<InvoiceGenerationInputData>invoicePreProcessingAction, 
            Func<InvoicePostProcessingData,InvoicePostProcessingData> invoicePostProcessingAction)
        {
            this.InvoiceGenerationInputData = invoiceGenerationInputData;
            this.InvoicePreProcessingAction = invoicePreProcessingAction;
            this.InvoicePostProcessingAction = invoicePostProcessingAction;
            this.Cmd = this.InvoiceGenerationInputData.Context.Database.Connection.CreateCommand();
            this.ServiceGroup = this.InvoiceGenerationInputData.SelectedServiceGroup;
            if(this.ServiceGroup==null)
                throw new Exception("Servicegroup not found for invoice to be generated.");
            this.Tbc = this.InvoiceGenerationInputData.Tbc;
        }
        public InvoicePostProcessingData GenerateInvoice()
        {
            int idServiceGroup = this.ServiceGroup.Id;
            ServiceGroupConfiguration serviceGroupConfiguration = null;
            this.Tbc.CdrSetting.ServiceGroupConfigurations.TryGetValue(idServiceGroup, out serviceGroupConfiguration);
            if(serviceGroupConfiguration == null) throw new Exception("serviceGroupConfiguration cannot be null.");
            string configuredInvoiceGenerationRuleName = serviceGroupConfiguration.InvoiceGenerationRuleName;
            IInvoiceGenerationRule invoiceGenerationRule =
                this.InvoiceGenerationInputData.InvoiceGenerationRules[configuredInvoiceGenerationRuleName];
            InvoicePostProcessingData invoicePostProcessingData= invoiceGenerationRule.Execute(this.InvoiceGenerationInputData);
            return invoicePostProcessingData;
        }
        public void ExecInvoicePreProcessing()
        {
            this.InvoicePreProcessingAction.Invoke(this.InvoiceGenerationInputData);
        }
        public void ExecInvoicePostProcessing(InvoicePostProcessingData invoicePostProcessingData)
        {
            this.InvoicePostProcessingAction.Invoke(invoicePostProcessingData);
        }
    }
}
