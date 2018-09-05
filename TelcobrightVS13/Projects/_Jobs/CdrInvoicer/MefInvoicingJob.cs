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
using LibraryExtensions;
using TelcobrightFileOperations;
using MediationModel;
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Config;
using TelcobrightMediation;
namespace Jobs
{

    [Export("Job", typeof(ITelcobrightJob))]
    public class MefInvoicingJob : ITelcobrightJob
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string HelpText => "Invoice generation job";
        public int Id => 12;
        public JobCompletionStatus Execute(ITelcobrightJobInput jobInputData)
        {
            InvoiceGenerationInputData invoiceGenerationInputData = (InvoiceGenerationInputData)jobInputData;
            InvoiceGenerationHelper invoiceGenerationHelper =
                new InvoiceGenerationHelper(invoiceGenerationInputData, null, null);//null will use invoice pre & processing 
                                            //from serviceGroup, if not null then servicGroups methods will be overridden.
            invoiceGenerationInputData = invoiceGenerationHelper.ExecInvoicePreProcessing(invoiceGenerationInputData);
            InvoicePostProcessingData invoicePostProcessingData =
                invoiceGenerationHelper.GenerateInvoice(invoiceGenerationInputData);
            invoicePostProcessingData= invoiceGenerationHelper.ExecInvoicePostProcessing(invoicePostProcessingData);
            invoicePostProcessingData.TempTransaction = CreateTempTransaction(invoicePostProcessingData);
            WriteToDb(invoicePostProcessingData);
            return JobCompletionStatus.Complete;
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
        
        private void WriteToDb(InvoicePostProcessingData invoicePostProcessingData)
        {
            InvoiceGenerationInputData invoiceGenerationInputData =
                                       invoicePostProcessingData.InvoiceGenerationInputData;
            var context = invoiceGenerationInputData.Context;
            var cmd = context.Database.Connection.CreateCommand();
            invoice invoiceWithItem = invoicePostProcessingData.Invoice;
            cmd.CommandText = $"insert into invoice (billing_account_id,description) values(" +
                              $"{invoiceWithItem.BILLING_ACCOUNT_ID},'{invoiceWithItem.DESCRIPTION}');";
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
            string uom = invoiceGenerationInputData.InvoiceJsonDetail["uom"];
            invoice_item invoiceItem = invoiceWithItem.invoice_item.Single();
            cmd.CommandText = $"insert into invoice_item " +
                              $"(invoice_id,product_id,uom_Id,amount,json_detail) values (" +
                              $"{generatedInvoiceId},'{invoiceItem.PRODUCT_ID}','{uom}'," +
                              $"{invoiceItem.AMOUNT},'{invoiceItem.JSON_DETAIL}')";
            cmd.ExecuteNonQuery();
            acc_temp_transaction tempTransaction = invoicePostProcessingData.TempTransaction;
            cmd.CommandText = $"insert into acc_temp_transaction" +
                              $"(transactionTime,glAccountId,amount,createdByJobId) values" +
                              $"({tempTransaction.transactionTime.ToMySqlField()},{tempTransaction.glAccountId}," +
                              $"{tempTransaction.glAccountId},{tempTransaction.amount}," +
                              $"{invoiceGenerationInputData.TelcobrightJob.id})";
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


