using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class cdrduration:ICacheble<cdrduration>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.insertedduration.ToMySqlField()).Append(",")
				.Append(this.deletedduration.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<cdrduration,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<cdrduration,string> whereClauseMethod)
		{
			return new StringBuilder("update cdrduration set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("insertedduration=").Append(this.insertedduration.ToMySqlField()).Append(",")
				.Append("deletedduration=").Append(this.deletedduration.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<cdrduration,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<cdrduration,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from cdrduration 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
