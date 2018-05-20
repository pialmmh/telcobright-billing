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
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.idRatePlanAssignmentTuple.ToMySqlField()).Append(",")
				.Append(this.idBillingRule.ToMySqlField()).Append(",")
				.Append(this.idServiceGroup.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<billingruleassignment,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<billingruleassignment,string> whereClauseMethod)
		{
			return new StringBuilder("update billingruleassignment set ")
				.Append("idRatePlanAssignmentTuple=").Append(this.idRatePlanAssignmentTuple.ToMySqlField()).Append(",")
				.Append("idBillingRule=").Append(this.idBillingRule.ToMySqlField()).Append(",")
				.Append("idServiceGroup=").Append(this.idServiceGroup.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<billingruleassignment,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<billingruleassignment,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from billingruleassignment 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
