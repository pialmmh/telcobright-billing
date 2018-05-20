using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class user:ICacheble<user>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.Id.ToMySqlField()).Append(",")
				.Append(this.Email.ToMySqlField()).Append(",")
				.Append(this.EmailConfirmed.ToMySqlField()).Append(",")
				.Append(this.PasswordHash.ToMySqlField()).Append(",")
				.Append(this.SecurityStamp.ToMySqlField()).Append(",")
				.Append(this.PhoneNumber.ToMySqlField()).Append(",")
				.Append(this.PhoneNumberConfirmed.ToMySqlField()).Append(",")
				.Append(this.TwoFactorEnabled.ToMySqlField()).Append(",")
				.Append(this.LockoutEndDateUtc.ToMySqlField()).Append(",")
				.Append(this.LockoutEnabled.ToMySqlField()).Append(",")
				.Append(this.AccessFailedCount.ToMySqlField()).Append(",")
				.Append(this.UserName.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<user,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<user,string> whereClauseMethod)
		{
			return new StringBuilder("update users set ")
				.Append("Id=").Append(this.Id.ToMySqlField()).Append(",")
				.Append("Email=").Append(this.Email.ToMySqlField()).Append(",")
				.Append("EmailConfirmed=").Append(this.EmailConfirmed.ToMySqlField()).Append(",")
				.Append("PasswordHash=").Append(this.PasswordHash.ToMySqlField()).Append(",")
				.Append("SecurityStamp=").Append(this.SecurityStamp.ToMySqlField()).Append(",")
				.Append("PhoneNumber=").Append(this.PhoneNumber.ToMySqlField()).Append(",")
				.Append("PhoneNumberConfirmed=").Append(this.PhoneNumberConfirmed.ToMySqlField()).Append(",")
				.Append("TwoFactorEnabled=").Append(this.TwoFactorEnabled.ToMySqlField()).Append(",")
				.Append("LockoutEndDateUtc=").Append(this.LockoutEndDateUtc.ToMySqlField()).Append(",")
				.Append("LockoutEnabled=").Append(this.LockoutEnabled.ToMySqlField()).Append(",")
				.Append("AccessFailedCount=").Append(this.AccessFailedCount.ToMySqlField()).Append(",")
				.Append("UserName=").Append(this.UserName.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<user,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<user,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from users 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
