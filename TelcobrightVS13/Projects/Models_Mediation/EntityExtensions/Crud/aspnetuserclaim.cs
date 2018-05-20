using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class aspnetuserclaim:ICacheble<aspnetuserclaim>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.Id.ToMySqlField()).Append(",")
				.Append(this.UserId.ToMySqlField()).Append(",")
				.Append(this.ClaimType.ToMySqlField()).Append(",")
				.Append(this.ClaimValue.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<aspnetuserclaim,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<aspnetuserclaim,string> whereClauseMethod)
		{
			return new StringBuilder("update aspnetuserclaims set ")
				.Append("Id=").Append(this.Id.ToMySqlField()).Append(",")
				.Append("UserId=").Append(this.UserId.ToMySqlField()).Append(",")
				.Append("ClaimType=").Append(this.ClaimType.ToMySqlField()).Append(",")
				.Append("ClaimValue=").Append(this.ClaimValue.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<aspnetuserclaim,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<aspnetuserclaim,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from aspnetuserclaims 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
