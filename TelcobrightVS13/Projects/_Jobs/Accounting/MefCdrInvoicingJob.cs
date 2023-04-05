using TelcobrightMediation;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using LibraryExtensions;
using TelcobrightFileOperations;
using MediationModel;
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Config;
using TelcobrightMediation;
namespace Jobs
{

    [Export("Job", typeof(ITelcobrightJob))]
    public class MefCdrInvoicingJob : ITelcobrightJob
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string HelpText => "Invoice generation job";
        public int Id => 12;
        public JobCompletionStatus Execute(ITelcobrightJobInput jobInputData)
        {
            InvoiceGenerationInputData invoiceGenerationInputData = (InvoiceGenerationInputData)jobInputData;
            job telcobrightJob = invoiceGenerationInputData.TelcobrightJob;
            Dictionary<string, string> jobParamsMap =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(telcobrightJob.JobParameter);
            var jsonDetailMainServiceGroup = jobParamsMap;//carry on jobs param along with invoice detail
            invoiceGenerationInputData.JsonDetail = jsonDetailMainServiceGroup;
            PartnerEntities context = jobInputData.Context;
            long serviceAccountId = Convert.ToInt64(jsonDetailMainServiceGroup["serviceAccountId"]);
            account parentAccount = context.accounts.Where(a => a.id == serviceAccountId).ToList().Single();
            partner customer = context.Database.SqlQuery<partner>($"select * from partner " +
                                                                  $"where idpartner in (" +
                                                                  $"select idpartner from account where " +
                                                                  $"id={serviceAccountId})").ToList().Single();
            string partnerType = context.enumpartnertypes.Where(c => c.id == customer.PartnerType).ToList().Single().Type;
            IServiceGroup serviceGroup = null;
            invoiceGenerationInputData.ServiceGroups.TryGetValue(parentAccount.serviceGroup, out serviceGroup);
            if (serviceGroup == null)
                throw new Exception("Service group should be set already thus cannot be null while " +
                                    "executing invoice generation by ledger summary.");
            jsonDetailMainServiceGroup.Add("idServiceGroup", parentAccount.serviceGroup.ToString());
            jsonDetailMainServiceGroup["serviceAccountId"] = serviceAccountId.ToString();
            jsonDetailMainServiceGroup.Add("uom", parentAccount.uom);
            jsonDetailMainServiceGroup.Add("idPartner", customer.idPartner.ToString());
            jsonDetailMainServiceGroup.Add("partnerType", partnerType);


            invoiceGenerationInputData.JsonDetail = jsonDetailMainServiceGroup.ToDictionary(kv=>kv.Key,kv=>kv.Value);

            
            List<long> childInvoicesIds = 
                GenerateChildInvoicesToBeMerged(invoiceGenerationInputData, jsonDetailMainServiceGroup, context, 
                                                parentAccount, customer, partnerType, serviceGroup);

            jsonDetailMainServiceGroup.Add("mergedInvoices", string.Join(",", childInvoicesIds));
            invoiceGenerationInputData.JsonDetail = jsonDetailMainServiceGroup;
            InvoiceGenerationHelper invoiceGenerationHelper =
                new InvoiceGenerationHelper(invoiceGenerationInputData, null, null, parentAccount, customer, partnerType);//null will use invoice pre & processing 
                                                                                                                          //from serviceGroup, if not null then servicGroups methods will be overridden.
            invoiceGenerationInputData = invoiceGenerationHelper.ExecInvoicePreProcessing(invoiceGenerationInputData);
            InvoicePostProcessingData invoicePostProcessingData =
                invoiceGenerationHelper.GenerateInvoice(invoiceGenerationInputData);
            invoicePostProcessingData = invoiceGenerationHelper.ExecInvoicePostProcessing(invoicePostProcessingData);
            invoicePostProcessingData.TempTransaction = CreateTempTransaction(invoicePostProcessingData);
            WriteToDb(invoicePostProcessingData);//write the main invoice to db
            return JobCompletionStatus.Complete;
        }

