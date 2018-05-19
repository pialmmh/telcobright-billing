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
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.TriggerTime.ToMySqlField()).Append(",")
				.Append(this.RateChangeType.ToMySqlField()).Append(",")
				.Append(this.prefix.ToMySqlField()).Append(",")
				.Append(this.JobCreated.ToMySqlField()).Append(",")
				.Append(this.RatePlanAssignmentFlag.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<lcrpoint,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<lcrpoint,string> whereClauseMethod)
		{
			return new StringBuilder("update lcrpoint set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("TriggerTime=").Append(this.TriggerTime.ToMySqlField()).Append(",")
				.Append("RateChangeType=").Append(this.RateChangeType.ToMySqlField()).Append(",")
				.Append("prefix=").Append(this.prefix.ToMySqlField()).Append(",")
				.Append("JobCreated=").Append(this.JobCreated.ToMySqlField()).Append(",")
				.Append("RatePlanAssignmentFlag=").Append(this.RatePlanAssignmentFlag.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<lcrpoint,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<lcrpoint,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from lcrpoint 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
