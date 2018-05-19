using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class zone:ICacheble<zone>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.zone_id.ToMySqlField()).Append(",")
				.Append(this.country_code.ToMySqlField()).Append(",")
				.Append(this.zone_name.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<zone,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<zone,string> whereClauseMethod)
		{
			return new StringBuilder("update zone set ")
				.Append("zone_id=").Append(this.zone_id.ToMySqlField()).Append(",")
				.Append("country_code=").Append(this.country_code.ToMySqlField()).Append(",")
				.Append("zone_name=").Append(this.zone_name.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<zone,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<zone,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from zone 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
