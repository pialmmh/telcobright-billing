using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumpostpaidinvoicestatu:ICacheble<enumpostpaidinvoicestatu>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.Type.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<enumpostpaidinvoicestatu,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<enumpostpaidinvoicestatu,string> whereClauseMethod)
		{
			return new StringBuilder("update enumpostpaidinvoicestatus set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("Type=").Append(this.Type.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<enumpostpaidinvoicestatu,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<enumpostpaidinvoicestatu,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from enumpostpaidinvoicestatus 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
