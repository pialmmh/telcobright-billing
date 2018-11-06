using System;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation.Accounting;

namespace InvoiceSectionGenerator
{
    [Export("InvoiceSectionGenerator", typeof(IInvoiceSectionGenerator))]
    public class XyzSection2GeneratorIgw : AbstractInvoiceSectionGenerator
    {
        public override string RuleName => this.GetType().Name;
        public override InvoiceSection GetInvoiceSection(InvoiceSectionGeneratorData invoiceSectionGeneratorData)
        {
            string sql = $@"select p.partnername as OutPartnerName,x.TotalCalls,x.TotalMinutes,x.XAmount,
                       x.YAmount,x.XYAmount,x.Revenue from
                       (select                                                         
                       tup_sourceId,
                       sum(successfulcalls 	)	as TotalCalls,    
                       sum(roundedduration   )/60  as TotalMinutes,   
                       sum(longDecimalAmount1)  as XAmount,
                       sum(longDecimalAmount2)  as YAmount,
                       sum(longDecimalAmount3)  as XYAmount,
                       sum(customercost      )  as Revenue      
                       from {invoiceSectionGeneratorData.CdrOrSummaryTableName}                                               
                       where {invoiceSectionGeneratorData.GetWhereClauseForDateCustomerId("tup_inPartnerId")}
                       and successfulcalls>0
                       group by tup_sourceId) x                     
                       left join partner p
                       on x.tup_sourceId=p.idpartner;";
            return base.GetInvoiceSection<InvoiceSectionDataRowForA2ZVoice>(invoiceSectionGeneratorData, sql);
        }
    }
}
