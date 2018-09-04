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
        private InvoiceGenerationInputData InvoiceGenerationInputData { get; }
        private Func<InvoiceGenerationInputData, InvoiceGenerationInputData> InvoicePreProcessingAction { get;}
        private Func<InvoicePostProcessingData, InvoicePostProcessingData> InvoicePostProcessingAction { get; }
        private account Account { get; }
        public InvoiceGenerationHelper(InvoiceGenerationInputData invoiceGenerationInputData,
            Func<InvoiceGenerationInputData, InvoiceGenerationInputData> invoicePreProcessingAction,
            Func<InvoicePostProcessingData, InvoicePostProcessingData> invoicePostProcessingAction)
        {
            this.InvoiceGenerationInputData = invoiceGenerationInputData;
            job telcobrightJob = invoiceGenerationInputData.TelcobrightJob;
            Dictionary<string, string> jobParamsMap =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(telcobrightJob.JobParameter);
            long serviceAccountId = Convert.ToInt64(jobParamsMap["serviceAccountId"]);
            var context = this.InvoiceGenerationInputData.Context;
            this.Account = context.accounts.Where(c => c.id == serviceAccountId).ToList().Single();
            IServiceGroup serviceGroup = null;
            this.InvoiceGenerationInputData.ServiceGroups.TryGetValue(this.Account.serviceGroup, out serviceGroup);
            if (serviceGroup == null)
                throw new Exception("Service group should be set already thus cannot be null while " +
                                    "executing invoice generation by ledger summary.");
            jobParamsMap.Add("idServiceGroup",this.Account.serviceGroup.ToString());
            jobParamsMap.Add("uom", this.Account.uom);
            this.InvoiceGenerationInputData.InvoiceJsonDetail = jobParamsMap;
            if (invoicePreProcessingAction == null)
            {
                this.InvoicePreProcessingAction = serviceGroup.ExecInvoicePreProcessing;
            }
            if (invoicePostProcessingAction==null)
            {
                this.InvoicePostProcessingAction = serviceGroup.ExecInvoicePostProcessing;
            }
        }

        public InvoicePostProcessingData GenerateInvoice(InvoiceGenerationInputData invoiceGenerationInputData)
        {
            ServiceGroupConfiguration serviceGroupConfiguration = null;
            this.InvoiceGenerationInputData.Tbc.CdrSetting.ServiceGroupConfigurations.TryGetValue(this.Account.serviceGroup,
                out serviceGroupConfiguration);
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
