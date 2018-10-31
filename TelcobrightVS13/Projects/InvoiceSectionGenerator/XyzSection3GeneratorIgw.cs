using System;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation.Accounting;

namespace InvoiceSectionGenerator
{
    [Export("InvoiceSectionGenerator", typeof(IInvoiceSectionGenerator))]
    public class XyzSection3GeneratorIgw : AbstractInvoiceSectionGenerator
    {
        public override string RuleName => this.GetType().Name;
        public override InvoiceSection GetInvoiceSection(InvoiceSectionGeneratorData invoiceSectionGeneratorData)
        {
            string sql = $@"select                                                         
                          tup_starttime as `Date`,
                          sum(successfulcalls 	)	as TotalCalls,    
                          sum(roundedduration   )/60  as TotalMinutes,   
                          sum(longDecimalAmount1)  as XAmount,
                          sum(longDecimalAmount2)  as YAmount,
                          sum(longDecimalAmount3)  as XYAmount,
                          sum(customercost      )  as Revenue      
                          from {invoiceSectionGeneratorData.CdrOrSummaryTableName}                                             
                          where {invoiceSectionGeneratorData.GetWhereClauseForDateCustomerId("tup_inPartnerId")}
                          and totalcalls>0
                          group by tup_starttime;";
            return base.GetInvoiceSection<InvoiceSectionDataRowForA2ZVoice>(invoiceSectionGeneratorData, sql);
        }
    }
}
