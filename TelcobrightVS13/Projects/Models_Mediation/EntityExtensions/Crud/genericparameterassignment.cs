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
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.JsonExpAssignedTo.ToMySqlField()).Append(",")
				.Append(this.GenericInstanceFactoryName.ToMySqlField()).Append(",")
				.Append(this.description.ToMySqlField()).Append(",")
				.Append(this.JsonParametersToAssign.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<genericparameterassignment,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<genericparameterassignment,string> whereClauseMethod)
		{
			return new StringBuilder("update genericparameterassignment set ")
				.Append("JsonExpAssignedTo=").Append(this.JsonExpAssignedTo.ToMySqlField()).Append(",")
				.Append("GenericInstanceFactoryName=").Append(this.GenericInstanceFactoryName.ToMySqlField()).Append(",")
				.Append("description=").Append(this.description.ToMySqlField()).Append(",")
				.Append("JsonParametersToAssign=").Append(this.JsonParametersToAssign.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<genericparameterassignment,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<genericparameterassignment,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from genericparameterassignment 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
