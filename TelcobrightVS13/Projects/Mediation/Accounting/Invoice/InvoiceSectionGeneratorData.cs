namespace TelcobrightMediation.Accounting
{
    public class InvoiceSectionGeneratorData
    {
        public InvoicePostProcessingData InvoicePostProcessingData { get; set; }
        public int SectionNumber { get; set; }
        public string TemplateName { get; set; }
        public string CdrOrSummaryTableName { get; set; }
        public InvoiceSectionGeneratorData(InvoicePostProcessingData invoicePostProcessingData, 
            int sectionNumber, string templateName,string cdrOrSummaryTableName)
        {
            InvoicePostProcessingData = invoicePostProcessingData;
            SectionNumber = sectionNumber;
            TemplateName = templateName;
            this.CdrOrSummaryTableName = cdrOrSummaryTableName;
        }
    }
}