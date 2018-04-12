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
		public string GetExtInsertValues()
		{
			return $@"(
				{UOM_ID.ToMySqlField()},
				{UOM_ID_TO.ToMySqlField()},
				{CONVERSION_FACTOR.ToMySqlField()},
				{CUSTOM_METHOD_ID.ToMySqlField()},
				{DECIMAL_SCALE.ToMySqlField()},
				{ROUNDING_MODE.ToMySqlField()},
				{LAST_UPDATED_STAMP.ToMySqlField()},
				{LAST_UPDATED_TX_STAMP.ToMySqlField()},
				{CREATED_STAMP.ToMySqlField()},
				{CREATED_TX_STAMP.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<uom_conversion,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<uom_conversion,string> whereClauseMethod)
		{
			return $@"update uom_conversion set 
				UOM_ID={UOM_ID.ToMySqlField()+" "},
				UOM_ID_TO={UOM_ID_TO.ToMySqlField()+" "},
				CONVERSION_FACTOR={CONVERSION_FACTOR.ToMySqlField()+" "},
				CUSTOM_METHOD_ID={CUSTOM_METHOD_ID.ToMySqlField()+" "},
				DECIMAL_SCALE={DECIMAL_SCALE.ToMySqlField()+" "},
				ROUNDING_MODE={ROUNDING_MODE.ToMySqlField()+" "},
				LAST_UPDATED_STAMP={LAST_UPDATED_STAMP.ToMySqlField()+" "},
				LAST_UPDATED_TX_STAMP={LAST_UPDATED_TX_STAMP.ToMySqlField()+" "},
				CREATED_STAMP={CREATED_STAMP.ToMySqlField()+" "},
				CREATED_TX_STAMP={CREATED_TX_STAMP.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<uom_conversion,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<uom_conversion,string> whereClauseMethod)
		{
			return $@"delete from uom_conversion 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
