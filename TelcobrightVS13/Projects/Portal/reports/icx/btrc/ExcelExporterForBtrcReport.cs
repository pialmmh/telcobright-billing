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
using System.Globalization;


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


        public static void createInternationalReportInExcel(ExcelWorksheet ws, List<InternationalReportRow> data, string cell, int cellNo, string partnerName)
        {
            int rowCount = data.Count;
            decimal sumOfincomingMinutes = data.Sum(r => r.incomingMinutes);
            decimal sumOfInNoOfCalls = data.Sum(r => r.inNoOfCalls);
            decimal sumOfOutgoingMinutes = data.Sum(r => r.outgoingMinutes);
            decimal sumOfOutNoOfCalls = data.Sum(r => r.outNoOfCalls);
            string reportStartCol = cell;
            int reportStartRow = cellNo;
            int totalStartRow = reportStartRow + rowCount + 1;
            string reportStartRange = reportStartCol + reportStartRow.ToString();
            string totalStartRange = reportStartCol + totalStartRow.ToString();
            ws.Cells[reportStartRange].LoadFromCollection<InternationalReportRow>(data, true);
            ws.Cells[totalStartRange].Value = "Total";
            ws.Cells[totalStartRange].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Cells[totalStartRange].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells[totalStartRange].Style.Font.Bold = true;
            string nextCol = getNextRangeAtRight(totalStartRange);
            ws.Cells[nextCol].Value = $"{sumOfInNoOfCalls:n0}";
            ws.Cells[nextCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            ws.Cells[nextCol].Style.Font.Bold = true;
            ws.Cells[reportStartRange].Value = partnerName;
            ws.Cells[reportStartRange].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Cells[reportStartRange].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            nextCol = getNextRangeAtRight(reportStartRange);
            ws.Cells[nextCol].Value = "No.of Calls";
            ws.Cells[nextCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Cells[nextCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            nextCol = getNextRangeAtRight(nextCol);
            ws.Cells[nextCol].Value = "No.of Minutes";
            ws.Cells[nextCol].Style.Font.Bold = true;
            ws.Cells[nextCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Cells[nextCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            nextCol = getNextRangeAtRight(nextCol);
            ws.Cells[nextCol].Value = "No.of Calls";
            ws.Cells[nextCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Cells[nextCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            nextCol = getNextRangeAtRight(nextCol);
            ws.Cells[nextCol].Value = "No.of Minutes";
            ws.Cells[nextCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Cells[nextCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            nextCol = getNextRangeAtRight(totalStartRange);
            nextCol = getNextRangeAtRight(nextCol);
            ws.Cells[nextCol].Value = $"{sumOfincomingMinutes:n0}";
            ws.Cells[nextCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            ws.Cells[nextCol].Style.Font.Bold = true;
            nextCol = getNextRangeAtRight(nextCol);
            ws.Cells[nextCol].Value = $"{sumOfOutNoOfCalls:n0}";
            ws.Cells[nextCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            ws.Cells[nextCol].Style.Font.Bold = true;
            nextCol = getNextRangeAtRight(nextCol);
            ws.Cells[nextCol].Value = $"{sumOfOutgoingMinutes:n0}";
            ws.Cells[nextCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            ws.Cells[nextCol].Style.Font.Bold = true;
            styleAllBorder(ws, reportStartRange + ":" + nextCol);
            return;
        }

        public static void createBtrcReportInExcel(ExcelWorksheet ws, List<BtrcReportRow> data, string cell, int cellNo, string partnerCat)
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
            styleBoldWithCenter(ws, totalStartRange);
            string nextCol = getNextRangeAtRight(totalStartRange);
            ws.Cells[nextCol].Value = $"{sum:n0}";
            ws.Cells[nextCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            ws.Cells[nextCol].Style.Font.Bold = true;
            styleAllBorder(ws, reportStartRange + ":" + nextCol);
            ws.Cells[reportStartRange].Value = partnerCat;
            ws.Cells[reportStartRange].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Cells[reportStartRange].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            string mntCol = getNextRangeAtRight(reportStartRange);
            ws.Cells[mntCol].Value = "No.of Minutes";
            ws.Cells[mntCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Cells[mntCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            return;
        }

        private static void styleBoldWithCenter(ExcelWorksheet ws, string cell)
        {
            ws.Cells[cell].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Cells[cell].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells[cell].Style.Font.Bold = true;
        }

        private static void styleAlignmentWithBold(ExcelWorksheet ws, string cell)
        {
            ws.Cells[cell].Style.Font.Bold = true;
            ws.Cells[cell].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Cells[cell].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        public static void createBtrcHeader(ExcelWorksheet ws, string headerCell, int cellNo, string header)
        {
            if (!ws.Cells[headerCell].Merge) ws.Cells[headerCell].Merge = true;
            ws.Cells[headerCell].Value = header;
            ws.Cells[headerCell].Style.Font.Bold = true;
            ws.Cells[headerCell].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Cells[headerCell].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            styleAllBorder(ws, headerCell);
            ws.Cells["A" + cellNo + ":B" + cellNo].Style.Font.Bold = true;
            ws.Cells["D" + cellNo + ":E" + cellNo].Style.Font.Bold = true;
            return;
        }

        private static void styleAllBorder(ExcelWorksheet ws, string cell)
        {
            ws.Cells[cell].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            ws.Cells[cell].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            ws.Cells[cell].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            ws.Cells[cell].Style.Border.Left.Style = ExcelBorderStyle.Thin;
        }

        public static bool ExportToExcelMonthlyRoamingReport(string filename, HttpResponse response, List<SumDataOfMonth> monthlyRecords, string partnerName)
        {
            try
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    //Create the worksheet
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Sheet1");


                    //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                    string icxCell = "A2";


                    ws.Cells["B3"].Style.Font.Bold = true;

                    createBtrcHeader(ws, "A1:H1", 8, $@"Monthly Roaming Report of -{ partnerName }");
                    //createBtrcHeader(ws, "A2:A3", 8, "Sl No");
                    //for (int i=1;i<monthlyRecords.Count;i++) ws.Cells[$"A{4+i.ToString()}"].Value= i;
                    createBtrcHeader(ws, "A2", 8, "Start Time");
                    createBtrcHeader(ws, "B2", 8, "Source Network");
                    createBtrcHeader(ws, "C2", 8, "Destination Network");
                    createBtrcHeader(ws, "D2", 8, "Caller Number (A NUM)");
                    createBtrcHeader(ws, "E2", 8, "Called Number (B Num)");
                    createBtrcHeader(ws, "F2", 8, "Redirect Number");
                    createBtrcHeader(ws, "G2", 8, "Billed Duration");
                    createBtrcHeader(ws, "H2", 8, "Remarks");







                    int rowCount = monthlyRecords.Count;
                    
                    string reportStartCol = "A";
                    int reportStartRow = 2;
                    int totalStartRow = reportStartRow + rowCount + 1;
                    string reportStartRange = reportStartCol + reportStartRow.ToString();
                    string totalStartRange = reportStartCol + totalStartRow.ToString();
                    ws.Cells[reportStartRange].LoadFromCollection<SumDataOfMonth>(monthlyRecords, true);
                   // ws.Cells[totalStartRange].Value = "Total";
                    ws.Cells[totalStartRange].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[totalStartRange].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[totalStartRange].Style.Font.Bold = true;
                    string nextCol = getNextRangeAtRight(totalStartRange);
                    nextCol = getNextRangeAtRight(nextCol);
                    //ws.Cells[nextCol].Value = $"{sum:n0}";
                    ws.Cells[nextCol].Style.Font.Bold = true;
                    ws.Cells[nextCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[nextCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    nextCol = getNextRangeAtRight(nextCol);
                    //ws.Cells[nextCol].Value = $"{sum2:n0}";
                    ws.Cells[nextCol].Style.Font.Bold = true;
                    ws.Cells[nextCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[nextCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    nextCol = getNextRangeAtRight(nextCol);
                    //ws.Cells[nextCol].Value = $"{sum3:n0}";
                    ws.Cells[nextCol].Style.Font.Bold = true;
                    ws.Cells[nextCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[nextCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    nextCol = getNextRangeAtRight(nextCol);
                    //ws.Cells[nextCol].Value = $"{sum4:n0}";
                    ws.Cells[nextCol].Style.Font.Bold = true;
                    ws.Cells[nextCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[nextCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    nextCol = getNextRangeAtRight(nextCol);
                    //ws.Cells[nextCol].Value = $"{sum5:n0}";
                    ws.Cells[nextCol].Style.Font.Bold = true;
                    ws.Cells[nextCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[nextCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    nextCol = getNextRangeAtRight(nextCol);
                    //ws.Cells[nextCol].Value = $"{sum6:n0}";
                    ws.Cells[nextCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[nextCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[nextCol].Style.Font.Bold = true;
                    ws.Cells[reportStartRange + ":" + nextCol].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[reportStartRange + ":" + nextCol].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ws.Cells[reportStartRange + ":" + nextCol].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ws.Cells[reportStartRange + ":" + nextCol].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    ws.Cells[reportStartRange].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[reportStartRange].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    string mntCol = getNextRangeAtRight(reportStartRange);
                    //ws.Cells[mntCol].Value = "No.of Minutes";
                    ws.Cells[mntCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[mntCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                   

                    ws.Cells["A8:B8"].Style.Font.Bold = false;
                    ws.Cells["D8:E8"].Style.Font.Bold = false;
                    ws.Column(1).Width = 14;
                    ws.Column(3).Width = 14;
                    ws.Column(4).Width = 14;
                    ws.Column(5).Width = 14;
                    ws.Column(7).Width = 14;
                    ws.Column(8).Width = 14;
                    // Set baseColumns to auto-fit
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


        public static bool ExportToExcelMonthlyReport(string filename, HttpResponse response, List<MonthlyReportRowForExcel> monthlyRecords, string partnerName)
        {
            try
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    //Create the worksheet
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Sheet1");


                    //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                    string icxCell = "A2";

        
                    ws.Cells["B3"].Style.Font.Bold = true;

                    createBtrcHeader(ws, "A1:H1", 8, $@"Name of ICX -{ partnerName }");
                    //createBtrcHeader(ws, "A2:A3", 8, "Sl No");
                    //for (int i=1;i<monthlyRecords.Count;i++) ws.Cells[$"A{4+i.ToString()}"].Value= i;
                    createBtrcHeader(ws, "A2:A3", 8, "SL No.");
                    createBtrcHeader(ws, "B2:B3", 8, "Date");
                    createBtrcHeader(ws, "C2:D2", 8, "International Incoming");
                    createBtrcHeader(ws, "E2:F2", 8, "International Outgoing");
                    createBtrcHeader(ws, "G2:H2", 8, "Domestic Calls");





                    //createBtrcReportInExcel(ws, monthlyRecords, "A",3, "No of Calls");
                    int rowCount = monthlyRecords.Count;
                    decimal sum = monthlyRecords.Sum(r => r.IntIncomingNoOfCalls);
                    decimal sum2 = monthlyRecords.Sum(r => r.IntIncomingNoOfMinutes);
                    decimal sum3 = monthlyRecords.Sum(r => r.IntOutgoingNoOfCalls);
                    decimal sum4 = monthlyRecords.Sum(r => r.IntOutgoingNoOfMinutes);
                    decimal sum5 = monthlyRecords.Sum(r => r.domNoOfCalls);
                    decimal sum6 = monthlyRecords.Sum(r => r.domesticMinutes);
                    string reportStartCol = "A";
                    int reportStartRow = 3;
                    int totalStartRow = reportStartRow + rowCount + 1;
                    string reportStartRange = reportStartCol + reportStartRow.ToString();
                    string totalStartRange = reportStartCol + totalStartRow.ToString();
                    ws.Cells[reportStartRange].LoadFromCollection<MonthlyReportRowForExcel>(monthlyRecords, true);
                    ws.Cells[totalStartRange].Value = "Total";
                    ws.Cells[totalStartRange].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[totalStartRange].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[totalStartRange].Style.Font.Bold = true;
                    string nextCol = getNextRangeAtRight(totalStartRange);
                    nextCol = getNextRangeAtRight(nextCol);
                    ws.Cells[nextCol].Value = $"{sum:n0}";
                    ws.Cells[nextCol].Style.Font.Bold = true;
                    ws.Cells[nextCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[nextCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    nextCol = getNextRangeAtRight(nextCol);
                    ws.Cells[nextCol].Value = $"{sum2:n0}";
                    ws.Cells[nextCol].Style.Font.Bold = true;
                    ws.Cells[nextCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[nextCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    nextCol = getNextRangeAtRight(nextCol);
                    ws.Cells[nextCol].Value = $"{sum3:n0}";
                    ws.Cells[nextCol].Style.Font.Bold = true;
                    ws.Cells[nextCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[nextCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    nextCol = getNextRangeAtRight(nextCol);
                    ws.Cells[nextCol].Value = $"{sum4:n0}";
                    ws.Cells[nextCol].Style.Font.Bold = true;
                    ws.Cells[nextCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[nextCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    nextCol = getNextRangeAtRight(nextCol);
                    ws.Cells[nextCol].Value = $"{sum5:n0}";
                    ws.Cells[nextCol].Style.Font.Bold = true;
                    ws.Cells[nextCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[nextCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    nextCol = getNextRangeAtRight(nextCol);
                    ws.Cells[nextCol].Value = $"{sum6:n0}";
                    ws.Cells[nextCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[nextCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[nextCol].Style.Font.Bold = true;
                    ws.Cells[reportStartRange + ":" + nextCol].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[reportStartRange + ":" + nextCol].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ws.Cells[reportStartRange + ":" + nextCol].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ws.Cells[reportStartRange + ":" + nextCol].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    ws.Cells[reportStartRange].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[reportStartRange].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    string mntCol = getNextRangeAtRight(reportStartRange);
                    //ws.Cells[mntCol].Value = "No.of Minutes";
                    ws.Cells[mntCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[mntCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    createBtrcHeader(ws, "C3", 8, "Total No Of Calls");
                    createBtrcHeader(ws, "D3", 8, "Total No Of Minutes");
                    createBtrcHeader(ws, "E3", 8, "Total No Of Calls");
                    createBtrcHeader(ws, "F3", 8, "Total No Of Minutes");
                    createBtrcHeader(ws, "G3", 8, "Total No Of Calls");
                    createBtrcHeader(ws, "H3", 8, "Total No Of Minutes");

                    ws.Cells["A8:B8"].Style.Font.Bold = false;
                    ws.Cells["D8:E8"].Style.Font.Bold = false;
                    ws.Column(1).Width = 14;
                    ws.Column(3).Width = 14;
                    ws.Column(4).Width = 14;
                    ws.Column(5).Width = 14;
                    ws.Column(7).Width = 14;
                    ws.Column(8).Width = 14;
                    // Set baseColumns to auto-fit
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


        public static bool ExportToExcelMonthlyOutgoingReport(string filename, HttpResponse response, List<MonthlyOutSummary> monthlyRecords, string partnerName)
        {
            try
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    //Create the worksheet
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Sheet1");


                    //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                    //string icxCell = "A2";


                    //ws.Cells["B3"].Style.Font.Bold = true;

                    //createBtrcHeader(ws, "A1:H1", 8, $@"Name of ICX -{ partnerName }");





                    int rowCount = monthlyRecords.Count;
                    string reportStartCol = "A";
                    int reportStartRow = 1;
                    int totalStartRow = reportStartRow + rowCount + 1;
                    string reportStartRange = reportStartCol + reportStartRow.ToString();
                    string totalStartRange = reportStartCol + totalStartRow.ToString();
                    ws.Cells[reportStartRange].LoadFromCollection<MonthlyOutSummary>(monthlyRecords, true);
        
                    // Set baseColumns to auto-fit
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

        //My Code
        public static bool ExportToExcelMonthlyOutgoingDetailReport(string filename, HttpResponse response, List<MonthlyOutSummaryDetail> monthlyRecords, string partnerName)
        {
            try
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    //Create the worksheet
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Sheet1");


                    //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                    //string icxCell = "A2";


                    //ws.Cells["B3"].Style.Font.Bold = true;

                    //createBtrcHeader(ws, "A1:H1", 8, $@"Name of ICX -{ partnerName }");





                    int rowCount = monthlyRecords.Count;
                    string reportStartCol = "A";
                    int reportStartRow = 1;
                    int totalStartRow = reportStartRow + rowCount + 1;
                    string reportStartRange = reportStartCol + reportStartRow.ToString();
                    string totalStartRange = reportStartCol + totalStartRow.ToString();
                    ws.Cells[reportStartRange].LoadFromCollection<MonthlyOutSummaryDetail>(monthlyRecords, true);

                    // Set baseColumns to auto-fit
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

        public static bool ExportToExcelMonthlyCustomReport(string filename, HttpResponse response, List<CustomReportRow> monthlyRecords, string partnerName)
        {
            try
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    //Create the worksheet
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Sheet1");


                    //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                    //ws.Cells["B3"].Style.Font.Bold = true;
                    //createBtrcHeader(ws, "A1:H1", 8, $@"Name of ICX -{ partnerName }");

                    int rowCount = monthlyRecords.Count;
                    string reportStartCol = "A";
                    int reportStartRow = 1;
                    int totalStartRow = reportStartRow + rowCount + 1;
                    string reportStartRange = reportStartCol + reportStartRow.ToString();
                    string totalStartRange = reportStartCol + totalStartRow.ToString();
                    ws.Cells[reportStartRange].LoadFromCollection<CustomReportRow>(monthlyRecords, true);

                    //ws.Cells[reportStartRange + ":" + nextCol].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    //ws.Cells[reportStartRange + ":" + nextCol].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    //ws.Cells[reportStartRange + ":" + nextCol].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    //ws.Cells[reportStartRange + ":" + nextCol].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    //ws.Cells[reportStartRange].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    //ws.Cells[reportStartRange].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    //string mntCol = getNextRangeAtRight(reportStartRange);
                    ////ws.Cells[mntCol].Value = "No.of Minutes";
                    //ws.Cells[mntCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    //ws.Cells[mntCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                    // Set baseColumns to auto-fit
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
        public static bool ExportToExcelDomesticWeeklyReport(string filename, HttpResponse response, List<DomesticReportRow> domesticRecords, string startDate, string endDate, string partnerName
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
                    ws.Cells["B2"].Value = partnerName;
                    ws.Cells["B2"].Style.Font.Bold = true;
                    string dateCell = "A3";
                    ws.Cells[dateCell].Value = "Date:";
                    ws.Cells[dateCell].Style.Font.Bold = true;
                    ws.Cells[dateCell].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[dateCell].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["B3"].Value = DateTime.ParseExact(startDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd") + " - " + DateTime.ParseExact(endDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                    ws.Cells["B3"].Style.Font.Bold = true;
                    ws.Cells["A7"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells["A7"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ws.Cells["A7"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ws.Cells["A7"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    createBtrcHeader(ws, "B7:C7", 1, "Weekly Domestic Calls");
                    int rowCount = domesticRecords.Count;
                    decimal sum = domesticRecords.Sum(r => r.minutes);
                    decimal sumOfCalls = domesticRecords.Sum(r => r.noOfCalls);
                    string reportStartCol = "A";
                    int reportStartRow = 8;
                    int totalStartRow = reportStartRow + rowCount + 1;
                    string reportStartRange = reportStartCol + reportStartRow.ToString();
                    string totalStartRange = reportStartCol + totalStartRow.ToString();
                    ws.Cells[reportStartRange].LoadFromCollection<DomesticReportRow>(domesticRecords, true);
                    ws.Cells[totalStartRange].Value = "Total";
                    ws.Cells[totalStartRange].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[totalStartRange].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[totalStartRange].Style.Font.Bold = true;
                    string nextCol = getNextRangeAtRight(totalStartRange);
                    ws.Cells[nextCol].Value = $"{sumOfCalls:n0}";
                    ws.Cells[nextCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[nextCol].Style.Font.Bold = true;
                    nextCol = getNextRangeAtRight(nextCol);
                    ws.Cells[nextCol].Value = $"{sum:n0}";
                    ws.Cells[nextCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[nextCol].Style.Font.Bold = true;
                    ws.Cells[reportStartRange + ":" + nextCol].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[reportStartRange + ":" + nextCol].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ws.Cells[reportStartRange + ":" + nextCol].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ws.Cells[reportStartRange + ":" + nextCol].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    ws.Cells[reportStartRange].Value = "Name of ANS";
                    ws.Cells[reportStartRange].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[reportStartRange].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    string mntCol = getNextRangeAtRight(reportStartRange);
                    ws.Cells[mntCol].Value = "No.of Calls";
                    ws.Cells[mntCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[mntCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    mntCol = getNextRangeAtRight(mntCol);
                    ws.Cells[mntCol].Value = "No.of Minutes";
                    ws.Cells[mntCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[mntCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    //createBtrcReportInExcel(ws, domesticRecords, "A", 8, "Name of ANS");

                    // Set baseColumns to auto-fit
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

        // Acd Report
        public static bool AcdReport(string filename, HttpResponse response, List<AcdReportRow> acdRecords, string startDate, string endDate, string partnerName
        )
        {
            try
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    //Create the worksheet
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Sheet1");

                    int rowCount = acdRecords.Count;
                    string reportStartCol = "A";
                    int reportStartRow = 1;
                    int totalStartRow = reportStartRow + rowCount + 1;
                    string reportStartRange = reportStartCol + reportStartRow.ToString();
                    string totalStartRange = reportStartCol + totalStartRow.ToString();
                    ws.Cells[reportStartRange].LoadFromCollection<AcdReportRow>(acdRecords, true);

                    // Set baseColumns to auto-fit
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
        public static bool ExportToExcelInternationalWeeklyReport(string filename, HttpResponse response, List<InternationalReportRow> internationalReports,
            string startDate, string endDate, string partnerName
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
                    styleAlignmentWithBold(ws, icxCell);
                    ws.Cells["B2:C2"].Merge = true;
                    icxCell = getNextRangeAtRight(icxCell);
                    ws.Cells[icxCell].Value = partnerName;
                    ws.Cells[icxCell].Style.Font.Bold = true;
                    string dateCell = "A3";
                    ws.Cells[dateCell].Value = "Date:";
                    styleAlignmentWithBold(ws,dateCell);
                    dateCell = getNextRangeAtRight(dateCell);
                    ws.Cells[dateCell].Value = /*startDate*/DateTime.ParseExact(startDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                    ws.Cells[dateCell].Style.Font.Bold = true;
                    dateCell = getNextRangeAtRight(dateCell);
                    ws.Cells[dateCell].Value = /*endDate*/ DateTime.ParseExact(endDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                    ws.Cells[dateCell].Style.Font.Bold = true;
                    string headerCell = "D7:E7";
                    ws.Cells[headerCell].Merge = true;
                    ws.Cells[headerCell].Value = "   International Outgoing Calls   ";
                    styleAlignmentWithBold(ws, headerCell);
                    createBtrcHeader(ws, "B7:C7", 8, "International Incoming Calls");
                    string emptyCell = "A7";
                    headerCell = getNextRangeAtRight(emptyCell);
                    headerCell = getNextRangeAtRight(headerCell);
                    headerCell = getNextRangeAtRight(headerCell);
                    headerCell = getNextRangeAtRight(headerCell);
                    styleAllBorder(ws, emptyCell + ":" + headerCell);



                    createInternationalReportInExcel(ws, internationalReports, "A", 8, "Name of IOS");


                    // Set baseColumns to auto-fit
                    for (int i = 1; i <= ws.Dimension.Columns; i++)
                    {
                        ws.Column(i).AutoFit();
                    }
                    ws.Column(2).Width = 14;
                    ws.Column(3).Width = 14;
                    ws.Column(4).Width = 14;
                    ws.Column(5).Width = 14;
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
            List<BtrcReportRow> intOutComing_2_Records,
            List<InternationalReportRow> international_Daily_Records,
            string startDate,
            string partnerName
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
                    ws.Cells["B2"].Value = partnerName;
                    ws.Cells["B2"].Style.Font.Bold = true;
                    string dateCell = "A3";
                    ws.Cells[dateCell].Value = "Date:";
                    ws.Cells[dateCell].Style.Font.Bold = true;
                    ws.Cells[dateCell].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[dateCell].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["B3"].Value = DateTime.ParseExact(startDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                    ws.Cells["B3"].Style.Font.Bold = true;

                    createBtrcHeader(ws, "A7:E7", 8, "International Incoming Calls");

                    createBtrcReportInExcel(ws, intInComing_1_Records, "A", 8, "Name of ANS");

                    createBtrcReportInExcel(ws, intInComing_2_Records, "D", 8, "Name of IOS");

                    createBtrcHeader(ws, "A19:E19", 20, "International Outgoing Calls");

                    createBtrcReportInExcel(ws, intOutComing_1_Records, "A", 20, "Name of ANS");

                    createBtrcReportInExcel(ws, intOutComing_2_Records, "D", 20, "Name of IOS");

                    createBtrcHeader(ws, "A29:B29", 30, "Domestic Calls");

                    createBtrcReportInExcel(ws, domesticRecords, "A", 30, "Name of ANS");

                    // Set baseColumns to auto-fit
                    for (int i = 1; i <= ws.Dimension.Columns; i++)
                    {
                        ws.Column(i).AutoFit();
                    }

                    ExcelWorksheet ws2 = pck.Workbook.Worksheets.Add("Sheet2");


                    icxCell = "A2";
                    ws2.Cells[icxCell].Value = "NAME OF ICX:";
                    ws2.Cells[icxCell].Style.Font.Bold = true;
                    ws2.Cells[icxCell].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws2.Cells[icxCell].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws2.Cells["B2:C2"].Merge = true;
                    icxCell = getNextRangeAtRight(icxCell);
                    ws2.Cells[icxCell].Value = partnerName;
                    ws2.Cells[icxCell].Style.Font.Bold = true;
                    dateCell = "A3";
                    ws2.Cells[dateCell].Value = "Date:";
                    ws2.Cells[dateCell].Style.Font.Bold = true;
                    ws2.Cells[dateCell].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws2.Cells[dateCell].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    dateCell = getNextRangeAtRight(dateCell);
                    ws2.Cells[dateCell].Value = /*startDate*/DateTime.ParseExact(startDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                    ws2.Cells[dateCell].Style.Font.Bold = true;
                    dateCell = getNextRangeAtRight(dateCell);
                    //ws2.Cells[dateCell].Value = /*endDate*/ DateTime.ParseExact(endDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                    ws2.Cells[dateCell].Style.Font.Bold = true;
                    string headerCell = "D7:E7";
                    ws2.Cells[headerCell].Merge = true;
                    ws2.Cells[headerCell].Value = "   International Outgoing Calls   ";
                    ws2.Cells[headerCell].Style.Font.Bold = true;
                    ws2.Cells[headerCell].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws2.Cells[headerCell].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    createBtrcHeader(ws2, "B7:C7", 8, "International Incoming Calls");
                    string emptyCell = "A7";
                    headerCell = getNextRangeAtRight(emptyCell);
                    headerCell = getNextRangeAtRight(headerCell);
                    headerCell = getNextRangeAtRight(headerCell);
                    headerCell = getNextRangeAtRight(headerCell);
                    styleAllBorder(ws2, emptyCell + ":" + headerCell);



                    createInternationalReportInExcel(ws2, international_Daily_Records, "A", 8, "Name of IOS");


                    // Set baseColumns to auto-fit
                    for (int i = 1; i <= ws2.Dimension.Columns; i++)
                    {
                        ws2.Column(i).AutoFit();
                    }
                    ws2.Column(2).Width = 14;
                    ws2.Column(3).Width = 14;
                    ws2.Column(4).Width = 14;
                    ws2.Column(5).Width = 14;

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