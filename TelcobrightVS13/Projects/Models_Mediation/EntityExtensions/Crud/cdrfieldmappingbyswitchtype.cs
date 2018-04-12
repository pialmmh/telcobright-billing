using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class cdrfieldmappingbyswitchtype:ICacheble<cdrfieldmappingbyswitchtype>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{Id.ToMySqlField()},
				{FieldNumber.ToMySqlField()},
				{idCdrFormat.ToMySqlField()},
				{FieldPositionInCDRRow.ToMySqlField()},
				{BinByteOffset.ToMySqlField()},
				{BinByteLen.ToMySqlField()},
				{BinByteType.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<cdrfieldmappingbyswitchtype,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<cdrfieldmappingbyswitchtype,string> whereClauseMethod)
		{
			return $@"update cdrfieldmappingbyswitchtype set 
				Id={Id.ToMySqlField()+" "},
				FieldNumber={FieldNumber.ToMySqlField()+" "},
				idCdrFormat={idCdrFormat.ToMySqlField()+" "},
				FieldPositionInCDRRow={FieldPositionInCDRRow.ToMySqlField()+" "},
				BinByteOffset={BinByteOffset.ToMySqlField()+" "},
				BinByteLen={BinByteLen.ToMySqlField()+" "},
				BinByteType={BinByteType.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<cdrfieldmappingbyswitchtype,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<cdrfieldmappingbyswitchtype,string> whereClauseMethod)
		{
			return $@"delete from cdrfieldmappingbyswitchtype 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
