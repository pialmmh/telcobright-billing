using System;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation.Accounting;

namespace PartnerRules
{
    [Export("InvoiceSectionGenerator", typeof(IInvoiceSectionGenerator))]
    public class A2ZInvoiceSectionStd1Genarator : IInvoiceSectionGenerator
    {
        public string SectionType => this.GetType().Name;
        public InvoiceSection GetInvoiceSection(InvoiceSectionGeneratorData invoiceSectionGeneratorData)
        {
            InvoiceSectionCreator<InvoiceSectionDataRowForVoiceCall>
                invoiceSectionCreator = new InvoiceSectionCreator<InvoiceSectionDataRowForVoiceCall>(
                    invoicePostProcessingData: invoiceSectionGeneratorData.InvoicePostProcessingData, 
                    sectionNumber: invoiceSectionGeneratorData.SectionNumber,
                    templateName: invoiceSectionGeneratorData.TemplateName);
            string sql = $@"select                                                         
                           sum(successfulcalls)	as TotalCalls,    
                           sum(roundedduration)/60  as TotalMinutes,   
                           sum(customercost)  as Amount      
                           from {invoiceSectionGeneratorData.CdrOrSummaryTableName}                          
                           where {invoiceSectionCreator.GetWhereClauseForDateRange()};";
            return invoiceSectionCreator.CreateInvoiceSection(sql);
        }
    }
}
