using TelcobrightMediation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using MediationModel;
using Process = System.Diagnostics.Process;
using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
//using IgwModel;
using System.Text;
using LibraryExtensions;
using MediationModel;
using net.bytebuddy.asm;

/// <summary>
/// Summary description for myExcel
/// </summary>
public class MyExcel
{
    public Dictionary<string, countrycode> DicCountryCode = new Dictionary<string, countrycode>();
    public Dictionary<string, countrycode> DicCountryName = new Dictionary<string, countrycode>();

    public MyExcel()
    {
        //
        // TODO: Add constructor logic here
        //
        //using(
        using (PartnerEntities context = new PartnerEntities())
        {
            this.DicCountryCode = context.countrycodes.ToDictionary(c => c.Code);
        }

    }


    public enum ImportTaskType
    {
        RateChanges,
        CodeChanges
    }

    public int GetVendorFormat(string filePath, ref List<ratetask> lstRateTask, rateplan rp, bool endAllPrevPrefix, string[] dateFormats)
    {
        //todo: remove temp code
        Console.WriteLine("application running under: " + Environment.UserName);
        //end

        string dateSeparator = "";
        //ApplicationClass app = new ApplicationClass(); // the Excel application.
        var app = new Application();
        // the reference to the workbook,
        // which is the xls document to read from.
        Workbook book = null;
        // the reference to the worksheet,
        // we'll assume the first sheet in the book.


        // the range object is used to hold the data
        // we'll be reading from and to find the range of data.
        app.Visible = false;
        app.ScreenUpdating = false;
        app.DisplayAlerts = false;

        book = app.Workbooks.Open(filePath,
               Missing.Value, Missing.Value, Missing.Value,
               Missing.Value, Missing.Value, Missing.Value, Missing.Value,
               Missing.Value, Missing.Value, Missing.Value, Missing.Value,
               Missing.Value, Missing.Value, Missing.Value);
        Worksheet sheet = getTargetSheetWithRates(book,dateFormats);

        SheetImportTask importTask = new SheetImportTask(ImportTaskType.RateChanges,
            sheet, this.DicCountryCode, this.DicCountryName, rp);

        return importTask.GetVendorFormat(ref lstRateTask, rp, book, dateFormats, ref dateSeparator);

    }

    private Worksheet getTargetSheetWithRates(Workbook book, string[] dateFormats)
    {
        Worksheet targetSheet = null;

        foreach (var sh in book.Worksheets)
        {
            var thisSheet = (Worksheet)sh;
            var range = thisSheet.UsedRange;
            object[,] objArray = (object[,])range.get_Value(XlRangeValueDataType.xlRangeValueDefault);
            int dim1 = objArray.GetLength(0);
            int dim2 = objArray.GetLength(1);

            //replace nulls with zero lengh string
            for (int r = 1; r <= dim1; r++)
            {
                for (int c = 1; c <= dim2; c++)
                {
                    if (objArray[r, c] == null)
                        objArray[r, c] = "";
                }
            }

            //remove char ' (single quote) from obj array, caused problem in Sql
            for (int r = 1; r <= dim1; r++)
            {
                for (int c = 1; c <= dim2; c++)
                {
                    if (objArray[r, c] != null)
                        objArray[r, c] = objArray[r, c].ToString().Replace("'", "");
                }
            }

            //for text and csv/delimited files dim2 could be one
            //have to split the rows first
            char sepCharForCsv = ',';
            if (dim2 == 1)
            {
                List<string[]> csvArr = new List<string[]>();
                csvArrayHelper.GetArrayFromCsv(ref objArray, ref csvArr, ref sepCharForCsv);

                //convert 0 based csv array to 1 based obj array to handle delimated files
                dim1 = csvArr.Count;
                dim2 = csvArr[0].GetLength(0);
                //objArray = new object[Dim1, Dim2];//don't use the 0 index, start from 1
                //objArray = Array.CreateInstance(typeof(object), new object[] { 1, Dim1 }, new object[] { 1, Dim2 });
                objArray = (object[,])Array.CreateInstance(typeof(object),
                    new int[] { dim1, dim2 },
                    new int[] { 1, 1 });
                for (int r = 1; r <= dim1; r++)
                {
                    for (int c = 1; c <= dim2; c++)
                    {
                        objArray[r, c] = csvArr[r - 1][c - 1];
                    }
                }
            }
            string dateSeparator = "";
            SheetImportTask sit = new SheetImportTask(ImportTaskType.RateChanges,
                thisSheet, this.DicCountryCode, this.DicCountryName, new rateplan());
            int firstRow = sit.FindFirstRow(ref objArray, dateFormats, ref dateSeparator);
            if (firstRow > 0)
            {
                targetSheet = thisSheet;
                break;
            }

        }

        //targetSheet = (Worksheet)book.Worksheets[1];
        if (targetSheet == null)
        {
            throw new Exception("Target sheet containing rates not found.");
        }

        return targetSheet;
    }





    public enum VendorFormat
    {
        None = 0,
        GenericExcel = 1,
        GenericText = 2,
        GenericWithColumnHeaders=3,
        Tata = 101,
        Bharti = 102,
        Idt = 103,
        Mainberg = 104,
        Vodafone = 105,
        Idea = 106,
        Ckri = 107,
        DialogAxiata=108,
        Deutsche=109,
        Singtel=110,
        Starhub=111,
            Tm=112
    }


    //vendor specific parameters
    public class VendorSpecificParameters
    {
        public ParamBharti ParamBharti = new ParamBharti();
        public ParamDialog ParamDialog = new ParamDialog();
    }
    public class ParamBharti
    {
        public DateTime DefaultCodeUpdateValidityDate = new DateTime(1, 1, 1);
        public DateTime DefaultIncreaseValidityDate = new DateTime(1, 1, 1);
        public DateTime DefaultUnchangedValidityDate = new DateTime(1, 1, 1);
        public DateTime DefaultDecreaseValidityDate = new DateTime(1, 1, 1);
    }
    public class ParamDialog
    {
        public int RateColIndex = 5;
    }
    //End vendor specific parameters

    





}