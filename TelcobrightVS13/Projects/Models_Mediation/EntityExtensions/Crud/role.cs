using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class role:ICacheble<role>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.Id.ToMySqlField()).Append(",")
				.Append(this.Name.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<role,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<role,string> whereClauseMethod)
		{
			return new StringBuilder("update roles set ")
				.Append("Id=").Append(this.Id.ToMySqlField()).Append(",")
				.Append("Name=").Append(this.Name.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<role,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<role,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from roles 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
