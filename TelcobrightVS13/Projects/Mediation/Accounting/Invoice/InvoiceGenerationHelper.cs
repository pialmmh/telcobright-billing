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
            Func<InvoicePostProcessingData, InvoicePostProcessingData> invoicePostProcessingAction)
        {
            this.InvoiceGenerationInputData = invoiceGenerationInputData;
            job telcobrightJob = invoiceGenerationInputData.TelcobrightJob;
            Dictionary<string, string> jobParamsMap =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(telcobrightJob.JobParameter);
            var invoiceJsonDetail = jobParamsMap;//carry on jobs param along with invoice detail
            this.InvoiceGenerationInputData.JsonDetail = invoiceJsonDetail;
            long serviceAccountId = Convert.ToInt64(invoiceJsonDetail["serviceAccountId"]);
            var context = this.InvoiceGenerationInputData.Context;
            this.Account = context.accounts.Where(c => c.id == serviceAccountId).ToList().Single();
            IServiceGroup serviceGroup = null;
            this.InvoiceGenerationInputData.ServiceGroups.TryGetValue(this.Account.serviceGroup, out serviceGroup);
            if (serviceGroup == null)
                throw new Exception("Service group should be set already thus cannot be null while " +
                                    "executing invoice generation by ledger summary.");
            invoiceJsonDetail.Add("idServiceGroup",this.Account.serviceGroup.ToString());
            invoiceJsonDetail.Add("uom", this.Account.uom);
            
            int timeZoneId = Convert.ToInt32(invoiceJsonDetail["timeZoneId"]);
            partner customer = context.Database.SqlQuery<partner>($"select * from partner " +
                                                                  $"where idpartner in (" +
                                                                  $"select idpartner from account where " +
                                                                  $"id={serviceAccountId})").ToList().Single();
            string partnerType = context.enumpartnertypes.Where(c => c.id == customer.PartnerType).ToList().Single().Type;
            DateTime startDate = Convert.ToDateTime(invoiceJsonDetail["startDate"]);
            DateTime endDate = Convert.ToDateTime(invoiceJsonDetail["endDate"]);
            //invoiceJsonDetail.Add("billingPeriod", $"{startDate.ToMySqlFormatWithoutQuote()}" +
            //                                       $" to {endDate.ToMySqlFormatWithoutQuote()}");
            invoiceJsonDetail.Add("billingStartDate", $"{startDate.ToMySqlFormatWithoutQuote()}");
            invoiceJsonDetail.Add("billingEndDate", $"{endDate.ToMySqlFormatWithoutQuote()}");
            invoiceJsonDetail.Add("idPartner", customer.idPartner.ToString());
            invoiceJsonDetail.Add("partnerName", customer.PartnerName);
            invoiceJsonDetail.Add("companyName", customer.AlternateNameInvoice);
            invoiceJsonDetail.Add("customerType",partnerType);
            invoiceJsonDetail.Add("billingAddress", customer.invoiceAddress);
            invoiceJsonDetail.Add("vatRegNo", customer.vatRegistrationNo);
            var tz = context.timezones.Where(c => c.id == timeZoneId).ToList().Single();
            invoiceJsonDetail.Add("timeZone", "GMT" + (tz.gmt_offset < 0 ? " - " : " + ") +
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
