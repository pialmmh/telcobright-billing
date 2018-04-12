using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class jobcompletion:ICacheble<jobcompletion>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{idJob.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<jobcompletion,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<jobcompletion,string> whereClauseMethod)
		{
			return $@"update jobcompletion set 
				idJob={idJob.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<jobcompletion,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<jobcompletion,string> whereClauseMethod)
		{
			return $@"delete from jobcompletion 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
