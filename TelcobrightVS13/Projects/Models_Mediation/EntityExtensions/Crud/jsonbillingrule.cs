using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class jsonbillingrule:ICacheble<jsonbillingrule>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.ruleName.ToMySqlField()).Append(",")
				.Append(this.description.ToMySqlField()).Append(",")
				.Append(this.JsonExpression.ToMySqlField()).Append(",")
				.Append(this.isPrepaid.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<jsonbillingrule,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<jsonbillingrule,string> whereClauseMethod)
		{
			return new StringBuilder("update jsonbillingrule set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("ruleName=").Append(this.ruleName.ToMySqlField()).Append(",")
				.Append("description=").Append(this.description.ToMySqlField()).Append(",")
				.Append("JsonExpression=").Append(this.JsonExpression.ToMySqlField()).Append(",")
				.Append("isPrepaid=").Append(this.isPrepaid.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<jsonbillingrule,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<jsonbillingrule,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from jsonbillingrule 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
