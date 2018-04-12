using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class temp_ledger_summary2:ICacheble<temp_ledger_summary2>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{idAccount.ToMySqlField()},
				{transactionDate.ToMySqlField()},
				{AMOUNT.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<temp_ledger_summary2,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<temp_ledger_summary2,string> whereClauseMethod)
		{
			return $@"update temp_ledger_summary2 set 
				id={id.ToMySqlField()+" "},
				idAccount={idAccount.ToMySqlField()+" "},
				transactionDate={transactionDate.ToMySqlField()+" "},
				AMOUNT={AMOUNT.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<temp_ledger_summary2,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<temp_ledger_summary2,string> whereClauseMethod)
		{
			return $@"delete from temp_ledger_summary2 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
