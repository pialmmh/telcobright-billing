#define INCLUDE_WEB_FUNCTIONS

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Web;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace PortalApp._portalHelper
{
    //
    //  November 2013
    //  http://www.mikesknowledgebase.com
    //
    //  Note: if you plan to use this in an ASP.Net application, remember to add a reference to "System.Web", and to uncomment
    //  the "INCLUDE_WEB_FUNCTIONS" definition at the top of this file.
    //
    //  Release history
    //   - Nov 2013: 
    //        Changed "CreateExcelDocument(DataTable dt, string xlsxFilePath)" to remove the DataTable from the DataSet after creating the Excel file.
    //        You can now create an Excel file via a Stream (making it more ASP.Net friendly)
    //   - Jan 2013: Fix: Couldn't open .xlsx files using OLEDB  (was missing "WorkbookStylesPart" part)
    //   - Nov 2012: 
    //        List<>s with Nullable columns weren't be handled properly.
    //        If a value in a numeric column doesn't have any data, don't write anything to the Excel file (previously, it'd write a '0')
    //   - Jul 2012: Fix: Some worksheets weren't exporting their numeric data properly, causing "Excel found unreadable content in '___.xslx'" errors.
    //   - Mar 2012: Fixed issue, where Microsoft.ACE.OLEDB.12.0 wasn't able to connect to the Excel files created using this class.
    //

    public class CreateExcelFileAspNet
    {
        public static bool CreateExcelDocument<T>(List<T> list, string xlsxFilePath)
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(ListToDataTable(list));

            return CreateExcelDocument(ds, xlsxFilePath);
        }
#region HELPER_FUNCTIONS
        //  This function is adapated from: http://www.codeguru.com/forum/showthread.php?t=450171
        //  My thanks to Carl Quirion, for making it "nullable-friendly".
        public static DataTable ListToDataTable<T>(List<T> list)
        {
            DataTable dt = new DataTable();

            foreach (PropertyInfo info in typeof(T).GetProperties())
            {
                dt.Columns.Add(new DataColumn(info.Name, GetNullableType(info.PropertyType)));
            }
            foreach (T t in list)
            {
                DataRow row = dt.NewRow();
                foreach (PropertyInfo info in typeof(T).GetProperties())
                {
                    if (!IsNullableType(info.PropertyType))
                        row[info.Name] = info.GetValue(t, null);
                    else
                        row[info.Name] = (info.GetValue(t, null) ?? DBNull.Value);
                }
                dt.Rows.Add(row);
            }
            return dt;
        }
        private static Type GetNullableType(Type t)
        {
            Type returnType = t;
            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                returnType = Nullable.GetUnderlyingType(t);
            }
            return returnType;
        }
        private static bool IsNullableType(Type type)
        {
            return (type == typeof(string) ||
                    type.IsArray ||
                    (type.IsGenericType &&
                     type.GetGenericTypeDefinition().Equals(typeof(Nullable<>))));
        }

        public static bool CreateExcelDocument(DataTable dt, string xlsxFilePath)
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(dt);
            bool result = CreateExcelDocument(ds, xlsxFilePath);
            ds.Tables.Remove(dt);
            return result;
        }
#endregion

