using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumcurrency:ICacheble<enumcurrency>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{Type.ToMySqlField()},
				{Description.ToMySqlField()},
				{Symbol.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<enumcurrency,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<enumcurrency,string> whereClauseMethod)
		{
			return $@"update enumcurrency set 
				id={id.ToMySqlField()+" "},
				Type={Type.ToMySqlField()+" "},
				Description={Description.ToMySqlField()+" "},
				Symbol={Symbol.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<enumcurrency,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<enumcurrency,string> whereClauseMethod)
		{
			return $@"delete from enumcurrency 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
