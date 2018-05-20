using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumjobdefinition:ICacheble<enumjobdefinition>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.jobtypeid.ToMySqlField()).Append(",")
				.Append(this.Type.ToMySqlField()).Append(",")
				.Append(this.Priority.ToMySqlField()).Append(",")
				.Append(this.BatchCreatable.ToMySqlField()).Append(",")
				.Append(this.JobQueue.ToMySqlField()).Append(",")
				.Append(this.Disabled.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<enumjobdefinition,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<enumjobdefinition,string> whereClauseMethod)
		{
			return new StringBuilder("update enumjobdefinition set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("jobtypeid=").Append(this.jobtypeid.ToMySqlField()).Append(",")
				.Append("Type=").Append(this.Type.ToMySqlField()).Append(",")
				.Append("Priority=").Append(this.Priority.ToMySqlField()).Append(",")
				.Append("BatchCreatable=").Append(this.BatchCreatable.ToMySqlField()).Append(",")
				.Append("JobQueue=").Append(this.JobQueue.ToMySqlField()).Append(",")
				.Append("Disabled=").Append(this.Disabled.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<enumjobdefinition,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<enumjobdefinition,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from enumjobdefinition 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
