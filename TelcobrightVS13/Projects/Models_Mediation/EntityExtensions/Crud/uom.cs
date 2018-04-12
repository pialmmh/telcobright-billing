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
		public string GetExtInsertValues()
		{
			return $@"(
				{UOM_ID.ToMySqlField()},
				{UOM_TYPE_ID.ToMySqlField()},
				{ABBREVIATION.ToMySqlField()},
				{DESCRIPTION.ToMySqlField()},
				{LAST_UPDATED_STAMP.ToMySqlField()},
				{LAST_UPDATED_TX_STAMP.ToMySqlField()},
				{CREATED_STAMP.ToMySqlField()},
				{CREATED_TX_STAMP.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<uom,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<uom,string> whereClauseMethod)
		{
			return $@"update uom set 
				UOM_ID={UOM_ID.ToMySqlField()+" "},
				UOM_TYPE_ID={UOM_TYPE_ID.ToMySqlField()+" "},
				ABBREVIATION={ABBREVIATION.ToMySqlField()+" "},
				DESCRIPTION={DESCRIPTION.ToMySqlField()+" "},
				LAST_UPDATED_STAMP={LAST_UPDATED_STAMP.ToMySqlField()+" "},
				LAST_UPDATED_TX_STAMP={LAST_UPDATED_TX_STAMP.ToMySqlField()+" "},
				CREATED_STAMP={CREATED_STAMP.ToMySqlField()+" "},
				CREATED_TX_STAMP={CREATED_TX_STAMP.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<uom,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<uom,string> whereClauseMethod)
		{
			return $@"delete from uom 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
