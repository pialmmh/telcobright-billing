using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumdateparsestring:ICacheble<enumdateparsestring>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{value.ToMySqlField()},
				{ParseString.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<enumdateparsestring,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<enumdateparsestring,string> whereClauseMethod)
		{
			return $@"update enumdateparsestring set 
				id={id.ToMySqlField()+" "},
				value={value.ToMySqlField()+" "},
				ParseString={ParseString.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<enumdateparsestring,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<enumdateparsestring,string> whereClauseMethod)
		{
			return $@"delete from enumdateparsestring 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
