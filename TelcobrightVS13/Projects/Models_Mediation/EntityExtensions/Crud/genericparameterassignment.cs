using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class genericparameterassignment:ICacheble<genericparameterassignment>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{JsonExpAssignedTo.ToMySqlField()},
				{GenericInstanceFactoryName.ToMySqlField()},
				{description.ToMySqlField()},
				{JsonParametersToAssign.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<genericparameterassignment,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<genericparameterassignment,string> whereClauseMethod)
		{
			return $@"update genericparameterassignment set 
				JsonExpAssignedTo={JsonExpAssignedTo.ToMySqlField()+" "},
				GenericInstanceFactoryName={GenericInstanceFactoryName.ToMySqlField()+" "},
				description={description.ToMySqlField()+" "},
				JsonParametersToAssign={JsonParametersToAssign.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<genericparameterassignment,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<genericparameterassignment,string> whereClauseMethod)
		{
			return $@"delete from genericparameterassignment 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
