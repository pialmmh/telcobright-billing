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
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.idroute.ToMySqlField()).Append(",")
				.Append(this.RouteName.ToMySqlField()).Append(",")
				.Append(this.SwitchId.ToMySqlField()).Append(",")
				.Append(this.CommonRoute.ToMySqlField()).Append(",")
				.Append(this.idPartner.ToMySqlField()).Append(",")
				.Append(this.NationalOrInternational.ToMySqlField()).Append(",")
				.Append(this.Description.ToMySqlField()).Append(",")
				.Append(this.Status.ToMySqlField()).Append(",")
				.Append(this.date1.ToMySqlField()).Append(",")
				.Append(this.field1.ToMySqlField()).Append(",")
				.Append(this.field2.ToMySqlField()).Append(",")
				.Append(this.field3.ToMySqlField()).Append(",")
				.Append(this.field4.ToMySqlField()).Append(",")
				.Append(this.field5.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<route,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<route,string> whereClauseMethod)
		{
			return new StringBuilder("update route set ")
				.Append("idroute=").Append(this.idroute.ToMySqlField()).Append(",")
				.Append("RouteName=").Append(this.RouteName.ToMySqlField()).Append(",")
				.Append("SwitchId=").Append(this.SwitchId.ToMySqlField()).Append(",")
				.Append("CommonRoute=").Append(this.CommonRoute.ToMySqlField()).Append(",")
				.Append("idPartner=").Append(this.idPartner.ToMySqlField()).Append(",")
				.Append("NationalOrInternational=").Append(this.NationalOrInternational.ToMySqlField()).Append(",")
				.Append("Description=").Append(this.Description.ToMySqlField()).Append(",")
				.Append("Status=").Append(this.Status.ToMySqlField()).Append(",")
				.Append("date1=").Append(this.date1.ToMySqlField()).Append(",")
				.Append("field1=").Append(this.field1.ToMySqlField()).Append(",")
				.Append("field2=").Append(this.field2.ToMySqlField()).Append(",")
				.Append("field3=").Append(this.field3.ToMySqlField()).Append(",")
				.Append("field4=").Append(this.field4.ToMySqlField()).Append(",")
				.Append("field5=").Append(this.field5.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<route,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<route,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from route 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
