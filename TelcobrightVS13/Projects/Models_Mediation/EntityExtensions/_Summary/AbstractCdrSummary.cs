using System;
using LibraryExtensions;

namespace MediationModel
{
	using CdrSummaryTuple = ValueTuple<int, int, int, string, string, decimal, decimal,
		 ValueTuple<string, string, string, string, string, string, string,
			 ValueTuple<string, string, string, string, string, string>>>;
	public abstract class AbstractCdrSummary : ICacheble<AbstractCdrSummary>, ISummary<AbstractCdrSummary, CdrSummaryTuple>
	{
		protected AbstractCdrSummary() { }//don't remove, was required at runtime by some code
		public abstract long id { get; set; }
		public abstract int tup_switchid { get; set; }
		public abstract int tup_inpartnerid { get; set; }
		public abstract int tup_outpartnerid { get; set; }
		public abstract string tup_incomingroute { get; set; }
		public abstract string tup_outgoingroute { get; set; }
		public abstract decimal tup_customerrate { get; set; }
		public abstract decimal tup_supplierrate { get; set; }   
		public abstract string tup_incomingip { get; set; }
		public abstract string tup_outgoingip { get; set; }
		public abstract string tup_countryorareacode { get; set; }
		public abstract string tup_matchedprefixcustomer { get; set; }
		public abstract string tup_matchedprefixsupplier { get; set; }
		public abstract string tup_sourceId { get; set; }
		public abstract string tup_destinationId { get; set; }
		public abstract string tup_customercurrency { get; set; }
		public abstract string tup_suppliercurrency { get; set; }
		public abstract string tup_tax1currency { get; set; }
		public abstract string tup_tax2currency { get; set; }
		public abstract string tup_vatcurrency { get; set; }
		public abstract DateTime tup_starttime { get; set; }
		public abstract long totalcalls { get; set; }
		public abstract long connectedcalls { get; set; }
		public abstract long connectedcallsCC { get; set; }
		public abstract long successfulcalls { get; set; }
		public abstract decimal actualduration { get; set; }
		public abstract decimal roundedduration { get; set; }
		public abstract decimal duration1 { get; set; }
		public abstract decimal duration2 { get; set; }
		public abstract decimal duration3 { get; set; }
		public abstract decimal PDD { get; set; }
		public abstract decimal customercost { get; set; }
		public abstract decimal suppliercost { get; set; }
		public abstract decimal tax1 { get; set; }
		public abstract decimal tax2 { get; set; }
		public abstract decimal vat { get; set; }
		public abstract int intAmount1 { get; set; }
		public abstract int intAmount2 { get; set; }
		public abstract int intAmount3 { get; set; }
		public abstract long longAmount1 { get; set; }
		public abstract long longAmount2 { get; set; }
		public abstract long longAmount3 { get; set; }
		public abstract decimal longDecimalAmount1 { get; set; }
		public abstract decimal longDecimalAmount2 { get; set; }
		public abstract decimal longDecimalAmount3 { get; set; }
		public abstract decimal decimalAmount1 { get; set; }
		public abstract decimal decimalAmount2 { get; set; }
		public abstract decimal decimalAmount3 { get; set; }
		public CdrSummaryTuple GetTupleKey()
		{
			var tup3 = new ValueTuple<string, string, string, string, string, string>(
				tup_tax1currency,
				tup_tax2currency,
				tup_vatcurrency,
				tup_starttime.ToMySqlField(),
				tup_customercurrency,
				tup_suppliercurrency
				);
			var tup2 = new ValueTuple<string, string, string, string, string, string, string,
				ValueTuple<string, string, string, string, string, string>>(
				tup_incomingip,
				tup_outgoingip,
				tup_countryorareacode,
				tup_matchedprefixcustomer,
				tup_matchedprefixsupplier,
				tup_sourceId,
				tup_destinationId,
				tup3
				);
			var tup1 = new CdrSummaryTuple(
				tup_switchid,
				this.tup_inpartnerid,
				this.tup_outpartnerid,
				this.tup_incomingroute,
				this.tup_outgoingroute,
				tup_customerrate,
				tup_supplierrate,
				tup2);
			return tup1;
		}
		public void Merge(AbstractCdrSummary newSummary)
		{
			totalcalls += newSummary.totalcalls;
			connectedcalls += newSummary.connectedcalls;
			successfulcalls += newSummary.successfulcalls;
			actualduration += newSummary.actualduration;
			roundedduration += newSummary.roundedduration;
			duration1 += newSummary.duration1;
			duration2 += newSummary.duration2;
			duration3 += newSummary.duration3;
			PDD += newSummary.PDD;
			customercost += newSummary.customercost;
			suppliercost += newSummary.suppliercost;
			tax1 += newSummary.tax1;
			tax2 += newSummary.tax2;
			vat += newSummary.vat;
			intAmount1 += newSummary.intAmount1;
			intAmount2 += newSummary.intAmount2;
			intAmount3 += newSummary.intAmount3;
			longAmount1 += newSummary.longAmount1;
			longAmount2 += newSummary.longAmount2;
			longAmount3 += newSummary.longAmount3;
			this.longDecimalAmount1 += newSummary.longDecimalAmount1;
			this.longDecimalAmount2 += newSummary.longDecimalAmount2;
			this.longDecimalAmount3 += newSummary.longDecimalAmount3;

			decimalAmount1 += newSummary.decimalAmount1;
			decimalAmount2 += newSummary.decimalAmount2;
			decimalAmount3 += newSummary.decimalAmount3;
		}
		public void Multiply(int value)
		{
			totalcalls = value * totalcalls;
			connectedcalls = value * connectedcalls;
			successfulcalls = value * successfulcalls;
			actualduration = value * actualduration;
			roundedduration = value * roundedduration;
			duration1 = value * duration1;
			duration2 = value * duration2;
			duration3 = value * duration3;
			PDD = value * PDD;
			customercost = value * customercost;
			suppliercost = value * suppliercost;
			tax1 = value * tax1;
			tax2 = value * tax2;
			vat = value * vat;
			intAmount1 = value * intAmount1;
			intAmount2 = value * intAmount2;
			intAmount3 = value * intAmount3;
			longAmount1 = value * longAmount1;
			longAmount2 = value * longAmount2;
			longAmount3 = value * longAmount3;
			this.longDecimalAmount1 = value * this.longDecimalAmount1;
			this.longDecimalAmount2 = value * this.longDecimalAmount2;
			this.longDecimalAmount3 = value * this.longDecimalAmount3;

			decimalAmount1 = value * decimalAmount1;
			decimalAmount2 = value * decimalAmount2;
			decimalAmount3 = value * decimalAmount3;
		}

