using System;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation.Accounting;

namespace InvoiceSectionGenerator
{
    [Export("InvoiceSectionGenerator", typeof(IInvoiceSectionGenerator))]
    public class A2ZInvoiceSection2GeneratorWithTax : AbstractInvoiceSectionGenerator
    {
        public override string RuleName => this.GetType().Name;
        public override InvoiceSection GetInvoiceSection(InvoiceSectionGeneratorData invoiceSectionGeneratorData)
        {
            decimal vatPercentage = Convert.ToDecimal(invoiceSectionGeneratorData.InvoicePostProcessingData
                .JsonDetail["vat"]);
            string sql = $@"select p.partnerName as OutPartnerName,tup_customerrate as Rate, 
                            TotalCalls,TotalMinutes, Amount from
                            (select tup_outpartnerId,tup_customerrate,
                            sum(successfulcalls)	as TotalCalls,                                  
                            sum(duration1)/60  as TotalMinutes,                                   
                            sum(customercost)  as Amount,
                            sum(customercost)*{vatPercentage} as TaxOrVatAmount,
                            sum(customercost)*(1+{vatPercentage}) as GrandTotalAmount              
                            from {invoiceSectionGeneratorData.CdrOrSummaryTableName}                          
                            where {invoiceSectionGeneratorData.GetWhereClauseForDateCustomerId("tup_inPartnerId")}
                            group by tup_outpartnerId,tup_customerrate) x
                            left join partner p
                            on x.tup_outPartnerId=p.idpartner;";
            return base.GetInvoiceSection<InvoiceSectionDataRowForA2ZVoice>(invoiceSectionGeneratorData, sql);
        }
    }
}
