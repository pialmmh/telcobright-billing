using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class partner:ICacheble<partner>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.idPartner.ToMySqlField()).Append(",")
				.Append(this.PartnerName.ToMySqlField()).Append(",")
				.Append(this.Address1.ToMySqlField()).Append(",")
				.Append(this.Address2.ToMySqlField()).Append(",")
				.Append(this.City.ToMySqlField()).Append(",")
				.Append(this.State.ToMySqlField()).Append(",")
				.Append(this.PostalCode.ToMySqlField()).Append(",")
				.Append(this.Country.ToMySqlField()).Append(",")
				.Append(this.Telephone.ToMySqlField()).Append(",")
				.Append(this.email.ToMySqlField()).Append(",")
				.Append(this.CustomerPrePaid.ToMySqlField()).Append(",")
				.Append(this.PartnerType.ToMySqlField()).Append(",")
				.Append(this.billingdate.ToMySqlField()).Append(",")
				.Append(this.AllowedDaysForInvoicePayment.ToMySqlField()).Append(",")
				.Append(this.timezone.ToMySqlField()).Append(",")
				.Append(this.date1.ToMySqlField()).Append(",")
				.Append(this.field1.ToMySqlField()).Append(",")
				.Append(this.field2.ToMySqlField()).Append(",")
				.Append(this.field3.ToMySqlField()).Append(",")
				.Append(this.field4.ToMySqlField()).Append(",")
				.Append(this.field5.ToMySqlField()).Append(",")
				.Append(this.refasr.ToMySqlField()).Append(",")
				.Append(this.refacd.ToMySqlField()).Append(",")
				.Append(this.refccr.ToMySqlField()).Append(",")
				.Append(this.refccrbycc.ToMySqlField()).Append(",")
				.Append(this.refpdd.ToMySqlField()).Append(",")
				.Append(this.refasrfas.ToMySqlField()).Append(",")
				.Append(this.DefaultCurrency.ToMySqlField()).Append(",")
				.Append(this.invoiceAddress.ToMySqlField()).Append(",")
				.Append(this.vatRegistrationNo.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<partner,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<partner,string> whereClauseMethod)
		{
			return new StringBuilder("update partner set ")
				.Append("idPartner=").Append(this.idPartner.ToMySqlField()).Append(",")
				.Append("PartnerName=").Append(this.PartnerName.ToMySqlField()).Append(",")
				.Append("Address1=").Append(this.Address1.ToMySqlField()).Append(",")
				.Append("Address2=").Append(this.Address2.ToMySqlField()).Append(",")
				.Append("City=").Append(this.City.ToMySqlField()).Append(",")
				.Append("State=").Append(this.State.ToMySqlField()).Append(",")
				.Append("PostalCode=").Append(this.PostalCode.ToMySqlField()).Append(",")
				.Append("Country=").Append(this.Country.ToMySqlField()).Append(",")
				.Append("Telephone=").Append(this.Telephone.ToMySqlField()).Append(",")
				.Append("email=").Append(this.email.ToMySqlField()).Append(",")
				.Append("CustomerPrePaid=").Append(this.CustomerPrePaid.ToMySqlField()).Append(",")
				.Append("PartnerType=").Append(this.PartnerType.ToMySqlField()).Append(",")
				.Append("billingdate=").Append(this.billingdate.ToMySqlField()).Append(",")
				.Append("AllowedDaysForInvoicePayment=").Append(this.AllowedDaysForInvoicePayment.ToMySqlField()).Append(",")
				.Append("timezone=").Append(this.timezone.ToMySqlField()).Append(",")
				.Append("date1=").Append(this.date1.ToMySqlField()).Append(",")
				.Append("field1=").Append(this.field1.ToMySqlField()).Append(",")
				.Append("field2=").Append(this.field2.ToMySqlField()).Append(",")
				.Append("field3=").Append(this.field3.ToMySqlField()).Append(",")
				.Append("field4=").Append(this.field4.ToMySqlField()).Append(",")
				.Append("field5=").Append(this.field5.ToMySqlField()).Append(",")
				.Append("refasr=").Append(this.refasr.ToMySqlField()).Append(",")
				.Append("refacd=").Append(this.refacd.ToMySqlField()).Append(",")
				.Append("refccr=").Append(this.refccr.ToMySqlField()).Append(",")
				.Append("refccrbycc=").Append(this.refccrbycc.ToMySqlField()).Append(",")
				.Append("refpdd=").Append(this.refpdd.ToMySqlField()).Append(",")
				.Append("refasrfas=").Append(this.refasrfas.ToMySqlField()).Append(",")
				.Append("DefaultCurrency=").Append(this.DefaultCurrency.ToMySqlField()).Append(",")
				.Append("invoiceAddress=").Append(this.invoiceAddress.ToMySqlField()).Append(",")
				.Append("vatRegistrationNo=").Append(this.vatRegistrationNo.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<partner,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<partner,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from partner 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
