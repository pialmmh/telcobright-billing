using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class rateplanassign:ICacheble<rateplanassign>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{Type.ToMySqlField()},
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
				{RatePlanName.ToMySqlField()},
				{Resolution.ToMySqlField()},
				{MinDurationSec.ToMySqlField()},
				{SurchargeTime.ToMySqlField()},
				{SurchargeAmount.ToMySqlField()},
				{Category.ToMySqlField()},
				{SubCategory.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<rateplanassign,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<rateplanassign,string> whereClauseMethod)
		{
			return $@"update rateplanassign set 
				id={id.ToMySqlField()+" "},
				Type={Type.ToMySqlField()+" "},
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
				RatePlanName={RatePlanName.ToMySqlField()+" "},
				Resolution={Resolution.ToMySqlField()+" "},
				MinDurationSec={MinDurationSec.ToMySqlField()+" "},
				SurchargeTime={SurchargeTime.ToMySqlField()+" "},
				SurchargeAmount={SurchargeAmount.ToMySqlField()+" "},
				Category={Category.ToMySqlField()+" "},
				SubCategory={SubCategory.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<rateplanassign,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<rateplanassign,string> whereClauseMethod)
		{
			return $@"delete from rateplanassign 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
