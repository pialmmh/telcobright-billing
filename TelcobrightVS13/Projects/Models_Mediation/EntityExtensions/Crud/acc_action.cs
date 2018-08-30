using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class acc_action:ICacheble<acc_action>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.idAccount.ToMySqlField()).Append(",")
				.Append(this.threshhold_value.ToMySqlField()).Append(",")
				.Append(this.idAccountAction.ToMySqlField()).Append(",")
				.Append(this.isNotified.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<acc_action,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<acc_action,string> whereClauseMethod)
		{
			return new StringBuilder("update acc_action set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("idAccount=").Append(this.idAccount.ToMySqlField()).Append(",")
				.Append("threshhold_value=").Append(this.threshhold_value.ToMySqlField()).Append(",")
				.Append("idAccountAction=").Append(this.idAccountAction.ToMySqlField()).Append(",")
				.Append("isNotified=").Append(this.isNotified.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<acc_action,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<acc_action,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from acc_action 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
