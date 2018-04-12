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
		public string GetExtInsertValues()
		{
			return $@"(
				{UOM_ID.ToMySqlField()},
				{UOM_ID_TO.ToMySqlField()},
				{FROM_DATE.ToMySqlField()},
				{THRU_DATE.ToMySqlField()},
				{CONVERSION_FACTOR.ToMySqlField()},
				{CUSTOM_METHOD_ID.ToMySqlField()},
				{DECIMAL_SCALE.ToMySqlField()},
				{ROUNDING_MODE.ToMySqlField()},
				{PURPOSE_ENUM_ID.ToMySqlField()},
				{LAST_UPDATED_STAMP.ToMySqlField()},
				{LAST_UPDATED_TX_STAMP.ToMySqlField()},
				{CREATED_STAMP.ToMySqlField()},
				{CREATED_TX_STAMP.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<uom_conversion_dated,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<uom_conversion_dated,string> whereClauseMethod)
		{
			return $@"update uom_conversion_dated set 
				UOM_ID={UOM_ID.ToMySqlField()+" "},
				UOM_ID_TO={UOM_ID_TO.ToMySqlField()+" "},
				FROM_DATE={FROM_DATE.ToMySqlField()+" "},
				THRU_DATE={THRU_DATE.ToMySqlField()+" "},
				CONVERSION_FACTOR={CONVERSION_FACTOR.ToMySqlField()+" "},
				CUSTOM_METHOD_ID={CUSTOM_METHOD_ID.ToMySqlField()+" "},
				DECIMAL_SCALE={DECIMAL_SCALE.ToMySqlField()+" "},
				ROUNDING_MODE={ROUNDING_MODE.ToMySqlField()+" "},
				PURPOSE_ENUM_ID={PURPOSE_ENUM_ID.ToMySqlField()+" "},
				LAST_UPDATED_STAMP={LAST_UPDATED_STAMP.ToMySqlField()+" "},
				LAST_UPDATED_TX_STAMP={LAST_UPDATED_TX_STAMP.ToMySqlField()+" "},
				CREATED_STAMP={CREATED_STAMP.ToMySqlField()+" "},
				CREATED_TX_STAMP={CREATED_TX_STAMP.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<uom_conversion_dated,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<uom_conversion_dated,string> whereClauseMethod)
		{
			return $@"delete from uom_conversion_dated 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
