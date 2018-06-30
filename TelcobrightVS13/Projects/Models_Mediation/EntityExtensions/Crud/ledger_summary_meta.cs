using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class ledger_summary_meta:ICacheble<ledger_summary_meta>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.totalInsertedAmount.ToMySqlField()).Append(",")
				.Append(this.totalDeletedAmount.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<ledger_summary_meta,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<ledger_summary_meta,string> whereClauseMethod)
		{
			return new StringBuilder("update ledger_summary_meta set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("totalInsertedAmount=").Append(this.totalInsertedAmount.ToMySqlField()).Append(",")
				.Append("totalDeletedAmount=").Append(this.totalDeletedAmount.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<ledger_summary_meta,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<ledger_summary_meta,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from ledger_summary_meta 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
