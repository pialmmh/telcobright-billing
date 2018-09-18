﻿using System;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation.Accounting;

namespace InvoiceSectionGenerator
{
    [Export("InvoiceSectionGenerator", typeof(IInvoiceSectionGenerator))]
    public class TollFreeInvoiceSection1Generator : AbstractInvoiceSectionGenerator
    {
        public override string RuleName => this.GetType().Name;
        public override InvoiceSection GetInvoiceSection(InvoiceSectionGeneratorData invoiceSectionGeneratorData)
        {
            string sql = $@"select                                                         
                           sum(successfulcalls)	as TotalCalls,    
                           sum(duration1)/60  as TotalMinutes,   
                           sum(customercost)  as Amount      
                           from {invoiceSectionGeneratorData.CdrOrSummaryTableName}                          
                           where {invoiceSectionGeneratorData.GetWhereClauseForDateCustomerId("tup_OutPartnerId")};";
            return base.GetInvoiceSection<InvoiceSectionDataRowForA2ZVoice>(invoiceSectionGeneratorData, sql);
        }
    }
}
