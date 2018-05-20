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
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.idService.ToMySqlField()).Append(",")
				.Append(this.AssignDirection.ToMySqlField()).Append(",")
				.Append(this.idpartner.ToMySqlField()).Append(",")
				.Append(this.route.ToMySqlField()).Append(",")
				.Append(this.priority.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<rateplanassignmenttuple,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<rateplanassignmenttuple,string> whereClauseMethod)
		{
			return new StringBuilder("update rateplanassignmenttuple set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("idService=").Append(this.idService.ToMySqlField()).Append(",")
				.Append("AssignDirection=").Append(this.AssignDirection.ToMySqlField()).Append(",")
				.Append("idpartner=").Append(this.idpartner.ToMySqlField()).Append(",")
				.Append("route=").Append(this.route.ToMySqlField()).Append(",")
				.Append("priority=").Append(this.priority.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<rateplanassignmenttuple,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<rateplanassignmenttuple,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from rateplanassignmenttuple 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
