using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class billingruleassignment:ICacheble<billingruleassignment>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{idRatePlanAssignmentTuple.ToMySqlField()},
				{idBillingRule.ToMySqlField()},
				{idServiceGroup.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<billingruleassignment,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<billingruleassignment,string> whereClauseMethod)
		{
			return $@"update billingruleassignment set 
				idRatePlanAssignmentTuple={idRatePlanAssignmentTuple.ToMySqlField()+" "},
				idBillingRule={idBillingRule.ToMySqlField()+" "},
				idServiceGroup={idServiceGroup.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<billingruleassignment,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<billingruleassignment,string> whereClauseMethod)
		{
			return $@"delete from billingruleassignment 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
