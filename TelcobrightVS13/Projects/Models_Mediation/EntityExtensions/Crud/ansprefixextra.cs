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
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.PrefixBeforeAnsNumber.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<ansprefixextra,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<ansprefixextra,string> whereClauseMethod)
		{
			return new StringBuilder("update ansprefixextra set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("PrefixBeforeAnsNumber=").Append(this.PrefixBeforeAnsNumber.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<ansprefixextra,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<ansprefixextra,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from ansprefixextra 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
