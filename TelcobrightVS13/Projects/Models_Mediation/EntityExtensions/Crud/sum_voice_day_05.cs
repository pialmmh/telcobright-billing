using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class sum_voice_day_05:ICacheble<sum_voice_day_05>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{tup_switchid.ToMySqlField()},
				{tup_inpartnerid.ToMySqlField()},
				{tup_outpartnerid.ToMySqlField()},
				{tup_incomingroute.ToMySqlField()},
				{tup_outgoingroute.ToMySqlField()},
				{tup_customerrate.ToMySqlField()},
				{tup_supplierrate.ToMySqlField()},
				{tup_incomingip.ToMySqlField()},
				{tup_outgoingip.ToMySqlField()},
				{tup_countryorareacode.ToMySqlField()},
				{tup_matchedprefixcustomer.ToMySqlField()},
				{tup_matchedprefixsupplier.ToMySqlField()},
				{tup_sourceId.ToMySqlField()},
				{tup_destinationId.ToMySqlField()},
				{tup_customercurrency.ToMySqlField()},
				{tup_suppliercurrency.ToMySqlField()},
				{tup_tax1currency.ToMySqlField()},
				{tup_tax2currency.ToMySqlField()},
				{tup_vatcurrency.ToMySqlField()},
				{tup_starttime.ToMySqlField()},
				{totalcalls.ToMySqlField()},
				{connectedcalls.ToMySqlField()},
				{connectedcallsCC.ToMySqlField()},
				{successfulcalls.ToMySqlField()},
				{actualduration.ToMySqlField()},
				{roundedduration.ToMySqlField()},
				{duration1.ToMySqlField()},
				{duration2.ToMySqlField()},
				{duration3.ToMySqlField()},
				{PDD.ToMySqlField()},
				{customercost.ToMySqlField()},
				{suppliercost.ToMySqlField()},
				{tax1.ToMySqlField()},
				{tax2.ToMySqlField()},
				{vat.ToMySqlField()},
				{intAmount1.ToMySqlField()},
				{intAmount2.ToMySqlField()},
				{longAmount1.ToMySqlField()},
				{longAmount2.ToMySqlField()},
				{longDecimalAmount1.ToMySqlField()},
				{longDecimalAmount2.ToMySqlField()},
				{intAmount3.ToMySqlField()},
				{longAmount3.ToMySqlField()},
				{longDecimalAmount3.ToMySqlField()},
				{decimalAmount1.ToMySqlField()},
				{decimalAmount2.ToMySqlField()},
				{decimalAmount3.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<sum_voice_day_05,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<sum_voice_day_05,string> whereClauseMethod)
		{
			return $@"update sum_voice_day_05 set 
				id={id.ToMySqlField()+" "},
				tup_switchid={tup_switchid.ToMySqlField()+" "},
				tup_inpartnerid={tup_inpartnerid.ToMySqlField()+" "},
				tup_outpartnerid={tup_outpartnerid.ToMySqlField()+" "},
				tup_incomingroute={tup_incomingroute.ToMySqlField()+" "},
				tup_outgoingroute={tup_outgoingroute.ToMySqlField()+" "},
				tup_customerrate={tup_customerrate.ToMySqlField()+" "},
				tup_supplierrate={tup_supplierrate.ToMySqlField()+" "},
				tup_incomingip={tup_incomingip.ToMySqlField()+" "},
				tup_outgoingip={tup_outgoingip.ToMySqlField()+" "},
				tup_countryorareacode={tup_countryorareacode.ToMySqlField()+" "},
				tup_matchedprefixcustomer={tup_matchedprefixcustomer.ToMySqlField()+" "},
				tup_matchedprefixsupplier={tup_matchedprefixsupplier.ToMySqlField()+" "},
				tup_sourceId={tup_sourceId.ToMySqlField()+" "},
				tup_destinationId={tup_destinationId.ToMySqlField()+" "},
				tup_customercurrency={tup_customercurrency.ToMySqlField()+" "},
				tup_suppliercurrency={tup_suppliercurrency.ToMySqlField()+" "},
				tup_tax1currency={tup_tax1currency.ToMySqlField()+" "},
				tup_tax2currency={tup_tax2currency.ToMySqlField()+" "},
				tup_vatcurrency={tup_vatcurrency.ToMySqlField()+" "},
				tup_starttime={tup_starttime.ToMySqlField()+" "},
				totalcalls={totalcalls.ToMySqlField()+" "},
				connectedcalls={connectedcalls.ToMySqlField()+" "},
				connectedcallsCC={connectedcallsCC.ToMySqlField()+" "},
				successfulcalls={successfulcalls.ToMySqlField()+" "},
				actualduration={actualduration.ToMySqlField()+" "},
				roundedduration={roundedduration.ToMySqlField()+" "},
				duration1={duration1.ToMySqlField()+" "},
				duration2={duration2.ToMySqlField()+" "},
				duration3={duration3.ToMySqlField()+" "},
				PDD={PDD.ToMySqlField()+" "},
				customercost={customercost.ToMySqlField()+" "},
				suppliercost={suppliercost.ToMySqlField()+" "},
				tax1={tax1.ToMySqlField()+" "},
				tax2={tax2.ToMySqlField()+" "},
				vat={vat.ToMySqlField()+" "},
				intAmount1={intAmount1.ToMySqlField()+" "},
				intAmount2={intAmount2.ToMySqlField()+" "},
				longAmount1={longAmount1.ToMySqlField()+" "},
				longAmount2={longAmount2.ToMySqlField()+" "},
				longDecimalAmount1={longDecimalAmount1.ToMySqlField()+" "},
				longDecimalAmount2={longDecimalAmount2.ToMySqlField()+" "},
				intAmount3={intAmount3.ToMySqlField()+" "},
				longAmount3={longAmount3.ToMySqlField()+" "},
				longDecimalAmount3={longDecimalAmount3.ToMySqlField()+" "},
				decimalAmount1={decimalAmount1.ToMySqlField()+" "},
				decimalAmount2={decimalAmount2.ToMySqlField()+" "},
				decimalAmount3={decimalAmount3.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<sum_voice_day_05,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<sum_voice_day_05,string> whereClauseMethod)
		{
			return $@"delete from sum_voice_day_05 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
