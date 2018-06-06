using System;
using System.Text;
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
	            this.tup_tax1currency,
	            this.tup_tax2currency,
	            this.tup_vatcurrency,
	            this.tup_starttime.ToMySqlField(),
	            this.tup_customercurrency,
	            this.tup_suppliercurrency
	        );
	        var tup2 = new ValueTuple<string, string, string, string, string, string, string,
	            ValueTuple<string, string, string, string, string, string>>(
	            this.tup_incomingip,
	            this.tup_outgoingip,
	            this.tup_countryorareacode,
	            this.tup_matchedprefixcustomer,
	            this.tup_matchedprefixsupplier,
	            this.tup_sourceId,
	            this.tup_destinationId,
	            tup3
	        );
	        var tup1 = new CdrSummaryTuple(
	            this.tup_switchid,
	            this.tup_inpartnerid,
	            this.tup_outpartnerid,
	            this.tup_incomingroute,
	            this.tup_outgoingroute,
	            this.tup_customerrate,
	            this.tup_supplierrate,
	            tup2);
	        return tup1;
	    }

	    public void Merge(AbstractCdrSummary newSummary)
		{
			this.totalcalls += newSummary.totalcalls;
			this.connectedcalls += newSummary.connectedcalls;
			this.successfulcalls += newSummary.successfulcalls;
			this.actualduration += newSummary.actualduration;
			this.roundedduration += newSummary.roundedduration;
			this.duration1 += newSummary.duration1;
			this.duration2 += newSummary.duration2;
			this.duration3 += newSummary.duration3;
			this.PDD += newSummary.PDD;
			this.customercost += newSummary.customercost;
			this.suppliercost += newSummary.suppliercost;
			this.tax1 += newSummary.tax1;
			this.tax2 += newSummary.tax2;
			this.vat += newSummary.vat;
			this.intAmount1 += newSummary.intAmount1;
			this.intAmount2 += newSummary.intAmount2;
			this.intAmount3 += newSummary.intAmount3;
			this.longAmount1 += newSummary.longAmount1;
			this.longAmount2 += newSummary.longAmount2;
			this.longAmount3 += newSummary.longAmount3;
			this.longDecimalAmount1 += newSummary.longDecimalAmount1;
			this.longDecimalAmount2 += newSummary.longDecimalAmount2;
			this.longDecimalAmount3 += newSummary.longDecimalAmount3;

			this.decimalAmount1 += newSummary.decimalAmount1;
			this.decimalAmount2 += newSummary.decimalAmount2;
			this.decimalAmount3 += newSummary.decimalAmount3;
		}
		public void Multiply(int value)
		{
			this.totalcalls = value * this.totalcalls;
			this.connectedcalls = value * this.connectedcalls;
			this.successfulcalls = value * this.successfulcalls;
			this.actualduration = value * this.actualduration;
			this.roundedduration = value * this.roundedduration;
			this.duration1 = value * this.duration1;
			this.duration2 = value * this.duration2;
			this.duration3 = value * this.duration3;
			this.PDD = value * this.PDD;
			this.customercost = value * this.customercost;
			this.suppliercost = value * this.suppliercost;
			this.tax1 = value * this.tax1;
			this.tax2 = value * this.tax2;
			this.vat = value * this.vat;
			this.intAmount1 = value * this.intAmount1;
			this.intAmount2 = value * this.intAmount2;
			this.intAmount3 = value * this.intAmount3;
			this.longAmount1 = value * this.longAmount1;
			this.longAmount2 = value * this.longAmount2;
			this.longAmount3 = value * this.longAmount3;
			this.longDecimalAmount1 = value * this.longDecimalAmount1;
			this.longDecimalAmount2 = value * this.longDecimalAmount2;
			this.longDecimalAmount3 = value * this.longDecimalAmount3;

			this.decimalAmount1 = value * this.decimalAmount1;
			this.decimalAmount2 = value * this.decimalAmount2;
			this.decimalAmount3 = value * this.decimalAmount3;
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

		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")                       
				.Append(this.tup_switchid.ToMySqlField()).Append(",")             
				.Append(this.tup_inpartnerid.ToMySqlField()).Append(",")          
				.Append(this.tup_outpartnerid.ToMySqlField()).Append(",")         
				.Append(this.tup_incomingroute.ToNotNullSqlField()).Append(",")        
				.Append(this.tup_outgoingroute.ToNotNullSqlField()).Append(",")        
				.Append(this.tup_customerrate.ToMySqlField()).Append(",")         
				.Append(this.tup_supplierrate.ToMySqlField()).Append(",")         
				.Append(this.tup_incomingip.ToNotNullSqlField()).Append(",")           
				.Append(this.tup_outgoingip.ToNotNullSqlField()).Append(",")           
				.Append(this.tup_countryorareacode.ToNotNullSqlField()).Append(",")    
				.Append(this.tup_matchedprefixcustomer.ToNotNullSqlField()).Append(",")
				.Append(this.tup_matchedprefixsupplier.ToNotNullSqlField()).Append(",")
				.Append(this.tup_sourceId.ToNotNullSqlField()).Append(",")             
				.Append(this.tup_destinationId.ToNotNullSqlField()).Append(",")        
				.Append(this.tup_customercurrency.ToNotNullSqlField()).Append(",")     
				.Append(this.tup_suppliercurrency.ToNotNullSqlField()).Append(",")     
				.Append(this.tup_tax1currency.ToNotNullSqlField()).Append(",")         
				.Append(this.tup_tax2currency.ToNotNullSqlField()).Append(",")         
				.Append(this.tup_vatcurrency.ToNotNullSqlField()).Append(",")          
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
				.Append(this.decimalAmount3.ToMySqlField()).Append(")");
		}
		public StringBuilder GetExtInsertCustom(Func<AbstractCdrSummary, string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public StringBuilder GetUpdateCommand(Func<AbstractCdrSummary, string> whereClauseMethod)
		{
			return new StringBuilder($@"update AbstractCdrSummary set 
				totalcalls={this.totalcalls.ToMySqlField() + " "},                     
				connectedcalls={this.connectedcalls.ToMySqlField() + " "},             
				connectedcallsCC={this.connectedcallsCC.ToMySqlField() + " "},         
				successfulcalls={this.successfulcalls.ToMySqlField() + " "},           
				actualduration={this.actualduration.ToMySqlField() + " "},             
				roundedduration={this.roundedduration.ToMySqlField() + " "},           
				duration1={this.duration1.ToMySqlField() + " "},                       
				duration2={this.duration2.ToMySqlField() + " "},                       
				duration3={this.duration3.ToMySqlField() + " "},                       
				PDD={this.PDD.ToMySqlField() + " "},                                   
				customercost={this.customercost.ToMySqlField() + " "},                 
				suppliercost={this.suppliercost.ToMySqlField() + " "},                 
				tax1={this.tax1.ToMySqlField() + " "},                                 
				tax2={this.tax2.ToMySqlField() + " "},                                 
				vat={this.vat.ToMySqlField() + " "},                                   
				intAmount1={this.intAmount1.ToMySqlField() + " "},                     
				intAmount2={this.intAmount2.ToMySqlField() + " "},                     
				longAmount1={this.longAmount1.ToMySqlField() + " "},                   
				longAmount2={this.longAmount2.ToMySqlField() + " "},                   
				longDecimalAmount1={this.longDecimalAmount1.ToMySqlField() + " "},
				longDecimalAmount2={this.longDecimalAmount2.ToMySqlField() + " "},
				intAmount3={this.intAmount3.ToMySqlField() + " "},                     
				longAmount3={this.longAmount3.ToMySqlField() + " "},                   
				longDecimalAmount3={this.longDecimalAmount3.ToMySqlField() + " "},
				decimalAmount1={this.decimalAmount1.ToMySqlField() + " "},             
				decimalAmount2={this.decimalAmount2.ToMySqlField() + " "},             
				decimalAmount3={this.decimalAmount3.ToMySqlField() + " "}              
				{whereClauseMethod.Invoke(this)}                                
				");
		}
		public StringBuilder GetUpdateCommandCustom(Func<AbstractCdrSummary, string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public StringBuilder GetDeleteCommand(Func<AbstractCdrSummary, string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from AbstractCdrSummary 
				{whereClauseMethod.Invoke(this)}
				");
		}
	}
}
