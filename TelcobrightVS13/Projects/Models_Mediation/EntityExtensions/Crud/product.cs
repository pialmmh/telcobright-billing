using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class product:ICacheble<product>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.Prefix.ToMySqlField()).Append(",")
				.Append(this.Name.ToMySqlField()).Append(",")
				.Append(this.description.ToMySqlField()).Append(",")
				.Append(this.Category.ToMySqlField()).Append(",")
				.Append(this.SubCategory.ToMySqlField()).Append(",")
				.Append(this.ServiceFamily.ToMySqlField()).Append(",")
				.Append(this.AccountingId.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<product,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<product,string> whereClauseMethod)
		{
			return new StringBuilder("update product set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("Prefix=").Append(this.Prefix.ToMySqlField()).Append(",")
				.Append("Name=").Append(this.Name.ToMySqlField()).Append(",")
				.Append("description=").Append(this.description.ToMySqlField()).Append(",")
				.Append("Category=").Append(this.Category.ToMySqlField()).Append(",")
				.Append("SubCategory=").Append(this.SubCategory.ToMySqlField()).Append(",")
				.Append("ServiceFamily=").Append(this.ServiceFamily.ToMySqlField()).Append(",")
				.Append("AccountingId=").Append(this.AccountingId.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<product,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<product,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from product 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
