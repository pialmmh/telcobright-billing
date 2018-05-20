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
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.Type.ToMySqlField()).Append(",")
				.Append(this.Description.ToMySqlField()).Append(",")
				.Append(this.date1.ToMySqlField()).Append(",")
				.Append(this.field1.ToMySqlField()).Append(",")
				.Append(this.field2.ToMySqlField()).Append(",")
				.Append(this.field3.ToMySqlField()).Append(",")
				.Append(this.field4.ToMySqlField()).Append(",")
				.Append(this.field5.ToMySqlField()).Append(",")
				.Append(this.TimeZone.ToMySqlField()).Append(",")
				.Append(this.idCarrier.ToMySqlField()).Append(",")
				.Append(this.Currency.ToMySqlField()).Append(",")
				.Append(this.codedeletedate.ToMySqlField()).Append(",")
				.Append(this.ChangeCommitted.ToMySqlField()).Append(",")
				.Append(this.RatePlanName.ToMySqlField()).Append(",")
				.Append(this.Resolution.ToMySqlField()).Append(",")
				.Append(this.MinDurationSec.ToMySqlField()).Append(",")
				.Append(this.SurchargeTime.ToMySqlField()).Append(",")
				.Append(this.SurchargeAmount.ToMySqlField()).Append(",")
				.Append(this.Category.ToMySqlField()).Append(",")
				.Append(this.SubCategory.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<rateplanassign,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<rateplanassign,string> whereClauseMethod)
		{
			return new StringBuilder("update rateplanassign set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("Type=").Append(this.Type.ToMySqlField()).Append(",")
				.Append("Description=").Append(this.Description.ToMySqlField()).Append(",")
				.Append("date1=").Append(this.date1.ToMySqlField()).Append(",")
				.Append("field1=").Append(this.field1.ToMySqlField()).Append(",")
				.Append("field2=").Append(this.field2.ToMySqlField()).Append(",")
				.Append("field3=").Append(this.field3.ToMySqlField()).Append(",")
				.Append("field4=").Append(this.field4.ToMySqlField()).Append(",")
				.Append("field5=").Append(this.field5.ToMySqlField()).Append(",")
				.Append("TimeZone=").Append(this.TimeZone.ToMySqlField()).Append(",")
				.Append("idCarrier=").Append(this.idCarrier.ToMySqlField()).Append(",")
				.Append("Currency=").Append(this.Currency.ToMySqlField()).Append(",")
				.Append("codedeletedate=").Append(this.codedeletedate.ToMySqlField()).Append(",")
				.Append("ChangeCommitted=").Append(this.ChangeCommitted.ToMySqlField()).Append(",")
				.Append("RatePlanName=").Append(this.RatePlanName.ToMySqlField()).Append(",")
				.Append("Resolution=").Append(this.Resolution.ToMySqlField()).Append(",")
				.Append("MinDurationSec=").Append(this.MinDurationSec.ToMySqlField()).Append(",")
				.Append("SurchargeTime=").Append(this.SurchargeTime.ToMySqlField()).Append(",")
				.Append("SurchargeAmount=").Append(this.SurchargeAmount.ToMySqlField()).Append(",")
				.Append("Category=").Append(this.Category.ToMySqlField()).Append(",")
				.Append("SubCategory=").Append(this.SubCategory.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<rateplanassign,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<rateplanassign,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from rateplanassign 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
