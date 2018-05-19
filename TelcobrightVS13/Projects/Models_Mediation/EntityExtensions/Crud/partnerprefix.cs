using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class partnerprefix:ICacheble<partnerprefix>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.idPartner.ToMySqlField()).Append(",")
				.Append(this.PrefixType.ToMySqlField()).Append(",")
				.Append(this.Prefix.ToMySqlField()).Append(",")
				.Append(this.CommonTG.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<partnerprefix,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<partnerprefix,string> whereClauseMethod)
		{
			return new StringBuilder("update partnerprefix set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("idPartner=").Append(this.idPartner.ToMySqlField()).Append(",")
				.Append("PrefixType=").Append(this.PrefixType.ToMySqlField()).Append(",")
				.Append("Prefix=").Append(this.Prefix.ToMySqlField()).Append(",")
				.Append("CommonTG=").Append(this.CommonTG.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<partnerprefix,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<partnerprefix,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from partnerprefix 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
