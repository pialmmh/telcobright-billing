using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class rateplan:ICacheble<rateplan>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{Type.ToMySqlField()},
				{RatePlanName.ToMySqlField()},
				{Description.ToMySqlField()},
				{date1.ToMySqlField()},
				{field1.ToMySqlField()},
				{field2.ToMySqlField()},
				{field3.ToMySqlField()},
				{field4.ToMySqlField()},
				{field5.ToMySqlField()},
				{TimeZone.ToMySqlField()},
				{idCarrier.ToMySqlField()},
				{Currency.ToMySqlField()},
				{codedeletedate.ToMySqlField()},
				{ChangeCommitted.ToMySqlField()},
				{Resolution.ToMySqlField()},
				{minDurationSec.ToMySqlField()},
				{SurchargeTime.ToMySqlField()},
				{SurchargeAmount.ToMySqlField()},
				{Category.ToMySqlField()},
				{SubCategory.ToMySqlField()},
				{BillingSpan.ToMySqlField()},
				{RateAmountRoundupDecimal.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<rateplan,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<rateplan,string> whereClauseMethod)
		{
			return $@"update rateplan set 
				id={id.ToMySqlField()+" "},
				Type={Type.ToMySqlField()+" "},
				RatePlanName={RatePlanName.ToMySqlField()+" "},
				Description={Description.ToMySqlField()+" "},
				date1={date1.ToMySqlField()+" "},
				field1={field1.ToMySqlField()+" "},
				field2={field2.ToMySqlField()+" "},
				field3={field3.ToMySqlField()+" "},
				field4={field4.ToMySqlField()+" "},
				field5={field5.ToMySqlField()+" "},
				TimeZone={TimeZone.ToMySqlField()+" "},
				idCarrier={idCarrier.ToMySqlField()+" "},
				Currency={Currency.ToMySqlField()+" "},
				codedeletedate={codedeletedate.ToMySqlField()+" "},
				ChangeCommitted={ChangeCommitted.ToMySqlField()+" "},
				Resolution={Resolution.ToMySqlField()+" "},
				minDurationSec={minDurationSec.ToMySqlField()+" "},
				SurchargeTime={SurchargeTime.ToMySqlField()+" "},
				SurchargeAmount={SurchargeAmount.ToMySqlField()+" "},
				Category={Category.ToMySqlField()+" "},
				SubCategory={SubCategory.ToMySqlField()+" "},
				BillingSpan={BillingSpan.ToMySqlField()+" "},
				RateAmountRoundupDecimal={RateAmountRoundupDecimal.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<rateplan,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<rateplan,string> whereClauseMethod)
		{
			return $@"delete from rateplan 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
