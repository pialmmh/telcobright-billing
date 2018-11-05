using System;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation.Accounting;

namespace InvoiceSectionGenerator
{
    [Export("InvoiceSectionGenerator", typeof(IInvoiceSectionGenerator))]
    public class TollFreeInvoiceSection3GeneratorWithTax : AbstractInvoiceSectionGenerator
    {
        public override string RuleName => this.GetType().Name;
        public override InvoiceSection GetInvoiceSection(InvoiceSectionGeneratorData invoiceSectionGeneratorData)
        {
            decimal vatPercentage = Convert.ToDecimal(invoiceSectionGeneratorData.InvoicePostProcessingData
                .JsonDetail["vat"]);
            string sql = $@"select tup_starttime as Date,tup_customerrate as Rate, TotalCalls,TotalMinutes, Amount, 
                            TaxOrVatAmount, GrandTotalAmount from
                            (select tup_starttime,tup_customerrate,
                            sum(successfulcalls)	as TotalCalls,                                  
                            sum(duration1)/60  as TotalMinutes,                                   
                            sum(customercost)  as Amount,
                            sum(customercost)*{vatPercentage} as TaxOrVatAmount,
                            sum(customercost)*(1+{vatPercentage}) as GrandTotalAmount                                                                                    
                            from {invoiceSectionGeneratorData.CdrOrSummaryTableName}                          
                            where {invoiceSectionGeneratorData.GetWhereClauseForDateCustomerId("tup_OutPartnerId")}
                            group by tup_starttime,tup_customerrate) x;";
            return base.GetInvoiceSection<InvoiceSectionDataRowForA2ZVoice>(invoiceSectionGeneratorData, sql);
        }
    }
}