#if INCLUDE_WEB_FUNCTIONS
        /// <summary>
        /// Create an Excel file, and write it out to a MemoryStream (rather than directly to a file)
        /// </summary>
        /// <param name="dt">DataTable containing the data to be written to the Excel.</param>
        /// <param name="filename">The filename (without a path) to call the new Excel file.</param>
        /// <param name="Response">HttpResponse of the current page.</param>
        /// <returns>True if it was created succesfully, otherwise false.</returns>
        //public static bool CreateExcelDocument(DataTable dt, string filename, System.Web.HttpResponse Response)
        //{
        //    try
        //    {
        //        DataSet ds = new DataSet();
        //        ds.Tables.Add(dt);
        //        CreateExcelDocumentAsStream(ds, filename, Response);
        //        ds.Tables.Remove(dt);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.WriteLine("Failed, exception thrown: " + ex.Message);
        //        return false;
        //    }
        //}


        public static ExcelPackage CreateExcelDocument<T>(List<T> list)
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(ListToDataTable(list));
            return CreateExcelDocumentAsStreamEpPlusPackage(ds);
        }
        public static void CreateExcelDocumentAndWriteBrowser<T>(List<T> list, string filename, HttpResponse response)
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(ListToDataTable(list));
            WriteExcelSheetBrowser(CreateExcelDocumentAsStreamEpPlusPackage(ds), filename, response);
        }
        public static void WriteExcelSheetBrowser(ExcelPackage pck, string filename, HttpResponse response)
        {
            //Write it back to the client
            response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            response.AddHeader("content-disposition", "attachment;  filename=" + filename + "");
            response.BinaryWrite(pck.GetAsByteArray());
            response.End();
        }
        //multiple sheet
        //public static bool CreateExcelDocument<T>(List<List<T>> list, string filename, System.Web.HttpResponse Response)
        //{
        //    //try
        //    {
        //        DataSet ds = new DataSet();
        //        foreach(List<T> table in list)
        //        {
        //            ds.Tables.Add(ListToDataTable(table));
        //        }
                
        //        CreateExcelDocumentAsStreamEpPlusPackage(ds, filename, Response);
        //        return true;
        //    }

        //}

        /// <summary>
        /// Create an Excel file, and write it out to a MemoryStream (rather than directly to a file)
        /// </summary>
        /// <param name="ds">DataSet containing the data to be written to the Excel.</param>
        /// <param name="filename">The filename (without a path) to call the new Excel file.</param>
        /// <param name="response">HttpResponse of the current page.</param>
        /// <returns>Either a MemoryStream, or NULL if something goes wrong.</returns>
        public static bool CreateExcelDocumentAsStream(DataSet ds, string filename, HttpResponse response)
        {
            try
            {
                MemoryStream stream = new MemoryStream();
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true))
                {
                    WriteExcelFile(ds, document);
                }
                stream.Flush();
                stream.Position = 0;

                response.ClearContent();
                response.Clear();
                response.Buffer = true;
                response.Charset = "";

                //  NOTE: If you get an "HttpCacheability does not exist" error on the following line, make sure you have
                //  manually added System.Web to this project's References.

                response.Cache.SetCacheability(HttpCacheability.NoCache);
                response.AddHeader("content-disposition", "attachment; filename=" + filename);
                response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                byte[] data1 = new byte[stream.Length];
                stream.Read(data1, 0, data1.Length);
                stream.Close();
                response.BinaryWrite(data1);
                response.Flush();
                response.End();

                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Failed, exception thrown: " + ex.Message);
                return false;
            }
        }


        public static bool CreateExcelDocumentAsStreamClosedXml(DataSet ds, string filename, HttpResponse response)
        {
            //this one threw out of memory exception for large file with about 60,000 ratetask
            //use epplus package instead
            
            try
            {
                var workbook = new ClosedXML.Excel.XLWorkbook();
                foreach (DataTable dt in ds.Tables)
                {
                    var worksheet = workbook.Worksheets.Add(dt.TableName);
                    worksheet.Cell(1, 1).InsertTable(dt);
                    worksheet.Columns().AdjustToContents();
                }
                //workbook.SaveAs(destination);
                //workbook.Dispose();
                // Prepare the response
                HttpResponse httpResponse = response;
                httpResponse.Clear();
                httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                httpResponse.AddHeader("content-disposition", "attachment;filename=\""+filename+".xlsx\"");

                // Flush the workbook to the Response.OutputStream
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.WriteTo(httpResponse.OutputStream);
                    memoryStream.Close();
                }

                httpResponse.End();
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Failed, exception thrown: " + ex.Message);
                return false;
            }
        }


        //public static bool CreateExcelDocumentAsStreamEpPlusPackage(DataSet ds, string filename, System.Web.HttpResponse Response)
        //{
        //    //this one threw out of memory exception for large file with about 60,000 ratetask
        //    //use epplus package instead

        //    try
        //    {

        //        using (ExcelPackage pck = new ExcelPackage())
        //        {
        //            //Create the worksheet
        //            DataTable tbl = ds.Tables[0];//
        //            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Sheet1");

        //            //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
        //            ws.Cells["A1"].LoadFromDataTable(tbl, true);

        //            //Format the header for column 1-3
        //            //using (ExcelRange rng = ws.Cells["A1:C1"])
        //            //{
        //            //    rng.Style.Font.Bold = true;
        //            //    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
        //            //    rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));  //Set color to dark blue
        //            //    rng.Style.Font.Color.SetColor(Color.White);
        //            //}

        //            ////Example how to Format Column 1 as numeric 
        //            //using (ExcelRange col = ws.Cells[2, 1, 2 + tbl.Rows.Count, 1])
        //            //{
        //            //    col.Style.Numberformat.Format = "#,##0.00";
        //            //    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
        //            //}

        //            //Write it back to the client
        //            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //            Response.AddHeader("content-disposition", "attachment;  filename="+filename+"");
        //            Response.BinaryWrite(pck.GetAsByteArray());
        //            Response.End();
        //            return true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.WriteLine("Failed, exception thrown: " + ex.Message);
        //        return false;
        //    }
        //}

        public static ExcelPackage CreateExcelDocumentAsStreamEpPlusPackage(DataSet ds)
        {
            //this one threw out of memory exception for large file with about 60,000 ratetask
            //use epplus package instead


            ExcelPackage pck = new ExcelPackage();

            for (int s = 0; s < ds.Tables.Count; s++)
            {
                //Create the worksheet
                DataTable tbl = ds.Tables[s];//
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Sheet" + (s + 1).ToString());

                //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                ws.Cells["A1"].LoadFromDataTable(tbl, true);

                //Format the header for column 1-3
                //using (ExcelRange rng = ws.Cells["A1:C1"])
                //{
                //    rng.Style.Font.Bold = true;
                //    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                //    rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));  //Set color to dark blue
                //    rng.Style.Font.Color.SetColor(Color.White);
                //}

                ////Example how to Format Column 1 as numeric 
                //using (ExcelRange col = ws.Cells[2, 1, 2 + tbl.Rows.Count, 1])
                //{
                //    col.Style.Numberformat.Format = "#,##0.00";
                //    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                //}
            }

            return pck;

        }


        static readonly string[] Columns = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "AA", "AB", "AC", "AD", "AE", "AF", "AG", "AH", "AI", "AJ", "AK", "AL", "AM", "AN", "AO", "AP", "AQ", "AR", "AS", "AT", "AU", "AV", "AW", "AX", "AY", "AZ", "BA", "BB", "BC", "BD", "BE", "BF", "BG", "BH" };
        public static string IndexToColumn(int index)
        {
            if (index <= 0)
                throw new IndexOutOfRangeException("index must be a positive number");

            return Columns[index - 1];
        }

        

        public static bool CreateExcelDocumentAsStreamEpPlusPackageLastRowSummary(DataTable tbl, string filename, HttpResponse response)
        {
            //this one threw out of memory exception for large file with about 60,000 ratetask
            //use epplus package instead

            try
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    //Create the worksheet
                    //DataTable tbl = ds.Tables[0];//
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Sheet1");

                    //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                    ws.Cells["A1"].LoadFromDataTable(tbl, true);
                    int noOfCols = tbl.Columns.Count;
                    string lastColExcel = IndexToColumn(noOfCols);
                    int noOfRows = tbl.Rows.Count;
                    int summaryRowIndex = noOfRows + 1;
                    // Set columns to auto-fit
                    for (int i = 1; i <= ws.Dimension.Columns; i++)
                    {
                        ws.Column(i).AutoFit();
                    }
                    //set numberformat
                    int dtColIndex = 0;
                    foreach (DataColumn dc in tbl.Columns)
                    {
                        string numberFormat = "";
                        if (dc.ExtendedProperties.ContainsKey("NumberFormat"))
                        {
                            numberFormat = dc.ExtendedProperties["NumberFormat"].ToString();
                            string excelColIndexAlphabet = IndexToColumn(dtColIndex + 1);
                            using (ExcelRange rng = ws.Cells[excelColIndexAlphabet + 2 + ":" + excelColIndexAlphabet + summaryRowIndex])
                            {
                                rng.Style.Numberformat.Format = numberFormat;
                                rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            }
                            //right align header text for number fields
                            using (ExcelRange rng = ws.Cells[excelColIndexAlphabet + 1 + ":" + excelColIndexAlphabet + 1])
                            {
                                rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            }
                        }
                        dtColIndex++;
                    }
                    //Format the header
                    using (ExcelRange rng = ws.Cells["A1:" + lastColExcel + "1"])
                    {
                        rng.Style.Font.Bold = true;
                        //rng.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                        //rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));  //Set color to dark blue
                        //rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                        rng.Style.WrapText = true;
                        rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    }
                    //Format the SummaryRow
                    using (ExcelRange rng = ws.Cells["A" + summaryRowIndex + ":" + lastColExcel + summaryRowIndex])
                    {
                        rng.Style.Font.Bold = true;
                        //rng.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                        //rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));  //Set color to dark blue
                        //rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    ////Example how to Format Column 1 as numeric 
                    //using (ExcelRange col = ws.Cells[2, 1, 2 + tbl.Rows.Count, 1])
                    //{
                    //    col.Style.Numberformat.Format = "#,##0.00";
                    //    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
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

        

        public static bool CreateExcelDocumentAsStreamEpPlusPackageLastRowSummary(DataSet ds, string filename, HttpResponse response)
        {
            //this one threw out of memory exception for large file with about 60,000 ratetask
            //use epplus package instead

            try
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    //Create the worksheet
                    DataTable tbl = ds.Tables[0];//
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Sheet1");

                    //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                    ws.Cells["A1"].LoadFromDataTable(tbl, true);
                    int noOfCols = tbl.Columns.Count;
                    string lastColExcel = IndexToColumn(noOfCols);
                    int noOfRows = tbl.Rows.Count;
                    int summaryRowIndex = noOfRows + 1;
                    // Set columns to auto-fit
                    for (int i = 1; i <= ws.Dimension.Columns; i++)
                    {
                        ws.Column(i).AutoFit();
                    }
                    //set numberformat
                    int dtColIndex = 0;
                    foreach (DataColumn dc in tbl.Columns)
                    {
                        string numberFormat = "";
                        if (dc.ExtendedProperties.ContainsKey("NumberFormat"))
                        {
                            numberFormat = dc.ExtendedProperties["NumberFormat"].ToString();
                            string excelColIndexAlphabet = IndexToColumn(dtColIndex+1);
                            using (ExcelRange rng = ws.Cells[excelColIndexAlphabet + 2 + ":" + excelColIndexAlphabet + summaryRowIndex])
                            {
                                rng.Style.Numberformat.Format = numberFormat;
                                rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            }
                            //right align header text for number fields
                            using (ExcelRange rng = ws.Cells[excelColIndexAlphabet + 1 + ":" + excelColIndexAlphabet + 1])
                            {
                                rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            }
                        }
                        dtColIndex++;
                    }
                    //Format the header
                    using (ExcelRange rng = ws.Cells["A1:" + lastColExcel + "1"])
                    {
                        rng.Style.Font.Bold = true;
                        rng.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                        rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));  //Set color to dark blue
                        rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                        rng.Style.WrapText = true;
                        rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        
                    }
                    //Format the SummaryRow
                    using (ExcelRange rng = ws.Cells["A" + summaryRowIndex + ":" + lastColExcel + summaryRowIndex])
                    {
                        rng.Style.Font.Bold = true;
                        rng.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                        rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));  //Set color to dark blue
                        rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    ////Example how to Format Column 1 as numeric 
                    //using (ExcelRange col = ws.Cells[2, 1, 2 + tbl.Rows.Count, 1])
                    //{
                    //    col.Style.Numberformat.Format = "#,##0.00";
                    //    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
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


