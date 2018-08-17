using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class invoice_item:ICacheble<invoice_item>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.INVOICE_ID.ToMySqlField()).Append(",")
				.Append(this.INVOICE_ITEM_SEQ_ID.ToMySqlField()).Append(",")
				.Append(this.INVOICE_ITEM_TYPE_ID.ToMySqlField()).Append(",")
				.Append(this.OVERRIDE_GL_ACCOUNT_ID.ToMySqlField()).Append(",")
				.Append(this.OVERRIDE_ORG_PARTY_ID.ToMySqlField()).Append(",")
				.Append(this.INVENTORY_ITEM_ID.ToMySqlField()).Append(",")
				.Append(this.PRODUCT_ID.ToMySqlField()).Append(",")
				.Append(this.PRODUCT_FEATURE_ID.ToMySqlField()).Append(",")
				.Append(this.PARENT_INVOICE_ID.ToMySqlField()).Append(",")
				.Append(this.PARENT_INVOICE_ITEM_SEQ_ID.ToMySqlField()).Append(",")
				.Append(this.UOM_ID.ToMySqlField()).Append(",")
				.Append(this.TAXABLE_FLAG.ToMySqlField()).Append(",")
				.Append(this.QUANTITY.ToMySqlField()).Append(",")
				.Append(this.AMOUNT.ToMySqlField()).Append(",")
				.Append(this.DESCRIPTION.ToMySqlField()).Append(",")
				.Append(this.TAX_AUTH_PARTY_ID.ToMySqlField()).Append(",")
				.Append(this.TAX_AUTH_GEO_ID.ToMySqlField()).Append(",")
				.Append(this.TAX_AUTHORITY_RATE_SEQ_ID.ToMySqlField()).Append(",")
				.Append(this.SALES_OPPORTUNITY_ID.ToMySqlField()).Append(",")
				.Append(this.JSON_DETAIL.ToMySqlField()).Append(",")
				.Append(this.LAST_UPDATED_STAMP.ToMySqlField()).Append(",")
				.Append(this.LAST_UPDATED_TX_STAMP.ToMySqlField()).Append(",")
				.Append(this.CREATED_STAMP.ToMySqlField()).Append(",")
				.Append(this.CREATED_TX_STAMP.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<invoice_item,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<invoice_item,string> whereClauseMethod)
		{
			return new StringBuilder("update invoice_item set ")
				.Append("INVOICE_ID=").Append(this.INVOICE_ID.ToMySqlField()).Append(",")
				.Append("INVOICE_ITEM_SEQ_ID=").Append(this.INVOICE_ITEM_SEQ_ID.ToMySqlField()).Append(",")
				.Append("INVOICE_ITEM_TYPE_ID=").Append(this.INVOICE_ITEM_TYPE_ID.ToMySqlField()).Append(",")
				.Append("OVERRIDE_GL_ACCOUNT_ID=").Append(this.OVERRIDE_GL_ACCOUNT_ID.ToMySqlField()).Append(",")
				.Append("OVERRIDE_ORG_PARTY_ID=").Append(this.OVERRIDE_ORG_PARTY_ID.ToMySqlField()).Append(",")
				.Append("INVENTORY_ITEM_ID=").Append(this.INVENTORY_ITEM_ID.ToMySqlField()).Append(",")
				.Append("PRODUCT_ID=").Append(this.PRODUCT_ID.ToMySqlField()).Append(",")
				.Append("PRODUCT_FEATURE_ID=").Append(this.PRODUCT_FEATURE_ID.ToMySqlField()).Append(",")
				.Append("PARENT_INVOICE_ID=").Append(this.PARENT_INVOICE_ID.ToMySqlField()).Append(",")
				.Append("PARENT_INVOICE_ITEM_SEQ_ID=").Append(this.PARENT_INVOICE_ITEM_SEQ_ID.ToMySqlField()).Append(",")
				.Append("UOM_ID=").Append(this.UOM_ID.ToMySqlField()).Append(",")
				.Append("TAXABLE_FLAG=").Append(this.TAXABLE_FLAG.ToMySqlField()).Append(",")
				.Append("QUANTITY=").Append(this.QUANTITY.ToMySqlField()).Append(",")
				.Append("AMOUNT=").Append(this.AMOUNT.ToMySqlField()).Append(",")
				.Append("DESCRIPTION=").Append(this.DESCRIPTION.ToMySqlField()).Append(",")
				.Append("TAX_AUTH_PARTY_ID=").Append(this.TAX_AUTH_PARTY_ID.ToMySqlField()).Append(",")
				.Append("TAX_AUTH_GEO_ID=").Append(this.TAX_AUTH_GEO_ID.ToMySqlField()).Append(",")
				.Append("TAX_AUTHORITY_RATE_SEQ_ID=").Append(this.TAX_AUTHORITY_RATE_SEQ_ID.ToMySqlField()).Append(",")
				.Append("SALES_OPPORTUNITY_ID=").Append(this.SALES_OPPORTUNITY_ID.ToMySqlField()).Append(",")
				.Append("JSON_DETAIL=").Append(this.JSON_DETAIL.ToMySqlField()).Append(",")
				.Append("LAST_UPDATED_STAMP=").Append(this.LAST_UPDATED_STAMP.ToMySqlField()).Append(",")
				.Append("LAST_UPDATED_TX_STAMP=").Append(this.LAST_UPDATED_TX_STAMP.ToMySqlField()).Append(",")
				.Append("CREATED_STAMP=").Append(this.CREATED_STAMP.ToMySqlField()).Append(",")
				.Append("CREATED_TX_STAMP=").Append(this.CREATED_TX_STAMP.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<invoice_item,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<invoice_item,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from invoice_item 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
