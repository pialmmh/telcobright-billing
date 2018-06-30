using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class cdrsummarymeta_day_02:ICacheble<cdrsummarymeta_day_02>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.totalInsertedDuration.ToMySqlField()).Append(",")
				.Append(this.totalDeletedDuration.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<cdrsummarymeta_day_02,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<cdrsummarymeta_day_02,string> whereClauseMethod)
		{
			return new StringBuilder("update cdrsummarymeta_day_02 set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("totalInsertedDuration=").Append(this.totalInsertedDuration.ToMySqlField()).Append(",")
				.Append("totalDeletedDuration=").Append(this.totalDeletedDuration.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<cdrsummarymeta_day_02,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<cdrsummarymeta_day_02,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from cdrsummarymeta_day_02 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
