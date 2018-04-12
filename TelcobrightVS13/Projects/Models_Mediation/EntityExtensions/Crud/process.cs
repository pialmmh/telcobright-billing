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
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{ProcessName.ToMySqlField()},
				{LastRun.ToMySqlField()},
				{ProcessParamaterJson.ToMySqlField()},
				{AdminState.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<process,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<process,string> whereClauseMethod)
		{
			return $@"update process set 
				id={id.ToMySqlField()+" "},
				ProcessName={ProcessName.ToMySqlField()+" "},
				LastRun={LastRun.ToMySqlField()+" "},
				ProcessParamaterJson={ProcessParamaterJson.ToMySqlField()+" "},
				AdminState={AdminState.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<process,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<process,string> whereClauseMethod)
		{
			return $@"delete from process 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
