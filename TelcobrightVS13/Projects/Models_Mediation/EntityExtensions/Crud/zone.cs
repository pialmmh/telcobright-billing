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
		public string GetExtInsertValues()
		{
			return $@"(
				{zone_id.ToMySqlField()},
				{country_code.ToMySqlField()},
				{zone_name.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<zone,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<zone,string> whereClauseMethod)
		{
			return $@"update zone set 
				zone_id={zone_id.ToMySqlField()+" "},
				country_code={country_code.ToMySqlField()+" "},
				zone_name={zone_name.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<zone,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<zone,string> whereClauseMethod)
		{
			return $@"delete from zone 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
