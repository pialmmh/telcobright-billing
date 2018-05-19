using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class ratetaskreference:ICacheble<ratetaskreference>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.idRatePlan.ToMySqlField()).Append(",")
				.Append(this.Description.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<ratetaskreference,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<ratetaskreference,string> whereClauseMethod)
		{
			return new StringBuilder("update ratetaskreference set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("idRatePlan=").Append(this.idRatePlan.ToMySqlField()).Append(",")
				.Append("Description=").Append(this.Description.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<ratetaskreference,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<ratetaskreference,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from ratetaskreference 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
