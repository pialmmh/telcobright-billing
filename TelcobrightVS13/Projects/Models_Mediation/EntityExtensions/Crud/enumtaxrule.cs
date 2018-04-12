using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumtaxrule:ICacheble<enumtaxrule>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{Type.ToMySqlField()},
				{description.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<enumtaxrule,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<enumtaxrule,string> whereClauseMethod)
		{
			return $@"update enumtaxrule set 
				id={id.ToMySqlField()+" "},
				Type={Type.ToMySqlField()+" "},
				description={description.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<enumtaxrule,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<enumtaxrule,string> whereClauseMethod)
		{
			return $@"delete from enumtaxrule 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
