using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class process:ICacheble<process>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.ProcessName.ToMySqlField()).Append(",")
				.Append(this.LastRun.ToMySqlField()).Append(",")
				.Append(this.ProcessParamaterJson.ToMySqlField()).Append(",")
				.Append(this.AdminState.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<process,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<process,string> whereClauseMethod)
		{
			return new StringBuilder("update process set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("ProcessName=").Append(this.ProcessName.ToMySqlField()).Append(",")
				.Append("LastRun=").Append(this.LastRun.ToMySqlField()).Append(",")
				.Append("ProcessParamaterJson=").Append(this.ProcessParamaterJson.ToMySqlField()).Append(",")
				.Append("AdminState=").Append(this.AdminState.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<process,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<process,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from process 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
