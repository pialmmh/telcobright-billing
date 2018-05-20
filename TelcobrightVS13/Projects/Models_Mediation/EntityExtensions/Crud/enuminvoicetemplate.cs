using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enuminvoicetemplate:ICacheble<enuminvoicetemplate>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.TemplateName.ToMySqlField()).Append(",")
				.Append(this.Description.ToMySqlField()).Append(",")
				.Append(this.json.ToMySqlField()).Append(",")
				.Append(this.OtherInfo.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<enuminvoicetemplate,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<enuminvoicetemplate,string> whereClauseMethod)
		{
			return new StringBuilder("update enuminvoicetemplate set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("TemplateName=").Append(this.TemplateName.ToMySqlField()).Append(",")
				.Append("Description=").Append(this.Description.ToMySqlField()).Append(",")
				.Append("json=").Append(this.json.ToMySqlField()).Append(",")
				.Append("OtherInfo=").Append(this.OtherInfo.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<enuminvoicetemplate,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<enuminvoicetemplate,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from enuminvoicetemplate 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
