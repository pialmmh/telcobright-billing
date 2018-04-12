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
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{idParent.ToMySqlField()},
				{idParentExternal.ToMySqlField()},
				{idPartner.ToMySqlField()},
				{accountName.ToMySqlField()},
				{iduom.ToMySqlField()},
				{Depth.ToMySqlField()},
				{Lineage.ToMySqlField()},
				{remark.ToMySqlField()},
				{isBillable.ToMySqlField()},
				{isCustomerAccount.ToMySqlField()},
				{isSupplierAccount.ToMySqlField()},
				{balanceBefore.ToMySqlField()},
				{lastAmount.ToMySqlField()},
				{balanceAfter.ToMySqlField()},
				{lastUpdated.ToMySqlField()},
				{superviseNegativeBalance.ToMySqlField()},
				{negativeBalanceLimit.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<account,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<account,string> whereClauseMethod)
		{
			return $@"update account set 
				id={id.ToMySqlField()+" "},
				idParent={idParent.ToMySqlField()+" "},
				idParentExternal={idParentExternal.ToMySqlField()+" "},
				idPartner={idPartner.ToMySqlField()+" "},
				accountName={accountName.ToMySqlField()+" "},
				iduom={iduom.ToMySqlField()+" "},
				Depth={Depth.ToMySqlField()+" "},
				Lineage={Lineage.ToMySqlField()+" "},
				remark={remark.ToMySqlField()+" "},
				isBillable={isBillable.ToMySqlField()+" "},
				isCustomerAccount={isCustomerAccount.ToMySqlField()+" "},
				isSupplierAccount={isSupplierAccount.ToMySqlField()+" "},
				balanceBefore={balanceBefore.ToMySqlField()+" "},
				lastAmount={lastAmount.ToMySqlField()+" "},
				balanceAfter={balanceAfter.ToMySqlField()+" "},
				lastUpdated={lastUpdated.ToMySqlField()+" "},
				superviseNegativeBalance={superviseNegativeBalance.ToMySqlField()+" "},
				negativeBalanceLimit={negativeBalanceLimit.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<account,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<account,string> whereClauseMethod)
		{
			return $@"delete from account 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
