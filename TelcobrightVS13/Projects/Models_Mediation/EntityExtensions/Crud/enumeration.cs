using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumeration:ICacheble<enumeration>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.ENUM_ID.ToMySqlField()).Append(",")
				.Append(this.ENUM_TYPE_ID.ToMySqlField()).Append(",")
				.Append(this.ENUM_CODE.ToMySqlField()).Append(",")
				.Append(this.SEQUENCE_ID.ToMySqlField()).Append(",")
				.Append(this.DESCRIPTION.ToMySqlField()).Append(",")
				.Append(this.LAST_UPDATED_STAMP.ToMySqlField()).Append(",")
				.Append(this.LAST_UPDATED_TX_STAMP.ToMySqlField()).Append(",")
				.Append(this.CREATED_STAMP.ToMySqlField()).Append(",")
				.Append(this.CREATED_TX_STAMP.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<enumeration,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<enumeration,string> whereClauseMethod)
		{
			return new StringBuilder("update enumeration set ")
				.Append("ENUM_ID=").Append(this.ENUM_ID.ToMySqlField()).Append(",")
				.Append("ENUM_TYPE_ID=").Append(this.ENUM_TYPE_ID.ToMySqlField()).Append(",")
				.Append("ENUM_CODE=").Append(this.ENUM_CODE.ToMySqlField()).Append(",")
				.Append("SEQUENCE_ID=").Append(this.SEQUENCE_ID.ToMySqlField()).Append(",")
				.Append("DESCRIPTION=").Append(this.DESCRIPTION.ToMySqlField()).Append(",")
				.Append("LAST_UPDATED_STAMP=").Append(this.LAST_UPDATED_STAMP.ToMySqlField()).Append(",")
				.Append("LAST_UPDATED_TX_STAMP=").Append(this.LAST_UPDATED_TX_STAMP.ToMySqlField()).Append(",")
				.Append("CREATED_STAMP=").Append(this.CREATED_STAMP.ToMySqlField()).Append(",")
				.Append("CREATED_TX_STAMP=").Append(this.CREATED_TX_STAMP.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<enumeration,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<enumeration,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from enumeration 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
