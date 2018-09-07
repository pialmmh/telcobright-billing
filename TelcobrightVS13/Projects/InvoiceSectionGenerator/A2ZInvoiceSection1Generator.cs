using System;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation.Accounting;

namespace PartnerRules
{
    [Export("InvoiceSectionGenerator", typeof(IInvoiceSectionGenerator))]
    public class A2ZInvoiceSection1Generator : AbstractInvoiceSectionGenerator
    {
        public override string RuleName => this.GetType().Name;
        public override InvoiceSection GetInvoiceSection(InvoiceSectionGeneratorData invoiceSectionGeneratorData)
        {
            string sql = $@"select                                                         
                           sum(successfulcalls)	as TotalCalls,    
                           sum(duration1)/60  as TotalMinutes,   
                           sum(customercost)  as Amount      
                           from {invoiceSectionGeneratorData.CdrOrSummaryTableName}                          
                           where {invoiceSectionGeneratorData.GetWhereClauseForDateRange()};";
            return base.GetInvoiceSection<InvoiceSectionDataRowForA2ZVoice>(invoiceSectionGeneratorData, sql);
        }
    }
}
