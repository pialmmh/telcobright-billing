using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumdatedassignment:ICacheble<enumdatedassignment>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.Type.ToMySqlField()).Append(",")
				.Append(this.Tuple.ToMySqlField()).Append(",")
				.Append(this.description.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<enumdatedassignment,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<enumdatedassignment,string> whereClauseMethod)
		{
			return new StringBuilder("update enumdatedassignment set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("Type=").Append(this.Type.ToMySqlField()).Append(",")
				.Append("Tuple=").Append(this.Tuple.ToMySqlField()).Append(",")
				.Append("description=").Append(this.description.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<enumdatedassignment,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<enumdatedassignment,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from enumdatedassignment 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
