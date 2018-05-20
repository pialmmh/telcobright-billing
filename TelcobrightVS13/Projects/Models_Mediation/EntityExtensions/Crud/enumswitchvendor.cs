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
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.Type.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<enumswitchvendor,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<enumswitchvendor,string> whereClauseMethod)
		{
			return new StringBuilder("update enumswitchvendor set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("Type=").Append(this.Type.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<enumswitchvendor,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<enumswitchvendor,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from enumswitchvendor 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
