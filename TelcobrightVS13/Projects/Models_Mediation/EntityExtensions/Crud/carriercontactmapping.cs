using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class carriercontactmapping:ICacheble<carriercontactmapping>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{idCarrierContactMapping.ToMySqlField()},
				{Name.ToMySqlField()},
				{Designation.ToMySqlField()},
				{Department.ToMySqlField()},
				{OfficePhone.ToMySqlField()},
				{Mobile.ToMySqlField()},
				{email.ToMySqlField()},
				{idCarrier.ToMySqlField()},
				{date1.ToMySqlField()},
				{field1.ToMySqlField()},
				{field2.ToMySqlField()},
				{field3.ToMySqlField()},
				{field4.ToMySqlField()},
				{field5.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<carriercontactmapping,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<carriercontactmapping,string> whereClauseMethod)
		{
			return $@"update carriercontactmapping set 
				idCarrierContactMapping={idCarrierContactMapping.ToMySqlField()+" "},
				Name={Name.ToMySqlField()+" "},
				Designation={Designation.ToMySqlField()+" "},
				Department={Department.ToMySqlField()+" "},
				OfficePhone={OfficePhone.ToMySqlField()+" "},
				Mobile={Mobile.ToMySqlField()+" "},
				email={email.ToMySqlField()+" "},
				idCarrier={idCarrier.ToMySqlField()+" "},
				date1={date1.ToMySqlField()+" "},
				field1={field1.ToMySqlField()+" "},
				field2={field2.ToMySqlField()+" "},
				field3={field3.ToMySqlField()+" "},
				field4={field4.ToMySqlField()+" "},
				field5={field5.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<carriercontactmapping,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<carriercontactmapping,string> whereClauseMethod)
		{
			return $@"delete from carriercontactmapping 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
