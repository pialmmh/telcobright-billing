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


        public static void createBtrcReportInExcel(ExcelWorksheet ws, List<BtrcReportRow> data, string cell, int cellNo, string partnerName)
        {
            int rowCount = data.Count;
            decimal sum = data.Sum(r => r.minutes);
            string reportStartCol = cell;
            int reportStartRow = cellNo;
            int totalStartRow = reportStartRow + rowCount + 1;
            string reportStartRange = reportStartCol + reportStartRow.ToString();
            string totalStartRange = reportStartCol + totalStartRow.ToString();
            ws.Cells[reportStartRange].LoadFromCollection<BtrcReportRow>(data, true);
            ws.Cells[totalStartRange].Value = "Total";
            ws.Cells[totalStartRange].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Cells[totalStartRange].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells[totalStartRange].Style.Font.Bold = true;
            string nextCol = getNextRangeAtRight(totalStartRange);
            ws.Cells[nextCol].Value = $"{sum:n0}";
            ws.Cells[nextCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            ws.Cells[nextCol].Style.Font.Bold = true;
            ws.Cells[reportStartRange + ":" + nextCol].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            ws.Cells[reportStartRange + ":" + nextCol].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            ws.Cells[reportStartRange + ":" + nextCol].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            ws.Cells[reportStartRange + ":" + nextCol].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            ws.Cells[reportStartRange].Value = partnerName;
            ws.Cells[reportStartRange].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Cells[reportStartRange].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            string mntCol = getNextRangeAtRight(reportStartRange);
            ws.Cells[mntCol].Value = "No.of Minutes";
            ws.Cells[mntCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Cells[mntCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            return;
        }

        public static void createBtrcHeader(ExcelWorksheet ws,string headerCell,int cellNo,string header)
        {
                    ws.Cells[headerCell].Merge = true;
                    ws.Cells[headerCell].Value = header;
                    ws.Cells[headerCell].Style.Font.Bold = true;
                    ws.Cells[headerCell].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[headerCell].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["A"+cellNo+":B"+cellNo].Style.Font.Bold = true;
                    ws.Cells["D"+ cellNo + ":E"+cellNo].Style.Font.Bold = true;
            return; 
         }


        //btrc
        public static bool ExportToExcelDomesticWeeklyReport(string filename, HttpResponse response, List<BtrcReportRow> domesticRecords
)
        {
            try
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    //Create the worksheet
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Sheet1");


                    //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                    string icxCell = "A2";
                    ws.Cells[icxCell].Value = "NAME OF ICX:";
                    ws.Cells[icxCell].Style.Font.Bold = true;
                    ws.Cells[icxCell].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[icxCell].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["B2:E2"].Merge = true;
                    ws.Cells["B2"].Value = "Summit Communication Ltd";
                    ws.Cells["B2"].Style.Font.Bold = true;
                    string dateCell = "A3";
                    ws.Cells[dateCell].Value = "Date:";
                    ws.Cells[dateCell].Style.Font.Bold = true;
                    ws.Cells[dateCell].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[dateCell].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["B3"].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    ws.Cells["B3"].Style.Font.Bold = true;

                    createBtrcHeader(ws, "A7:B7", 8, "Domestic Calls");

                    createBtrcReportInExcel(ws, domesticRecords, "A", 8, "Name of ANS");

                    // Set columns to auto-fit
                    for (int i = 1; i <= ws.Dimension.Columns; i++)
                    {
                        ws.Column(i).AutoFit();
                    }

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
                    string icxCell = "A2";
                    ws.Cells[icxCell].Value = "NAME OF ICX:";
                    ws.Cells[icxCell].Style.Font.Bold = true;
                    ws.Cells[icxCell].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[icxCell].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["B2:E2"].Merge = true;
                    ws.Cells["B2"].Value = "Summit Communication Ltd";
                    ws.Cells["B2"].Style.Font.Bold = true;
                    string dateCell = "A3";
                    ws.Cells[dateCell].Value = "Date:";
                    ws.Cells[dateCell].Style.Font.Bold = true;
                    ws.Cells[dateCell].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[dateCell].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["B3"].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    ws.Cells["B3"].Style.Font.Bold = true;

                    createBtrcHeader(ws, "A7:E7",8, "International Incoming Calls");
                    
                    createBtrcReportInExcel(ws, intInComing_1_Records, "A", 8, "Name of ANS");

                    createBtrcReportInExcel(ws, intInComing_2_Records, "D", 8, "Name of IOS");

                    createBtrcHeader(ws, "A19:E19", 20, "International Outgoing Calls");
                    
                    createBtrcReportInExcel(ws, intOutComing_1_Records, "A", 20, "Name of ANS");

                    createBtrcReportInExcel(ws, intOutComing_2_Records, "D", 20, "Name of IOS");

                    createBtrcHeader(ws, "A29:B29", 30, "Domestic Calls");
                   
                    createBtrcReportInExcel(ws, domesticRecords, "A", 30, "Name of ANS");
                   
                    // Set columns to auto-fit
                    for (int i = 1; i <= ws.Dimension.Columns; i++)
                    {
                        ws.Column(i).AutoFit();
                    }


                    
                    //int noOfCols = tbl.Columns.Count;
                    //string lastColExcel = IndexToColumn(noOfCols);
                    //int noOfRows = tbl.Rows.Count;
                    //int summaryRowIndex = noOfRows + 1;

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