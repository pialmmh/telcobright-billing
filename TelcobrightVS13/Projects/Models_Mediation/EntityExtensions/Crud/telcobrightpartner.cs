using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class telcobrightpartner:ICacheble<telcobrightpartner>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.idCustomer.ToMySqlField()).Append(",")
				.Append(this.CustomerName.ToMySqlField()).Append(",")
				.Append(this.idOperatorType.ToMySqlField()).Append(",")
				.Append(this.databasename.ToMySqlField()).Append(",")
				.Append(this.databasetype.ToMySqlField()).Append(",")
				.Append(this.user.ToMySqlField()).Append(",")
				.Append(this.pass.ToMySqlField()).Append(",")
				.Append(this.ServerNameOrIP.ToMySqlField()).Append(",")
				.Append(this.IBServerNameOrIP.ToMySqlField()).Append(",")
				.Append(this.IBdatabasename.ToMySqlField()).Append(",")
				.Append(this.IBdatabasetype.ToMySqlField()).Append(",")
				.Append(this.IBuser.ToMySqlField()).Append(",")
				.Append(this.IBpass.ToMySqlField()).Append(",")
				.Append(this.TransactionSizeForCDRLoading.ToMySqlField()).Append(",")
				.Append(this.NativeTimeZone.ToMySqlField()).Append(",")
				.Append(this.IgwPrefix.ToMySqlField()).Append(",")
				.Append(this.RateDictionaryMaxRecords.ToMySqlField()).Append(",")
				.Append(this.MinMSForIntlOut.ToMySqlField()).Append(",")
				.Append(this.RawCdrKeepDurationDays.ToMySqlField()).Append(",")
				.Append(this.SummaryKeepDurationDays.ToMySqlField()).Append(",")
				.Append(this.AutoDeleteOldData.ToMySqlField()).Append(",")
				.Append(this.AutoDeleteStartHour.ToMySqlField()).Append(",")
				.Append(this.AutoDeleteEndHour.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<telcobrightpartner,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<telcobrightpartner,string> whereClauseMethod)
		{
			return new StringBuilder("update telcobrightpartner set ")
				.Append("idCustomer=").Append(this.idCustomer.ToMySqlField()).Append(",")
				.Append("CustomerName=").Append(this.CustomerName.ToMySqlField()).Append(",")
				.Append("idOperatorType=").Append(this.idOperatorType.ToMySqlField()).Append(",")
				.Append("databasename=").Append(this.databasename.ToMySqlField()).Append(",")
				.Append("databasetype=").Append(this.databasetype.ToMySqlField()).Append(",")
				.Append("user=").Append(this.user.ToMySqlField()).Append(",")
				.Append("pass=").Append(this.pass.ToMySqlField()).Append(",")
				.Append("ServerNameOrIP=").Append(this.ServerNameOrIP.ToMySqlField()).Append(",")
				.Append("IBServerNameOrIP=").Append(this.IBServerNameOrIP.ToMySqlField()).Append(",")
				.Append("IBdatabasename=").Append(this.IBdatabasename.ToMySqlField()).Append(",")
				.Append("IBdatabasetype=").Append(this.IBdatabasetype.ToMySqlField()).Append(",")
				.Append("IBuser=").Append(this.IBuser.ToMySqlField()).Append(",")
				.Append("IBpass=").Append(this.IBpass.ToMySqlField()).Append(",")
				.Append("TransactionSizeForCDRLoading=").Append(this.TransactionSizeForCDRLoading.ToMySqlField()).Append(",")
				.Append("NativeTimeZone=").Append(this.NativeTimeZone.ToMySqlField()).Append(",")
				.Append("IgwPrefix=").Append(this.IgwPrefix.ToMySqlField()).Append(",")
				.Append("RateDictionaryMaxRecords=").Append(this.RateDictionaryMaxRecords.ToMySqlField()).Append(",")
				.Append("MinMSForIntlOut=").Append(this.MinMSForIntlOut.ToMySqlField()).Append(",")
				.Append("RawCdrKeepDurationDays=").Append(this.RawCdrKeepDurationDays.ToMySqlField()).Append(",")
				.Append("SummaryKeepDurationDays=").Append(this.SummaryKeepDurationDays.ToMySqlField()).Append(",")
				.Append("AutoDeleteOldData=").Append(this.AutoDeleteOldData.ToMySqlField()).Append(",")
				.Append("AutoDeleteStartHour=").Append(this.AutoDeleteStartHour.ToMySqlField()).Append(",")
				.Append("AutoDeleteEndHour=").Append(this.AutoDeleteEndHour.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<telcobrightpartner,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<telcobrightpartner,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from telcobrightpartner 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