#endif      //  End of "INCLUDE_WEB_FUNCTIONS" section

        /// <summary>
        /// Create an Excel file, and write it to a file.
        /// </summary>
        /// <param name="ds">DataSet containing the data to be written to the Excel.</param>
        /// <param name="excelFilename">Name of file to be written.</param>
        /// <returns>True if successful, false if something went wrong.</returns>
        public static bool CreateExcelDocument(DataSet ds, string excelFilename)
        {
            try
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(excelFilename, SpreadsheetDocumentType.Workbook))
                {
                    WriteExcelFile(ds, document);
                }
                Trace.WriteLine("Successfully created: " + excelFilename);
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Failed, exception thrown: " + ex.Message);
                return false;
            }
        }

        private static void WriteExcelFile(DataSet ds, SpreadsheetDocument spreadsheet)
        {
            //  Create the Excel file contents.  This function is used when creating an Excel file either writing 
            //  to a file, or writing to a MemoryStream.
            spreadsheet.AddWorkbookPart();
            spreadsheet.WorkbookPart.Workbook = new Workbook();

            //  My thanks to James Miera for the following line of code (which prevents crashes in Excel 2010)
            spreadsheet.WorkbookPart.Workbook.Append(new BookViews(new WorkbookView()));

            //  If we don't add a "WorkbookStylesPart", OLEDB will refuse to connect to this .xlsx file !
            WorkbookStylesPart workbookStylesPart = spreadsheet.WorkbookPart.AddNewPart<WorkbookStylesPart>("rIdStyles");
            Stylesheet stylesheet = new Stylesheet();
            workbookStylesPart.Stylesheet = stylesheet;

            //  Loop through each of the DataTables in our DataSet, and create a new Excel Worksheet for each.
            uint worksheetNumber = 1;
            foreach (DataTable dt in ds.Tables)
            {
                //  For each worksheet you want to create
                string workSheetId = "rId" + worksheetNumber.ToString();
                string worksheetName = dt.TableName;

                WorksheetPart newWorksheetPart = spreadsheet.WorkbookPart.AddNewPart<WorksheetPart>();
                newWorksheetPart.Worksheet = new Worksheet();

                // create sheet data
                newWorksheetPart.Worksheet.AppendChild(new SheetData());

                // save worksheet
                WriteDataTableToExcelWorksheet(dt, newWorksheetPart);
                newWorksheetPart.Worksheet.Save();

                // create the worksheet to workbook relation
                if (worksheetNumber == 1)
                    spreadsheet.WorkbookPart.Workbook.AppendChild(new Sheets());

                spreadsheet.WorkbookPart.Workbook.GetFirstChild<Sheets>().AppendChild(new Sheet()
                {
                    Id = spreadsheet.WorkbookPart.GetIdOfPart(newWorksheetPart),
                    SheetId = (uint)worksheetNumber,
                    Name = dt.TableName
                });

                worksheetNumber++;
            }

            spreadsheet.WorkbookPart.Workbook.Save();
        }


        private static void WriteDataTableToExcelWorksheet(DataTable dt, WorksheetPart worksheetPart)
        {
            var worksheet = worksheetPart.Worksheet;
            var sheetData = worksheet.GetFirstChild<SheetData>();

            string cellValue = "";

            //  Create a Header Row in our Excel file, containing one header for each Column of data in our DataTable.
            //
            //  We'll also create an array, showing which type each column of data is (Text or Numeric), so when we come to write the actual
            //  cells of data, we'll know if to write Text values or Numeric cell values.
            int numberOfColumns = dt.Columns.Count;
            bool[] isNumericColumn = new bool[numberOfColumns];

            string[] excelColumnNames = new string[numberOfColumns];
            for (int n = 0; n < numberOfColumns; n++)
                excelColumnNames[n] = GetExcelColumnName(n);

            //
            //  Create the Header row in our Excel Worksheet
            //
            uint rowIndex = 1;

            var headerRow = new Row { RowIndex = rowIndex };  // add a row at the top of spreadsheet
            sheetData.Append(headerRow);

            for (int colInx = 0; colInx < numberOfColumns; colInx++)
            {
                DataColumn col = dt.Columns[colInx];
                AppendTextCell(excelColumnNames[colInx] + "1", col.ColumnName, headerRow);
                isNumericColumn[colInx] = (col.DataType.FullName == "System.Decimal") || (col.DataType.FullName == "System.Int32");
            }

            //
            //  Now, step through each row of data in our DataTable...
            //
            double cellNumericValue = 0;
            foreach (DataRow dr in dt.Rows)
            {
                // ...create a new row, and append a set of this row's data to it.
                ++rowIndex;
                var newExcelRow = new Row { RowIndex = rowIndex };  // add a row at the top of spreadsheet
                sheetData.Append(newExcelRow);

                for (int colInx = 0; colInx < numberOfColumns; colInx++)
                {
                    cellValue = dr.ItemArray[colInx].ToString();

                    // Create cell with data
                    if (isNumericColumn[colInx])
                    {
                        //  For numeric cells, make sure our input data IS a number, then write it out to the Excel file.
                        //  If this numeric value is NULL, then don't write anything to the Excel file.
                        cellNumericValue = 0;
                        if (double.TryParse(cellValue, out cellNumericValue))
                        {
                            cellValue = cellNumericValue.ToString();
                            AppendNumericCell(excelColumnNames[colInx] + rowIndex.ToString(), cellValue, newExcelRow);
                        }
                    }
                    else
                    {
                        //  For text cells, just write the input data straight out to the Excel file.
                        AppendTextCell(excelColumnNames[colInx] + rowIndex.ToString(), cellValue, newExcelRow);
                    }
                }
            }
        }

        private static void AppendTextCell(string cellReference, string cellStringValue, Row excelRow)
        {
            //  Add a new Excel Cell to our Row 
            Cell cell = new Cell() { CellReference = cellReference, DataType = CellValues.String };
            CellValue cellValue = new CellValue();
            cellValue.Text = cellStringValue;
            cell.Append(cellValue);
            excelRow.Append(cell);
        }

        private static void AppendNumericCell(string cellReference, string cellStringValue, Row excelRow)
        {
            //  Add a new Excel Cell to our Row 
            Cell cell = new Cell() { CellReference = cellReference };
            CellValue cellValue = new CellValue();
            cellValue.Text = cellStringValue;
            cell.Append(cellValue);
            excelRow.Append(cell);
        }

        private static string GetExcelColumnName(int columnIndex)
        {
            //  Convert a zero-based column index into an Excel column reference  (A, B, C.. Y, Y, AA, AB, AC... AY, AZ, B1, B2..)
            //
            //  eg  GetExcelColumnName(0) should return "A"
            //      GetExcelColumnName(1) should return "B"
            //      GetExcelColumnName(25) should return "Z"
            //      GetExcelColumnName(26) should return "AA"
            //      GetExcelColumnName(27) should return "AB"
            //      ..etc..
            //
            if (columnIndex < 26)
                return ((char)('A' + columnIndex)).ToString();

            char firstChar = (char)('A' + (columnIndex / 26) - 1);
            char secondChar = (char)('A' + (columnIndex % 26));

            return string.Format("{0}{1}", firstChar, secondChar);
        }
    }
}
