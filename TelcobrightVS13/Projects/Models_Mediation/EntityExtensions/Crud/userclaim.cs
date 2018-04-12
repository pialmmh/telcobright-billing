using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class userclaim:ICacheble<userclaim>
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
		public  string GetExtInsertCustom(Func<userclaim,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<userclaim,string> whereClauseMethod)
		{
			return $@"update userclaims set 
				Id={Id.ToMySqlField()+" "},
				UserId={UserId.ToMySqlField()+" "},
				ClaimType={ClaimType.ToMySqlField()+" "},
				ClaimValue={ClaimValue.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<userclaim,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<userclaim,string> whereClauseMethod)
		{
			return $@"delete from userclaims 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
