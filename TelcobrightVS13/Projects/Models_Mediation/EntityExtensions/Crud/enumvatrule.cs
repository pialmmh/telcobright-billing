using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumvatrule:ICacheble<enumvatrule>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.Type.ToMySqlField()).Append(",")
				.Append(this.description.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<enumvatrule,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<enumvatrule,string> whereClauseMethod)
		{
			return new StringBuilder("update enumvatrule set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("Type=").Append(this.Type.ToMySqlField()).Append(",")
				.Append("description=").Append(this.description.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<enumvatrule,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<enumvatrule,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from enumvatrule 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
