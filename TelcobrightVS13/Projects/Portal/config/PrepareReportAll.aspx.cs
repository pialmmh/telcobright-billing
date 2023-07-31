using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using iTextSharp.text;
using iTextSharp.text.pdf;
using MediationModel;
using TelcobrightMediation;
using Newtonsoft.Json;
namespace PortalApp.config
{
    public partial class PrepareReportAll : System.Web.UI.Page
    {
        private static TelcobrightConfig Tbc { get; set; }
        private static Dictionary<string, IInvoiceTemplate> invoiceTemplates { get; set; }

     
        protected void Page_Load(object sender, EventArgs e)
        {
            var reportNames = Request.QueryString["reportNames"];
            var invoiceId = Request.QueryString["invoiceId"];
            
            
            var templetNamesCommaSeparated = Request.QueryString["templetNamesCommaSeparated"];
            if (reportNames != null)
            {
                using (PartnerEntities context = new PartnerEntities())
                {
                    Tbc = PageUtil.GetTelcobrightConfig();
                    List<KeyValuePair<Regex, string>> serviceAliases = Tbc.ServiceAliasesRegex;
                    InvoiceTemplateComposer invoiceTemplateComposer = new InvoiceTemplateComposer();
                    DirectoryInfo dllDir = new DirectoryInfo(PageUtil.GetPortalBinPath()).Parent
                        .GetDirectories("Extensions")
                        .Single().GetDirectories("InvoiceTemplates").Single();
                    invoiceTemplateComposer.ComposeFromPath(dllDir.FullName);
                    invoiceTemplates = invoiceTemplateComposer.InvoiceTemplates.ToDictionary(c => c.TemplateName);

                    //String reportName = linkButton.CommandName;
                    //int startPos = reportNames.LastIndexOf("Template-", StringComparison.Ordinal) + "Template-".Length;
                    //int length = reportNames.Length - startPos;
                    //reportNames = $"{Tbc.DatabaseSetting.DatabaseName}#{reportNames.Substring(startPos, length)}";
                    List<string> TempReportNames = reportNames.Split(',').ToList();
                    List<string> generatedPdfPaths = new List<string>();

                    int INVOICE_ID = Convert.ToInt32(invoiceId);
                    invoice invoice = context.invoices.First(x => x.INVOICE_ID == INVOICE_ID);


                    for (int i = 0; i < TempReportNames.Count; i++)
                    {
                        TempReportNames[i] = TempReportNames[i].Replace("Template-", "").Trim();
                        reportNames = $"{Tbc.DatabaseSetting.DatabaseName}#{TempReportNames[i]}";
                        List<string> reportNamesAsList = reportNames.Split(',').ToList();

                        List<IInvoiceTemplate> templates = invoiceTemplates.Values
                            .Where(tmpl => reportNamesAsList.Contains(tmpl.TemplateName)).ToList();
                        for (int j = 0; j < templates.Count; j++)
                        {
                            IInvoiceTemplate template = templates[j];
                            //GridViewRow gvrow = (GridViewRow)linkButton.NamingContainer;
                            //int INVOICE_ID = Convert.ToInt32(gvInvoice.DataKeys[gvrow.RowIndex].Value);
                            
                            invoice_item invoiceItem = context.invoice_item.First(ii => ii.INVOICE_ID == invoice.INVOICE_ID);
                            Dictionary<string, string> jsonDetail = JsonConvert.DeserializeObject<Dictionary<string, string>>(invoiceItem.JSON_DETAIL);
                            List<long> mergedInvoiceIds = new List<long>();
                            List<invoice> mergedInvoices = new List<invoice>();
                            if (jsonDetail.ContainsKey("mergedInvoices")
                                && !string.IsNullOrEmpty(jsonDetail["mergedInvoices"]) && !string.IsNullOrWhiteSpace(jsonDetail["mergedInvoices"]))
                            {
                                mergedInvoiceIds = jsonDetail["mergedInvoices"].Split(',').Select(childInvoiceId => Convert.ToInt64(childInvoiceId)).ToList();
                            }
                            String refNo = Guid.NewGuid().ToString();
                            //int tempNum = 1;
                            //tempNum++;
                            if (mergedInvoiceIds.Any())
                            {
                                //mergedInvoices = context.Database.SqlQuery<invoice>($@"select * from invoice where invoice_id in ({string.Join(",", mergedInvoiceIds)})").ToList();

                                mergedInvoices = context.invoices.Where(iii => mergedInvoiceIds.Contains(iii.INVOICE_ID)).ToList();

                                Dictionary<string, object> invoiceWithMergeIds = new Dictionary<string, object>()
                                {
                                    { "invoice",invoice},
                                    { "mergedInvoice", mergedInvoices.First()}//do one for now
                                };
                                template.GenerateInvoice(invoiceWithMergeIds);
                                //template.SaveToPdf(@"C:\temp\abcd" + tempNum + ".pdf");
                                string pdfFileName = $"C:\\temp\\invoice_{Guid.NewGuid()}.pdf";
                                template.SaveToPdf(pdfFileName);
                                generatedPdfPaths.Add(pdfFileName);

                            }
                            else
                            {
                                template.GenerateInvoice(invoice);
                                
                                //template.SaveToPdf(@"C:\temp\abcd" + tempNum + ".pdf");
                                string pdfFileName = $"C:\\temp\\invoice_{Guid.NewGuid()}.pdf";
                                template.SaveToPdf(pdfFileName);
                                generatedPdfPaths.Add(pdfFileName);
                            }
                        }
                    }

                    // Call the updated method to merge the generated PDFs and get the merged PDF as a MemoryStream
                    //MemoryStream mergedPdfStream = MergeGeneratedPdfs(generatedPdfPaths);

                    // Show the preview using a PDF viewer control on the web page
                    //ShowPdfPreview(mergedPdfStream);


                    // Call the updated method to merge the generated PDFs and get the merged PDF as a MemoryStream
                    MemoryStream mergedPdfStream = MergeGeneratedPdfs(generatedPdfPaths);

                    // Add page numbers to the merged PDF
                    MemoryStream numberedPdfStream = AddPageNumbersToPdf(mergedPdfStream);
                    
                    // Show the preview using a PDF viewer control on the web page
                    
                    ShowPdfPreview(numberedPdfStream, invoice.DESCRIPTION);

                    //MergeGeneratedPdfs(generatedPdfPaths);
                    //string reportNamesTemp = string.Join(",", TempReportNames);

                    //else
                    //{
                    //    //template.GenerateInvoice(invoice);

                    //}
                    //this.Session[refNo] = template;
                    //Response.Redirect("~/config/ViewReport.aspx?refNo=" + HttpUtility.UrlEncode(refNo), false);
                    //Context.ApplicationInstance.CompleteRequest();
                }
            }
        }

