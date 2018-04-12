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
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{ruleName.ToMySqlField()},
				{description.ToMySqlField()},
				{JsonExpression.ToMySqlField()},
				{isPrepaid.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<jsonbillingrule,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<jsonbillingrule,string> whereClauseMethod)
		{
			return $@"update jsonbillingrule set 
				id={id.ToMySqlField()+" "},
				ruleName={ruleName.ToMySqlField()+" "},
				description={description.ToMySqlField()+" "},
				JsonExpression={JsonExpression.ToMySqlField()+" "},
				isPrepaid={isPrepaid.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<jsonbillingrule,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<jsonbillingrule,string> whereClauseMethod)
		{
			return $@"delete from jsonbillingrule 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
