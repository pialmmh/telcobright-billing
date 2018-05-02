using TelcobrightMediation;
using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using LibraryExtensions;
using MediationModel;
using Process = System.Diagnostics.Process;

/// <summary>
/// Summary description for myExcel
/// </summary>
namespace RateSheetFormat
{
    [Export("RateSheetFormat", typeof(IRateSheetFormat))]
    public class RsBics : IRateSheetFormat//change here
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        private string _helpText = "Rate Sheet Format for Bics";//change for each extension
        public string RuleName
        {
            get { return "rsFormatBics"; }//change for each extension
        }

        public string HelpText
        {
            get { return this._helpText; }
        }

        public int Id
        {
            get { return 11; }//change for each extension
        }

        public Dictionary<string, countrycode> DicCountryCode = new Dictionary<string, countrycode>();
        public Dictionary<string, countrycode> DicCountryName = new Dictionary<string, countrycode>();

        public RsBics()//change here
        {
            //
            // TODO: Add constructor logic here
            //
            //using(
            using (PartnerEntities context = new PartnerEntities())
            {
                foreach (countrycode cn in context.countrycodes.ToList())
                {
                    this.DicCountryCode.Add(cn.Code, cn);
                    this.DicCountryName.Add(cn.Name.ToLower(), cn);
                }
            }

        }


        public enum ImportTaskType
        {
            RateChanges,
            CodeChanges
        }

