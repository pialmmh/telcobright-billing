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
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{jobtypeid.ToMySqlField()},
				{Type.ToMySqlField()},
				{Priority.ToMySqlField()},
				{BatchCreatable.ToMySqlField()},
				{JobQueue.ToMySqlField()},
				{Disabled.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<enumjobdefinition,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<enumjobdefinition,string> whereClauseMethod)
		{
			return $@"update enumjobdefinition set 
				id={id.ToMySqlField()+" "},
				jobtypeid={jobtypeid.ToMySqlField()+" "},
				Type={Type.ToMySqlField()+" "},
				Priority={Priority.ToMySqlField()+" "},
				BatchCreatable={BatchCreatable.ToMySqlField()+" "},
				JobQueue={JobQueue.ToMySqlField()+" "},
				Disabled={Disabled.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<enumjobdefinition,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<enumjobdefinition,string> whereClauseMethod)
		{
			return $@"delete from enumjobdefinition 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
