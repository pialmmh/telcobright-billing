using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Web;
using OfficeOpenXml.Style;

namespace PortalApp.ReportHelper
{
    
    public class ExcelExporterForBtrcReport
    {
        static bool isNumeric(char c)
        {
            int i = 0;
            if (int.TryParse(c.ToString(), out i) == false)
            {
                return false;
            }
            return true;
        }
        static string getNextRangeAtRight(string fromRange)
        {
            char[] characters = fromRange.ToCharArray();
            string col = string.Join("", characters.TakeWhile(c => isNumeric(c) == false)).ToLower();
            string row = fromRange.Substring(col.Length);
            int colAsciiValue = (int)(col.ToArray().First());
            int nextCol = colAsciiValue + 1;
            string nextColAsString = Convert.ToChar(nextCol).ToString().ToUpper();
            return nextColAsString + row;
        }

        //btrc
        public static bool ExportToExcelBtrcReport(string filename, HttpResponse response, List<BtrcReportRow> domesticRecords,
List<BtrcReportRow> intInComing_1_Records,
List<BtrcReportRow> intInComing_2_Records,
List<BtrcReportRow> intOutComing_1_Records,
List<BtrcReportRow> intOutComing_2_Records
)
        {
            try
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    //Create the worksheet
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Sheet1");


                    //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                    ws.PrinterSettings.PrintArea = ws.Cells["A:A,G:G"];

                    //ws.Cells["A7:E7"].Merge = true;
                    List<BtrcReportRow> report = domesticRecords;
                    int rowCount = report.Count;
                    decimal sum = report.Sum(r => r.minutes);
                    string reportStartCol = "A";
                    int reportStartRow = 1;
                    int totalStartRow = reportStartRow+rowCount+1;
                    string reportStartRange = reportStartCol + reportStartRow.ToString();
                    string totalStartRange = reportStartCol + totalStartRow.ToString();
                    ws.Cells[reportStartRange].LoadFromCollection<BtrcReportRow>(report, true);
                    ws.Cells[totalStartRange].Value = "Total";
                    string nextCol = getNextRangeAtRight(totalStartRange);
                    ws.Cells[nextCol].Value = sum;

                    //int noOfCols = tbl.Columns.Count;
                    //string lastColExcel = IndexToColumn(noOfCols);
                    //int noOfRows = tbl.Rows.Count;
                    //int summaryRowIndex = noOfRows + 1;
                    // Set columns to auto-fit
                    for (int i = 1; i <= ws.Dimension.Columns; i++)
                    {
                        ws.Column(i).AutoFit();
                    }
                    //set numberformat
                    //int dtColIndex = 0;
                    //foreach (DataColumn dc in tbl.Columns)
                    //{
                    //    string numberFormat = "";
                    //    if (dc.ExtendedProperties.ContainsKey("NumberFormat"))
                    //    {
                    //        numberFormat = dc.ExtendedProperties["NumberFormat"].ToString();
                    //        string excelColIndexAlphabet = IndexToColumn(dtColIndex + 1);
                    //        using (ExcelRange rng = ws.Cells[excelColIndexAlphabet + 2 + ":" + excelColIndexAlphabet + summaryRowIndex])
                    //        {
                    //            rng.Style.Numberformat.Format = numberFormat;
                    //            rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    //        }
                    //        //right align header text for number fields
                    //        using (ExcelRange rng = ws.Cells[excelColIndexAlphabet + 1 + ":" + excelColIndexAlphabet + 1])
                    //        {
                    //            rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    //        }
                    //    }
                    //    dtColIndex++;
                    //}
                    ////Format the header
                    //using (ExcelRange rng = ws.Cells["A1:" + lastColExcel + "1"])
                    //{
                    //    rng.Style.Font.Bold = true;
                    //    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                    //    rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));  //Set color to dark blue
                    //    rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    //    rng.Style.WrapText = true;
                    //    rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    //}
                    ////Format the SummaryRow
                    //using (ExcelRange rng = ws.Cells["A" + summaryRowIndex + ":" + lastColExcel + summaryRowIndex])
                    //{
                    //    rng.Style.Font.Bold = true;
                    //    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                    //    rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));  //Set color to dark blue
                    //    rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    //}
                    //Write it back to the client
                    response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    response.AddHeader("content-disposition", "attachment;  filename=" + filename + "");
                    response.BinaryWrite(pck.GetAsByteArray());
                    response.End();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Failed, exception thrown: " + ex.Message);
                return false;
            }
        }
    }
}