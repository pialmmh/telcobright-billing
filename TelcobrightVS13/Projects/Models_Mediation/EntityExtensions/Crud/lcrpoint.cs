using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class lcrpoint:ICacheble<lcrpoint>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{TriggerTime.ToMySqlField()},
				{RateChangeType.ToMySqlField()},
				{prefix.ToMySqlField()},
				{JobCreated.ToMySqlField()},
				{RatePlanAssignmentFlag.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<lcrpoint,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<lcrpoint,string> whereClauseMethod)
		{
			return $@"update lcrpoint set 
				id={id.ToMySqlField()+" "},
				TriggerTime={TriggerTime.ToMySqlField()+" "},
				RateChangeType={RateChangeType.ToMySqlField()+" "},
				prefix={prefix.ToMySqlField()+" "},
				JobCreated={JobCreated.ToMySqlField()+" "},
				RatePlanAssignmentFlag={RatePlanAssignmentFlag.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<lcrpoint,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<lcrpoint,string> whereClauseMethod)
		{
			return $@"delete from lcrpoint 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
