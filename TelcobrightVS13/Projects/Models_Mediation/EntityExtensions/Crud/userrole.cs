using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class userrole:ICacheble<userrole>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.UserId.ToMySqlField()).Append(",")
				.Append(this.RoleId.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<userrole,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<userrole,string> whereClauseMethod)
		{
			return new StringBuilder("update userroles set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("UserId=").Append(this.UserId.ToMySqlField()).Append(",")
				.Append("RoleId=").Append(this.RoleId.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<userrole,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<userrole,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from userroles 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
