using System;
using System.Collections.Generic;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation.Accounting;

namespace InvoiceSectionGenerator
{
    [Export("InvoiceSectionGenerator", typeof(IInvoiceSectionGenerator))]
    public class A2ZInvoiceSection1GeneratorWithCurrencyConversion : AbstractInvoiceSectionGenerator
    {
        public override string RuleName => this.GetType().Name;
        public override InvoiceSection GetInvoiceSection(InvoiceSectionGeneratorData invoiceSectionGeneratorData)
        {
            InvoicePostProcessingData invoicePostProcessingData = invoiceSectionGeneratorData.InvoicePostProcessingData;
            invoice invoice = invoicePostProcessingData.Invoice;
            decimal currencyConversionFactor = invoice.currencyConversionFactor;
            string sql = $@"select                                                         
                           sum(successfulcalls)	as TotalCalls,    
                           sum(duration1)/60  as TotalMinutes,   
                           sum(customercost)*{currencyConversionFactor}  as Amount      
                           from {invoiceSectionGeneratorData.CdrOrSummaryTableName}                          
                           where {invoiceSectionGeneratorData.GetWhereClauseForDateCustomerId("tup_inPartnerId")};";
            return base.GetInvoiceSection<InvoiceSectionDataRowForA2ZVoice>(invoiceSectionGeneratorData, sql);
        }
    }
}