        private MemoryStream MergeGeneratedPdfs(List<string> pdfFilePaths)
        {
            MemoryStream mergedPdfStream = new MemoryStream();

            using (Document document = new Document())
            {
                PdfCopy pdf = new PdfCopy(document, mergedPdfStream);

                document.Open();
                int totalNumPages = 0;
                

                foreach (string pdfFilePath in pdfFilePaths)
                {
                    PdfReader reader = new PdfReader(pdfFilePath);
                    totalNumPages += reader.NumberOfPages;
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        PdfImportedPage page = pdf.GetImportedPage(reader, i);
                        pdf.AddPage(page);
                    }
                    reader.Close();
                }
            }

            return mergedPdfStream;
        }
        private void ShowPdfPreview(MemoryStream pdfStream, string invoiceName)
        {
            // Set the appropriate content type for PDF
            Response.ContentType = "application/pdf";
            //Response.AddHeader("hhhh");

            // Provide a filename for the merged PDF, if needed
            
            Response.AddHeader("Content-Disposition", $"attachment; filename={invoiceName}.pdf");

            // Write the PDF content to the response stream for preview
            Response.BinaryWrite(pdfStream.ToArray());
            //Response.End();
        }
        private MemoryStream AddPageNumbersToPdf(MemoryStream pdfStream)
        {
            float xLocation = 270; // X-coordinate of the page number
            float yLocation = 40;  // Y-coordinate of the page number

            // Clone the pdfStream to avoid object disposed exception
            MemoryStream clonedPdfStream = new MemoryStream(pdfStream.ToArray());

            MemoryStream numberedPdfStream = new MemoryStream();
            using (PdfReader reader = new PdfReader(clonedPdfStream))
            using (PdfStamper stamper = new PdfStamper(reader, numberedPdfStream))
            {
                int totalNumPages = reader.NumberOfPages;

                for (int pageNum = 1; pageNum <= totalNumPages; pageNum++)
                {
                    PdfContentByte pdfContent = stamper.GetOverContent(pageNum);
                    Rectangle mediabox = reader.GetPageSize(pageNum);

                    pdfContent.BeginText();

                    // Set font and size for the page number text
                    BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    pdfContent.SetFontAndSize(baseFont, 10);

                    pdfContent.ShowTextAligned(0, $"Page {pageNum} of {totalNumPages}", xLocation, yLocation, 0);
                    pdfContent.EndText();
                }

                stamper.Close();
            }

            return numberedPdfStream;
        }
    }
}