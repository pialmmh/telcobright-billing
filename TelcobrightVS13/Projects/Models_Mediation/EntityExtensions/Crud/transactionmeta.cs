using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class transactionmeta:ICacheble<transactionmeta>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.totalInsertedAmount.ToMySqlField()).Append(",")
				.Append(this.totalDeletedAmount.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<transactionmeta,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<transactionmeta,string> whereClauseMethod)
		{
			return new StringBuilder("update transactionmeta set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("totalInsertedAmount=").Append(this.totalInsertedAmount.ToMySqlField()).Append(",")
				.Append("totalDeletedAmount=").Append(this.totalDeletedAmount.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<transactionmeta,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<transactionmeta,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from transactionmeta 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
