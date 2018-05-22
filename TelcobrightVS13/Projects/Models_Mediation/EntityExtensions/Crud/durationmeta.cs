using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class durationmeta:ICacheble<durationmeta>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.filename.ToMySqlField()).Append(",")
				.Append(this.oldDuration.ToMySqlField()).Append(",")
				.Append(this.newDuration.ToMySqlField()).Append(",")
				.Append(this.durationAfterJob.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<durationmeta,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<durationmeta,string> whereClauseMethod)
		{
			return new StringBuilder("update durationmeta set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("filename=").Append(this.filename.ToMySqlField()).Append(",")
				.Append("oldDuration=").Append(this.oldDuration.ToMySqlField()).Append(",")
				.Append("newDuration=").Append(this.newDuration.ToMySqlField()).Append(",")
				.Append("durationAfterJob=").Append(this.durationAfterJob.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<durationmeta,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<durationmeta,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from durationmeta 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
