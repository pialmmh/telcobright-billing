using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class routeaddressmapping:ICacheble<routeaddressmapping>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{idRouteAddressMapping.ToMySqlField()},
				{IpTdmAddress.ToMySqlField()},
				{NoOfChannels.ToMySqlField()},
				{AddressType.ToMySqlField()},
				{SignalingProtocol.ToMySqlField()},
				{SS7NetworkIndicator.ToMySqlField()},
				{TransportProtocol.ToMySqlField()},
				{SignalingPort.ToMySqlField()},
				{Comment.ToMySqlField()},
				{idRoute.ToMySqlField()},
				{SwitchVendor.ToMySqlField()},
				{date1.ToMySqlField()},
				{field1.ToMySqlField()},
				{field2.ToMySqlField()},
				{field3.ToMySqlField()},
				{field4.ToMySqlField()},
				{field5.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<routeaddressmapping,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<routeaddressmapping,string> whereClauseMethod)
		{
			return $@"update routeaddressmapping set 
				idRouteAddressMapping={idRouteAddressMapping.ToMySqlField()+" "},
				IpTdmAddress={IpTdmAddress.ToMySqlField()+" "},
				NoOfChannels={NoOfChannels.ToMySqlField()+" "},
				AddressType={AddressType.ToMySqlField()+" "},
				SignalingProtocol={SignalingProtocol.ToMySqlField()+" "},
				SS7NetworkIndicator={SS7NetworkIndicator.ToMySqlField()+" "},
				TransportProtocol={TransportProtocol.ToMySqlField()+" "},
				SignalingPort={SignalingPort.ToMySqlField()+" "},
				Comment={Comment.ToMySqlField()+" "},
				idRoute={idRoute.ToMySqlField()+" "},
				SwitchVendor={SwitchVendor.ToMySqlField()+" "},
				date1={date1.ToMySqlField()+" "},
				field1={field1.ToMySqlField()+" "},
				field2={field2.ToMySqlField()+" "},
				field3={field3.ToMySqlField()+" "},
				field4={field4.ToMySqlField()+" "},
				field5={field5.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<routeaddressmapping,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<routeaddressmapping,string> whereClauseMethod)
		{
			return $@"delete from routeaddressmapping 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
