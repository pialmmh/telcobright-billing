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
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{idjobdefinition.ToMySqlField()},
				{JobName.ToMySqlField()},
				{OwnerServer.ToMySqlField()},
				{idNE.ToMySqlField()},
				{priority.ToMySqlField()},
				{SerialNumber.ToMySqlField()},
				{Status.ToMySqlField()},
				{Progress.ToMySqlField()},
				{CreationTime.ToMySqlField()},
				{LastExecuted.ToMySqlField()},
				{CompletionTime.ToMySqlField()},
				{NoOfSteps.ToMySqlField()},
				{JobSummary.ToMySqlField()},
				{Error.ToMySqlField()},
				{JobParameter.ToMySqlField()},
				{JobState.ToMySqlField()},
				{JobAdditionalInfo.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<job,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<job,string> whereClauseMethod)
		{
			return $@"update job set 
				id={id.ToMySqlField()+" "},
				idjobdefinition={idjobdefinition.ToMySqlField()+" "},
				JobName={JobName.ToMySqlField()+" "},
				OwnerServer={OwnerServer.ToMySqlField()+" "},
				idNE={idNE.ToMySqlField()+" "},
				priority={priority.ToMySqlField()+" "},
				SerialNumber={SerialNumber.ToMySqlField()+" "},
				Status={Status.ToMySqlField()+" "},
				Progress={Progress.ToMySqlField()+" "},
				CreationTime={CreationTime.ToMySqlField()+" "},
				LastExecuted={LastExecuted.ToMySqlField()+" "},
				CompletionTime={CompletionTime.ToMySqlField()+" "},
				NoOfSteps={NoOfSteps.ToMySqlField()+" "},
				JobSummary={JobSummary.ToMySqlField()+" "},
				Error={Error.ToMySqlField()+" "},
				JobParameter={JobParameter.ToMySqlField()+" "},
				JobState={JobState.ToMySqlField()+" "},
				JobAdditionalInfo={JobAdditionalInfo.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<job,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<job,string> whereClauseMethod)
		{
			return $@"delete from job 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