        List<ratetask> GetCodeDeletes(Worksheet delSheet, SheetImportTask delTask, rateplan rp, string[] dateFormats, ref string dateSeparator)
        {
            Range range1;
            range1 = delSheet.UsedRange;
            object[,] objArray1 = (object[,])range1.get_Value(XlRangeValueDataType.xlRangeValueDefault);
            int dim11 = objArray1.GetLength(0);
            int dim12 = objArray1.GetLength(1);
            int firstRow1 = delTask.FindFirstRow(ref objArray1, dateFormats, ref dateSeparator);

            List<ratetask> lstCodeDels = new List<ratetask>();
            int i = firstRow1;
            if(firstRow1>-1)//code deletes can be empty
            {
                while (true)
                {
                    //check for code ending task
                    if (i > dim11) break;
                    if ((objArray1[i, 4] == null ? "" : objArray1[i, 4].ToString()) != "End Dated")
                    {
                        i++;
                        continue;
                    }
                    ratetask thisTask = new ratetask();
                    thisTask.CountryCode = (objArray1[i, 2] == null ? "" : objArray1[i, 2].ToString());
                    if (thisTask.CountryCode.Trim() != "")
                    {
                        thisTask.Prefix = (objArray1[i, 3] == null ? "" : objArray1[i, 3].ToString());
                        thisTask.Prefix = thisTask.CountryCode + thisTask.Prefix;
                        thisTask.description = (objArray1[i, 1] == null ? "" : objArray1[i, 1].ToString());
                        string strTempDate = (objArray1[i, 6] == null ? "" : objArray1[i, 6].ToString());
                        DateTime tempDate = new DateTime();
                        if (DateTime.TryParseExact(strTempDate, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate))
                        {
                            thisTask.startdate = tempDate.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        thisTask.description = "End " + thisTask.description;
                        thisTask.Resolution = "1";//set any resolution
                        thisTask.MinDurationSec = "1";//set any minduration
                        thisTask.rateamount = "-1";
                        lstCodeDels.Add(thisTask);
                        i++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            
            return lstCodeDels;
        }

        List<ratetask> ProcessRateSheet(Worksheet sheet1, Worksheet sheet2, SheetImportTask importTask1, SheetImportTask importTask2,rateplan rp, string[] dateFormats,ref string dateSeparator)
        {
            Range range1;
            range1 = sheet1.UsedRange;
            //Range range2 = sheet2.UsedRange;
            
            object[,] objArray1 = (object[,])range1.get_Value(XlRangeValueDataType.xlRangeValueDefault);
            //object[,] objArray2 = (object[,])range2.get_Value(XlRangeValueDataType.xlRangeValueXMLSpreadsheet);

            int dim11 = objArray1.GetLength(0);
            int dim12 = objArray1.GetLength(1);

            //int Dim21 = objArray2.GetLength(0);
            //int Dim22 = objArray2.GetLength(1);

            int firstRow1 = importTask1.FindFirstRow(ref objArray1,dateFormats,ref dateSeparator);
            //int FirstRow2 = ImportTask2.FindFirstRow(ref objArray2, DateFormats, ref DateSeparator);

            //create prefix based dictionary from 2nd sheet
            List<ratetask> lstPrefix = new List<ratetask>();
            int i = firstRow1;
            List<string> cachedFormats = new List<string>() { "yyyy-MM-dd"};//must add at least one so that foreach later works
            while (true)
            {
                if (i > dim11) break;
                ratetask thisTask = new ratetask();
                thisTask.CountryCode = (objArray1[i, 5] == null ? "" : objArray1[i, 5].ToString());
                if (thisTask.CountryCode.Trim() != "")
                {
                    thisTask.Prefix = (objArray1[i, 6] == null ? "" : objArray1[i, 6].ToString());
                    thisTask.Prefix = thisTask.CountryCode + thisTask.Prefix;
                    thisTask.description = (objArray1[i, 2] == null ? "" : objArray1[i, 2].ToString());
                    thisTask.MinDurationSec = "1";
                    thisTask.Resolution = "1";
                    double rateAmount = 0;
                    if(double.TryParse((objArray1[i, 7] == null ? "" : objArray1[i, 7].ToString()),out rateAmount)== true)
                    {
                        thisTask.rateamount = rateAmount.ToString();
                    }
                    string strTempDate = (objArray1[i, 11] == null ? "" : objArray1[i, 11].ToString());
                    DateTime tempDate = new DateTime();
                    //usally there will be only one dateformat in ratesheet
                    //program will learn that or any other using cached & noncached list
                    //SIGNIFICANT improvement of performance, no need to tryparse thousands of date formats
                    bool matchFound = false;
                    foreach (string format in cachedFormats)
                    {
                        if (DateTime.TryParseExact(strTempDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate))
                        {
                            thisTask.startdate = tempDate.ToString("yyyy-MM-dd HH:mm:ss");
                            matchFound = true;
                            break;
                        }
                    }
                    if (matchFound == false)  //format doesn't exist in the cache, try all formats
                    {
                        foreach (string nonCachedFormat in dateFormats)
                        {
                            if (DateTime.TryParseExact(strTempDate, nonCachedFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate))
                            {
                                cachedFormats.Insert(0,nonCachedFormat);//keep latest match at first.
                                thisTask.startdate = tempDate.ToString("yyyy-MM-dd HH:mm:ss");
                                matchFound = true;
                                break;
                            }
                        }
                    }

                    lstPrefix.Add(thisTask);
                    i++;
                }
                else
                {
                    break;
                }
            }

            return lstPrefix;

            ////retrieve effective date from 3rd sheet
            //List<ratetask> lstRateTask2 = new List<ratetask>();
            //i = FirstRow2;
            //while (true)
            //{
            //    ratetask ThisTask = new ratetask();
            //    ThisTask.CountryCode = (objArray2[i, 2] == null?"": objArray2[i, 2].ToString());
            //    if (ThisTask.description.Trim() != "")
            //    {
            //        ThisTask.Prefix = ThisTask.CountryCode + (objArray2[i, 3] == null ? "" : objArray2[i, 3].ToString());
            //        ratetask MatchedTask = null;
            //        dicPrefix.TryGetValue(ThisTask.Prefix, out MatchedTask);
            //        if (MatchedTask != null)
            //        {//rate and date has to be found based on description, add to list only if key found in the dic
            //            string strTempDate = (objArray2[i, 5] == null ? "" : objArray2[i, 5].ToString());
            //            DateTime TempDate = new DateTime();
            //            if (DateTime.TryParseExact(strTempDate, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out TempDate))
            //            {
            //                MatchedTask.startdate = TempDate.ToString("yyyy-MM-dd HH:mm:ss");
            //            }
            //            MatchedTask.Resolution = "1";//set any resolution
            //            MatchedTask.MinDurationSec = "1";//set any minduration
            //        }
            //        i++;
            //    }
            //    else
            //    {
            //        break;//empty string, quit while loop
            //    }
            //}
            //return dicPrefix.Values.ToList();
        }

        public string GetRates(string filePath, ref List<ratetask> lstRateTask, rateplan rp, bool endAllPrevPrefix, string[] dateFormats)
        {
            //try
            {
                string dateSeparator = "";
                //ApplicationClass app = new ApplicationClass(); // the Excel application.
                var app = new Application();app.Visible = false;
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

                //try calling code deltes first as it was throwing exception in used range
                //for Bics Code Deletes are Mentioned Explicitly
                Worksheet sheet3 = (Worksheet)book.Worksheets[4];
                SheetImportTask delTask = new SheetImportTask(ImportTaskType.RateChanges,
                    sheet3, this.DicCountryCode, this.DicCountryName, rp);
                List<ratetask> lstCodeDeletes = GetCodeDeletes(sheet3, delTask, rp, dateFormats, ref dateSeparator);




                Worksheet sheet = (Worksheet)book.Worksheets[2];
                Worksheet sheet2= (Worksheet)book.Worksheets[3];
                

                SheetImportTask importTask = new SheetImportTask(ImportTaskType.RateChanges,
                    sheet, this.DicCountryCode, this.DicCountryName, rp);
                //preformat to find out prefix based on description
                SheetImportTask importTask2 = new SheetImportTask(ImportTaskType.RateChanges,
                    sheet2, this.DicCountryCode, this.DicCountryName, rp);
                lstRateTask =ProcessRateSheet(sheet, sheet2,importTask,importTask2,rp,dateFormats,ref dateSeparator);

                
                
                //merge code delete instructions with main prefix list
                lstRateTask.AddRange(lstCodeDeletes);//add the code delete instructions to strlines

                //delete all instance of excel, because it wasn't getting ended by the code of excel instancing
                foreach (Process process in Process.GetProcessesByName("Excel"))
                {
                    process.Kill();
                }

                return "";
            }
            //catch (Exception e1)
            //{
            //    return e1.InnerException.ToString();
            //}
        }



        public enum VendorFormat
        {
            None = 0,
            GenericExcel = 1,
            GenericText = 2,
            GenericWithColumnHeaders = 3,
            Tata = 101,
            Bharti = 102,
            Idt = 103,
            Mainberg = 104,
            Vodafone = 105,
            Idea = 106,
            Ckri = 107,
            DialogAxiata = 108,
            Deutsche = 109,
            Singtel = 110,
            Starhub = 111,
            Tm = 112
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

        public class SheetImportTask
        {
            public VendorFormat VFormat = VendorFormat.None;
            public VendorSpecificParameters VendorParams = new VendorSpecificParameters();

            public rateplan RatePlan;
            string _includedTechPrefix = "";
            ImportTaskType _importTaskType;
            Worksheet _sheet = null;
            object[,] _objArray = null;
            public int FirstRow;

            RateTableMetaData _tableData;
            Range _range = null;

            Dictionary<string, countrycode> _dicCountryCode = new Dictionary<string, countrycode>();
            Dictionary<string, countrycode> _dicCountryName = new Dictionary<string, countrycode>();

            //constructor
            public SheetImportTask(ImportTaskType pImportTaskType, Worksheet pSheet, Dictionary<string, countrycode> pDicCountryCode, Dictionary<string, countrycode> pDicCountryName, rateplan pRatePlan)
            {
                this._dicCountryCode = pDicCountryCode;
                this._dicCountryName = pDicCountryName;
                this._importTaskType = pImportTaskType;
                this._sheet = pSheet;
                //range = sheet.UsedRange;
                this.RatePlan = pRatePlan;
            }

            string ArrayFromCsv(ref object[,] objArray, ref List<string[]> strLines, ref char sepCharForCsv)
            {
                try
                {
                    char[] sepArr = new char[] { ',', ':', ';', '`' };
                    sepCharForCsv = ',';
                    //find separator chracter
                    Dictionary<char, int> dicCharCount = new Dictionary<char, int>();
                    dicCharCount.Add(',', 0);
                    dicCharCount.Add(':', 0);
                    dicCharCount.Add(';', 0);
                    dicCharCount.Add('`', 0);

                    int dim1 = objArray.GetLength(0);
                    int dim2 = objArray.GetLength(1);
                    //count occurance of each char, max will be the separator
                    for (int i = 1; i <= dim1; i++)
                        for (int j = 1; j <= dim2; j++)
                            foreach (char ch in sepArr)
                            {
                                dicCharCount[ch] += objArray[i, j].ToString().Count(c => c == ch);
                            }
                    sepCharForCsv = (from x in dicCharCount where x.Value == dicCharCount.Max(v => v.Value) select x.Key).First();
                    //split csv rows to multiple field row
                    for (int i = 1; i <= dim1; i++)
                    {
                        strLines.Add(objArray[i, 1].ToString().Split(sepCharForCsv));
                    }

                    return "";
                }
                catch (Exception e1)
                {
                    return e1.Message;
                }
            }

            public string GetRateArray(object[,] objArray,ref List<ratetask> lstRateTask, rateplan rp, Workbook book, string[] dateFormats, ref string dateSeparator,int firstRow)
            {
                try
                {
                    //object[,] objArray = (object[,])range.get_Value(XlRangeValueDataType.xlRangeValueDefault);
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
                        ArrayFromCsv(ref objArray, ref csvArr, ref sepCharForCsv);

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
                    if(firstRow==-1)
                    {
                        firstRow = FindFirstRow(ref objArray, dateFormats, ref dateSeparator);
                    }
                    

                    //rather than handling intlout and in cases, try a generic method, based on importing with column header
                    if (firstRow == 2)//there is coumn header
                    {
                        Dictionary<int, string> dicCols = new Dictionary<int, string>();
                        //if at least 2 value matches 2 column names then the first row contains the columnname
                        int matchedColumnCount = 0;
                        List<string> colNames = new List<string>();
                        colNames.Add("prefix");
                        colNames.Add("rateamount");
                        for (int j = 1; j <= dim2; j++)
                        {
                            string fldName = objArray[1, j].ToString().Trim().ToLower();
                            if (fldName != "" && dicCols.ContainsValue(fldName) == false)
                            {
                                dicCols.Add(j, fldName);
                            }

                            if (colNames.Contains(objArray[1, j]))
                            {
                                matchedColumnCount++;
                            }
                        }
                        if (matchedColumnCount >= 2)//first row contains heading
                        {
                            Exception e1 = null;
                            lstRateTask = GetRateArrayByCol(ref objArray, dicCols, ref e1);
                            if (e1 != null)
                            {
                                return e1.Message + "<br/>" + e1.InnerException.ToString();
                            }
                            return "";
                        }
                    }


                    ImportTaskType thisSheetType = ImportTaskType.RateChanges;
                    this.VFormat = VendorFormat.None;
                    this._tableData = new RateTableMetaData();
                    FindTableMetaData(ref objArray, ref this._tableData, firstRow, this.VFormat, dateFormats, ref dateSeparator);

                    List<string[]> strLines = MultipleToSinglePrefixArray(thisSheetType, ref objArray, this._tableData, dateFormats, ref dateSeparator);
                    if (strLines == null)
                    {
                        throw new Exception("Error converting multiple to single prefix array!");
                    }

                    lstRateTask = GetFinalRateArray(strLines, this._tableData, this.VFormat, dateFormats, ref dateSeparator);
                    return "";
                }
                catch (Exception e1)
                {
                    return e1.Message + "<br/>" + e1.InnerException.ToString();
                }
            }


            List<ratetask> GetRateArrayByCol(ref object[,] valueArray, Dictionary<int, string> dicCols, ref Exception e1)
            {
                List<ratetask> lstRateTask = new List<ratetask>();
                try
                {
                    int dim1 = valueArray.GetLength(0);
                    int dim2 = valueArray.GetLength(1);
                    for (int i = 2; i <= dim1; i++)//skip first row with column headers
                    {
                        List<string> lstJson = new List<string>();
                        for (int j = 1; j <= dim2; j++)
                        {
                            //if this colindex doesn't exist in the diccols, then continue
                            if (dicCols.ContainsKey(j) == false)
                            {
                                continue;
                            }
                            string fldName = "";
                            dicCols.TryGetValue(j, out fldName);

                            string val = "";
                            if (StringExtensions.IsNumeric(valueArray[i, j].ToString()) == true)
                            {
                                val = valueArray[i, j].ToString();
                                //only for prefix, which is numeric but text
                                if (fldName == "prefix")
                                {
                                    val = "'" + val + "'";
                                }

                            }
                            else//nto numeric
                            {
                                val = "'" + valueArray[i, j].ToString() + "'";
                                if (fldName == "startdate")
                                {
                                    DateTime eDate = new DateTime();
                                    if (DateTime.TryParse(val.Replace("'", ""), out eDate) == true)
                                    {
                                        val = "'" + eDate.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                    }
                                }
                            }
                            lstJson.Add("'" + fldName + "':" + val);
                        }
                        StringBuilder sbJson = new StringBuilder().Append("{ ").Append(string.Join(",", lstJson)).Append(" }");
                        ratetask newtask = JsonConvert.DeserializeObject<ratetask>(sbJson.ToString());
                        lstRateTask.Add(newtask);
                    }
                }
                catch (Exception e)
                {
                    e1 = e;
                }
                return lstRateTask;
            }

            void CheckCountryCodeInclusionAndCommonPrefix(ref List<ratetask> lstTask)
            {
                string firstChar = lstTask[0].Prefix.Substring(0, 1);
                int commonPrefixCount = 0;
                int countryCodeInclusionCount = 0;
                int ccAtStart = 0;
                bool ccIncluded = false;
                foreach (ratetask rtask in lstTask)
                {
                    if (rtask.CountryCode.Trim() != "" && rtask.Prefix.StartsWith(rtask.CountryCode))
                    {
                        ccAtStart++;
                    }
                    if (rtask.Prefix.StartsWith(firstChar))
                    {
                        commonPrefixCount++;
                    }
                    if (rtask.CountryCode.Trim() != "" && rtask.Prefix.Contains(rtask.CountryCode))
                    {
                        countryCodeInclusionCount++;
                    }
                    else if (rtask.CountryCode.Trim() == "")//if country code is not mentioned, prefix must contain cc
                    {
                        countryCodeInclusionCount++;
                    }
                }
                if (Convert.ToSingle(decimal.Divide(ccAtStart, lstTask.Count)) >= .8)//>80%
                {
                    ccIncluded = true;
                }
                else if (commonPrefixCount == lstTask.Count && countryCodeInclusionCount == lstTask.Count)
                {
                    //prefix starts with common prefix
                    //and country code exists in all prefix
                    ccIncluded = true;
                }
                if (ccIncluded == false)
                {
                    ////concat country+prefix for tata
                    foreach (ratetask rTask in lstTask)
                    {
                        rTask.Prefix = rTask.CountryCode + rTask.Prefix;
                    }
                }

            }
            class RateFormatWithOrder
            {
                public string DateFormat = "";
                public int Order = 100;
                public RateFormatWithOrder(string formatString, int sortOrder)
                {
                    this.DateFormat = formatString;
                    this.Order = sortOrder;
                }
                public override string ToString()
                {
                    return this.Order.ToString() + "=>" + this.DateFormat;
                }
            }
            List<ratetask> GetFinalRateArray(List<string[]> lstSinglePrefixArray, RateTableMetaData tableData, VendorFormat vFormat, string[] dateFormats, ref string dateSeparator)
            {

                //here order by dateformats with the help of DateSeparator to improve performance during parsing with huge format array
                if (dateSeparator != "")
                {
                    List<RateFormatWithOrder> lstFormatOrder = new List<RateFormatWithOrder>();
                    foreach (string format in dateFormats)
                    {
                        lstFormatOrder.Add(new RateFormatWithOrder(format, (format.Contains(dateSeparator) == true ? 0 : 100)));
                    }
                    lstFormatOrder = lstFormatOrder.OrderBy(c => c.Order).ToList();
                    List<string> dateFormatsOrdered = new List<string>();
                    foreach (RateFormatWithOrder formatOrder in lstFormatOrder)
                    {
                        dateFormatsOrdered.Add(formatOrder.DateFormat);
                    }
                    dateFormats = dateFormatsOrdered.ToArray();
                }

                List<ratetask> lstRates = new List<ratetask>();
                List<string> cachedFormats = new List<string>() { "yyyy-MM-dd" };//must add at least one so that foreach later works
                try
                {
                    foreach (string[] thisRow in lstSinglePrefixArray)
                    {

                        ratetask newTask = new ratetask();
                        //Row 0://prefix
                        //NewRow[0] 
                        newTask.Prefix = (tableData.IndexPrefix1) > -1 ? thisRow[tableData.IndexPrefix1 - 1] : "";//0 & 1 based array adjustment
                                                                                                                  //Row 1://description
                                                                                                                  //NewRow[1] 
                        newTask.description = (tableData.IndexDescription) > -1 ? thisRow[tableData.IndexDescription - 1] : "";
                        //Row 2://rate
                        if (this._importTaskType == ImportTaskType.RateChanges)
                        {
                            if (vFormat == VendorFormat.DialogAxiata)
                            {
                                newTask.rateamount = thisRow[this.VendorParams.ParamDialog.RateColIndex];
                            }
                            else
                            {
                                newTask.rateamount = (tableData.IndexRate) > -1 ? thisRow[tableData.IndexRate - 1] : "";
                            }
                        }
                        else//code delete
                        {
                            //NewRow[2] 
                            newTask.rateamount = "-1";
                        }
                        //Row 3://pulse
                        //NewRow[3] = 
                        newTask.Resolution = (tableData.IndexPulse) > -1 ? thisRow[tableData.IndexPulse - 1] : "";
                        if (newTask.Resolution == "")
                        {
                            newTask.Resolution = this.RatePlan.Resolution.ToString();//fetch default
                        }
                        //Row 4://minduration
                        //NewRow[4] 
                        newTask.MinDurationSec = (tableData.IndexMinDuration) > -1 ? thisRow[tableData.IndexMinDuration - 1] : "";
                        if (newTask.MinDurationSec == "")
                        {
                            newTask.MinDurationSec = this.RatePlan.minDurationSec.ToString();//fetch default
                        }
                        //Row 5://countrycode
                        //NewRow[5] 
                        newTask.CountryCode = (tableData.IndexCountryCode) > -1 ? thisRow[tableData.IndexCountryCode - 1] : "";
                        //Row 6://effectivedate
                        DateTime tempDate = new DateTime();
                        if (vFormat == VendorFormat.Bharti)//bharti specific
                        {
                            switch (thisRow[5].ToLower().Trim())
                            {
                                case ""://unchanged
                                        //NewRow[6] 
                                    newTask.startdate = this.VendorParams.ParamBharti.DefaultUnchangedValidityDate.ToString("yyyy-MM-dd HH:mm:ss");
                                    break;
                                case "increase":
                                    newTask.startdate = this.VendorParams.ParamBharti.DefaultIncreaseValidityDate.ToString("yyyy-MM-dd HH:mm:ss");
                                    break;
                                case "decrease":
                                    newTask.startdate = this.VendorParams.ParamBharti.DefaultDecreaseValidityDate.ToString("yyyy-MM-dd HH:mm:ss");
                                    break;
                                default://code update
                                    newTask.startdate = this.VendorParams.ParamBharti.DefaultCodeUpdateValidityDate.ToString("yyyy-MM-dd HH:mm:ss");
                                    break;
                            }
                        }
                        else
                        {
                            newTask.startdate = (tableData.IndexStartDate) > -1 ? thisRow[tableData.IndexStartDate - 1] : "";

                            //usally there will be only one dateformat in ratesheet
                            //program will learn that or any other using cached & noncached list
                            //SIGNIFICANT improvement of performance, no need to tryparse thousands of date formats
                            bool matchFound = false;
                            foreach (string format in cachedFormats)
                            {
                                if (DateTime.TryParseExact(newTask.startdate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate))
                                {
                                    newTask.startdate = tempDate.ToString("yyyy-MM-dd HH:mm:ss");
                                    matchFound = true;
                                    break;
                                }
                            }
                            if (matchFound == false)  //format doesn't exist in the cache, try all formats
                            {
                                foreach (string nonCachedFormat in dateFormats)
                                {
                                    if (DateTime.TryParseExact(newTask.startdate, nonCachedFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate))
                                    {
                                        cachedFormats.Insert(0, nonCachedFormat);//keep latest match at first.
                                        newTask.startdate = tempDate.ToString("yyyy-MM-dd HH:mm:ss");
                                        matchFound = true;
                                        break;
                                    }
                                }
                            }
                            if (DateTime.TryParseExact(newTask.startdate, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate))
                            {
                                newTask.startdate = tempDate.ToString("yyyy-MM-dd HH:mm:ss");
                            }

                        }

                        //Row 7://enddate
                        //NewRow[7] 
                        newTask.enddate = (tableData.IndexEndDate) > -1 ? thisRow[tableData.IndexEndDate - 1] : "";
                        //format date
                        DateTime tempDate1 = new DateTime();
                        if (DateTime.TryParse(newTask.enddate, out tempDate1) == true)
                        {
                            newTask.enddate = tempDate.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        //Row 8://surchargetime
                        //NewRow[8] 
                        newTask.SurchargeTime = (tableData.IndexSurchargeTime) > -1 ? thisRow[tableData.IndexSurchargeTime - 1] : "";
                        if (newTask.SurchargeTime == "")
                        {
                            newTask.SurchargeTime = this.RatePlan.SurchargeTime.ToString();//fetch default
                        }
                        //Row 9://surcharge amount
                        //NewRow[9] 
                        newTask.SurchargeAmount = (tableData.IndexSurchargeAmount) > -1 ? thisRow[tableData.IndexSurchargeAmount - 1] : "";
                        if (newTask.SurchargeAmount == "")
                        {
                            newTask.SurchargeAmount = this.RatePlan.SurchargeAmount.ToString();//fetch default
                        }
                        //Row 10://service type
                        //NewRow[10] 
                        newTask.Category = (tableData.IndexServiceType) > -1 ? thisRow[tableData.IndexServiceType - 1] : "";
                        if (newTask.Category == "")
                        {
                            newTask.Category = this.RatePlan.Category.ToString();//fetch default
                        }
                        //Row 11://sub service type
                        //NewRow[11] 
                        newTask.SubCategory = (tableData.IndexSubServiceType) > -1 ? thisRow[tableData.IndexSubServiceType - 1] : "";
                        if (newTask.SubCategory == "")
                        {
                            newTask.SubCategory = this.RatePlan.SubCategory.ToString();//fetch default
                        }

                        ////***vendor specific modification
                        ////concat country+prefix for tata
                        //if (VFormat == VendorFormat.Tata)
                        //{
                        //    //NewRow[0] = NewRow[5] + NewRow[0];
                        //    NewTask.Prefix = NewTask.CountryCode + NewTask.Prefix;
                        //}

                        lstRates.Add(newTask);
                    }

                }//try
                catch (Exception e1)
                {
                    return null;
                }
                CheckCountryCodeInclusionAndCommonPrefix(ref lstRates);
                return lstRates;
            }

            enum RangeType
            {
                None,
                Whole,//6114-6118
                TrailingDigits//6114-20
            }
            string RangeToMultiplePrefix(string value)
            {
                string retval = "";
                if (value.Contains("-") == false)
                {
                    return value;
                }

                value = value.Replace(" ", "");//replace space
                value = value.Replace("\t", "");//replace space
                                                //separate cell value as comma (or other then -) separated values
                char[] sepArr = new char[] { ',', ':', ';' };
                char sepChar = ',';
                //find separator chracter
                foreach (char ch in sepArr)
                {
                    if (value.Contains(ch))
                    {
                        sepChar = ch;
                        break;
                    }
                }

                string[] separatedPrefixArr = value.Split(sepChar);
                List<string> lstRangeNormalizedArr = new List<string>();

                foreach (string thisValue in separatedPrefixArr)
                {

                    if (thisValue.Contains("-") == false)
                    {
                        lstRangeNormalizedArr.Add(thisValue);
                        continue;
                    }

                    // '-' exists, normalize range for this pair e.g. 6114-6118
                    string prefixBeforeDash = thisValue.Split('-')[0];
                    string prefixAfterDash = thisValue.Split('-')[1];


                    List<string> lstPrefixeOneRange = new List<string>();


                    RangeType rangetype = RangeType.None;
                    if (prefixAfterDash.Length >= prefixBeforeDash.Length)
                    {
                        rangetype = RangeType.Whole;
                    }
                    else
                    {
                        rangetype = RangeType.TrailingDigits;
                    }

                    switch (rangetype)
                    {
                        case RangeType.Whole:
                            long endNumber = 0;
                            long startNumber = 0;
                            if (long.TryParse(prefixAfterDash, out endNumber))
                            {
                                if (long.TryParse(prefixBeforeDash, out startNumber))
                                {
                                    if (endNumber > startNumber)
                                    {
                                        long difference = endNumber - startNumber;
                                        for (long tLong = startNumber; tLong <= endNumber; tLong++)
                                        {
                                            lstPrefixeOneRange.Add(tLong.ToString());
                                        }
                                    }
                                }
                            }
                            break;
                        case RangeType.TrailingDigits:
                            lstPrefixeOneRange = new List<string>();
                            string commonprefix = prefixBeforeDash.Substring(0, prefixBeforeDash.Length - prefixAfterDash.Length);
                            long numToIncrement = long.Parse(prefixBeforeDash.Substring(commonprefix.Length, prefixBeforeDash.Length - commonprefix.Length));
                            long incrementUpto = long.Parse(prefixAfterDash);
                            if (incrementUpto > numToIncrement)
                            {
                                for (long i = numToIncrement; i <= incrementUpto; i++)
                                {
                                    lstPrefixeOneRange.Add(commonprefix + i.ToString());
                                }
                            }
                            break;
                    }
                    foreach (string str in lstPrefixeOneRange)
                    {
                        lstRangeNormalizedArr.Add(str);
                    }

                }//for each separated value, each may or may not contain '-'

                //return the cell as comma separated

                return string.Join(",", lstRangeNormalizedArr);
            }

            class SeparatedPrefixArray
            {
                public string[] PrefixArray = null;
                public SeparatedPrefixArray(string value)
                {
                    //value could be character separated too
                    //find separator chracter
                    char[] sepArr = new char[] { ',', ':', ';' };
                    char sepChar = ',';

                    foreach (char ch in sepArr)
                    {
                        if (value.Contains(ch))
                        {
                            sepChar = ch;
                            break;
                        }
                    }
                    this.PrefixArray = value.Split(sepChar);
                }
            }

            List<string[]> MultipleToSinglePrefixArray(ImportTaskType thisSheetType, ref object[,] valueArray, RateTableMetaData tableData, string[] dateFormats, ref string dateSeparator)
            {
                //sometime prefix without countrycode and combined prefix both can exist
                //in that case later has more chracters and it should be the prefix column
                if (thisSheetType == ImportTaskType.RateChanges && tableData.IndexPrefix1 > -1 && tableData.IndexPrefix2 > -1)
                {
                    if (tableData.DicColData[tableData.IndexPrefix2].ColAttribs.CharacterCount
                        > tableData.DicColData[tableData.IndexPrefix1].ColAttribs.CharacterCount)
                    {
                        tableData.IndexPrefix1 = tableData.IndexPrefix2;
                    }
                }

                List<string[]> singlePrefixTableRows = new List<string[]>();
                try
                {
                    int dim2 = valueArray.GetLength(1);
                    for (int i = tableData.FirstRow; ; i++)//1 based array returned by excel
                    {
                        if (CheckIfBeyondLastRow(i, ref valueArray, dateFormats, ref dateSeparator) == true)
                        {
                            return singlePrefixTableRows;
                        }

                        string value = valueArray[i, tableData.IndexPrefix1].ToString().Replace(" ", "").Replace("\t", "");
                        value = RangeToMultiplePrefix(value);

                        SeparatedPrefixArray sepOldCodes = new SeparatedPrefixArray(value);
                        string[] finalPrefixesForThisRow = null;

                        if (thisSheetType == ImportTaskType.RateChanges)
                        {
                            finalPrefixesForThisRow = sepOldCodes.PrefixArray;
                        }
                        else//code changes,//code delete handling e.g. for tata
                        {
                            string valNewCodes = valueArray[i, tableData.IndexPrefix2].ToString().Replace(" ", "").Replace("\t", "");
                            valNewCodes = RangeToMultiplePrefix(valNewCodes);
                            SeparatedPrefixArray sepNewCodes = new SeparatedPrefixArray(valNewCodes);
                            finalPrefixesForThisRow = (from s in sepOldCodes.PrefixArray.Except(sepNewCodes.PrefixArray)
                                                       where s != ""//exclude where old entries had only country, new code added, not significant for code deletion
                                                       select s).ToArray();
                        }

                        int prefixcount = finalPrefixesForThisRow.Count();

                        List<string[]> lstRows = new List<string[]>();//multi to single prefix normalized row

                        for (int k = 0; k < prefixcount; k++)
                        {
                            string[] oneRow = new string[dim2];
                            for (int j = 1; j <= dim2; j++)
                            {
                                if (j != tableData.IndexPrefix1)
                                {
                                    oneRow[j - 1] = valueArray[i, j] != null ? valueArray[i, j].ToString().Trim() : "";
                                }
                            }
                            oneRow[tableData.IndexPrefix1 - 1] = finalPrefixesForThisRow[k].Trim();//code deletes are normalized in index1 like rate changes
                            lstRows.Add(oneRow);
                        }

                        foreach (string[] rstr in lstRows)//lstrows may contain one or multiple rows depending on
                                                          //single or multiple prefix
                        {
                            singlePrefixTableRows.Add(rstr);
                        }
                    }
                }//try
                catch (Exception e1)
                {
                    return null;
                }
                return singlePrefixTableRows;
            }

            enum CellDataType
            {
                Prefix,
                MultiplePrefix,
                CountryCode,
                CountryName,
                DescriptionSingleWord,
                DescriptionMultipleWord,
                Rate,
                Datetime,
                Undetermined,
                Null
            }

            class RateTableMetaData
            {
                public int FirstRow = -1;
                public int LastRow = -1;

                public int IndexPrefix1 = -1;
                public int IndexPrefix2 = -1;//can be present e.g. code update sheet of Tata
                public int IndexDescription = -1;
                public int IndexRate = -1;
                public int IndexPulse = -1;
                public int IndexMinDuration = -1;
                public int IndexCountryCode = -1;
                public int IndexCountryName = -1;
                public int IndexStartDate = -1;
                public int IndexEndDate = -1;
                public int IndexSurchargeTime = -1;
                public int IndexSurchargeAmount = -1;
                public int IndexServiceType = -1;
                public int IndexSubServiceType = -1;

                public Dictionary<int, ColumnMetaData> DicColData = new Dictionary<int, ColumnMetaData>();//keep
                                                                                                          //row column attribs for each column (key, 1 based) in case they are required 
            }

            class ColumnMetaData
            {
                public CellDataType ColumnType = CellDataType.Null;
                public RowColumnAttributes ColAttribs = new RowColumnAttributes();
            }


            class RowColumnAttributes
            {
                public int CharacterCount = 0;
                public Dictionary<string, int> DicUnqWord = new Dictionary<string, int>();

                public int PossUndeterminedCount = 0;
                public int PossRateCount = 0;
                public int PossPrefixCount = 0;
                public int PossCountryCodeCount = 0;
                public int PossCountryNameCount = 0;
                public int PossMultipleWordCount = 0;
                public int PossDateCount = 0;




                public int GetAttributeCount()
                {
                    return this.PossRateCount + this.PossPrefixCount + this.PossCountryCodeCount + this.PossCountryNameCount + this.PossMultipleWordCount + this.PossDateCount;
                }

                public CellDataType GetCellDataTypeByMaxAttributeCount()
                {
                    int max = this.PossUndeterminedCount;
                    CellDataType maxLike = CellDataType.Undetermined;
                    if (this.PossRateCount > max)
                    {
                        max = this.PossRateCount;
                        maxLike = CellDataType.Rate;
                    }
                    if (this.PossPrefixCount > max)
                    {
                        max = this.PossPrefixCount;
                        maxLike = CellDataType.Prefix;//could be multipleprefix also, but set column type=prefix will do
                    }
                    if (this.PossCountryCodeCount > max)
                    {
                        max = this.PossCountryCodeCount;
                        maxLike = CellDataType.CountryCode;
                    }
                    if (this.PossCountryNameCount > max)
                    {
                        max = this.PossCountryNameCount;
                        maxLike = CellDataType.CountryName;
                    }
                    if (this.PossMultipleWordCount > max)
                    {
                        max = this.PossMultipleWordCount;
                        maxLike = CellDataType.DescriptionMultipleWord;
                    }
                    if (this.PossDateCount > max)
                    {
                        max = this.PossDateCount;
                        maxLike = CellDataType.Datetime;
                    }
                    return maxLike;
                }

            }


            enum FirstOrLastRow
            {
                FirstRow,
                LastRow
            }

            string HeaderTextByAllSheet(Workbook book)
            {
                //concat all text in first 1000 lines, first 10 columns in each sheets
                StringBuilder sbstr = new StringBuilder();
                foreach (Worksheet Sheet in book.Worksheets)
                {
                    Range range = Sheet.UsedRange;
                    object[,] objArray = (object[,])range.get_Value(XlRangeValueDataType.xlRangeValueDefault);
                    int i = 1;

                    for (i = 1; i < 1000; i++)//1 based array returned by excel
                    {
                        int j = 1;
                        for (j = 1; j <= 10; j++)  //1 based array
                        {
                            CellDataType? thisColumnLike = (objArray[i, j] != null ? CellDataType.Undetermined : CellDataType.Null);
                            switch (thisColumnLike)
                            {
                                case CellDataType.Null:
                                    continue;
                                default:
                                    sbstr.Append(objArray[i, j].ToString().ToLower());
                                    break;
                            }
                        }
                    }
                }//for each sheet the leading texts are copied in dbstr

                return sbstr.ToString();

            }



            VendorFormat GetVendorFormat(ref object[,] objArray, int firstRow, Workbook book)
            {
                int dim2 = objArray.GetLength(1);
                string headerText = "";
                if ((firstRow == 2 || firstRow == 1) && dim2 == 12)//1st row column header
                {
                    return VendorFormat.GenericExcel;
                }
                else if (firstRow <= 0)//first worksheet does not contain rates and vendor information
                {
                    headerText = HeaderTextByAllSheet(book);
                }
                else
                {
                    int i = 1;
                    //concat all text before first rate row
                    StringBuilder sbstr = new StringBuilder();
                    for (i = 1; i < firstRow; i++)//1 based array returned by excel
                    {
                        RowColumnAttributes rAttrib = new RowColumnAttributes();
                        int j = 1;
                        for (j = 1; j <= dim2; j++)  //1 based array
                        {
                            CellDataType? thisColumnLike = (objArray[i, j] != null ? CellDataType.Undetermined : CellDataType.Null);
                            switch (thisColumnLike)
                            {
                                case CellDataType.Null:
                                    continue;
                                default:
                                    sbstr.Append(objArray[i, j].ToString().ToLower());
                                    break;
                            }
                        }
                    }
                    headerText = sbstr.ToString();
                }



                if (headerText.Contains("bharti airtel") && headerText.Contains("increase validity date")
                    && headerText.Contains("decrease validity date") && headerText.Contains("unchanged validity date"))
                {
                    //find bharti specific paramaters
                    for (int i = 1; i < firstRow; i++)//1 based array returned by excel
                    {
                        int j = 1;
                        for (j = 1; j <= dim2; j++)  //1 based array
                        {
                            DateTime tempDate = new DateTime(1, 1, 1);
                            string cellValue = objArray[i, j] == null ? "" : objArray[i, j].ToString().ToLower();
                            string dateValue = "";
                            if (j < dim2)
                            {
                                dateValue = objArray[i, j + 1] == null ? "" : objArray[i, j + 1].ToString();
                            }
                            switch (cellValue)
                            {
                                case "increase validity date:":
                                    if (DateTime.TryParse(dateValue, out tempDate) == true)
                                    {
                                        this.VendorParams.ParamBharti.DefaultIncreaseValidityDate = tempDate;
                                    }
                                    break;
                                case "decrease validity date:":
                                    if (DateTime.TryParse(dateValue, out tempDate) == true)
                                    {
                                        this.VendorParams.ParamBharti.DefaultDecreaseValidityDate = tempDate;
                                    }
                                    break;
                                case "code update validity date:":
                                    if (DateTime.TryParse(dateValue, out tempDate) == true)
                                    {
                                        this.VendorParams.ParamBharti.DefaultCodeUpdateValidityDate = tempDate;
                                    }
                                    break;
                                case "unchanged validity date:":
                                    if (DateTime.TryParse(dateValue, out tempDate) == true)
                                    {
                                        this.VendorParams.ParamBharti.DefaultUnchangedValidityDate = tempDate;
                                    }
                                    break;
                            }
                        }
                    }
                    return VendorFormat.Bharti;
                }
                else if (headerText.Contains("tata communications") && headerText.Contains("billing entity")
                    && headerText.Contains("rate change amendment") && headerText.Contains("customer no"))
                {
                    return VendorFormat.Tata;
                }
                else if (headerText.Contains("a-z termination rates") && headerText.Contains("billing increment")
                    && headerText.Contains(" new = n") && headerText.Contains("no change = nc"))
                {
                    return VendorFormat.Mainberg;
                }
                else if (headerText.Contains("time zone") && headerText.Contains("date format")
                    && headerText.Contains("price stated") && headerText.Contains("ckri"))
                {
                    return VendorFormat.Ckri;
                }
                else if (headerText.Contains("dialog axiata plc") && headerText.Contains("increase notice period"))
                {
                    return VendorFormat.DialogAxiata;
                }
                else if (headerText.Contains("deutsche telekom ag") && headerText.Contains("price list content "))
                {
                    return VendorFormat.Deutsche;
                }
                else if (headerText.Contains("singapore telecommunications limited"))
                {
                    return VendorFormat.Singtel;
                }
                else if (headerText.Contains("confidential pricing to:") && headerText.Contains("pricing valid from: "))
                {
                    return VendorFormat.Tm;
                }
                else if (headerText.Contains("the stipulated rates are effective") && headerText.Contains("starhub"))
                {
                    return VendorFormat.Starhub;
                }
                else
                {
                    return VendorFormat.None;
                }

            }




            public int FindFirstRow(ref object[,] objArray, string[] dateFormats, ref string dateSeparator)
            {
                int rowIndex = -1;
                int dim1 = objArray.GetLength(0);
                int dim2 = objArray.GetLength(1);

                dim1 = objArray.GetLength(0);
                dim2 = objArray.GetLength(1);

                int i = 1;
                for (i = 1; i <= dim1; i++)//1 based array returned by excel
                {
                    RowColumnAttributes rAttrib = new RowColumnAttributes();
                    int j = 1;

                    for (j = 1; j <= dim2; j++)  //1 based array
                    {
                        CellDataType? thisColumnLike = CellDataType.Null;

                        thisColumnLike = (objArray[i, j] != null ? FindCellDataType(objArray[i, j].ToString().Trim(), dateFormats, ref dateSeparator) : CellDataType.Null);

                        switch (thisColumnLike)
                        {
                            case CellDataType.Null:
                                continue;
                                break;
                            case CellDataType.MultiplePrefix:
                            case CellDataType.Prefix:
                                rAttrib.PossPrefixCount++;
                                break;
                            case CellDataType.Datetime:
                                rAttrib.PossDateCount++;
                                break;
                            case CellDataType.Rate:
                                rAttrib.PossRateCount++;
                                break;
                            case CellDataType.CountryName:
                                rAttrib.PossCountryNameCount++;
                                break;
                            case CellDataType.CountryCode:
                                rAttrib.PossCountryCodeCount++;
                                break;
                        }
                    }

                    if (rAttrib.GetAttributeCount() >= 2)
                    {
                        rowIndex = i;
                        return rowIndex;
                    }
                }
                return rowIndex;
            }

            bool CheckIfBeyondLastRow(int rowindex, ref object[,] objArray, string[] dateFormats, ref string dateSeparator)
            {
                int dim1 = objArray.GetLength(0);
                if (rowindex > dim1)
                {
                    return true;
                }
                int i = rowindex;
                int dim2 = objArray.GetLength(1);

                RowColumnAttributes rAttrib = new RowColumnAttributes();
                int j = 1;

                for (j = 1; j <= dim2; j++)  //1 based array
                {
                    CellDataType? thisColumnLike = (objArray[i, j] != null ? FindCellDataType(objArray[i, j].ToString().Trim(), dateFormats, ref dateSeparator) : CellDataType.Null);
                    switch (thisColumnLike)
                    {
                        case CellDataType.Null:
                            continue;
                            break;
                        case CellDataType.MultiplePrefix:
                        case CellDataType.Prefix:
                            rAttrib.PossPrefixCount++;
                            break;
                        case CellDataType.Datetime:
                            rAttrib.PossDateCount++;
                            break;
                        case CellDataType.Rate:
                            rAttrib.PossRateCount++;
                            break;
                        case CellDataType.CountryName:
                            rAttrib.PossCountryNameCount++;
                            break;
                        case CellDataType.CountryCode:
                            rAttrib.PossCountryCodeCount++;
                            break;
                    }
                }

                if (rAttrib.GetAttributeCount() < 2)
                {

                    return true;
                }


                return false;

            }

            int FindColumnAttributes(ref object[,] objArray, int firstRow, int columnIndex, ref RowColumnAttributes rAttrib, string[] dateFormats, ref string dateSeparator)
            {

                try
                {
                    int i = firstRow;
                    int j = columnIndex;
                    int dim1 = objArray.GetLength(0);
                    for (i = firstRow; i <= firstRow + 20 && i <= dim1; i++)//sampling over 20 rows will do
                    {
                        CellDataType? thisColumnLike = (objArray[i, j] != null ? FindCellDataType(objArray[i, j].ToString(), dateFormats, ref dateSeparator) : CellDataType.Null);
                        switch (thisColumnLike)
                        {
                            case CellDataType.Null:
                                continue;
                                break;
                            case CellDataType.MultiplePrefix:
                            case CellDataType.Prefix:
                                rAttrib.PossPrefixCount++;
                                break;
                            case CellDataType.Datetime:
                                rAttrib.PossDateCount++;
                                break;
                            case CellDataType.Rate:
                                rAttrib.PossRateCount++;
                                break;
                            case CellDataType.CountryName:
                                rAttrib.PossCountryNameCount++;
                                break;
                            case CellDataType.CountryCode:
                                rAttrib.PossCountryCodeCount++;
                                break;
                            case CellDataType.DescriptionMultipleWord:
                                rAttrib.PossMultipleWordCount++;
                                string[] words = objArray[i, j].ToString().Split(null);
                                foreach (string str in words)
                                {
                                    int count = 0;
                                    if (rAttrib.DicUnqWord.ContainsKey(str.Trim()) == true)
                                    {
                                        rAttrib.DicUnqWord[str] += 1;
                                    }
                                    else
                                    {
                                        rAttrib.DicUnqWord.Add(str, 1);
                                    }

                                }

                                break;
                        }
                        rAttrib.CharacterCount += objArray[i, j].ToString().Length;

                    }//for each row

                    //set charactercount for current column 


                    return firstRow;
                }
                catch (Exception e1)
                {
                    return -1;
                }
            }

            CellDataType FindCellDataType(string value, string[] dateFormats, ref string dateSeparator)//must send value after using tostring()
            {
                value = value.Trim();
                CellDataType thisLike = CellDataType.Undetermined;
                try
                {

                    double rateDouble = -1;
                    if (value.Contains(".") && double.TryParse(value, out rateDouble) == true)
                    {
                        thisLike = CellDataType.Rate;
                        return thisLike;
                    }

                    DateTime myDateTime = new DateTime(1, 1, 1);

                    if ((value.Contains("/")) &&
                        DateTime.TryParseExact(value, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out myDateTime))
                    {
                        thisLike = CellDataType.Datetime;
                        dateSeparator = "/";
                        return thisLike;
                    }
                    else if ((value.Contains("-")) &&
                        DateTime.TryParseExact(value, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out myDateTime))
                    {
                        thisLike = CellDataType.Datetime;
                        dateSeparator = "-";
                        return thisLike;
                    }
                    else if ((value.Contains(".")) &&
                        DateTime.TryParseExact(value, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out myDateTime))
                    {
                        thisLike = CellDataType.Datetime;
                        dateSeparator = ".";
                        return thisLike;
                    }
                    else if ((value.Contains(" ")) &&
                        DateTime.TryParseExact(value, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out myDateTime))
                    {
                        thisLike = CellDataType.Datetime;
                        dateSeparator = " ";
                        return thisLike;
                    }

                    //replace "/" with "-". If dates contain "/", datetime.tryparse didn't work with 
                    else if (DateTime.TryParse(value.Replace("/", "-"), out myDateTime) && (value.Contains("/") || value.Contains("-")))
                    {
                        thisLike = CellDataType.Datetime;
                        return thisLike;
                    }

                    string wholeNumbers = "0123456789";
                    bool wholeNumber = true;
                    bool mostlyDigits = false;
                    //value could be character separated too
                    char[] sepArr = new char[] { ',', ':', '-', ';' };
                    char sepChar = ',';
                    //find separator chracter
                    foreach (char ch in sepArr)
                    {
                        if (value.Contains(ch))
                        {
                            sepChar = ch;
                            break;
                        }
                    }

                    string[] vArr = null;
                    vArr = value.Split(sepChar);

                    //trim all
                    for (int s = 0; s < vArr.GetLength(0); s++)
                    {
                        vArr[s] = vArr[s].Trim();
                    }
                    foreach (char ch in value.ToCharArray())
                    {
                        if (wholeNumbers.Contains(ch) == false)
                        {
                            wholeNumber = false;
                            break;
                        }
                    }
                    if (wholeNumber == true)//prefix or country
                    {
                        long num = 0;
                        if (Int64.TryParse(vArr[0], out num))
                        {
                            if (num > 0)
                            {
                                if (this._dicCountryCode.ContainsKey(num.ToString()))
                                {
                                    thisLike = CellDataType.CountryCode;
                                    return thisLike;
                                }
                                else
                                {
                                    thisLike = CellDataType.Prefix;
                                    return thisLike;
                                }
                            }
                        }
                    }
                    else//not exactly numeric cell
                    {
                        if (this._dicCountryName.ContainsKey(vArr[0].ToString().ToLower()) && vArr.GetLength(0) == 1)
                        {
                            thisLike = CellDataType.CountryName;
                            return thisLike;
                        }
                        if (vArr.GetLength(0) > 1)//comma separated cell
                        {
                            //multiple values, comma separated
                            //cannot be single prefix
                            //could be Multiple prefix
                            //check against first 2 elements of array after split if they are numbers
                            long num = -1;
                            long num2 = -1;
                            if (Int64.TryParse(vArr[0].Trim(), out num))
                            {
                                if (num >= 0)
                                {
                                    //make another check with the 2nd element in the array
                                    if (Int64.TryParse(vArr[1].Trim(), out num2))
                                    {
                                        if (num2 >= 0)
                                        {
                                            //first two elements are numeric, so likely to be multiple prefix
                                            thisLike = CellDataType.MultiplePrefix;
                                            return thisLike;
                                        }
                                    }

                                }
                            }
                            else//comma separated but values not numeric
                            {
                                //one last check if this cell contains prefix
                                //check if mostly digits

                                int digitCount = 0;
                                int nonDigitCount = 0;
                                foreach (char ch in value.ToCharArray())
                                {
                                    if (wholeNumbers.Contains(ch))
                                    {
                                        digitCount++;
                                    }
                                    else nonDigitCount++;
                                }
                                if (digitCount > nonDigitCount) mostlyDigits = true;

                                if (mostlyDigits == true)
                                {
                                    thisLike = CellDataType.MultiplePrefix;
                                }
                                else
                                {
                                    thisLike = CellDataType.DescriptionMultipleWord;
                                }

                                return thisLike;
                            }

                        }
                        if (vArr.GetLength(0) == 1)//single cell, not comma separated and also not numeric
                        {

                            //single value only
                            //can be single or multiple word description
                            if (vArr[0].Split(null).GetLength(0) > 1)//split on white space yields multiple values in array
                            {
                                thisLike = CellDataType.DescriptionMultipleWord;
                                return thisLike;
                            }
                            else
                            {
                                thisLike = CellDataType.DescriptionSingleWord;
                                return thisLike;
                            }
                        }
                    }//if numeric==false

                    //one last check if this cell contains prefix
                    //check if mostly digits
                    if (thisLike != CellDataType.CountryCode && thisLike != CellDataType.Prefix && thisLike != CellDataType.MultiplePrefix)
                    {
                        int digitCount = 0;
                        int nonDigitCount = 0;
                        foreach (char ch in value.ToCharArray())
                        {
                            if (wholeNumbers.Contains(ch))
                            {
                                digitCount++;
                            }
                            else nonDigitCount++;
                        }
                        if (digitCount > nonDigitCount) mostlyDigits = true;

                        if (mostlyDigits == true)
                        {
                            thisLike = CellDataType.MultiplePrefix;
                        }
                    }
                    return thisLike;
                }
                catch (Exception e1)
                {
                    return CellDataType.Undetermined;
                }
            }

            string FindTableMetaData(ref object[,] valueArray, ref RateTableMetaData tableData, int firstRow, VendorFormat vFormat, string[] dateFormats, ref string dateSeparator)
            {
                //try
                {

                    tableData.FirstRow = firstRow;
                    if (vFormat == VendorFormat.GenericExcel || vFormat == VendorFormat.GenericText)
                    {
                        tableData.IndexPrefix1 = 1;
                        tableData.IndexPrefix2 = -1;//can be present e.g. code update sheet of Tata
                        tableData.IndexDescription = 2;
                        tableData.IndexRate = 3;
                        tableData.IndexPulse = 4;
                        tableData.IndexMinDuration = 5;
                        tableData.IndexCountryCode = 6;
                        tableData.IndexCountryName = -1;
                        tableData.IndexStartDate = 7;
                        tableData.IndexEndDate = 8;
                        tableData.IndexSurchargeTime = 9;
                        tableData.IndexSurchargeAmount = 10;
                        tableData.IndexServiceType = 11;
                        tableData.IndexSubServiceType = 12;
                        return "";
                    }

                    //scan by each column to find charctristic of a column
                    int dim2 = valueArray.GetLength(1);
                    List<int> possDateColsIndex = new List<int>();
                    List<int> possDateMulPrefixIndex = new List<int>();

                    for (int j = 1; j <= dim2; j++)  //1 based array, for each column
                    {
                        RowColumnAttributes rAttrib = new RowColumnAttributes();
                        FindColumnAttributes(ref valueArray, firstRow, j, ref rAttrib, dateFormats, ref dateSeparator);

                        CellDataType? thisColumnLike = rAttrib.GetCellDataTypeByMaxAttributeCount();
                        ColumnMetaData thisColData = new ColumnMetaData();
                        thisColData.ColumnType = (CellDataType)thisColumnLike;
                        thisColData.ColAttribs = rAttrib;
                        tableData.DicColData.Add(j, thisColData);

                        switch (thisColumnLike)
                        {
                            case CellDataType.MultiplePrefix:
                            case CellDataType.Prefix:
                                if (possDateMulPrefixIndex.Count == 0)
                                {
                                    possDateMulPrefixIndex.Add(j);
                                    tableData.IndexPrefix1 = j;
                                }
                                else
                                {
                                    if (possDateMulPrefixIndex.Count == 1 && j > possDateMulPrefixIndex[0])
                                    {
                                        possDateMulPrefixIndex.Add(j);
                                        tableData.IndexPrefix2 = j;
                                    }
                                }
                                break;

                            case CellDataType.Datetime:

                                if (possDateColsIndex.Count == 0)
                                {
                                    possDateColsIndex.Add(j);
                                    tableData.IndexStartDate = j;
                                }
                                else
                                {
                                    if (possDateColsIndex.Count == 1 && j > possDateColsIndex[0])
                                    {
                                        possDateColsIndex.Add(j);
                                        tableData.IndexEndDate = j;
                                    }
                                }
                                break;
                            case CellDataType.Rate:
                                tableData.IndexRate = j;
                                break;
                            case CellDataType.CountryName:
                                tableData.IndexCountryName = j;
                                break;
                            case CellDataType.CountryCode:
                                tableData.IndexCountryCode = j;
                                break;
                        }

                    }//for each column

                    //find out description column
                    int maxColIndexChar = -1;
                    int maxUniqueWordCount = 0;//prefix description column will have more unique word rather than increase/decrease/CLI route etc.
                    for (int c = 1; c <= tableData.DicColData.Count; c++)
                    {
                        ColumnMetaData cData = null;
                        tableData.DicColData.TryGetValue(c, out cData);
                        int max = 0;
                        if (cData.ColumnType != CellDataType.MultiplePrefix && cData.ColumnType != CellDataType.Prefix
                            && cData.ColumnType != CellDataType.Datetime && cData.ColumnType != CellDataType.Rate
                            && cData.ColumnType != CellDataType.CountryName)
                        {
                            if (cData.ColumnType == CellDataType.DescriptionMultipleWord && cData.ColAttribs.CharacterCount > max
                                && cData.ColAttribs.DicUnqWord.Count > maxUniqueWordCount)
                            {
                                max = cData.ColAttribs.CharacterCount;
                                maxUniqueWordCount = cData.ColAttribs.DicUnqWord.Count;
                                maxColIndexChar = c;
                            }
                        }
                    }

                    tableData.IndexDescription = maxColIndexChar;

                    return "";
                }
                //catch (Exception e1)
                //{
                //    return e1.InnerException.ToString();
                //}
            }

        }





    }
}
