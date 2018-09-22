using System;
using System.Collections.Generic;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using System.Linq;
using LibraryExtensions;
using MediationModel;
using TelcobrightMediation.Accounting;

namespace InvoiceSectionGenerator
{
    class PrefixVsDestinationName
    {
        public string Prefix { get; set; }
        public string DestinationName { get; set; }
    }
    [Export("InvoiceSectionGenerator", typeof(IInvoiceSectionGenerator))]
    public class A2ZInvoiceSection2GeneratorMatchedPrefixCustomer : AbstractInvoiceSectionGenerator
    {
        public override string RuleName => this.GetType().Name;
        private InvoiceSectionGeneratorData InvoiceSectionGeneratorData { get; set; }
        public override InvoiceSection GetInvoiceSection(InvoiceSectionGeneratorData invoiceSectionGeneratorData)
        {
            this.InvoiceSectionGeneratorData = invoiceSectionGeneratorData;
            string sql = $@"select tup_matchedPrefixCustomer as Destination,tup_customerrate as Rate, 
                            TotalCalls,TotalMinutes, Amount from
                            (select tup_matchedPrefixCustomer, tup_outpartnerId,tup_customerrate,
                            sum(successfulcalls)	as TotalCalls,                                  
                            sum(duration1)/60  as TotalMinutes,                                   
                            sum(customercost)  as Amount                                          
                            from {invoiceSectionGeneratorData.CdrOrSummaryTableName}                          
                            where {invoiceSectionGeneratorData.GetWhereClauseForDateCustomerId("tup_inPartnerId")}
                            group by tup_matchedPrefixCustomer,tup_customerrate) x
                            where totalcalls>0;";
            
            List<InvoiceSectionDataRowForA2ZVoice> sectiondataRows =
                this.GetInvoiceSectionDataRowsOnly<InvoiceSectionDataRowForA2ZVoice>(invoiceSectionGeneratorData, sql);

            SetMatchedPrefixCustomerDestinationNames(sectiondataRows);

            return new InvoiceSection(sectionName: "Section-" + invoiceSectionGeneratorData.SectionNumber,
                templateName: invoiceSectionGeneratorData.TemplateName,
                serializedData: new JsonCompressor<List<InvoiceSectionDataRowForA2ZVoice>>()
                    .SerializeToCompressedBase64(sectiondataRows));
        }
        

        private void SetMatchedPrefixCustomerDestinationNames(List<InvoiceSectionDataRowForA2ZVoice> sectiondataRows)
        {
            List<string> distinctPrefixes = sectiondataRows.Select(c => c.Destination).Distinct().ToList();
            var context = this.InvoiceSectionGeneratorData.InvoicePostProcessingData.InvoiceGenerationInputData.Context;
            Dictionary<string, string> prefixVsDescription =
                context.Database.SqlQuery<PrefixVsDestinationName>(
                    $@"SELECT prefix as Prefix, min(name) as DestinationName FROM product
                   where prefix in ('{string.Join(",", distinctPrefixes)}')
                   group by prefix").ToList().ToDictionary(pvd=>pvd.Prefix,pvd=>pvd.DestinationName);
            sectiondataRows.ForEach(r=>
            {
                var prefixOnly = r.Destination;
                string description = "";
                prefixVsDescription.TryGetValue(prefixOnly, out description);
                if (!string.IsNullOrEmpty(description))
                {
                    r.Destination = r.Destination + $" ({description})";    
                }
            });
        }
    }
}
