using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumbillingspan:ICacheble<enumbillingspan>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{ofbiz_uom_Id.ToMySqlField()},
				{Type.ToMySqlField()},
				{value.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<enumbillingspan,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<enumbillingspan,string> whereClauseMethod)
		{
			return $@"update enumbillingspan set 
				ofbiz_uom_Id={ofbiz_uom_Id.ToMySqlField()+" "},
				Type={Type.ToMySqlField()+" "},
				value={value.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<enumbillingspan,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<enumbillingspan,string> whereClauseMethod)
		{
			return $@"delete from enumbillingspan 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
