using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class login_history:ICacheble<login_history>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.username.ToMySqlField()).Append(",")
				.Append(this.remote_ip.ToMySqlField()).Append(",")
				.Append(this.login_time.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<login_history,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<login_history,string> whereClauseMethod)
		{
			return new StringBuilder("update login_history set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("username=").Append(this.username.ToMySqlField()).Append(",")
				.Append("remote_ip=").Append(this.remote_ip.ToMySqlField()).Append(",")
				.Append("login_time=").Append(this.login_time.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<login_history,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<login_history,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from login_history 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
