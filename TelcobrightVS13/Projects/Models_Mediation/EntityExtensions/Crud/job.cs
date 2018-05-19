using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class job:ICacheble<job>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.idjobdefinition.ToMySqlField()).Append(",")
				.Append(this.JobName.ToMySqlField()).Append(",")
				.Append(this.OwnerServer.ToMySqlField()).Append(",")
				.Append(this.idNE.ToMySqlField()).Append(",")
				.Append(this.priority.ToMySqlField()).Append(",")
				.Append(this.SerialNumber.ToMySqlField()).Append(",")
				.Append(this.Status.ToMySqlField()).Append(",")
				.Append(this.Progress.ToMySqlField()).Append(",")
				.Append(this.CreationTime.ToMySqlField()).Append(",")
				.Append(this.LastExecuted.ToMySqlField()).Append(",")
				.Append(this.CompletionTime.ToMySqlField()).Append(",")
				.Append(this.NoOfSteps.ToMySqlField()).Append(",")
				.Append(this.JobSummary.ToMySqlField()).Append(",")
				.Append(this.Error.ToMySqlField()).Append(",")
				.Append(this.JobParameter.ToMySqlField()).Append(",")
				.Append(this.JobState.ToMySqlField()).Append(",")
				.Append(this.JobAdditionalInfo.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<job,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<job,string> whereClauseMethod)
		{
			return new StringBuilder("update job set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("idjobdefinition=").Append(this.idjobdefinition.ToMySqlField()).Append(",")
				.Append("JobName=").Append(this.JobName.ToMySqlField()).Append(",")
				.Append("OwnerServer=").Append(this.OwnerServer.ToMySqlField()).Append(",")
				.Append("idNE=").Append(this.idNE.ToMySqlField()).Append(",")
				.Append("priority=").Append(this.priority.ToMySqlField()).Append(",")
				.Append("SerialNumber=").Append(this.SerialNumber.ToMySqlField()).Append(",")
				.Append("Status=").Append(this.Status.ToMySqlField()).Append(",")
				.Append("Progress=").Append(this.Progress.ToMySqlField()).Append(",")
				.Append("CreationTime=").Append(this.CreationTime.ToMySqlField()).Append(",")
				.Append("LastExecuted=").Append(this.LastExecuted.ToMySqlField()).Append(",")
				.Append("CompletionTime=").Append(this.CompletionTime.ToMySqlField()).Append(",")
				.Append("NoOfSteps=").Append(this.NoOfSteps.ToMySqlField()).Append(",")
				.Append("JobSummary=").Append(this.JobSummary.ToMySqlField()).Append(",")
				.Append("Error=").Append(this.Error.ToMySqlField()).Append(",")
				.Append("JobParameter=").Append(this.JobParameter.ToMySqlField()).Append(",")
				.Append("JobState=").Append(this.JobState.ToMySqlField()).Append(",")
				.Append("JobAdditionalInfo=").Append(this.JobAdditionalInfo.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<job,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<job,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from job 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
