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
		public string GetExtInsertValues()
		{
			return $@"(
				{Id.ToMySqlField()},
				{Email.ToMySqlField()},
				{EmailConfirmed.ToMySqlField()},
				{PasswordHash.ToMySqlField()},
				{SecurityStamp.ToMySqlField()},
				{PhoneNumber.ToMySqlField()},
				{PhoneNumberConfirmed.ToMySqlField()},
				{TwoFactorEnabled.ToMySqlField()},
				{LockoutEndDateUtc.ToMySqlField()},
				{LockoutEnabled.ToMySqlField()},
				{AccessFailedCount.ToMySqlField()},
				{UserName.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<user,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<user,string> whereClauseMethod)
		{
			return $@"update users set 
				Id={Id.ToMySqlField()+" "},
				Email={Email.ToMySqlField()+" "},
				EmailConfirmed={EmailConfirmed.ToMySqlField()+" "},
				PasswordHash={PasswordHash.ToMySqlField()+" "},
				SecurityStamp={SecurityStamp.ToMySqlField()+" "},
				PhoneNumber={PhoneNumber.ToMySqlField()+" "},
				PhoneNumberConfirmed={PhoneNumberConfirmed.ToMySqlField()+" "},
				TwoFactorEnabled={TwoFactorEnabled.ToMySqlField()+" "},
				LockoutEndDateUtc={LockoutEndDateUtc.ToMySqlField()+" "},
				LockoutEnabled={LockoutEnabled.ToMySqlField()+" "},
				AccessFailedCount={AccessFailedCount.ToMySqlField()+" "},
				UserName={UserName.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<user,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<user,string> whereClauseMethod)
		{
			return $@"delete from users 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