        private List<long> GenerateChildInvoicesToBeMerged(InvoiceGenerationInputData invoiceGenerationInputData, 
            Dictionary<string, string> jsonDetail, PartnerEntities context, 
            account parentAccount, partner customer, 
            string partnerType, IServiceGroup serviceGroup)
        {
            InvoiceGenerationConfig invoiceGenerationConfig = null;
            invoiceGenerationInputData.ServiceGroupWiseInvoiceGenerationConfigs.TryGetValue(serviceGroup.Id, out invoiceGenerationConfig);
            string serviceGroupsToMergeInvoiceStr = "";
            if (invoiceGenerationConfig.OtherParams != null)
            {
                invoiceGenerationConfig.OtherParams.TryGetValue("serviceGroupsToMergeInvoice", out serviceGroupsToMergeInvoiceStr);
            }
            List<int> serviceGroupsToMergeInvoice = serviceGroupsToMergeInvoiceStr.Split(',').Select(s => Convert.ToInt32(s.Trim())).ToList();

            //generate invoice for each child servicegroup to be merged
            List<long> childInvoicesIds = new List<long>();
            foreach (int serviceGroupToMerge in serviceGroupsToMergeInvoice)
            {
                account childAccount = context.accounts.Where(a => a.serviceGroup == serviceGroupToMerge && a.uom == parentAccount.uom
                    && a.idPartner == parentAccount.idPartner).ToList().SingleOrDefault();
                long childServiceAccountId = childAccount?.id ?? -1;
                if (childServiceAccountId > 0)
                {
                    Dictionary<string, string> childJsonDetail = jsonDetail.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    childJsonDetail["serviceAccountId"] = childServiceAccountId.ToString();
                    childJsonDetail["idServiceGroup"] = serviceGroupToMerge.ToString();
                    invoiceGenerationInputData.JsonDetail = childJsonDetail;//change the service account to the new service account that needs to be merged.
                    InvoiceGenerationHelper invoiceGenerationHelperChild =
                        new InvoiceGenerationHelper(invoiceGenerationInputData, null, null,
                        childAccount, customer, partnerType);//null will use invoice pre & processing 
                                                             //from serviceGroup, if not null then servicGroups methods will be overridden.
                    invoiceGenerationInputData = invoiceGenerationHelperChild.ExecInvoicePreProcessing(invoiceGenerationInputData);
                    InvoicePostProcessingData invoicePostProcessingDataChild =
                        invoiceGenerationHelperChild.GenerateInvoice(invoiceGenerationInputData);
                    invoicePostProcessingDataChild = invoiceGenerationHelperChild.ExecInvoicePostProcessing(invoicePostProcessingDataChild);
                    invoicePostProcessingDataChild.TempTransaction = CreateTempTransaction(invoicePostProcessingDataChild);
                    long generatedInvoiceId = WriteToDb(invoicePostProcessingDataChild);
                    childInvoicesIds.Add(generatedInvoiceId);
                }
            }
            return childInvoicesIds;
        }

        private acc_temp_transaction CreateTempTransaction(InvoicePostProcessingData invoicePostProcessingData)
        {
            acc_temp_transaction tempTransaction = new acc_temp_transaction()
            {
                transactionTime = DateTime.Now,
                glAccountId = Convert.ToInt64(invoicePostProcessingData.Invoice.BILLING_ACCOUNT_ID),
                amount = Convert.ToDecimal(invoicePostProcessingData.Invoice.invoice_item.Single().AMOUNT),
                createdByJob = invoicePostProcessingData.InvoiceGenerationInputData.TelcobrightJob.id
            };
            return tempTransaction;
        }
        
