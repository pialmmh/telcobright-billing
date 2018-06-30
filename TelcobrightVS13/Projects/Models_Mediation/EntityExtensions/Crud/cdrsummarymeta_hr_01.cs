using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class cdrsummarymeta_hr_01:ICacheble<cdrsummarymeta_hr_01>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.totalInsertedDuration.ToMySqlField()).Append(",")
				.Append(this.totalDeletedDuration.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<cdrsummarymeta_hr_01,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<cdrsummarymeta_hr_01,string> whereClauseMethod)
		{
			return new StringBuilder("update cdrsummarymeta_hr_01 set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("totalInsertedDuration=").Append(this.totalInsertedDuration.ToMySqlField()).Append(",")
				.Append("totalDeletedDuration=").Append(this.totalDeletedDuration.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<cdrsummarymeta_hr_01,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<cdrsummarymeta_hr_01,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from cdrsummarymeta_hr_01 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
