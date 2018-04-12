using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class ansprefixextra:ICacheble<ansprefixextra>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{PrefixBeforeAnsNumber.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<ansprefixextra,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<ansprefixextra,string> whereClauseMethod)
		{
			return $@"update ansprefixextra set 
				id={id.ToMySqlField()+" "},
				PrefixBeforeAnsNumber={PrefixBeforeAnsNumber.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<ansprefixextra,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<ansprefixextra,string> whereClauseMethod)
		{
			return $@"delete from ansprefixextra 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
