using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class cdrfieldlist:ICacheble<cdrfieldlist>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.fieldnumber.ToMySqlField()).Append(",")
				.Append(this.FieldName.ToMySqlField()).Append(",")
				.Append(this.IsNumeric.ToMySqlField()).Append(",")
				.Append(this.IsDateTime.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<cdrfieldlist,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<cdrfieldlist,string> whereClauseMethod)
		{
			return new StringBuilder("update cdrfieldlist set ")
				.Append("fieldnumber=").Append(this.fieldnumber.ToMySqlField()).Append(",")
				.Append("FieldName=").Append(this.FieldName.ToMySqlField()).Append(",")
				.Append("IsNumeric=").Append(this.IsNumeric.ToMySqlField()).Append(",")
				.Append("IsDateTime=").Append(this.IsDateTime.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<cdrfieldlist,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<cdrfieldlist,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from cdrfieldlist 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
