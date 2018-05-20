using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class userlogin:ICacheble<userlogin>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.LoginProvider.ToMySqlField()).Append(",")
				.Append(this.ProviderKey.ToMySqlField()).Append(",")
				.Append(this.UserId.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<userlogin,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<userlogin,string> whereClauseMethod)
		{
			return new StringBuilder("update userlogins set ")
				.Append("LoginProvider=").Append(this.LoginProvider.ToMySqlField()).Append(",")
				.Append("ProviderKey=").Append(this.ProviderKey.ToMySqlField()).Append(",")
				.Append("UserId=").Append(this.UserId.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<userlogin,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<userlogin,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from userlogins 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
