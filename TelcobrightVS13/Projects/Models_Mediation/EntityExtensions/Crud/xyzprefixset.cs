using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class xyzprefixset:ICacheble<xyzprefixset>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{Name.ToMySqlField()},
				{Description.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<xyzprefixset,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<xyzprefixset,string> whereClauseMethod)
		{
			return $@"update xyzprefixset set 
				id={id.ToMySqlField()+" "},
				Name={Name.ToMySqlField()+" "},
				Description={Description.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<xyzprefixset,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<xyzprefixset,string> whereClauseMethod)
		{
			return $@"delete from xyzprefixset 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
