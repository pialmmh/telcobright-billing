using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class autoincrementcounter:ICacheble<autoincrementcounter>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.tableName.ToMySqlField()).Append(",")
				.Append(this.value.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<autoincrementcounter,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<autoincrementcounter,string> whereClauseMethod)
		{
			return new StringBuilder("update autoincrementcounter set ")
				.Append("tableName=").Append(this.tableName.ToMySqlField()).Append(",")
				.Append("value=").Append(this.value.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<autoincrementcounter,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<autoincrementcounter,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from autoincrementcounter 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
