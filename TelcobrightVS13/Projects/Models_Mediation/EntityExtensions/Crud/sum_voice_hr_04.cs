using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class sum_voice_hr_04:ICacheble<sum_voice_hr_04>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.tup_switchid.ToMySqlField()).Append(",")
				.Append(this.tup_inpartnerid.ToMySqlField()).Append(",")
				.Append(this.tup_outpartnerid.ToMySqlField()).Append(",")
				.Append(this.tup_incomingroute.ToMySqlField()).Append(",")
				.Append(this.tup_outgoingroute.ToMySqlField()).Append(",")
				.Append(this.tup_customerrate.ToMySqlField()).Append(",")
				.Append(this.tup_supplierrate.ToMySqlField()).Append(",")
				.Append(this.tup_incomingip.ToMySqlField()).Append(",")
				.Append(this.tup_outgoingip.ToMySqlField()).Append(",")
				.Append(this.tup_countryorareacode.ToMySqlField()).Append(",")
				.Append(this.tup_matchedprefixcustomer.ToMySqlField()).Append(",")
				.Append(this.tup_matchedprefixsupplier.ToMySqlField()).Append(",")
				.Append(this.tup_sourceId.ToMySqlField()).Append(",")
				.Append(this.tup_destinationId.ToMySqlField()).Append(",")
				.Append(this.tup_customercurrency.ToMySqlField()).Append(",")
				.Append(this.tup_suppliercurrency.ToMySqlField()).Append(",")
				.Append(this.tup_tax1currency.ToMySqlField()).Append(",")
				.Append(this.tup_tax2currency.ToMySqlField()).Append(",")
				.Append(this.tup_vatcurrency.ToMySqlField()).Append(",")
				.Append(this.tup_starttime.ToMySqlField()).Append(",")
				.Append(this.totalcalls.ToMySqlField()).Append(",")
				.Append(this.connectedcalls.ToMySqlField()).Append(",")
				.Append(this.connectedcallsCC.ToMySqlField()).Append(",")
				.Append(this.successfulcalls.ToMySqlField()).Append(",")
				.Append(this.actualduration.ToMySqlField()).Append(",")
				.Append(this.roundedduration.ToMySqlField()).Append(",")
				.Append(this.duration1.ToMySqlField()).Append(",")
				.Append(this.duration2.ToMySqlField()).Append(",")
				.Append(this.duration3.ToMySqlField()).Append(",")
				.Append(this.PDD.ToMySqlField()).Append(",")
				.Append(this.customercost.ToMySqlField()).Append(",")
				.Append(this.suppliercost.ToMySqlField()).Append(",")
				.Append(this.tax1.ToMySqlField()).Append(",")
				.Append(this.tax2.ToMySqlField()).Append(",")
				.Append(this.vat.ToMySqlField()).Append(",")
				.Append(this.intAmount1.ToMySqlField()).Append(",")
				.Append(this.intAmount2.ToMySqlField()).Append(",")
				.Append(this.longAmount1.ToMySqlField()).Append(",")
				.Append(this.longAmount2.ToMySqlField()).Append(",")
				.Append(this.longDecimalAmount1.ToMySqlField()).Append(",")
				.Append(this.longDecimalAmount2.ToMySqlField()).Append(",")
				.Append(this.intAmount3.ToMySqlField()).Append(",")
				.Append(this.longAmount3.ToMySqlField()).Append(",")
				.Append(this.longDecimalAmount3.ToMySqlField()).Append(",")
				.Append(this.decimalAmount1.ToMySqlField()).Append(",")
				.Append(this.decimalAmount2.ToMySqlField()).Append(",")
				.Append(this.decimalAmount3.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<sum_voice_hr_04,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<sum_voice_hr_04,string> whereClauseMethod)
		{
			return new StringBuilder("update sum_voice_hr_04 set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("tup_switchid=").Append(this.tup_switchid.ToMySqlField()).Append(",")
				.Append("tup_inpartnerid=").Append(this.tup_inpartnerid.ToMySqlField()).Append(",")
				.Append("tup_outpartnerid=").Append(this.tup_outpartnerid.ToMySqlField()).Append(",")
				.Append("tup_incomingroute=").Append(this.tup_incomingroute.ToMySqlField()).Append(",")
				.Append("tup_outgoingroute=").Append(this.tup_outgoingroute.ToMySqlField()).Append(",")
				.Append("tup_customerrate=").Append(this.tup_customerrate.ToMySqlField()).Append(",")
				.Append("tup_supplierrate=").Append(this.tup_supplierrate.ToMySqlField()).Append(",")
				.Append("tup_incomingip=").Append(this.tup_incomingip.ToMySqlField()).Append(",")
				.Append("tup_outgoingip=").Append(this.tup_outgoingip.ToMySqlField()).Append(",")
				.Append("tup_countryorareacode=").Append(this.tup_countryorareacode.ToMySqlField()).Append(",")
				.Append("tup_matchedprefixcustomer=").Append(this.tup_matchedprefixcustomer.ToMySqlField()).Append(",")
				.Append("tup_matchedprefixsupplier=").Append(this.tup_matchedprefixsupplier.ToMySqlField()).Append(",")
				.Append("tup_sourceId=").Append(this.tup_sourceId.ToMySqlField()).Append(",")
				.Append("tup_destinationId=").Append(this.tup_destinationId.ToMySqlField()).Append(",")
				.Append("tup_customercurrency=").Append(this.tup_customercurrency.ToMySqlField()).Append(",")
				.Append("tup_suppliercurrency=").Append(this.tup_suppliercurrency.ToMySqlField()).Append(",")
				.Append("tup_tax1currency=").Append(this.tup_tax1currency.ToMySqlField()).Append(",")
				.Append("tup_tax2currency=").Append(this.tup_tax2currency.ToMySqlField()).Append(",")
				.Append("tup_vatcurrency=").Append(this.tup_vatcurrency.ToMySqlField()).Append(",")
				.Append("tup_starttime=").Append(this.tup_starttime.ToMySqlField()).Append(",")
				.Append("totalcalls=").Append(this.totalcalls.ToMySqlField()).Append(",")
				.Append("connectedcalls=").Append(this.connectedcalls.ToMySqlField()).Append(",")
				.Append("connectedcallsCC=").Append(this.connectedcallsCC.ToMySqlField()).Append(",")
				.Append("successfulcalls=").Append(this.successfulcalls.ToMySqlField()).Append(",")
				.Append("actualduration=").Append(this.actualduration.ToMySqlField()).Append(",")
				.Append("roundedduration=").Append(this.roundedduration.ToMySqlField()).Append(",")
				.Append("duration1=").Append(this.duration1.ToMySqlField()).Append(",")
				.Append("duration2=").Append(this.duration2.ToMySqlField()).Append(",")
				.Append("duration3=").Append(this.duration3.ToMySqlField()).Append(",")
				.Append("PDD=").Append(this.PDD.ToMySqlField()).Append(",")
				.Append("customercost=").Append(this.customercost.ToMySqlField()).Append(",")
				.Append("suppliercost=").Append(this.suppliercost.ToMySqlField()).Append(",")
				.Append("tax1=").Append(this.tax1.ToMySqlField()).Append(",")
				.Append("tax2=").Append(this.tax2.ToMySqlField()).Append(",")
				.Append("vat=").Append(this.vat.ToMySqlField()).Append(",")
				.Append("intAmount1=").Append(this.intAmount1.ToMySqlField()).Append(",")
				.Append("intAmount2=").Append(this.intAmount2.ToMySqlField()).Append(",")
				.Append("longAmount1=").Append(this.longAmount1.ToMySqlField()).Append(",")
				.Append("longAmount2=").Append(this.longAmount2.ToMySqlField()).Append(",")
				.Append("longDecimalAmount1=").Append(this.longDecimalAmount1.ToMySqlField()).Append(",")
				.Append("longDecimalAmount2=").Append(this.longDecimalAmount2.ToMySqlField()).Append(",")
				.Append("intAmount3=").Append(this.intAmount3.ToMySqlField()).Append(",")
				.Append("longAmount3=").Append(this.longAmount3.ToMySqlField()).Append(",")
				.Append("longDecimalAmount3=").Append(this.longDecimalAmount3.ToMySqlField()).Append(",")
				.Append("decimalAmount1=").Append(this.decimalAmount1.ToMySqlField()).Append(",")
				.Append("decimalAmount2=").Append(this.decimalAmount2.ToMySqlField()).Append(",")
				.Append("decimalAmount3=").Append(this.decimalAmount3.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<sum_voice_hr_04,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<sum_voice_hr_04,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from sum_voice_hr_04 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
