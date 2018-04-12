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
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{UserId.ToMySqlField()},
				{RoleId.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<userrole,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<userrole,string> whereClauseMethod)
		{
			return $@"update userroles set 
				id={id.ToMySqlField()+" "},
				UserId={UserId.ToMySqlField()+" "},
				RoleId={RoleId.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<userrole,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<userrole,string> whereClauseMethod)
		{
			return $@"delete from userroles 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
