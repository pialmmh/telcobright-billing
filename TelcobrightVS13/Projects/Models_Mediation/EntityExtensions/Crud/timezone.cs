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
		public string GetExtInsertValues()
		{
			return $@"(
				{zone_id.ToMySqlField()},
				{abbreviation.ToMySqlField()},
				{time_start.ToMySqlField()},
				{gmt_offset.ToMySqlField()},
				{dst.ToMySqlField()},
				{offsetdesc.ToMySqlField()},
				{id.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<timezone,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<timezone,string> whereClauseMethod)
		{
			return $@"update timezone set 
				zone_id={zone_id.ToMySqlField()+" "},
				abbreviation={abbreviation.ToMySqlField()+" "},
				time_start={time_start.ToMySqlField()+" "},
				gmt_offset={gmt_offset.ToMySqlField()+" "},
				dst={dst.ToMySqlField()+" "},
				offsetdesc={offsetdesc.ToMySqlField()+" "},
				id={id.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<timezone,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<timezone,string> whereClauseMethod)
		{
			return $@"delete from timezone 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
