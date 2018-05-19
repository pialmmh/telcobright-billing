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
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.idCarrierContactMapping.ToMySqlField()).Append(",")
				.Append(this.Name.ToMySqlField()).Append(",")
				.Append(this.Designation.ToMySqlField()).Append(",")
				.Append(this.Department.ToMySqlField()).Append(",")
				.Append(this.OfficePhone.ToMySqlField()).Append(",")
				.Append(this.Mobile.ToMySqlField()).Append(",")
				.Append(this.email.ToMySqlField()).Append(",")
				.Append(this.idCarrier.ToMySqlField()).Append(",")
				.Append(this.date1.ToMySqlField()).Append(",")
				.Append(this.field1.ToMySqlField()).Append(",")
				.Append(this.field2.ToMySqlField()).Append(",")
				.Append(this.field3.ToMySqlField()).Append(",")
				.Append(this.field4.ToMySqlField()).Append(",")
				.Append(this.field5.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<carriercontactmapping,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<carriercontactmapping,string> whereClauseMethod)
		{
			return new StringBuilder("update carriercontactmapping set ")
				.Append("idCarrierContactMapping=").Append(this.idCarrierContactMapping.ToMySqlField()).Append(",")
				.Append("Name=").Append(this.Name.ToMySqlField()).Append(",")
				.Append("Designation=").Append(this.Designation.ToMySqlField()).Append(",")
				.Append("Department=").Append(this.Department.ToMySqlField()).Append(",")
				.Append("OfficePhone=").Append(this.OfficePhone.ToMySqlField()).Append(",")
				.Append("Mobile=").Append(this.Mobile.ToMySqlField()).Append(",")
				.Append("email=").Append(this.email.ToMySqlField()).Append(",")
				.Append("idCarrier=").Append(this.idCarrier.ToMySqlField()).Append(",")
				.Append("date1=").Append(this.date1.ToMySqlField()).Append(",")
				.Append("field1=").Append(this.field1.ToMySqlField()).Append(",")
				.Append("field2=").Append(this.field2.ToMySqlField()).Append(",")
				.Append("field3=").Append(this.field3.ToMySqlField()).Append(",")
				.Append("field4=").Append(this.field4.ToMySqlField()).Append(",")
				.Append("field5=").Append(this.field5.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<carriercontactmapping,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<carriercontactmapping,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from carriercontactmapping 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
