using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class invoice:ICacheble<invoice>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.INVOICE_ID.ToMySqlField()).Append(",")
				.Append(this.INVOICE_TYPE_ID.ToMySqlField()).Append(",")
				.Append(this.PARTY_ID_FROM.ToMySqlField()).Append(",")
				.Append(this.PARTY_ID.ToMySqlField()).Append(",")
				.Append(this.ROLE_TYPE_ID.ToMySqlField()).Append(",")
				.Append(this.STATUS_ID.ToMySqlField()).Append(",")
				.Append(this.BILLING_ACCOUNT_ID.ToMySqlField()).Append(",")
				.Append(this.originalCurrency.ToMySqlField()).Append(",")
				.Append(this.convertedFinalCurrency.ToMySqlField()).Append(",")
				.Append(this.originalAmount.ToMySqlField()).Append(",")
				.Append(this.convertedFinalAmount.ToMySqlField()).Append(",")
				.Append(this.currencyConversionFactor.ToMySqlField()).Append(",")
				.Append(this.CONTACT_MECH_ID.ToMySqlField()).Append(",")
				.Append(this.INVOICE_DATE.ToMySqlField()).Append(",")
				.Append(this.DUE_DATE.ToMySqlField()).Append(",")
				.Append(this.PAID_DATE.ToMySqlField()).Append(",")
				.Append(this.INVOICE_MESSAGE.ToMySqlField()).Append(",")
				.Append(this.REFERENCE_NUMBER.ToMySqlField()).Append(",")
				.Append(this.DESCRIPTION.ToMySqlField()).Append(",")
				.Append(this.CURRENCY_UOM_ID.ToMySqlField()).Append(",")
				.Append(this.RECURRENCE_INFO_ID.ToMySqlField()).Append(",")
				.Append(this.LAST_UPDATED_STAMP.ToMySqlField()).Append(",")
				.Append(this.LAST_UPDATED_TX_STAMP.ToMySqlField()).Append(",")
				.Append(this.CREATED_STAMP.ToMySqlField()).Append(",")
				.Append(this.CREATED_TX_STAMP.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<invoice,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<invoice,string> whereClauseMethod)
		{
			return new StringBuilder("update invoice set ")
				.Append("INVOICE_ID=").Append(this.INVOICE_ID.ToMySqlField()).Append(",")
				.Append("INVOICE_TYPE_ID=").Append(this.INVOICE_TYPE_ID.ToMySqlField()).Append(",")
				.Append("PARTY_ID_FROM=").Append(this.PARTY_ID_FROM.ToMySqlField()).Append(",")
				.Append("PARTY_ID=").Append(this.PARTY_ID.ToMySqlField()).Append(",")
				.Append("ROLE_TYPE_ID=").Append(this.ROLE_TYPE_ID.ToMySqlField()).Append(",")
				.Append("STATUS_ID=").Append(this.STATUS_ID.ToMySqlField()).Append(",")
				.Append("BILLING_ACCOUNT_ID=").Append(this.BILLING_ACCOUNT_ID.ToMySqlField()).Append(",")
				.Append("originalCurrency=").Append(this.originalCurrency.ToMySqlField()).Append(",")
				.Append("convertedFinalCurrency=").Append(this.convertedFinalCurrency.ToMySqlField()).Append(",")
				.Append("originalAmount=").Append(this.originalAmount.ToMySqlField()).Append(",")
				.Append("convertedFinalAmount=").Append(this.convertedFinalAmount.ToMySqlField()).Append(",")
				.Append("currencyConversionFactor=").Append(this.currencyConversionFactor.ToMySqlField()).Append(",")
				.Append("CONTACT_MECH_ID=").Append(this.CONTACT_MECH_ID.ToMySqlField()).Append(",")
				.Append("INVOICE_DATE=").Append(this.INVOICE_DATE.ToMySqlField()).Append(",")
				.Append("DUE_DATE=").Append(this.DUE_DATE.ToMySqlField()).Append(",")
				.Append("PAID_DATE=").Append(this.PAID_DATE.ToMySqlField()).Append(",")
				.Append("INVOICE_MESSAGE=").Append(this.INVOICE_MESSAGE.ToMySqlField()).Append(",")
				.Append("REFERENCE_NUMBER=").Append(this.REFERENCE_NUMBER.ToMySqlField()).Append(",")
				.Append("DESCRIPTION=").Append(this.DESCRIPTION.ToMySqlField()).Append(",")
				.Append("CURRENCY_UOM_ID=").Append(this.CURRENCY_UOM_ID.ToMySqlField()).Append(",")
				.Append("RECURRENCE_INFO_ID=").Append(this.RECURRENCE_INFO_ID.ToMySqlField()).Append(",")
				.Append("LAST_UPDATED_STAMP=").Append(this.LAST_UPDATED_STAMP.ToMySqlField()).Append(",")
				.Append("LAST_UPDATED_TX_STAMP=").Append(this.LAST_UPDATED_TX_STAMP.ToMySqlField()).Append(",")
				.Append("CREATED_STAMP=").Append(this.CREATED_STAMP.ToMySqlField()).Append(",")
				.Append("CREATED_TX_STAMP=").Append(this.CREATED_TX_STAMP.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<invoice,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<invoice,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from invoice 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
