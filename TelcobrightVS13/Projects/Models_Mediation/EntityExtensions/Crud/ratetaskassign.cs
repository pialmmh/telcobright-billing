using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class ratetaskassign:ICacheble<ratetaskassign>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{Prefix.ToMySqlField()},
				{description.ToMySqlField()},
				{rateamount.ToMySqlField()},
				{WeekDayStart.ToMySqlField()},
				{WeekDayEnd.ToMySqlField()},
				{starttime.ToMySqlField()},
				{endtime.ToMySqlField()},
				{Resolution.ToMySqlField()},
				{MinDurationSec.ToMySqlField()},
				{SurchargeTime.ToMySqlField()},
				{SurchargeAmount.ToMySqlField()},
				{idrateplan.ToMySqlField()},
				{CountryCode.ToMySqlField()},
				{date1.ToMySqlField()},
				{field1.ToMySqlField()},
				{field2.ToMySqlField()},
				{field3.ToMySqlField()},
				{field4.ToMySqlField()},
				{field5.ToMySqlField()},
				{startdate.ToMySqlField()},
				{enddate.ToMySqlField()},
				{Inactive.ToMySqlField()},
				{RouteDisabled.ToMySqlField()},
				{Type.ToMySqlField()},
				{Currency.ToMySqlField()},
				{OtherAmount1.ToMySqlField()},
				{OtherAmount2.ToMySqlField()},
				{OtherAmount3.ToMySqlField()},
				{OtherAmount4.ToMySqlField()},
				{OtherAmount5.ToMySqlField()},
				{OtherAmount6.ToMySqlField()},
				{OtherAmount7.ToMySqlField()},
				{OtherAmount8.ToMySqlField()},
				{OtherAmount9.ToMySqlField()},
				{OtherAmount10.ToMySqlField()},
				{TimeZoneOffsetSec.ToMySqlField()},
				{RatePosition.ToMySqlField()},
				{IgwPercentageIn.ToMySqlField()},
				{ConflictingRateIds.ToMySqlField()},
				{ChangedByTaskId.ToMySqlField()},
				{ChangedOn.ToMySqlField()},
				{Status.ToMySqlField()},
				{idPreviousRate.ToMySqlField()},
				{EndPreviousRate.ToMySqlField()},
				{Category.ToMySqlField()},
				{SubCategory.ToMySqlField()},
				{changecommitted.ToMySqlField()},
				{OverlappingRates.ToMySqlField()},
				{ConflictingRates.ToMySqlField()},
				{AffectedRates.ToMySqlField()},
				{PartitionDate.ToMySqlField()},
				{Comment1.ToMySqlField()},
				{Comment2.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<ratetaskassign,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<ratetaskassign,string> whereClauseMethod)
		{
			return $@"update ratetaskassign set 
				id={id.ToMySqlField()+" "},
				Prefix={Prefix.ToMySqlField()+" "},
				description={description.ToMySqlField()+" "},
				rateamount={rateamount.ToMySqlField()+" "},
				WeekDayStart={WeekDayStart.ToMySqlField()+" "},
				WeekDayEnd={WeekDayEnd.ToMySqlField()+" "},
				starttime={starttime.ToMySqlField()+" "},
				endtime={endtime.ToMySqlField()+" "},
				Resolution={Resolution.ToMySqlField()+" "},
				MinDurationSec={MinDurationSec.ToMySqlField()+" "},
				SurchargeTime={SurchargeTime.ToMySqlField()+" "},
				SurchargeAmount={SurchargeAmount.ToMySqlField()+" "},
				idrateplan={idrateplan.ToMySqlField()+" "},
				CountryCode={CountryCode.ToMySqlField()+" "},
				date1={date1.ToMySqlField()+" "},
				field1={field1.ToMySqlField()+" "},
				field2={field2.ToMySqlField()+" "},
				field3={field3.ToMySqlField()+" "},
				field4={field4.ToMySqlField()+" "},
				field5={field5.ToMySqlField()+" "},
				startdate={startdate.ToMySqlField()+" "},
				enddate={enddate.ToMySqlField()+" "},
				Inactive={Inactive.ToMySqlField()+" "},
				RouteDisabled={RouteDisabled.ToMySqlField()+" "},
				Type={Type.ToMySqlField()+" "},
				Currency={Currency.ToMySqlField()+" "},
				OtherAmount1={OtherAmount1.ToMySqlField()+" "},
				OtherAmount2={OtherAmount2.ToMySqlField()+" "},
				OtherAmount3={OtherAmount3.ToMySqlField()+" "},
				OtherAmount4={OtherAmount4.ToMySqlField()+" "},
				OtherAmount5={OtherAmount5.ToMySqlField()+" "},
				OtherAmount6={OtherAmount6.ToMySqlField()+" "},
				OtherAmount7={OtherAmount7.ToMySqlField()+" "},
				OtherAmount8={OtherAmount8.ToMySqlField()+" "},
				OtherAmount9={OtherAmount9.ToMySqlField()+" "},
				OtherAmount10={OtherAmount10.ToMySqlField()+" "},
				TimeZoneOffsetSec={TimeZoneOffsetSec.ToMySqlField()+" "},
				RatePosition={RatePosition.ToMySqlField()+" "},
				IgwPercentageIn={IgwPercentageIn.ToMySqlField()+" "},
				ConflictingRateIds={ConflictingRateIds.ToMySqlField()+" "},
				ChangedByTaskId={ChangedByTaskId.ToMySqlField()+" "},
				ChangedOn={ChangedOn.ToMySqlField()+" "},
				Status={Status.ToMySqlField()+" "},
				idPreviousRate={idPreviousRate.ToMySqlField()+" "},
				EndPreviousRate={EndPreviousRate.ToMySqlField()+" "},
				Category={Category.ToMySqlField()+" "},
				SubCategory={SubCategory.ToMySqlField()+" "},
				changecommitted={changecommitted.ToMySqlField()+" "},
				OverlappingRates={OverlappingRates.ToMySqlField()+" "},
				ConflictingRates={ConflictingRates.ToMySqlField()+" "},
				AffectedRates={AffectedRates.ToMySqlField()+" "},
				PartitionDate={PartitionDate.ToMySqlField()+" "},
				Comment1={Comment1.ToMySqlField()+" "},
				Comment2={Comment2.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<ratetaskassign,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<ratetaskassign,string> whereClauseMethod)
		{
			return $@"delete from ratetaskassign 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
