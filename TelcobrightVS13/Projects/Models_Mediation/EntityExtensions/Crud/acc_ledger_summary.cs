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
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.idAccount.ToMySqlField()).Append(",")
				.Append(this.transactionDate.ToMySqlField()).Append(",")
				.Append(this.AMOUNT.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<acc_ledger_summary,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<acc_ledger_summary,string> whereClauseMethod)
		{
			return new StringBuilder("update acc_ledger_summary set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("idAccount=").Append(this.idAccount.ToMySqlField()).Append(",")
				.Append("transactionDate=").Append(this.transactionDate.ToMySqlField()).Append(",")
				.Append("AMOUNT=").Append(this.AMOUNT.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<acc_ledger_summary,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<acc_ledger_summary,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from acc_ledger_summary 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
