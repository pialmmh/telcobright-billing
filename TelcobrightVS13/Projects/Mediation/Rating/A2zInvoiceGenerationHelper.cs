using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;
using Newtonsoft.Json;
using TelcobrightMediation;
using TelcobrightMediation.Accounting;

namespace InvoiceSectionGenerator
{
    public class A2ZInvoiceSectionGenerator
    {
        
        public static InvoiceSection
            GetInvoiceSection2(InvoicePostProcessingData invoicePostProcessingData)
        {
            InvoiceSectionCreator<InvoiceSectionDataRowForVoiceCall>
                invoiceSectionCreator = new InvoiceSectionCreator<InvoiceSectionDataRowForVoiceCall>(
                    invoicePostProcessingData: invoicePostProcessingData, sectionNumber: 2,
                    templateName: "InternationalOutgoingToANSDetails1");
            string sql = $@"select p.partnername as OutPartnerName,x.TotalCalls,x.TotalMinutes,x.XAmount,
                       x.YAmount,x.XYAmount,x.Revenue from
                       (select                                                         
                       tup_outpartnerid,
                       sum(successfulcalls 	)	as TotalCalls,    
                       sum(roundedduration   )/60  as TotalMinutes,   
                       sum(longDecimalAmount1)  as XAmount,
                       sum(longDecimalAmount2)  as YAmount,
                       sum(longDecimalAmount3)  as XYAmount,
                       sum(customercost      )  as Revenue      
                       from sum_voice_day_02                          
                       where {invoiceSectionCreator.GetWhereClauseForDateRange()}
                       group by tup_outpartnerid) x                     
                       left join partner p
                       on x.tup_outpartnerid=p.idpartner;";
            return invoiceSectionCreator.CreateInvoiceSection(sql);
        }
        public static InvoiceSection
            GetInvoiceSection3(InvoicePostProcessingData invoicePostProcessingData)
        {
            InvoiceSectionCreator<InvoiceSectionDataRowForVoiceCall>
                invoiceSectionCreator = new InvoiceSectionCreator<InvoiceSectionDataRowForVoiceCall>(
                    invoicePostProcessingData: invoicePostProcessingData, sectionNumber: 3,
                    templateName: "InternationalOutgoingToANSDetails2");
            string sql = $@"select                                                         
                          tup_starttime as `Date`,
                          sum(successfulcalls 	)	as TotalCalls,    
                          sum(roundedduration   )/60  as TotalMinutes,   
                          sum(longDecimalAmount1)  as XAmount,
                          sum(longDecimalAmount2)  as YAmount,
                          sum(longDecimalAmount3)  as XYAmount,
                          sum(customercost      )  as Revenue      
                          from sum_voice_day_02                          
                          where {invoiceSectionCreator.GetWhereClauseForDateRange()}
                          group by tup_starttime;";
            return invoiceSectionCreator.CreateInvoiceSection(sql);
        }
    }
}
