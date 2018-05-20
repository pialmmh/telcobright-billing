using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumcreditrule:ICacheble<enumcreditrule>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.RuleName.ToMySqlField()).Append(",")
				.Append(this.Description.ToMySqlField()).Append(",")
				.Append(this.json.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<enumcreditrule,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<enumcreditrule,string> whereClauseMethod)
		{
			return new StringBuilder("update enumcreditrule set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("RuleName=").Append(this.RuleName.ToMySqlField()).Append(",")
				.Append("Description=").Append(this.Description.ToMySqlField()).Append(",")
				.Append("json=").Append(this.json.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<enumcreditrule,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<enumcreditrule,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from enumcreditrule 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
