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
		public string GetExtInsertValues()
		{
			return $@"(
				{LoginProvider.ToMySqlField()},
				{ProviderKey.ToMySqlField()},
				{UserId.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<userlogin,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<userlogin,string> whereClauseMethod)
		{
			return $@"update userlogins set 
				LoginProvider={LoginProvider.ToMySqlField()+" "},
				ProviderKey={ProviderKey.ToMySqlField()+" "},
				UserId={UserId.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<userlogin,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<userlogin,string> whereClauseMethod)
		{
			return $@"delete from userlogins 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
