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
using LibraryExtensions;
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
            Func<InvoicePostProcessingData, InvoicePostProcessingData> invoicePostProcessingAction,
            account _account, partner customer, string partnerType)
        {
            this.Account = _account;
            this.InvoiceGenerationInputData = invoiceGenerationInputData;
            //job telcobrightJob = invoiceGenerationInputData.TelcobrightJob;
            //Dictionary<string, string> jobParamsMap =
              //  JsonConvert.DeserializeObject<Dictionary<string, string>>(telcobrightJob.JobParameter);
            //var invoiceJsonDetail = jobParamsMap;//carry on jobs param along with invoice detail
            //this.InvoiceGenerationInputData.JsonDetail = invoiceJsonDetail;
            var jsonDetail = invoiceGenerationInputData.JsonDetail;
            long serviceAccountId = Convert.ToInt64(jsonDetail["serviceAccountId"]);
            int timeZoneId = Convert.ToInt32(jsonDetail["timeZoneId"]);
            PartnerEntities context = invoiceGenerationInputData.Context;
            
            DateTime startDate = Convert.ToDateTime(jsonDetail["startDate"]);
            DateTime endDate = Convert.ToDateTime(jsonDetail["endDate"]);
            //invoiceJsonDetail.Add("billingPeriod", $"{startDate.ToMySqlFormatWithoutQuote()}" +
            //                                       $" to {endDate.ToMySqlFormatWithoutQuote()}");

            int idServiceGroup = Convert.ToInt32(jsonDetail["idServiceGroup"]);
            IServiceGroup serviceGroup = null;
            invoiceGenerationInputData.ServiceGroups.TryGetValue(idServiceGroup, out serviceGroup);
            if(!jsonDetail.ContainsKey("billingStartDate")) jsonDetail.Add("billingStartDate", $"{startDate.ToMySqlFormatWithoutQuote()}");
            if (!jsonDetail.ContainsKey("billingEndDate")) jsonDetail.Add("billingEndDate", $"{endDate.ToMySqlFormatWithoutQuote()}");
            if (!jsonDetail.ContainsKey("idPartner")) jsonDetail.Add("idPartner", customer.idPartner.ToString());
            if (!jsonDetail.ContainsKey("partnerName")) jsonDetail.Add("partnerName", customer.PartnerName);
            if (!jsonDetail.ContainsKey("companyName")) jsonDetail.Add("companyName", customer.AlternateNameInvoice);
            if (!jsonDetail.ContainsKey("customerType")) jsonDetail.Add("customerType",partnerType);
            if (!jsonDetail.ContainsKey("billingAddress")) jsonDetail.Add("billingAddress", customer.invoiceAddress);
            if (!jsonDetail.ContainsKey("vatRegNo")) jsonDetail.Add("vatRegNo", customer.vatRegistrationNo);
            if (!jsonDetail.ContainsKey("paymentAdvice")) jsonDetail.Add("paymentAdvice", customer.paymentAdvice);
            var tz = context.timezones.Where(c => c.id == timeZoneId).ToList().Single();
            if(!jsonDetail.ContainsKey("timeZone"))
                jsonDetail.Add("timeZone", "GMT" + (tz.gmt_offset < 0 ? " - " : " + ") +
                                              NumberFormatter.RoundToWholeIfFractionPartIsZero(Math.Round((double)tz.gmt_offset / 3600, 2)));
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
            IInvoiceGenerationRule invoiceGenerationRule = GetInvoiceGenRuleFromServiceGroupConfig(invoiceGenerationInputData);
            InvoicePostProcessingData invoicePostProcessingData = invoiceGenerationRule.Execute(invoiceGenerationInputData);
            Dictionary<string, string> jsonDetail = invoicePostProcessingData.InvoiceGenerationInputData.JsonDetail;
            jsonDetail.Remove("sqlParam");
            invoicePostProcessingData.Invoice.invoice_item.Single().JSON_DETAIL = JsonConvert.SerializeObject(jsonDetail);
            return invoicePostProcessingData;
        }

        private IInvoiceGenerationRule GetInvoiceGenRuleFromServiceGroupConfig(InvoiceGenerationInputData invoiceGenerationInputData)
        {
            ServiceGroupConfiguration serviceGroupConfiguration = null;
            this.InvoiceGenerationInputData.Tbc.CdrSetting.ServiceGroupConfigurations.TryGetValue(this.Account.serviceGroup,
                out serviceGroupConfiguration);
            if (serviceGroupConfiguration == null) throw new Exception("serviceGroupConfiguration cannot be null.");
            string configuredInvoiceGenerationRuleName = serviceGroupConfiguration
                .InvoiceGenerationConfig.InvoiceGenerationRuleName;
            IInvoiceGenerationRule invoiceGenerationRule =
                invoiceGenerationInputData.InvoiceGenerationRules[configuredInvoiceGenerationRuleName];
            return invoiceGenerationRule;
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
