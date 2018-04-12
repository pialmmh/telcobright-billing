using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class acc_ledger_summary:ICacheble<acc_ledger_summary>
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
		public  string GetExtInsertCustom(Func<acc_ledger_summary,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<acc_ledger_summary,string> whereClauseMethod)
		{
			return $@"update acc_ledger_summary set 
				id={id.ToMySqlField()+" "},
				idAccount={idAccount.ToMySqlField()+" "},
				transactionDate={transactionDate.ToMySqlField()+" "},
				AMOUNT={AMOUNT.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<acc_ledger_summary,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<acc_ledger_summary,string> whereClauseMethod)
		{
			return $@"delete from acc_ledger_summary 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
