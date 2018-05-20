using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class rateassign:ICacheble<rateassign>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.Prefix.ToMySqlField()).Append(",")
				.Append(this.description.ToMySqlField()).Append(",")
				.Append(this.rateamount.ToMySqlField()).Append(",")
				.Append(this.WeekDayStart.ToMySqlField()).Append(",")
				.Append(this.WeekDayEnd.ToMySqlField()).Append(",")
				.Append(this.starttime.ToMySqlField()).Append(",")
				.Append(this.endtime.ToMySqlField()).Append(",")
				.Append(this.Resolution.ToMySqlField()).Append(",")
				.Append(this.MinDurationSec.ToMySqlField()).Append(",")
				.Append(this.SurchargeTime.ToMySqlField()).Append(",")
				.Append(this.SurchargeAmount.ToMySqlField()).Append(",")
				.Append(this.idrateplan.ToMySqlField()).Append(",")
				.Append(this.CountryCode.ToMySqlField()).Append(",")
				.Append(this.date1.ToMySqlField()).Append(",")
				.Append(this.field1.ToMySqlField()).Append(",")
				.Append(this.field2.ToMySqlField()).Append(",")
				.Append(this.field3.ToMySqlField()).Append(",")
				.Append(this.field4.ToMySqlField()).Append(",")
				.Append(this.field5.ToMySqlField()).Append(",")
				.Append(this.startdate.ToMySqlField()).Append(",")
				.Append(this.enddate.ToMySqlField()).Append(",")
				.Append(this.Inactive.ToMySqlField()).Append(",")
				.Append(this.RouteDisabled.ToMySqlField()).Append(",")
				.Append(this.Type.ToMySqlField()).Append(",")
				.Append(this.Currency.ToMySqlField()).Append(",")
				.Append(this.OtherAmount1.ToMySqlField()).Append(",")
				.Append(this.OtherAmount2.ToMySqlField()).Append(",")
				.Append(this.OtherAmount3.ToMySqlField()).Append(",")
				.Append(this.OtherAmount4.ToMySqlField()).Append(",")
				.Append(this.OtherAmount5.ToMySqlField()).Append(",")
				.Append(this.OtherAmount6.ToMySqlField()).Append(",")
				.Append(this.OtherAmount7.ToMySqlField()).Append(",")
				.Append(this.OtherAmount8.ToMySqlField()).Append(",")
				.Append(this.OtherAmount9.ToMySqlField()).Append(",")
				.Append(this.OtherAmount10.ToMySqlField()).Append(",")
				.Append(this.TimeZoneOffsetSec.ToMySqlField()).Append(",")
				.Append(this.RatePosition.ToMySqlField()).Append(",")
				.Append(this.IgwPercentageIn.ToMySqlField()).Append(",")
				.Append(this.ConflictingRateIds.ToMySqlField()).Append(",")
				.Append(this.ChangedByTaskId.ToMySqlField()).Append(",")
				.Append(this.ChangedOn.ToMySqlField()).Append(",")
				.Append(this.Status.ToMySqlField()).Append(",")
				.Append(this.idPreviousRate.ToMySqlField()).Append(",")
				.Append(this.EndPreviousRate.ToMySqlField()).Append(",")
				.Append(this.Category.ToMySqlField()).Append(",")
				.Append(this.SubCategory.ToMySqlField()).Append(",")
				.Append(this.ChangeCommitted.ToMySqlField()).Append(",")
				.Append(this.ConflictingRates.ToMySqlField()).Append(",")
				.Append(this.OverlappingRates.ToMySqlField()).Append(",")
				.Append(this.Comment1.ToMySqlField()).Append(",")
				.Append(this.Comment2.ToMySqlField()).Append(",")
				.Append(this.BillingParams.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<rateassign,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<rateassign,string> whereClauseMethod)
		{
			return new StringBuilder("update rateassign set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("Prefix=").Append(this.Prefix.ToMySqlField()).Append(",")
				.Append("description=").Append(this.description.ToMySqlField()).Append(",")
				.Append("rateamount=").Append(this.rateamount.ToMySqlField()).Append(",")
				.Append("WeekDayStart=").Append(this.WeekDayStart.ToMySqlField()).Append(",")
				.Append("WeekDayEnd=").Append(this.WeekDayEnd.ToMySqlField()).Append(",")
				.Append("starttime=").Append(this.starttime.ToMySqlField()).Append(",")
				.Append("endtime=").Append(this.endtime.ToMySqlField()).Append(",")
				.Append("Resolution=").Append(this.Resolution.ToMySqlField()).Append(",")
				.Append("MinDurationSec=").Append(this.MinDurationSec.ToMySqlField()).Append(",")
				.Append("SurchargeTime=").Append(this.SurchargeTime.ToMySqlField()).Append(",")
				.Append("SurchargeAmount=").Append(this.SurchargeAmount.ToMySqlField()).Append(",")
				.Append("idrateplan=").Append(this.idrateplan.ToMySqlField()).Append(",")
				.Append("CountryCode=").Append(this.CountryCode.ToMySqlField()).Append(",")
				.Append("date1=").Append(this.date1.ToMySqlField()).Append(",")
				.Append("field1=").Append(this.field1.ToMySqlField()).Append(",")
				.Append("field2=").Append(this.field2.ToMySqlField()).Append(",")
				.Append("field3=").Append(this.field3.ToMySqlField()).Append(",")
				.Append("field4=").Append(this.field4.ToMySqlField()).Append(",")
				.Append("field5=").Append(this.field5.ToMySqlField()).Append(",")
				.Append("startdate=").Append(this.startdate.ToMySqlField()).Append(",")
				.Append("enddate=").Append(this.enddate.ToMySqlField()).Append(",")
				.Append("Inactive=").Append(this.Inactive.ToMySqlField()).Append(",")
				.Append("RouteDisabled=").Append(this.RouteDisabled.ToMySqlField()).Append(",")
				.Append("Type=").Append(this.Type.ToMySqlField()).Append(",")
				.Append("Currency=").Append(this.Currency.ToMySqlField()).Append(",")
				.Append("OtherAmount1=").Append(this.OtherAmount1.ToMySqlField()).Append(",")
				.Append("OtherAmount2=").Append(this.OtherAmount2.ToMySqlField()).Append(",")
				.Append("OtherAmount3=").Append(this.OtherAmount3.ToMySqlField()).Append(",")
				.Append("OtherAmount4=").Append(this.OtherAmount4.ToMySqlField()).Append(",")
				.Append("OtherAmount5=").Append(this.OtherAmount5.ToMySqlField()).Append(",")
				.Append("OtherAmount6=").Append(this.OtherAmount6.ToMySqlField()).Append(",")
				.Append("OtherAmount7=").Append(this.OtherAmount7.ToMySqlField()).Append(",")
				.Append("OtherAmount8=").Append(this.OtherAmount8.ToMySqlField()).Append(",")
				.Append("OtherAmount9=").Append(this.OtherAmount9.ToMySqlField()).Append(",")
				.Append("OtherAmount10=").Append(this.OtherAmount10.ToMySqlField()).Append(",")
				.Append("TimeZoneOffsetSec=").Append(this.TimeZoneOffsetSec.ToMySqlField()).Append(",")
				.Append("RatePosition=").Append(this.RatePosition.ToMySqlField()).Append(",")
				.Append("IgwPercentageIn=").Append(this.IgwPercentageIn.ToMySqlField()).Append(",")
				.Append("ConflictingRateIds=").Append(this.ConflictingRateIds.ToMySqlField()).Append(",")
				.Append("ChangedByTaskId=").Append(this.ChangedByTaskId.ToMySqlField()).Append(",")
				.Append("ChangedOn=").Append(this.ChangedOn.ToMySqlField()).Append(",")
				.Append("Status=").Append(this.Status.ToMySqlField()).Append(",")
				.Append("idPreviousRate=").Append(this.idPreviousRate.ToMySqlField()).Append(",")
				.Append("EndPreviousRate=").Append(this.EndPreviousRate.ToMySqlField()).Append(",")
				.Append("Category=").Append(this.Category.ToMySqlField()).Append(",")
				.Append("SubCategory=").Append(this.SubCategory.ToMySqlField()).Append(",")
				.Append("ChangeCommitted=").Append(this.ChangeCommitted.ToMySqlField()).Append(",")
				.Append("ConflictingRates=").Append(this.ConflictingRates.ToMySqlField()).Append(",")
				.Append("OverlappingRates=").Append(this.OverlappingRates.ToMySqlField()).Append(",")
				.Append("Comment1=").Append(this.Comment1.ToMySqlField()).Append(",")
				.Append("Comment2=").Append(this.Comment2.ToMySqlField()).Append(",")
				.Append("BillingParams=").Append(this.BillingParams.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<rateassign,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<rateassign,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from rateassign 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
