using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class uom_conversion:ICacheble<uom_conversion>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.UOM_ID.ToMySqlField()).Append(",")
				.Append(this.UOM_ID_TO.ToMySqlField()).Append(",")
				.Append(this.CONVERSION_FACTOR.ToMySqlField()).Append(",")
				.Append(this.CUSTOM_METHOD_ID.ToMySqlField()).Append(",")
				.Append(this.DECIMAL_SCALE.ToMySqlField()).Append(",")
				.Append(this.ROUNDING_MODE.ToMySqlField()).Append(",")
				.Append(this.LAST_UPDATED_STAMP.ToMySqlField()).Append(",")
				.Append(this.LAST_UPDATED_TX_STAMP.ToMySqlField()).Append(",")
				.Append(this.CREATED_STAMP.ToMySqlField()).Append(",")
				.Append(this.CREATED_TX_STAMP.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<uom_conversion,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<uom_conversion,string> whereClauseMethod)
		{
			return new StringBuilder("update uom_conversion set ")
				.Append("UOM_ID=").Append(this.UOM_ID.ToMySqlField()).Append(",")
				.Append("UOM_ID_TO=").Append(this.UOM_ID_TO.ToMySqlField()).Append(",")
				.Append("CONVERSION_FACTOR=").Append(this.CONVERSION_FACTOR.ToMySqlField()).Append(",")
				.Append("CUSTOM_METHOD_ID=").Append(this.CUSTOM_METHOD_ID.ToMySqlField()).Append(",")
				.Append("DECIMAL_SCALE=").Append(this.DECIMAL_SCALE.ToMySqlField()).Append(",")
				.Append("ROUNDING_MODE=").Append(this.ROUNDING_MODE.ToMySqlField()).Append(",")
				.Append("LAST_UPDATED_STAMP=").Append(this.LAST_UPDATED_STAMP.ToMySqlField()).Append(",")
				.Append("LAST_UPDATED_TX_STAMP=").Append(this.LAST_UPDATED_TX_STAMP.ToMySqlField()).Append(",")
				.Append("CREATED_STAMP=").Append(this.CREATED_STAMP.ToMySqlField()).Append(",")
				.Append("CREATED_TX_STAMP=").Append(this.CREATED_TX_STAMP.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<uom_conversion,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<uom_conversion,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from uom_conversion 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
