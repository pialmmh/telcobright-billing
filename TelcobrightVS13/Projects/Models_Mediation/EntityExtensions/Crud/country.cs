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
		public string GetExtInsertValues()
		{
			return $@"(
				{country_code.ToMySqlField()},
				{country_name.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<country,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<country,string> whereClauseMethod)
		{
			return $@"update country set 
				country_code={country_code.ToMySqlField()+" "},
				country_name={country_name.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<country,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<country,string> whereClauseMethod)
		{
			return $@"delete from country 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
