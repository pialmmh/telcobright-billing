using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumcreditrule:ICacheble<enumcreditrule>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{RuleName.ToMySqlField()},
				{Description.ToMySqlField()},
				{json.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<enumcreditrule,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<enumcreditrule,string> whereClauseMethod)
		{
			return $@"update enumcreditrule set 
				id={id.ToMySqlField()+" "},
				RuleName={RuleName.ToMySqlField()+" "},
				Description={Description.ToMySqlField()+" "},
				json={json.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<enumcreditrule,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<enumcreditrule,string> whereClauseMethod)
		{
			return $@"delete from enumcreditrule 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
