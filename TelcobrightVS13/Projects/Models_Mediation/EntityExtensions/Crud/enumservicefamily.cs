using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumservicefamily:ICacheble<enumservicefamily>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.ServiceName.ToMySqlField()).Append(",")
				.Append(this.Description.ToMySqlField()).Append(",")
				.Append(this.PartnerAssignNotNeeded.ToMySqlField()).Append(",")
				.Append(this.ServiceCategory.ToMySqlField()).Append(",")
				.Append(this.AccountingId.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<enumservicefamily,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<enumservicefamily,string> whereClauseMethod)
		{
			return new StringBuilder("update enumservicefamily set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("ServiceName=").Append(this.ServiceName.ToMySqlField()).Append(",")
				.Append("Description=").Append(this.Description.ToMySqlField()).Append(",")
				.Append("PartnerAssignNotNeeded=").Append(this.PartnerAssignNotNeeded.ToMySqlField()).Append(",")
				.Append("ServiceCategory=").Append(this.ServiceCategory.ToMySqlField()).Append(",")
				.Append("AccountingId=").Append(this.AccountingId.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<enumservicefamily,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<enumservicefamily,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from enumservicefamily 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
