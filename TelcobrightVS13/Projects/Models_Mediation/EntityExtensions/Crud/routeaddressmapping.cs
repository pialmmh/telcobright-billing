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
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.idRouteAddressMapping.ToMySqlField()).Append(",")
				.Append(this.IpTdmAddress.ToMySqlField()).Append(",")
				.Append(this.NoOfChannels.ToMySqlField()).Append(",")
				.Append(this.AddressType.ToMySqlField()).Append(",")
				.Append(this.SignalingProtocol.ToMySqlField()).Append(",")
				.Append(this.SS7NetworkIndicator.ToMySqlField()).Append(",")
				.Append(this.TransportProtocol.ToMySqlField()).Append(",")
				.Append(this.SignalingPort.ToMySqlField()).Append(",")
				.Append(this.Comment.ToMySqlField()).Append(",")
				.Append(this.idRoute.ToMySqlField()).Append(",")
				.Append(this.SwitchVendor.ToMySqlField()).Append(",")
				.Append(this.date1.ToMySqlField()).Append(",")
				.Append(this.field1.ToMySqlField()).Append(",")
				.Append(this.field2.ToMySqlField()).Append(",")
				.Append(this.field3.ToMySqlField()).Append(",")
				.Append(this.field4.ToMySqlField()).Append(",")
				.Append(this.field5.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<routeaddressmapping,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<routeaddressmapping,string> whereClauseMethod)
		{
			return new StringBuilder("update routeaddressmapping set ")
				.Append("idRouteAddressMapping=").Append(this.idRouteAddressMapping.ToMySqlField()).Append(",")
				.Append("IpTdmAddress=").Append(this.IpTdmAddress.ToMySqlField()).Append(",")
				.Append("NoOfChannels=").Append(this.NoOfChannels.ToMySqlField()).Append(",")
				.Append("AddressType=").Append(this.AddressType.ToMySqlField()).Append(",")
				.Append("SignalingProtocol=").Append(this.SignalingProtocol.ToMySqlField()).Append(",")
				.Append("SS7NetworkIndicator=").Append(this.SS7NetworkIndicator.ToMySqlField()).Append(",")
				.Append("TransportProtocol=").Append(this.TransportProtocol.ToMySqlField()).Append(",")
				.Append("SignalingPort=").Append(this.SignalingPort.ToMySqlField()).Append(",")
				.Append("Comment=").Append(this.Comment.ToMySqlField()).Append(",")
				.Append("idRoute=").Append(this.idRoute.ToMySqlField()).Append(",")
				.Append("SwitchVendor=").Append(this.SwitchVendor.ToMySqlField()).Append(",")
				.Append("date1=").Append(this.date1.ToMySqlField()).Append(",")
				.Append("field1=").Append(this.field1.ToMySqlField()).Append(",")
				.Append("field2=").Append(this.field2.ToMySqlField()).Append(",")
				.Append("field3=").Append(this.field3.ToMySqlField()).Append(",")
				.Append("field4=").Append(this.field4.ToMySqlField()).Append(",")
				.Append("field5=").Append(this.field5.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<routeaddressmapping,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<routeaddressmapping,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from routeaddressmapping 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
