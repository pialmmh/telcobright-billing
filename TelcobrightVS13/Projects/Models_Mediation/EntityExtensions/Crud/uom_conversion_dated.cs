using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class uom_conversion_dated:ICacheble<uom_conversion_dated>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.UOM_ID.ToMySqlField()).Append(",")
				.Append(this.UOM_ID_TO.ToMySqlField()).Append(",")
				.Append(this.FROM_DATE.ToMySqlField()).Append(",")
				.Append(this.THRU_DATE.ToMySqlField()).Append(",")
				.Append(this.CONVERSION_FACTOR.ToMySqlField()).Append(",")
				.Append(this.CUSTOM_METHOD_ID.ToMySqlField()).Append(",")
				.Append(this.DECIMAL_SCALE.ToMySqlField()).Append(",")
				.Append(this.ROUNDING_MODE.ToMySqlField()).Append(",")
				.Append(this.PURPOSE_ENUM_ID.ToMySqlField()).Append(",")
				.Append(this.LAST_UPDATED_STAMP.ToMySqlField()).Append(",")
				.Append(this.LAST_UPDATED_TX_STAMP.ToMySqlField()).Append(",")
				.Append(this.CREATED_STAMP.ToMySqlField()).Append(",")
				.Append(this.CREATED_TX_STAMP.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<uom_conversion_dated,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<uom_conversion_dated,string> whereClauseMethod)
		{
			return new StringBuilder("update uom_conversion_dated set ")
				.Append("UOM_ID=").Append(this.UOM_ID.ToMySqlField()).Append(",")
				.Append("UOM_ID_TO=").Append(this.UOM_ID_TO.ToMySqlField()).Append(",")
				.Append("FROM_DATE=").Append(this.FROM_DATE.ToMySqlField()).Append(",")
				.Append("THRU_DATE=").Append(this.THRU_DATE.ToMySqlField()).Append(",")
				.Append("CONVERSION_FACTOR=").Append(this.CONVERSION_FACTOR.ToMySqlField()).Append(",")
				.Append("CUSTOM_METHOD_ID=").Append(this.CUSTOM_METHOD_ID.ToMySqlField()).Append(",")
				.Append("DECIMAL_SCALE=").Append(this.DECIMAL_SCALE.ToMySqlField()).Append(",")
				.Append("ROUNDING_MODE=").Append(this.ROUNDING_MODE.ToMySqlField()).Append(",")
				.Append("PURPOSE_ENUM_ID=").Append(this.PURPOSE_ENUM_ID.ToMySqlField()).Append(",")
				.Append("LAST_UPDATED_STAMP=").Append(this.LAST_UPDATED_STAMP.ToMySqlField()).Append(",")
				.Append("LAST_UPDATED_TX_STAMP=").Append(this.LAST_UPDATED_TX_STAMP.ToMySqlField()).Append(",")
				.Append("CREATED_STAMP=").Append(this.CREATED_STAMP.ToMySqlField()).Append(",")
				.Append("CREATED_TX_STAMP=").Append(this.CREATED_TX_STAMP.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<uom_conversion_dated,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<uom_conversion_dated,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from uom_conversion_dated 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
