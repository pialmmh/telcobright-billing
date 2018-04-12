using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RateSheetFormat
{
    class MyOpenXmlExcelHelper
    {
        public MyOpenXmlExcelHelper(string excelFilename)
        {

        }

        public static List<Row> GetUsedRows(SpreadsheetDocument document, WorksheetPart wsPart,out int maxColumn)
        {
            List<Row> lstOpenXmlRow = new List<Row>();
            bool hasValue;
            maxColumn = 1;
            //Iterate all rows except the first one.
            foreach (var row in wsPart.Worksheet.Descendants<Row>().Skip(1))
            {
                hasValue = false;
                int thisColumn = 1;
                foreach (var cell in row.Descendants<Cell>())
                {
                    //Find at least one cell with value for a row
                    if (!string.IsNullOrEmpty(GetCellValue(document, cell)))
                    {
                        hasValue = true;
                        if(thisColumn>maxColumn)
                        {
                            maxColumn = thisColumn;
                        }
                        break;
                    }
                    thisColumn++;
                }
                if (hasValue)
                {
                    //Return the row and keep iteration state.
                    lstOpenXmlRow.Add(row);
                }
            }
            return lstOpenXmlRow;
        }

        private static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            if (cell == null) return null;
            string value = cell.InnerText;

            //Process values particularly for those data types.
            if (cell.DataType != null)
            {
                switch (cell.DataType.Value)
                {
                    //Obtain values from shared string table.
                    case CellValues.SharedString:
                        var sstPart = document.WorkbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                        value = sstPart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
                        break;
                }
            }
            return value;
        }

    }
}
