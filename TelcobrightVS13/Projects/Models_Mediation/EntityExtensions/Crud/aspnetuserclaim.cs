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
		public string GetExtInsertValues()
		{
			return $@"(
				{Id.ToMySqlField()},
				{UserId.ToMySqlField()},
				{ClaimType.ToMySqlField()},
				{ClaimValue.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<aspnetuserclaim,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<aspnetuserclaim,string> whereClauseMethod)
		{
			return $@"update aspnetuserclaims set 
				Id={Id.ToMySqlField()+" "},
				UserId={UserId.ToMySqlField()+" "},
				ClaimType={ClaimType.ToMySqlField()+" "},
				ClaimValue={ClaimValue.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<aspnetuserclaim,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<aspnetuserclaim,string> whereClauseMethod)
		{
			return $@"delete from aspnetuserclaims 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
