using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumcallforwardingroamingtype:ICacheble<enumcallforwardingroamingtype>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{Type.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<enumcallforwardingroamingtype,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<enumcallforwardingroamingtype,string> whereClauseMethod)
		{
			return $@"update enumcallforwardingroamingtype set 
				id={id.ToMySqlField()+" "},
				Type={Type.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<enumcallforwardingroamingtype,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<enumcallforwardingroamingtype,string> whereClauseMethod)
		{
			return $@"delete from enumcallforwardingroamingtype 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
