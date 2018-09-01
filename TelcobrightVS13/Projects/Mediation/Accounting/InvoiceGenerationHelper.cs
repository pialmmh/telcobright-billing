using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.google.common.@base;
using MediationModel;
using Newtonsoft.Json;

namespace TelcobrightMediation.Accounting
{
    public class InvoiceGenerationHelper
    {
        private DbCommand Cmd { get; set; }
        private InvoiceGenerationInputData InvoiceGenerationInputData { get; set; }
        private IServiceGroup ServiceGroup { get; set; }
        private TelcobrightConfig Tbc { get; set; }
        private Func<InvoiceGenerationInputData, InvoiceGenerationInputData> InvoicePreProcessingAction { get;}
        private Func<InvoicePostProcessingData, InvoicePostProcessingData> InvoicePostProcessingAction { get; }
        public InvoiceGenerationHelper(InvoiceGenerationInputData invoiceGenerationInputData,
            Func<InvoiceGenerationInputData, InvoiceGenerationInputData> invoicePreProcessingAction,
            Func<InvoicePostProcessingData, InvoicePostProcessingData> invoicePostProcessingAction)
        {
            this.InvoiceGenerationInputData = invoiceGenerationInputData;
            job telcobrightJob = invoiceGenerationInputData.TelcobrightJob;
            Dictionary<string, string> jobParamsMap =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(telcobrightJob.JobParameter);
            long serviceAccountId = Convert.ToInt64(jobParamsMap["serviceAccountId"]);
            var context = invoiceGenerationInputData.Context;
            int idServiceGroup = context.accounts.Where(c => c.id == serviceAccountId).ToList().Single().serviceGroup;
            IServiceGroup serviceGroup = null;
            invoiceGenerationInputData.ServiceGroups.TryGetValue(idServiceGroup, out serviceGroup);
            if (serviceGroup == null)
                throw new Exception("Service group should be set already thus cannot be null while " +
                                    "executing invoice generation by ledger summary.");
            this.ServiceGroup = serviceGroup;
            if (invoicePreProcessingAction == null)
            {
                this.InvoicePreProcessingAction = this.ServiceGroup.ExecInvoicePreProcessing;
            }
            if (invoicePostProcessingAction==null)
            {
                this.InvoicePostProcessingAction = this.ServiceGroup.ExecInvoicePostProcessing;
            }
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
