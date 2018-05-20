using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumaccountingclass:ICacheble<enumaccountingclass>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.Name.ToMySqlField()).Append(",")
				.Append(this.NormalBalance.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<enumaccountingclass,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<enumaccountingclass,string> whereClauseMethod)
		{
			return new StringBuilder("update enumaccountingclass set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("Name=").Append(this.Name.ToMySqlField()).Append(",")
				.Append("NormalBalance=").Append(this.NormalBalance.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<enumaccountingclass,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<enumaccountingclass,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from enumaccountingclass 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
