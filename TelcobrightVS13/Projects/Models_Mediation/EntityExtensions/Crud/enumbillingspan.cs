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
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.ofbiz_uom_Id.ToMySqlField()).Append(",")
				.Append(this.Type.ToMySqlField()).Append(",")
				.Append(this.value.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<enumbillingspan,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<enumbillingspan,string> whereClauseMethod)
		{
			return new StringBuilder("update enumbillingspan set ")
				.Append("ofbiz_uom_Id=").Append(this.ofbiz_uom_Id.ToMySqlField()).Append(",")
				.Append("Type=").Append(this.Type.ToMySqlField()).Append(",")
				.Append("value=").Append(this.value.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<enumbillingspan,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<enumbillingspan,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from enumbillingspan 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
