using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class country:ICacheble<country>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.country_code.ToMySqlField()).Append(",")
				.Append(this.country_name.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<country,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<country,string> whereClauseMethod)
		{
			return new StringBuilder("update country set ")
				.Append("country_code=").Append(this.country_code.ToMySqlField()).Append(",")
				.Append("country_name=").Append(this.country_name.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<country,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<country,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from country 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
