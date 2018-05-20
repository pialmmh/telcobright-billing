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
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.Name.ToMySqlField()).Append(",")
				.Append(this.Description.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<xyzprefixset,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<xyzprefixset,string> whereClauseMethod)
		{
			return new StringBuilder("update xyzprefixset set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("Name=").Append(this.Name.ToMySqlField()).Append(",")
				.Append("Description=").Append(this.Description.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<xyzprefixset,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<xyzprefixset,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from xyzprefixset 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
