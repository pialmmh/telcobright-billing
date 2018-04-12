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
		public string GetExtInsertValues()
		{
			return $@"(
				{idCustomer.ToMySqlField()},
				{CustomerName.ToMySqlField()},
				{idOperatorType.ToMySqlField()},
				{databasename.ToMySqlField()},
				{databasetype.ToMySqlField()},
				{user.ToMySqlField()},
				{pass.ToMySqlField()},
				{ServerNameOrIP.ToMySqlField()},
				{IBServerNameOrIP.ToMySqlField()},
				{IBdatabasename.ToMySqlField()},
				{IBdatabasetype.ToMySqlField()},
				{IBuser.ToMySqlField()},
				{IBpass.ToMySqlField()},
				{TransactionSizeForCDRLoading.ToMySqlField()},
				{NativeTimeZone.ToMySqlField()},
				{IgwPrefix.ToMySqlField()},
				{RateDictionaryMaxRecords.ToMySqlField()},
				{MinMSForIntlOut.ToMySqlField()},
				{RawCdrKeepDurationDays.ToMySqlField()},
				{SummaryKeepDurationDays.ToMySqlField()},
				{AutoDeleteOldData.ToMySqlField()},
				{AutoDeleteStartHour.ToMySqlField()},
				{AutoDeleteEndHour.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<telcobrightpartner,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<telcobrightpartner,string> whereClauseMethod)
		{
			return $@"update telcobrightpartner set 
				idCustomer={idCustomer.ToMySqlField()+" "},
				CustomerName={CustomerName.ToMySqlField()+" "},
				idOperatorType={idOperatorType.ToMySqlField()+" "},
				databasename={databasename.ToMySqlField()+" "},
				databasetype={databasetype.ToMySqlField()+" "},
				user={user.ToMySqlField()+" "},
				pass={pass.ToMySqlField()+" "},
				ServerNameOrIP={ServerNameOrIP.ToMySqlField()+" "},
				IBServerNameOrIP={IBServerNameOrIP.ToMySqlField()+" "},
				IBdatabasename={IBdatabasename.ToMySqlField()+" "},
				IBdatabasetype={IBdatabasetype.ToMySqlField()+" "},
				IBuser={IBuser.ToMySqlField()+" "},
				IBpass={IBpass.ToMySqlField()+" "},
				TransactionSizeForCDRLoading={TransactionSizeForCDRLoading.ToMySqlField()+" "},
				NativeTimeZone={NativeTimeZone.ToMySqlField()+" "},
				IgwPrefix={IgwPrefix.ToMySqlField()+" "},
				RateDictionaryMaxRecords={RateDictionaryMaxRecords.ToMySqlField()+" "},
				MinMSForIntlOut={MinMSForIntlOut.ToMySqlField()+" "},
				RawCdrKeepDurationDays={RawCdrKeepDurationDays.ToMySqlField()+" "},
				SummaryKeepDurationDays={SummaryKeepDurationDays.ToMySqlField()+" "},
				AutoDeleteOldData={AutoDeleteOldData.ToMySqlField()+" "},
				AutoDeleteStartHour={AutoDeleteStartHour.ToMySqlField()+" "},
				AutoDeleteEndHour={AutoDeleteEndHour.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<telcobrightpartner,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<telcobrightpartner,string> whereClauseMethod)
		{
			return $@"delete from telcobrightpartner 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