		public AbstractCdrSummary CloneWithFakeId()
		{
			AbstractCdrSummary newSummary=new sum_voice_day_01();//take any summary type, has to be cast in calling method to proper type
			newSummary.id = -1;//must set externally
			newSummary.tup_switchid = this.tup_switchid;
			newSummary.tup_inpartnerid = this.tup_inpartnerid;
			newSummary.tup_outpartnerid = this.tup_outpartnerid;
			newSummary.tup_incomingroute = this.tup_incomingroute;
			newSummary.tup_outgoingroute = this.tup_outgoingroute;
			newSummary.tup_customerrate = this.tup_customerrate;
			newSummary.tup_supplierrate = this.tup_supplierrate;
			newSummary.tup_incomingip = this.tup_incomingip;
			newSummary.tup_outgoingip = this.tup_outgoingip;
			newSummary.tup_countryorareacode = this.tup_countryorareacode;
			newSummary.tup_matchedprefixcustomer = this.tup_matchedprefixcustomer;
			newSummary.tup_matchedprefixsupplier = this.tup_matchedprefixsupplier;
			newSummary.tup_sourceId = this.tup_sourceId;
			newSummary.tup_destinationId = this.tup_destinationId;
			newSummary.tup_customercurrency = this.tup_customercurrency;
			newSummary.tup_suppliercurrency = this.tup_suppliercurrency;
			newSummary.tup_tax1currency = this.tup_tax1currency;
			newSummary.tup_tax2currency = this.tup_tax2currency;
			newSummary.tup_vatcurrency = this.tup_vatcurrency;
			newSummary.tup_starttime = this.tup_starttime;
			newSummary.totalcalls = this.totalcalls;
			newSummary.connectedcalls = this.connectedcalls;
			newSummary.connectedcallsCC = this.connectedcallsCC;
			newSummary.successfulcalls = this.successfulcalls;
			newSummary.actualduration = this.actualduration;
			newSummary.roundedduration = this.roundedduration;
			newSummary.duration1 = this.duration1;
			newSummary.duration2 = this.duration2;
			newSummary.duration3 = this.duration3;
			newSummary.PDD = this.PDD;
			newSummary.customercost = this.customercost;
			newSummary.suppliercost = this.suppliercost;
			newSummary.tax1 = this.tax1;
			newSummary.tax2 = this.tax2;
			newSummary.vat = this.vat;
			newSummary.intAmount1 = this.intAmount1;
			newSummary.intAmount2 = this.intAmount2;
			newSummary.intAmount3 = this.intAmount3;
			newSummary.longAmount1 = this.longAmount1;
			newSummary.longAmount2 = this.longAmount2;
			newSummary.longAmount3 = this.longAmount3;
			newSummary.longDecimalAmount1 = this.longDecimalAmount1;
			newSummary.longDecimalAmount2 = this.longDecimalAmount2;
			newSummary.longDecimalAmount3 = this.longDecimalAmount3;
			newSummary.decimalAmount1 = this.decimalAmount1;
			newSummary.decimalAmount2 = this.decimalAmount2;
			newSummary.decimalAmount3 = this.decimalAmount3;
			return newSummary;
		}

		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},                       
				{tup_switchid.ToMySqlField()},             
				{this.tup_inpartnerid.ToMySqlField()},          
				{this.tup_outpartnerid.ToMySqlField()},         
				{this.tup_incomingroute.ToNotNullSqlField()},        
				{this.tup_outgoingroute.ToNotNullSqlField()},        
				{tup_customerrate.ToMySqlField()},         
				{tup_supplierrate.ToMySqlField()},         
				{tup_incomingip.ToNotNullSqlField()},           
				{tup_outgoingip.ToNotNullSqlField()},           
				{tup_countryorareacode.ToNotNullSqlField()},    
				{tup_matchedprefixcustomer.ToNotNullSqlField()},
				{tup_matchedprefixsupplier.ToNotNullSqlField()},
				{tup_sourceId.ToNotNullSqlField()},             
				{tup_destinationId.ToNotNullSqlField()},        
				{tup_customercurrency.ToNotNullSqlField()},     
				{tup_suppliercurrency.ToNotNullSqlField()},     
				{tup_tax1currency.ToNotNullSqlField()},         
				{tup_tax2currency.ToNotNullSqlField()},         
				{tup_vatcurrency.ToNotNullSqlField()},          
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
				{this.longDecimalAmount1.ToMySqlField()},            
				{this.longDecimalAmount2.ToMySqlField()},            
				{intAmount3.ToMySqlField()},               
				{longAmount3.ToMySqlField()},              
				{this.longDecimalAmount3.ToMySqlField()},            
				{decimalAmount1.ToMySqlField()},           
				{decimalAmount2.ToMySqlField()},           
				{decimalAmount3.ToMySqlField()})";
		}
		public string GetExtInsertCustom(Func<AbstractCdrSummary, string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public string GetUpdateCommand(Func<AbstractCdrSummary, string> whereClauseMethod)
		{
			return $@"update AbstractCdrSummary set 
				totalcalls={totalcalls.ToMySqlField() + " "},                     
				connectedcalls={connectedcalls.ToMySqlField() + " "},             
				connectedcallsCC={connectedcallsCC.ToMySqlField() + " "},         
				successfulcalls={successfulcalls.ToMySqlField() + " "},           
				actualduration={actualduration.ToMySqlField() + " "},             
				roundedduration={roundedduration.ToMySqlField() + " "},           
				duration1={duration1.ToMySqlField() + " "},                       
				duration2={duration2.ToMySqlField() + " "},                       
				duration3={duration3.ToMySqlField() + " "},                       
				PDD={PDD.ToMySqlField() + " "},                                   
				customercost={customercost.ToMySqlField() + " "},                 
				suppliercost={suppliercost.ToMySqlField() + " "},                 
				tax1={tax1.ToMySqlField() + " "},                                 
				tax2={tax2.ToMySqlField() + " "},                                 
				vat={vat.ToMySqlField() + " "},                                   
				intAmount1={intAmount1.ToMySqlField() + " "},                     
				intAmount2={intAmount2.ToMySqlField() + " "},                     
				longAmount1={longAmount1.ToMySqlField() + " "},                   
				longAmount2={longAmount2.ToMySqlField() + " "},                   
				longDecimalAmount1={this.longDecimalAmount1.ToMySqlField() + " "},
				longDecimalAmount2={this.longDecimalAmount2.ToMySqlField() + " "},
				intAmount3={intAmount3.ToMySqlField() + " "},                     
				longAmount3={longAmount3.ToMySqlField() + " "},                   
				longDecimalAmount3={this.longDecimalAmount3.ToMySqlField() + " "},
				decimalAmount1={decimalAmount1.ToMySqlField() + " "},             
				decimalAmount2={decimalAmount2.ToMySqlField() + " "},             
				decimalAmount3={decimalAmount3.ToMySqlField() + " "}              
				{whereClauseMethod.Invoke(this)};                                 
				";
		}
		public string GetUpdateCommandCustom(Func<AbstractCdrSummary, string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public string GetDeleteCommand(Func<AbstractCdrSummary, string> whereClauseMethod)
		{
			return $@"delete from AbstractCdrSummary 
				{whereClauseMethod.Invoke(this)};
				";
		}

	}

}
