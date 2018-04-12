using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumrateplantype:ICacheble<enumrateplantype>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{Type.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<enumrateplantype,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<enumrateplantype,string> whereClauseMethod)
		{
			return $@"update enumrateplantype set 
				id={id.ToMySqlField()+" "},
				Type={Type.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<enumrateplantype,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<enumrateplantype,string> whereClauseMethod)
		{
			return $@"delete from enumrateplantype 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
