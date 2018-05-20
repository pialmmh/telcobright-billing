using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class causecode:ICacheble<causecode>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.idSwitch.ToMySqlField()).Append(",")
				.Append(this.CC.ToMySqlField()).Append(",")
				.Append(this.Description.ToMySqlField()).Append(",")
				.Append(this.CallCompleteIndicator.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<causecode,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<causecode,string> whereClauseMethod)
		{
			return new StringBuilder("update causecode set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("idSwitch=").Append(this.idSwitch.ToMySqlField()).Append(",")
				.Append("CC=").Append(this.CC.ToMySqlField()).Append(",")
				.Append("Description=").Append(this.Description.ToMySqlField()).Append(",")
				.Append("CallCompleteIndicator=").Append(this.CallCompleteIndicator.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<causecode,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<causecode,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from causecode 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
