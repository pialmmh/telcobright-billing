using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class autoincrementcounter:ICacheble<autoincrementcounter>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{tableName.ToMySqlField()},
				{value.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<autoincrementcounter,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<autoincrementcounter,string> whereClauseMethod)
		{
			return $@"update autoincrementcounter set 
				tableName={tableName.ToMySqlField()+" "},
				value={value.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<autoincrementcounter,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<autoincrementcounter,string> whereClauseMethod)
		{
			return $@"delete from autoincrementcounter 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
