using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class allerror:ICacheble<allerror>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.idError.ToMySqlField()).Append(",")
				.Append(this.jobid.ToMySqlField()).Append(",")
				.Append(this.jobname.ToMySqlField()).Append(",")
				.Append(this.TimeRaised.ToMySqlField()).Append(",")
				.Append(this.TimeCleared.ToMySqlField()).Append(",")
				.Append(this.Status.ToMySqlField()).Append(",")
				.Append(this.ClearType.ToMySqlField()).Append(",")
				.Append(this.ClearingUser.ToMySqlField()).Append(",")
				.Append(this.ExceptionMessage.ToMySqlField()).Append(",")
				.Append(this.ProcessName.ToMySqlField()).Append(",")
				.Append(this.ExceptionDetail.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<allerror,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<allerror,string> whereClauseMethod)
		{
			return new StringBuilder("update allerror set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("idError=").Append(this.idError.ToMySqlField()).Append(",")
				.Append("jobid=").Append(this.jobid.ToMySqlField()).Append(",")
				.Append("jobname=").Append(this.jobname.ToMySqlField()).Append(",")
				.Append("TimeRaised=").Append(this.TimeRaised.ToMySqlField()).Append(",")
				.Append("TimeCleared=").Append(this.TimeCleared.ToMySqlField()).Append(",")
				.Append("Status=").Append(this.Status.ToMySqlField()).Append(",")
				.Append("ClearType=").Append(this.ClearType.ToMySqlField()).Append(",")
				.Append("ClearingUser=").Append(this.ClearingUser.ToMySqlField()).Append(",")
				.Append("ExceptionMessage=").Append(this.ExceptionMessage.ToMySqlField()).Append(",")
				.Append("ProcessName=").Append(this.ProcessName.ToMySqlField()).Append(",")
				.Append("ExceptionDetail=").Append(this.ExceptionDetail.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<allerror,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<allerror,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from allerror 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
