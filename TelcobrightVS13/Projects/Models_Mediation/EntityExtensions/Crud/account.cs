using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class account:ICacheble<account>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.idParent.ToMySqlField()).Append(",")
				.Append(this.idParentExternal.ToMySqlField()).Append(",")
				.Append(this.idPartner.ToMySqlField()).Append(",")
				.Append(this.accountName.ToMySqlField()).Append(",")
				.Append(this.serviceGroup.ToMySqlField()).Append(",")
				.Append(this.serviceFamily.ToMySqlField()).Append(",")
				.Append(this.product.ToMySqlField()).Append(",")
				.Append(this.billableType.ToMySqlField()).Append(",")
				.Append(this.uom.ToMySqlField()).Append(",")
				.Append(this.Depth.ToMySqlField()).Append(",")
				.Append(this.Lineage.ToMySqlField()).Append(",")
				.Append(this.remark.ToMySqlField()).Append(",")
				.Append(this.isBillable.ToMySqlField()).Append(",")
				.Append(this.isCustomerAccount.ToMySqlField()).Append(",")
				.Append(this.isSupplierAccount.ToMySqlField()).Append(",")
				.Append(this.balanceBefore.ToMySqlField()).Append(",")
				.Append(this.lastAmount.ToMySqlField()).Append(",")
				.Append(this.balanceAfter.ToMySqlField()).Append(",")
				.Append(this.lastUpdated.ToMySqlField()).Append(",")
				.Append(this.superviseNegativeBalance.ToMySqlField()).Append(",")
				.Append(this.negativeBalanceLimit.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<account,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<account,string> whereClauseMethod)
		{
			return new StringBuilder("update account set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("idParent=").Append(this.idParent.ToMySqlField()).Append(",")
				.Append("idParentExternal=").Append(this.idParentExternal.ToMySqlField()).Append(",")
				.Append("idPartner=").Append(this.idPartner.ToMySqlField()).Append(",")
				.Append("accountName=").Append(this.accountName.ToMySqlField()).Append(",")
				.Append("serviceGroup=").Append(this.serviceGroup.ToMySqlField()).Append(",")
				.Append("serviceFamily=").Append(this.serviceFamily.ToMySqlField()).Append(",")
				.Append("product=").Append(this.product.ToMySqlField()).Append(",")
				.Append("billableType=").Append(this.billableType.ToMySqlField()).Append(",")
				.Append("uom=").Append(this.uom.ToMySqlField()).Append(",")
				.Append("Depth=").Append(this.Depth.ToMySqlField()).Append(",")
				.Append("Lineage=").Append(this.Lineage.ToMySqlField()).Append(",")
				.Append("remark=").Append(this.remark.ToMySqlField()).Append(",")
				.Append("isBillable=").Append(this.isBillable.ToMySqlField()).Append(",")
				.Append("isCustomerAccount=").Append(this.isCustomerAccount.ToMySqlField()).Append(",")
				.Append("isSupplierAccount=").Append(this.isSupplierAccount.ToMySqlField()).Append(",")
				.Append("balanceBefore=").Append(this.balanceBefore.ToMySqlField()).Append(",")
				.Append("lastAmount=").Append(this.lastAmount.ToMySqlField()).Append(",")
				.Append("balanceAfter=").Append(this.balanceAfter.ToMySqlField()).Append(",")
				.Append("lastUpdated=").Append(this.lastUpdated.ToMySqlField()).Append(",")
				.Append("superviseNegativeBalance=").Append(this.superviseNegativeBalance.ToMySqlField()).Append(",")
				.Append("negativeBalanceLimit=").Append(this.negativeBalanceLimit.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<account,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<account,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from account 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
