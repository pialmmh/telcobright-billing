using System;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation.Accounting;

namespace PartnerRules
{
    [Export("InvoiceSectionGenerator", typeof(IInvoiceSectionGenerator))]
    public class XyzSection3Generator : AbstractInvoiceSectionGenerator
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
                          where {invoiceSectionGeneratorData.GetWhereClauseForDateRange()}
                          group by tup_starttime;";
            return base.GetInvoiceSection<InvoiceSectionDataRowForA2ZVoice>(invoiceSectionGeneratorData, sql);
        }
    }
}
