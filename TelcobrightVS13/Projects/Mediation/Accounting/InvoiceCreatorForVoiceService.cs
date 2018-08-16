using System;
using System.Data.Common;
using System.Security.Policy;
using MediationModel;

namespace TelcobrightMediation.Accounting
{
    public class InvoiceCreatorForVoiceService : IInvoiceCreator
    {
        private PartnerEntities Context { get; }
        public DbCommand Cmd { get; }
        public long AccountId { get; set; }
        public string Description { get; set; }
        public int ProductId { get; set; }
        public string UomId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }

        public InvoiceCreatorForVoiceService(DbCommand cmd, long accountId, string description, int productId, string uomId, decimal quantity, decimal amount)
        {
            Cmd = cmd;
            AccountId = accountId;
            Description = description;
            ProductId = productId;
            UomId = uomId;
            Quantity = quantity;
            Amount = amount;
        }

        public void CreateInvoice()
        {
            this.Cmd.CommandText = $"insert into invoice (billing_account_id,description) values(" +
                         $"{this.AccountId.ToString()},'{this.Description}');";
            this.Cmd.ExecuteNonQuery();
            this.Cmd.CommandText = "last_insert_id();";
            long generatedInvoiceId = (long)Cmd.ExecuteScalar();

            this.Cmd.CommandText = $"insert into invoice_item " +
                                   $"(invoice_id,product_id,uom_Id,quantity,amount) values (" +
                                   $"{generatedInvoiceId},'{this.ProductId}','{this.UomId}'," +
                                   $"{this.Quantity},{this.Amount})";
            this.Cmd.ExecuteNonQuery();
        }

        void CreateTempTransaction()
        {
            //var account=
            //TempTransactionCreator.CreateTempTransaction(this.AccountId,this.Amount,DateTime.Now,
            //    this.Cmd);
        }
    }
}