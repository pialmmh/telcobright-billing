using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class reporttemplate:ICacheble<reporttemplate>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.Templatename.ToMySqlField()).Append(",")
				.Append(this.PageUrl.ToMySqlField()).Append(",")
				.Append(this.ControlValues.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<reporttemplate,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<reporttemplate,string> whereClauseMethod)
		{
			return new StringBuilder("update reporttemplate set ")
				.Append("Templatename=").Append(this.Templatename.ToMySqlField()).Append(",")
				.Append("PageUrl=").Append(this.PageUrl.ToMySqlField()).Append(",")
				.Append("ControlValues=").Append(this.ControlValues.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<reporttemplate,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<reporttemplate,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from reporttemplate 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
