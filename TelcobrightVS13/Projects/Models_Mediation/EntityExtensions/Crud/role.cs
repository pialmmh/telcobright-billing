using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class role:ICacheble<role>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{Id.ToMySqlField()},
				{Name.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<role,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<role,string> whereClauseMethod)
		{
			return $@"update roles set 
				Id={Id.ToMySqlField()+" "},
				Name={Name.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<role,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<role,string> whereClauseMethod)
		{
			return $@"delete from roles 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
