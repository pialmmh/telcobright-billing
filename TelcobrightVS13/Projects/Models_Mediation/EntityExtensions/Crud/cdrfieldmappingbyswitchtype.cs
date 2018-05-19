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
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.Id.ToMySqlField()).Append(",")
				.Append(this.FieldNumber.ToMySqlField()).Append(",")
				.Append(this.idCdrFormat.ToMySqlField()).Append(",")
				.Append(this.FieldPositionInCDRRow.ToMySqlField()).Append(",")
				.Append(this.BinByteOffset.ToMySqlField()).Append(",")
				.Append(this.BinByteLen.ToMySqlField()).Append(",")
				.Append(this.BinByteType.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<cdrfieldmappingbyswitchtype,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<cdrfieldmappingbyswitchtype,string> whereClauseMethod)
		{
			return new StringBuilder("update cdrfieldmappingbyswitchtype set ")
				.Append("Id=").Append(this.Id.ToMySqlField()).Append(",")
				.Append("FieldNumber=").Append(this.FieldNumber.ToMySqlField()).Append(",")
				.Append("idCdrFormat=").Append(this.idCdrFormat.ToMySqlField()).Append(",")
				.Append("FieldPositionInCDRRow=").Append(this.FieldPositionInCDRRow.ToMySqlField()).Append(",")
				.Append("BinByteOffset=").Append(this.BinByteOffset.ToMySqlField()).Append(",")
				.Append("BinByteLen=").Append(this.BinByteLen.ToMySqlField()).Append(",")
				.Append("BinByteType=").Append(this.BinByteType.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<cdrfieldmappingbyswitchtype,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<cdrfieldmappingbyswitchtype,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from cdrfieldmappingbyswitchtype 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
