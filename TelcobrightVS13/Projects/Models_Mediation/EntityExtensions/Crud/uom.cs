using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class uom:ICacheble<uom>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.UOM_ID.ToMySqlField()).Append(",")
				.Append(this.UOM_TYPE_ID.ToMySqlField()).Append(",")
				.Append(this.ABBREVIATION.ToMySqlField()).Append(",")
				.Append(this.DESCRIPTION.ToMySqlField()).Append(",")
				.Append(this.LAST_UPDATED_STAMP.ToMySqlField()).Append(",")
				.Append(this.LAST_UPDATED_TX_STAMP.ToMySqlField()).Append(",")
				.Append(this.CREATED_STAMP.ToMySqlField()).Append(",")
				.Append(this.CREATED_TX_STAMP.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<uom,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<uom,string> whereClauseMethod)
		{
			return new StringBuilder("update uom set ")
				.Append("UOM_ID=").Append(this.UOM_ID.ToMySqlField()).Append(",")
				.Append("UOM_TYPE_ID=").Append(this.UOM_TYPE_ID.ToMySqlField()).Append(",")
				.Append("ABBREVIATION=").Append(this.ABBREVIATION.ToMySqlField()).Append(",")
				.Append("DESCRIPTION=").Append(this.DESCRIPTION.ToMySqlField()).Append(",")
				.Append("LAST_UPDATED_STAMP=").Append(this.LAST_UPDATED_STAMP.ToMySqlField()).Append(",")
				.Append("LAST_UPDATED_TX_STAMP=").Append(this.LAST_UPDATED_TX_STAMP.ToMySqlField()).Append(",")
				.Append("CREATED_STAMP=").Append(this.CREATED_STAMP.ToMySqlField()).Append(",")
				.Append("CREATED_TX_STAMP=").Append(this.CREATED_TX_STAMP.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<uom,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<uom,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from uom 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
