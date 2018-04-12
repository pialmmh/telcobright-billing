using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumswitchvendor:ICacheble<enumswitchvendor>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{Type.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<enumswitchvendor,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<enumswitchvendor,string> whereClauseMethod)
		{
			return $@"update enumswitchvendor set 
				id={id.ToMySqlField()+" "},
				Type={Type.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<enumswitchvendor,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<enumswitchvendor,string> whereClauseMethod)
		{
			return $@"delete from enumswitchvendor 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