        private long WriteToDb(InvoicePostProcessingData invoicePostProcessingData)
        {
            InvoiceGenerationInputData invoiceGenerationInputData =
                                       invoicePostProcessingData.InvoiceGenerationInputData;
            var context = invoiceGenerationInputData.Context;
            var cmd = context.Database.Connection.CreateCommand();
            invoice invoiceWithItem = invoicePostProcessingData.Invoice;
            cmd.CommandText = $"insert into invoice (billing_account_id,description,originalCurrency," +
                              $"convertedFinalCurrency,originalAmount,convertedFinalAmount," +
                              $"currencyConversionFactor,INVOICE_DATE) values(" +
                              $"{invoiceWithItem.BILLING_ACCOUNT_ID},'{invoiceWithItem.DESCRIPTION}'," +
                              $"'{invoiceWithItem.originalCurrency}', '{invoiceWithItem.convertedFinalCurrency}'," +
                              $"{invoiceWithItem.originalAmount},{invoiceWithItem.convertedFinalAmount}," +
                              $"{invoiceWithItem.currencyConversionFactor},{invoiceWithItem.INVOICE_DATE.ToMySqlField()});";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "SELECT LAST_INSERT_ID();";
            long generatedInvoiceId = 0;
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    generatedInvoiceId = Convert.ToInt64(reader[0].ToString());
                }
                reader.Close();
            }
            string uom = invoiceGenerationInputData.JsonDetail["uom"];
            invoice_item invoiceItem = invoiceWithItem.invoice_item.Single();
            cmd.CommandText = $"insert into invoice_item " +
                              $"(invoice_id,product_id,uom_Id,amount,json_detail) values (" +
                              $"{generatedInvoiceId},'{invoiceItem.PRODUCT_ID}','{uom}'," +
                              $"{invoiceItem.AMOUNT}," +
                              $"'{invoiceItem.JSON_DETAIL}')";
            cmd.ExecuteNonQuery();
            acc_temp_transaction tempTransaction = invoicePostProcessingData.TempTransaction;
            cmd.CommandText = $"insert into acc_temp_transaction" +
                              $"(transactionTime,glAccountId,amount,createdByJob,debitOrCredit,idEvent,uomId,balanceBefore" +
                              $",balanceAfter) values" +
                              $"({tempTransaction.transactionTime.ToMySqlField()},{tempTransaction.glAccountId}," +
                              $"{tempTransaction.amount},{invoiceGenerationInputData.TelcobrightJob.id},'d'," +
                              $"{generatedInvoiceId}," +
                              $"'{invoicePostProcessingData.Currency}',0,0)";
            cmd.ExecuteNonQuery();
            var dayWiseAmounts =
                invoicePostProcessingData.DayWiseLedgerSummaries.ToDictionary(kv => kv.Key, kv => (-1)*kv.Value.AMOUNT);
            StringBuilder sbSql=
                new StringBuilder($" insert into acc_ledger_summary_billed (idAccount,transactionDate,billedAmount, " +
                                                  $"eventType,invoiceOrEventId) values ");
            sbSql.Append(string.Join(",", dayWiseAmounts.Select(kv =>
                new StringBuilder("(").Append(
                        $"{invoicePostProcessingData.ServiceAccountId},{kv.Key.ToMySqlFormatWithQuote()},{kv.Value}," +
                        $"'i',{generatedInvoiceId}")
                    .Append(")").ToString())));
                
            cmd.CommandText = sbSql.ToString();
            cmd.ExecuteNonQuery();
            return generatedInvoiceId;
        }
        private Action<object> actionOnFinish = jobInput =>
        {
            var input = (AccountingJobInputData) jobInput;
            var context = input.Context;
            var cmd = context.Database.Connection.CreateCommand();
            cmd.CommandText = $"select jobstate from job where id={input.TelcobrightJob.id}";
            string json = (string) cmd.ExecuteScalar();
            var jobStateMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            decimal finalInvoicedAmount = Convert.ToDecimal(jobStateMap["invoicedAmountAfterLastSegment"]);
            //Create Account posting job here
        };
    }
}


