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
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.value.ToMySqlField()).Append(",")
				.Append(this.ParseString.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<enumdateparsestring,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<enumdateparsestring,string> whereClauseMethod)
		{
			return new StringBuilder("update enumdateparsestring set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("value=").Append(this.value.ToMySqlField()).Append(",")
				.Append("ParseString=").Append(this.ParseString.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<enumdateparsestring,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<enumdateparsestring,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from enumdateparsestring 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
