using System;
using System.ComponentModel.Composition;
using TelcobrightMediation;
using TelcobrightMediation.Accounting;

namespace InvoiceSectionGenerator
{
    [Export("InvoiceSectionGenerator", typeof(IInvoiceSectionGenerator))]
    public class XyzSection1GeneratorWithTax : AbstractInvoiceSectionGenerator
    {
        public override string RuleName => this.GetType().Name;
        public override InvoiceSection GetInvoiceSection(InvoiceSectionGeneratorData invoiceSectionGeneratorData)
        {
            //var jsonDetail = invoiceSectionGeneratorData.JsonDetail;
            decimal vatPercentage = Convert.ToDecimal(invoiceSectionGeneratorData.InvoicePostProcessingData
                .JsonDetail["vat"]);
            string sql = $@"select                                                         
                       sum(successfulcalls 	)	as TotalCalls,    
                       sum(roundedduration   )/60  as TotalMinutes,   
                       sum(longDecimalAmount1)  as XAmount,
                       sum(longDecimalAmount2)  as YAmount,
                       sum(longDecimalAmount3)  as XYAmount,
                       sum(customercost      )  as Amount,
                       sum(customercost)*{vatPercentage} as TaxOrVatAmount,
                       sum(customercost)*(1+{vatPercentage}) as GrandTotalAmount
                       from {invoiceSectionGeneratorData.CdrOrSummaryTableName}                                              
                       where {invoiceSectionGeneratorData.GetWhereClauseForDateCustomerId("tup_inPartnerId")};";
            return base.GetInvoiceSection<InvoiceSectionDataRowForA2ZVoice>(invoiceSectionGeneratorData, sql);
        }
    }
}