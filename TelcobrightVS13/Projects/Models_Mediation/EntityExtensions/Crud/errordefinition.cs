using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class errordefinition:ICacheble<errordefinition>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.idError.ToMySqlField()).Append(",")
				.Append(this.Description.ToMySqlField()).Append(",")
				.Append(this.Severity.ToMySqlField()).Append(",")
				.Append(this.Action.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<errordefinition,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<errordefinition,string> whereClauseMethod)
		{
			return new StringBuilder("update errordefinition set ")
				.Append("idError=").Append(this.idError.ToMySqlField()).Append(",")
				.Append("Description=").Append(this.Description.ToMySqlField()).Append(",")
				.Append("Severity=").Append(this.Severity.ToMySqlField()).Append(",")
				.Append("Action=").Append(this.Action.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<errordefinition,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<errordefinition,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from errordefinition 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
