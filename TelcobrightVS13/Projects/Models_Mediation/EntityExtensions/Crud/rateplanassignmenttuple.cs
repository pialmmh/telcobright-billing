using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class rateplanassignmenttuple:ICacheble<rateplanassignmenttuple>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{idService.ToMySqlField()},
				{AssignDirection.ToMySqlField()},
				{idpartner.ToMySqlField()},
				{route.ToMySqlField()},
				{priority.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<rateplanassignmenttuple,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<rateplanassignmenttuple,string> whereClauseMethod)
		{
			return $@"update rateplanassignmenttuple set 
				id={id.ToMySqlField()+" "},
				idService={idService.ToMySqlField()+" "},
				AssignDirection={AssignDirection.ToMySqlField()+" "},
				idpartner={idpartner.ToMySqlField()+" "},
				route={route.ToMySqlField()+" "},
				priority={priority.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<rateplanassignmenttuple,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<rateplanassignmenttuple,string> whereClauseMethod)
		{
			return $@"delete from rateplanassignmenttuple 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
