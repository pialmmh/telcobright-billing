using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class timezone:ICacheble<timezone>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.zone_id.ToMySqlField()).Append(",")
				.Append(this.abbreviation.ToMySqlField()).Append(",")
				.Append(this.time_start.ToMySqlField()).Append(",")
				.Append(this.gmt_offset.ToMySqlField()).Append(",")
				.Append(this.dst.ToMySqlField()).Append(",")
				.Append(this.offsetdesc.ToMySqlField()).Append(",")
				.Append(this.id.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<timezone,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<timezone,string> whereClauseMethod)
		{
			return new StringBuilder("update timezone set ")
				.Append("zone_id=").Append(this.zone_id.ToMySqlField()).Append(",")
				.Append("abbreviation=").Append(this.abbreviation.ToMySqlField()).Append(",")
				.Append("time_start=").Append(this.time_start.ToMySqlField()).Append(",")
				.Append("gmt_offset=").Append(this.gmt_offset.ToMySqlField()).Append(",")
				.Append("dst=").Append(this.dst.ToMySqlField()).Append(",")
				.Append("offsetdesc=").Append(this.offsetdesc.ToMySqlField()).Append(",")
				.Append("id=").Append(this.id.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<timezone,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<timezone,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from timezone 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
