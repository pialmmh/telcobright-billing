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
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.prefix.ToMySqlField()).Append(",")
				.Append(this.PrefixSet.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<xyzselected,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<xyzselected,string> whereClauseMethod)
		{
			return new StringBuilder("update xyzselected set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("prefix=").Append(this.prefix.ToMySqlField()).Append(",")
				.Append("PrefixSet=").Append(this.PrefixSet.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<xyzselected,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<xyzselected,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from xyzselected 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
