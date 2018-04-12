using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class xyzselected:ICacheble<xyzselected>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{prefix.ToMySqlField()},
				{PrefixSet.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<xyzselected,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<xyzselected,string> whereClauseMethod)
		{
			return $@"update xyzselected set 
				id={id.ToMySqlField()+" "},
				prefix={prefix.ToMySqlField()+" "},
				PrefixSet={PrefixSet.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<xyzselected,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<xyzselected,string> whereClauseMethod)
		{
			return $@"delete from xyzselected 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
