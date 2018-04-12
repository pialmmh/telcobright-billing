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
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{idError.ToMySqlField()},
				{jobid.ToMySqlField()},
				{jobname.ToMySqlField()},
				{TimeRaised.ToMySqlField()},
				{TimeCleared.ToMySqlField()},
				{Status.ToMySqlField()},
				{ClearType.ToMySqlField()},
				{ClearingUser.ToMySqlField()},
				{ExceptionMessage.ToMySqlField()},
				{ProcessName.ToMySqlField()},
				{ExceptionDetail.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<allerror,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<allerror,string> whereClauseMethod)
		{
			return $@"update allerror set 
				id={id.ToMySqlField()+" "},
				idError={idError.ToMySqlField()+" "},
				jobid={jobid.ToMySqlField()+" "},
				jobname={jobname.ToMySqlField()+" "},
				TimeRaised={TimeRaised.ToMySqlField()+" "},
				TimeCleared={TimeCleared.ToMySqlField()+" "},
				Status={Status.ToMySqlField()+" "},
				ClearType={ClearType.ToMySqlField()+" "},
				ClearingUser={ClearingUser.ToMySqlField()+" "},
				ExceptionMessage={ExceptionMessage.ToMySqlField()+" "},
				ProcessName={ProcessName.ToMySqlField()+" "},
				ExceptionDetail={ExceptionDetail.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<allerror,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<allerror,string> whereClauseMethod)
		{
			return $@"delete from allerror 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
