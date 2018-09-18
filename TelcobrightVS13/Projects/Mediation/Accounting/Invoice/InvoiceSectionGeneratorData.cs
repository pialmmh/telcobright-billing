using System;
using System.Collections.Generic;
using LibraryExtensions;
namespace TelcobrightMediation.Accounting
{
    public class InvoiceSectionGeneratorData
    {
        public InvoicePostProcessingData InvoicePostProcessingData { get; set; }
        public int SectionNumber { get; set; }
        public string TemplateName { get; set; }
        public string CdrOrSummaryTableName { get; set; }
        public Dictionary<string,string> JsonDetail { get; }
        public InvoiceSectionGeneratorData(InvoicePostProcessingData invoicePostProcessingData,
            int sectionNumber, string templateName, string cdrOrSummaryTableName)
        {
            InvoicePostProcessingData = invoicePostProcessingData;
            SectionNumber = sectionNumber;
            TemplateName = templateName;
            this.CdrOrSummaryTableName = cdrOrSummaryTableName;
            this.JsonDetail = invoicePostProcessingData.InvoiceGenerationInputData.JsonDetail;
        }
        public string GetWhereClauseForDateRange()
        {
            return this.InvoicePostProcessingData.GetWhereClauseForDateRange();
        }
        public string GetWhereClauseForServiceGroup() => $" serviceGroup={this.JsonDetail["idServiceGroup"]} ";
        public string GetWhereClauseForCustomerId(string colNameForInOrOutParnter) =>
            $" {colNameForInOrOutParnter}={this.JsonDetail["idPartner"]} ";

        public string GetWhereClauseForDateCustomerId(string colNameForInOrOutParnter) =>
            GetWhereClauseForDateRange() + " and " + GetWhereClauseForCustomerId(colNameForInOrOutParnter);
        
        public string GetWhereClauseForDatePartnerServiceGroup(string colNameForInOrOutParnter) =>
            GetWhereClauseForDateRange() + " and " + GetWhereClauseForServiceGroup()
            + " and " + GetWhereClauseForCustomerId(colNameForInOrOutParnter);
    }
}