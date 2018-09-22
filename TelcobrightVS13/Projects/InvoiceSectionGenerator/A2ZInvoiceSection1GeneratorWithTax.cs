using System;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation.Accounting;

namespace InvoiceSectionGenerator
{
    [Export("InvoiceSectionGenerator", typeof(IInvoiceSectionGenerator))]
    public class A2ZInvoiceSection1GeneratorWithTax : AbstractInvoiceSectionGenerator
    {
        public override string RuleName => this.GetType().Name;
        public override InvoiceSection GetInvoiceSection(InvoiceSectionGeneratorData invoiceSectionGeneratorData)
        {
            decimal vatPercentage = Convert.ToDecimal(invoiceSectionGeneratorData.InvoicePostProcessingData
                .JsonDetail["vat"]);
            string sql = $@"select                                                         
                           sum(successfulcalls)	as TotalCalls,    
                           sum(duration1)/60  as TotalMinutes,   
                           sum(customercost)  as Amount,
                           sum(customercost)*{vatPercentage} as TaxOrVatAmount,
                           sum(customercost)*(1+{vatPercentage}) as GrandTotalAmount
                           from {invoiceSectionGeneratorData.CdrOrSummaryTableName}                          
                           where {invoiceSectionGeneratorData.GetWhereClauseForDateCustomerId("tup_inPartnerId")};";
            return base.GetInvoiceSection<InvoiceSectionDataRowForA2ZVoice>(invoiceSectionGeneratorData, sql);
        }
    }
}
