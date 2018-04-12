using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class route:ICacheble<route>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{idroute.ToMySqlField()},
				{RouteName.ToMySqlField()},
				{SwitchId.ToMySqlField()},
				{CommonRoute.ToMySqlField()},
				{idPartner.ToMySqlField()},
				{NationalOrInternational.ToMySqlField()},
				{Description.ToMySqlField()},
				{Status.ToMySqlField()},
				{date1.ToMySqlField()},
				{field1.ToMySqlField()},
				{field2.ToMySqlField()},
				{field3.ToMySqlField()},
				{field4.ToMySqlField()},
				{field5.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<route,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<route,string> whereClauseMethod)
		{
			return $@"update route set 
				idroute={idroute.ToMySqlField()+" "},
				RouteName={RouteName.ToMySqlField()+" "},
				SwitchId={SwitchId.ToMySqlField()+" "},
				CommonRoute={CommonRoute.ToMySqlField()+" "},
				idPartner={idPartner.ToMySqlField()+" "},
				NationalOrInternational={NationalOrInternational.ToMySqlField()+" "},
				Description={Description.ToMySqlField()+" "},
				Status={Status.ToMySqlField()+" "},
				date1={date1.ToMySqlField()+" "},
				field1={field1.ToMySqlField()+" "},
				field2={field2.ToMySqlField()+" "},
				field3={field3.ToMySqlField()+" "},
				field4={field4.ToMySqlField()+" "},
				field5={field5.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<route,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<route,string> whereClauseMethod)
		{
			return $@"delete from route 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
