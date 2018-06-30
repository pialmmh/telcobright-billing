using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class cdrmeta:ICacheble<cdrmeta>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.totalInsertedDuration.ToMySqlField()).Append(",")
				.Append(this.totalDeletedDuration.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<cdrmeta,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<cdrmeta,string> whereClauseMethod)
		{
			return new StringBuilder("update cdrmeta set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("totalInsertedDuration=").Append(this.totalInsertedDuration.ToMySqlField()).Append(",")
				.Append("totalDeletedDuration=").Append(this.totalDeletedDuration.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<cdrmeta,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<cdrmeta,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from cdrmeta 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
