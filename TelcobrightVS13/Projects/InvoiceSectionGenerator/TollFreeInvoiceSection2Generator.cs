using System;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation.Accounting;

namespace InvoiceSectionGenerator
{
    [Export("InvoiceSectionGenerator", typeof(IInvoiceSectionGenerator))]
    public class TollFreeInvoiceSection2Generator : AbstractInvoiceSectionGenerator
    {
        public override string RuleName => this.GetType().Name;
        public override InvoiceSection GetInvoiceSection(InvoiceSectionGeneratorData invoiceSectionGeneratorData)
        {
            string sql = $@"select p.partnerName as InPartnerName,tup_customerrate as Rate, 
                            TotalCalls,TotalMinutes, Amount from
                            (select tup_inpartnerId,tup_customerrate,
                            sum(successfulcalls)	as TotalCalls,                                  
                            sum(duration1)/60  as TotalMinutes,                                   
                            sum(customercost)  as Amount                                          
                            from {invoiceSectionGeneratorData.CdrOrSummaryTableName}                          
                            where {invoiceSectionGeneratorData.GetWhereClauseForDateCustomerId("tup_OutPartnerId")}
                            group by tup_inpartnerId,tup_customerrate) x
                            left join partner p
                            on x.tup_inPartnerId=p.idpartner;";
            return base.GetInvoiceSection<InvoiceSectionDataRowForA2ZVoice>(invoiceSectionGeneratorData, sql);
        }
    }
}
