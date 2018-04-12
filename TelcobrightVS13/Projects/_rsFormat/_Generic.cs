using MediationModel;
using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using LibraryExtensions;
using TelcobrightMediation;
/// <summary>
/// Summary description for myExcel
/// </summary>
namespace RateSheetFormat
{
    [Export("RateSheetFormat", typeof(IRateSheetFormat))]
    public class rsGeneric : IRateSheetFormat//change here
    {
        public override string ToString()
        {
            return RuleName;
        }
        private string _helpText = "Rate Sheet Format for Generic/Excel";//change for each extension
        public string RuleName
        {
            get { return "rsFormatGeneric"; }//change for each extension
        }

        public string HelpText
        {
            get { return _helpText; }
        }

        public int Id
        {
            get { return 1; }//change for each extension
        }

        public Dictionary<string, countrycode> dicCountryCode = new Dictionary<string, countrycode>();
        public Dictionary<string, countrycode> dicCountryName = new Dictionary<string, countrycode>();

        public rsGeneric()//change here
        {
            //
            // TODO: Add constructor logic here
            //
            //using(
            using (PartnerEntities Context = new PartnerEntities())
            {
                foreach (countrycode Cn in Context.countrycodes.ToList())
                {
                    dicCountryCode.Add(Cn.Code, Cn);
                    dicCountryName.Add(Cn.Name.ToLower(), Cn);
                }
            }

        }


        public enum ImportTaskType
        {
            RateChanges,
            CodeChanges
        }

        public string GetRates(string FilePath, ref List<ratetask> lstRateTask, rateplan rp, bool EndAllPrevPrefix, string[] DateFormats)
        {
            //try
            {
                string DateSeparator = "";
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

                book = app.Workbooks.Open(FilePath,
                       Missing.Value, Missing.Value, Missing.Value,
                       Missing.Value, Missing.Value, Missing.Value, Missing.Value,
                       Missing.Value, Missing.Value, Missing.Value, Missing.Value,
                       Missing.Value, Missing.Value, Missing.Value);

                Worksheet sheet = new Worksheet();
                sheet = (Worksheet)book.Worksheets[1];

                SheetImportTask ImportTask = new SheetImportTask(ImportTaskType.RateChanges,
                    sheet, dicCountryCode, dicCountryName, rp);

                string Retval = ImportTask.GetRateArray(ref lstRateTask, rp, book, DateFormats, ref DateSeparator);
                if (Retval != "")
                {
                    return Retval;
                }



                //code delete part for tata, bharti and others
                List<ratetask> lstCodeDeletes = new List<ratetask>();
                if (EndAllPrevPrefix == true)
                {
                    ratetask DelTask = new ratetask();
                    DelTask.Prefix = "*";
                    DelTask.rateamount = "-1";//rate
                    DelTask.startdate = "";//delete date, will be set in createnewrate method in the ratetask.aspx.cs
                    DelTask.Resolution = "1";//set any resolution
                    DelTask.MinDurationSec = "1";//set any minduration
                    lstCodeDeletes.Add(DelTask);
                }
                

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
            IDT = 103,
            Mainberg = 104,
            Vodafone = 105,
            Idea = 106,
            CKRI = 107,
            DialogAxiata = 108,
            Deutsche = 109,
            Singtel = 110,
            Starhub = 111,
            TM = 112
        }


        //vendor specific parameters
        public class VendorSpecificParameters
        {
            public ParamBharti paramBharti = new ParamBharti();
            public ParamDialog paramDialog = new ParamDialog();
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
            string IncludedTechPrefix = "";
            ImportTaskType ImportTaskType;
            Worksheet sheet = null;
            object[,] objArray = null;
            public int FirstRow;

            RateTableMetaData TableData;
            Range range = null;

            Dictionary<string, countrycode> dicCountryCode = new Dictionary<string, countrycode>();
            Dictionary<string, countrycode> dicCountryName = new Dictionary<string, countrycode>();

            //constructor
            public SheetImportTask(ImportTaskType p_ImportTaskType, Worksheet p_sheet, Dictionary<string, countrycode> p_dicCountryCode, Dictionary<string, countrycode> p_dicCountryName, rateplan p_RatePlan)
            {
                dicCountryCode = p_dicCountryCode;
                dicCountryName = p_dicCountryName;
                ImportTaskType = p_ImportTaskType;
                sheet = p_sheet;
                range = sheet.UsedRange;
                RatePlan = p_RatePlan;
            }

            string ArrayFromCSV(ref object[,] objArray, ref List<string[]> strLines, ref char SepCharForCSV)
            {
                try
                {
                    char[] SepArr = new char[] { ',', ':', ';', '`' };
                    SepCharForCSV = ',';
                    //find separator chracter
                    Dictionary<char, int> dicCharCount = new Dictionary<char, int>();
                    dicCharCount.Add(',', 0);
                    dicCharCount.Add(':', 0);
                    dicCharCount.Add(';', 0);
                    dicCharCount.Add('`', 0);

                    int Dim1 = objArray.GetLength(0);
                    int Dim2 = objArray.GetLength(1);
                    //count occurance of each char, max will be the separator
                    for (int i = 1; i <= Dim1; i++)
                        for (int j = 1; j <= Dim2; j++)
                            foreach (char ch in SepArr)
                            {
                                dicCharCount[ch] += objArray[i, j].ToString().Count(c => c == ch);
                            }
                    SepCharForCSV = (from x in dicCharCount where x.Value == dicCharCount.Max(v => v.Value) select x.Key).First();
                    //split csv rows to multiple field row
                    for (int i = 1; i <= Dim1; i++)
                    {
                        strLines.Add(objArray[i, 1].ToString().Split(SepCharForCSV));
                    }

                    return "";
                }
                catch (Exception e1)
                {
                    return e1.Message;
                }
            }

            public string GetRateArray(ref List<ratetask> lstRateTask, rateplan RP, Workbook book, string[] DateFormats, ref string DateSeparator)
            {
                try
                {
                    object[,] objArray = (object[,])range.get_Value(XlRangeValueDataType.xlRangeValueDefault);
                    int Dim1 = objArray.GetLength(0);
                    int Dim2 = objArray.GetLength(1);

                    //replace nulls with zero lengh string
                    for (int r = 1; r <= Dim1; r++)
                    {
                        for (int c = 1; c <= Dim2; c++)
                        {
                            if (objArray[r, c] == null)
                                objArray[r, c] = "";
                        }
                    }

                    //remove char ' (single quote) from obj array, caused problem in Sql
                    for (int r = 1; r <= Dim1; r++)
                    {
                        for (int c = 1; c <= Dim2; c++)
                        {
                            if (objArray[r, c] != null)
                                objArray[r, c] = objArray[r, c].ToString().Replace("'", "");
                        }
                    }

                    //for text and csv/delimited files dim2 could be one
                    //have to split the rows first
                    char SepCharForCSV = ',';
                    if (Dim2 == 1)
                    {
                        List<string[]> csvArr = new List<string[]>();
                        ArrayFromCSV(ref objArray, ref csvArr, ref SepCharForCSV);

                        //convert 0 based csv array to 1 based obj array to handle delimated files
                        Dim1 = csvArr.Count;
                        Dim2 = csvArr[0].GetLength(0);
                        //objArray = new object[Dim1, Dim2];//don't use the 0 index, start from 1
                        //objArray = Array.CreateInstance(typeof(object), new object[] { 1, Dim1 }, new object[] { 1, Dim2 });
                        objArray = (object[,])Array.CreateInstance(typeof(object),
                                   new int[] { Dim1, Dim2 },
                                   new int[] { 1, 1 });
                        for (int r = 1; r <= Dim1; r++)
                        {
                            for (int c = 1; c <= Dim2; c++)
                            {
                                objArray[r, c] = csvArr[r - 1][c - 1];
                            }
                        }
                    }
                    FirstRow = FindFirstRow(ref objArray, DateFormats, ref DateSeparator);





                    //rather than handling intlout and in cases, try a generic method, based on importing with column header
                    if (FirstRow == 2)//there is coumn header
                    {
                        Dictionary<int, string> dicCols = new Dictionary<int, string>();
                        //if at least 2 value matches 2 column names then the first row contains the columnname
                        int MatchedColumnCount = 0;
                        List<string> ColNames = new List<string>();
                        ColNames.Add("prefix");
                        ColNames.Add("rateamount");
                        for (int j = 1; j <= Dim2; j++)
                        {
                            string FldName = objArray[1, j].ToString().Trim().ToLower();
                            if (FldName != "" && dicCols.ContainsValue(FldName) == false)
                            {
                                dicCols.Add(j, FldName);
                            }

                            if (ColNames.Contains(objArray[1, j]))
                            {
                                MatchedColumnCount++;
                            }
                        }
                        if (MatchedColumnCount >= 2)//first row contains heading
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


                    ImportTaskType ThisSheetType = ImportTaskType.RateChanges;
                    if (ImportTaskType == ImportTaskType.RateChanges)//vendor format already set after rate changes sheet processing
                    {
                        VFormat = VendorFormat.None;//GetVendorFormat(ref objArray, FirstRow, book);
                    }
                    else
                    {
                        ThisSheetType = ImportTaskType.CodeChanges;
                    }

                    TableData = new RateTableMetaData();
                    FindTableMetaData(ref objArray, ref TableData, FirstRow, VFormat, DateFormats, ref DateSeparator);



                    List<string[]> strLines = MultipleToSinglePrefixArray(ThisSheetType, ref objArray, TableData, DateFormats, ref DateSeparator);
                    if (strLines == null)
                    {
                        return "Error converting multiple to single prefix array!";
                    }

                    lstRateTask = GetFinalRateArray(strLines, TableData, VFormat, DateFormats, ref DateSeparator);
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
                    int Dim1 = valueArray.GetLength(0);
                    int Dim2 = valueArray.GetLength(1);
                    for (int i = 2; i <= Dim1; i++)//skip first row with column headers
                    {
                        List<string> lstJson = new List<string>();
                        for (int j = 1; j <= Dim2; j++)
                        {
                            //if this colindex doesn't exist in the diccols, then continue
                            if (dicCols.ContainsKey(j) == false)
                            {
                                continue;
                            }
                            string FldName = "";
                            dicCols.TryGetValue(j, out FldName);

                            string Val = "";
                            if (StringExtensions.IsNumeric(valueArray[i, j].ToString()) == true)
                            {
                                Val = valueArray[i, j].ToString();
                                //only for prefix, which is numeric but text
                                if (FldName == "prefix")
                                {
                                    Val = "'" + Val + "'";
                                }

                            }
                            else//nto numeric
                            {
                                Val = "'" + valueArray[i, j].ToString() + "'";
                                if (FldName == "startdate")
                                {
                                    DateTime eDate = new DateTime();
                                    if (DateTime.TryParse(Val.Replace("'", ""), out eDate) == true)
                                    {
                                        Val = "'" + eDate.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                    }
                                }
                            }
                            lstJson.Add("'" + FldName + "':" + Val);
                        }
                        StringBuilder sbJson = new StringBuilder().Append("{ ").Append(string.Join(",", lstJson)).Append(" }");
                        ratetask Newtask = JsonConvert.DeserializeObject<ratetask>(sbJson.ToString());
                        lstRateTask.Add(Newtask);
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
                if (lstTask[0].Prefix == "") return;
                string FirstChar = lstTask[0].Prefix.Substring(0, 1);
                int CommonPrefixCount = 0;
                int CountryCodeInclusionCount = 0;
                int CCAtStart = 0;
                bool CCIncluded = false;
                foreach (ratetask rtask in lstTask)
                {
                    if (rtask.CountryCode.Trim() != "" && rtask.Prefix.StartsWith(rtask.CountryCode))
                    {
                        CCAtStart++;
                    }
                    if (rtask.Prefix.StartsWith(FirstChar))
                    {
                        CommonPrefixCount++;
                    }
                    if (rtask.CountryCode.Trim() != "" && rtask.Prefix.Contains(rtask.CountryCode))
                    {
                        CountryCodeInclusionCount++;
                    }
                    else if (rtask.CountryCode.Trim() == "")//if country code is not mentioned, prefix must contain cc
                    {
                        CountryCodeInclusionCount++;
                    }
                }
                if (Convert.ToSingle(decimal.Divide(CCAtStart, lstTask.Count)) >= .8)//>80%
                {
                    CCIncluded = true;
                }
                else if (CommonPrefixCount == lstTask.Count && CountryCodeInclusionCount == lstTask.Count)
                {
                    //prefix starts with common prefix
                    //and country code exists in all prefix
                    CCIncluded = true;
                }
                if (CCIncluded == false)
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
                public RateFormatWithOrder(string formatString, int SortOrder)
                {
                    DateFormat = formatString;
                    Order = SortOrder;
                }
                public override string ToString()
                {
                    return Order.ToString() + "=>" + DateFormat;
                }
            }
            List<ratetask> GetFinalRateArray(List<string[]> lstSinglePrefixArray, RateTableMetaData TableData, VendorFormat VFormat, string[] DateFormats, ref string DateSeparator)
            {

                //here order by dateformats with the help of DateSeparator to improve performance during parsing with huge format array
                if (DateSeparator != "")
                {
                    List<RateFormatWithOrder> lstFormatOrder = new List<RateFormatWithOrder>();
                    foreach (string format in DateFormats)
                    {
                        lstFormatOrder.Add(new RateFormatWithOrder(format, (format.Contains(DateSeparator) == true ? 0 : 100)));
                    }
                    lstFormatOrder = lstFormatOrder.OrderBy(c => c.Order).ToList();
                    List<string> DateFormatsOrdered = new List<string>();
                    foreach (RateFormatWithOrder FormatOrder in lstFormatOrder)
                    {
                        DateFormatsOrdered.Add(FormatOrder.DateFormat);
                    }
                    DateFormats = DateFormatsOrdered.ToArray();
                }

                List<ratetask> lstRates = new List<ratetask>();
                List<string> cachedFormats = new List<string>() { "yyyy-MM-dd" };//must add at least one so that foreach later works
                try
                {
                    foreach (string[] ThisRow in lstSinglePrefixArray)
                    {

                        ratetask NewTask = new ratetask();
                        //Row 0://prefix
                        //NewRow[0] 
                        NewTask.Prefix = (TableData.IndexPrefix1) > -1 ? ThisRow[TableData.IndexPrefix1 - 1] : "";//0 & 1 based array adjustment
                                                                                                                  //Row 1://description
                                                                                                                  //NewRow[1] 
                        NewTask.description = (TableData.IndexDescription) > -1 ? ThisRow[TableData.IndexDescription - 1] : "";
                        //Row 2://rate
                        if (ImportTaskType == ImportTaskType.RateChanges)
                        {
                            if (VFormat == VendorFormat.DialogAxiata)
                            {
                                NewTask.rateamount = ThisRow[VendorParams.paramDialog.RateColIndex];
                            }
                            else
                            {
                                NewTask.rateamount = (TableData.IndexRate) > -1 ? ThisRow[TableData.IndexRate - 1] : "";
                            }
                        }
                        else//code delete
                        {
                            //NewRow[2] 
                            NewTask.rateamount = "-1";
                        }
                        //Row 3://pulse
                        //NewRow[3] = 
                        NewTask.Resolution = (TableData.IndexPulse) > -1 ? ThisRow[TableData.IndexPulse - 1] : "";
                        if (NewTask.Resolution == "")
                        {
                            NewTask.Resolution = RatePlan.Resolution.ToString();//fetch default
                        }
                        //Row 4://minduration
                        //NewRow[4] 
                        NewTask.MinDurationSec = (TableData.IndexMinDuration) > -1 ? ThisRow[TableData.IndexMinDuration - 1] : "";
                        if (NewTask.MinDurationSec == "")
                        {
                            NewTask.MinDurationSec = RatePlan.mindurationsec.ToString();//fetch default
                        }
                        //Row 5://countrycode
                        //NewRow[5] 
                        NewTask.CountryCode = (TableData.IndexCountryCode) > -1 ? ThisRow[TableData.IndexCountryCode - 1] : "";
                        //Row 6://effectivedate
                        DateTime TempDate = new DateTime();
                        if (VFormat == VendorFormat.Bharti)//bharti specific
                        {
                            switch (ThisRow[5].ToLower().Trim())
                            {
                                case ""://unchanged
                                        //NewRow[6] 
                                    NewTask.startdate = VendorParams.paramBharti.DefaultUnchangedValidityDate.ToString("yyyy-MM-dd HH:mm:ss");
                                    break;
                                case "increase":
                                    NewTask.startdate = VendorParams.paramBharti.DefaultIncreaseValidityDate.ToString("yyyy-MM-dd HH:mm:ss");
                                    break;
                                case "decrease":
                                    NewTask.startdate = VendorParams.paramBharti.DefaultDecreaseValidityDate.ToString("yyyy-MM-dd HH:mm:ss");
                                    break;
                                default://code update
                                    NewTask.startdate = VendorParams.paramBharti.DefaultCodeUpdateValidityDate.ToString("yyyy-MM-dd HH:mm:ss");
                                    break;
                            }
                        }
                        else
                        {
                            NewTask.startdate = (TableData.IndexStartDate) > -1 ? ThisRow[TableData.IndexStartDate - 1] : "";

                            //usally there will be only one dateformat in ratesheet
                            //program will learn that or any other using cached & noncached list
                            //SIGNIFICANT improvement of performance, no need to tryparse thousands of date formats
                            bool matchFound = false;
                            foreach (string format in cachedFormats)
                            {
                                if (DateTime.TryParseExact(NewTask.startdate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out TempDate))
                                {
                                    NewTask.startdate = TempDate.ToString("yyyy-MM-dd HH:mm:ss");
                                    matchFound = true;
                                    break;
                                }
                            }
                            if (matchFound == false)  //format doesn't exist in the cache, try all formats
                            {
                                foreach (string nonCachedFormat in DateFormats)
                                {
                                    if (DateTime.TryParseExact(NewTask.startdate, nonCachedFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out TempDate))
                                    {
                                        cachedFormats.Insert(0, nonCachedFormat);//keep latest match at first.
                                        NewTask.startdate = TempDate.ToString("yyyy-MM-dd HH:mm:ss");
                                        matchFound = true;
                                        break;
                                    }
                                }
                            }
                            if (DateTime.TryParseExact(NewTask.startdate, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out TempDate))
                            {
                                NewTask.startdate = TempDate.ToString("yyyy-MM-dd HH:mm:ss");
                            }


                        }

                        //Row 7://enddate
                        //NewRow[7] 
                        NewTask.enddate = (TableData.IndexEndDate) > -1 ? ThisRow[TableData.IndexEndDate - 1] : "";
                        //format date
                        DateTime TempDate1 = new DateTime();
                        if (DateTime.TryParse(NewTask.enddate, out TempDate1) == true)
                        {
                            NewTask.enddate = TempDate.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        //Row 8://surchargetime
                        //NewRow[8] 
                        NewTask.SurchargeTime = (TableData.IndexSurchargeTime) > -1 ? ThisRow[TableData.IndexSurchargeTime - 1] : "";
                        if (NewTask.SurchargeTime == "")
                        {
                            NewTask.SurchargeTime = RatePlan.SurchargeTime.ToString();//fetch default
                        }
                        //Row 9://surcharge amount
                        //NewRow[9] 
                        NewTask.SurchargeAmount = (TableData.IndexSurchargeAmount) > -1 ? ThisRow[TableData.IndexSurchargeAmount - 1] : "";
                        if (NewTask.SurchargeAmount == "")
                        {
                            NewTask.SurchargeAmount = RatePlan.SurchargeAmount.ToString();//fetch default
                        }
                        //Row 10://service type
                        //NewRow[10] 
                        NewTask.Category = (TableData.IndexServiceType) > -1 ? ThisRow[TableData.IndexServiceType - 1] : "";
                        if (NewTask.Category == "")
                        {
                            NewTask.Category = RatePlan.Category.ToString();//fetch default
                        }
                        //Row 11://sub service type
                        //NewRow[11] 
                        NewTask.SubCategory = (TableData.IndexSubServiceType) > -1 ? ThisRow[TableData.IndexSubServiceType - 1] : "";
                        if (NewTask.SubCategory == "")
                        {
                            NewTask.SubCategory = RatePlan.SubCategory.ToString();//fetch default
                        }

                        ////***vendor specific modification
                        ////concat country+prefix for tata
                        //if (VFormat == VendorFormat.Tata)
                        //{
                        //    //NewRow[0] = NewRow[5] + NewRow[0];
                        //    NewTask.Prefix = NewTask.CountryCode + NewTask.Prefix;
                        //}

                        lstRates.Add(NewTask);
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
            string RangeToMultiplePrefix(string Value)
            {
                string Retval = "";
                if (Value.Contains("-") == false)
                {
                    return Value;
                }

                Value = Value.Replace(" ", "");//replace space
                Value = Value.Replace("\t", "");//replace space
                                                //separate cell value as comma (or other then -) separated values
                char[] SepArr = new char[] { ',', ':', ';' };
                char SepChar = ',';
                //find separator chracter
                foreach (char ch in SepArr)
                {
                    if (Value.Contains(ch))
                    {
                        SepChar = ch;
                        break;
                    }
                }

                string[] SeparatedPrefixArr = Value.Split(SepChar);
                List<string> lstRangeNormalizedArr = new List<string>();

                foreach (string ThisValue in SeparatedPrefixArr)
                {

                    if (ThisValue.Contains("-") == false)
                    {
                        lstRangeNormalizedArr.Add(ThisValue);
                        continue;
                    }

                    // '-' exists, normalize range for this pair e.g. 6114-6118
                    string PrefixBeforeDash = ThisValue.Split('-')[0];
                    string PrefixAfterDash = ThisValue.Split('-')[1];


                    List<string> lstPrefixeOneRange = new List<string>();


                    RangeType rangetype = RangeType.None;
                    if (PrefixAfterDash.Length >= PrefixBeforeDash.Length)
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
                            long EndNumber = 0;
                            long StartNumber = 0;
                            if (long.TryParse(PrefixAfterDash, out EndNumber))
                            {
                                if (long.TryParse(PrefixBeforeDash, out StartNumber))
                                {
                                    if (EndNumber > StartNumber)
                                    {
                                        long Difference = EndNumber - StartNumber;
                                        for (long tLong = StartNumber; tLong <= EndNumber; tLong++)
                                        {
                                            lstPrefixeOneRange.Add(tLong.ToString());
                                        }
                                    }
                                }
                            }
                            break;
                        case RangeType.TrailingDigits:
                            lstPrefixeOneRange = new List<string>();
                            string Commonprefix = PrefixBeforeDash.Substring(0, PrefixBeforeDash.Length - PrefixAfterDash.Length);
                            long NumToIncrement = long.Parse(PrefixBeforeDash.Substring(Commonprefix.Length, PrefixBeforeDash.Length - Commonprefix.Length));
                            long IncrementUpto = long.Parse(PrefixAfterDash);
                            if (IncrementUpto > NumToIncrement)
                            {
                                for (long i = NumToIncrement; i <= IncrementUpto; i++)
                                {
                                    lstPrefixeOneRange.Add(Commonprefix + i.ToString());
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
                public SeparatedPrefixArray(string Value)
                {
                    //value could be character separated too
                    //find separator chracter
                    char[] SepArr = new char[] { ',', ':', ';' };
                    char SepChar = ',';

                    foreach (char ch in SepArr)
                    {
                        if (Value.Contains(ch))
                        {
                            SepChar = ch;
                            break;
                        }
                    }
                    PrefixArray = Value.Split(SepChar);
                }
            }

            List<string[]> MultipleToSinglePrefixArray(ImportTaskType ThisSheetType, ref object[,] valueArray, RateTableMetaData TableData, string[] DateFormats, ref string DateSeparator)
            {
                //sometime prefix without countrycode and combined prefix both can exist
                //in that case later has more chracters and it should be the prefix column
                if (ThisSheetType == ImportTaskType.RateChanges && TableData.IndexPrefix1 > -1 && TableData.IndexPrefix2 > -1)
                {
                    if (TableData.dicColData[TableData.IndexPrefix2].ColAttribs.CharacterCount
                        > TableData.dicColData[TableData.IndexPrefix1].ColAttribs.CharacterCount)
                    {
                        TableData.IndexPrefix1 = TableData.IndexPrefix2;
                    }
                }

                List<string[]> SinglePrefixTableRows = new List<string[]>();
                try
                {
                    int Dim2 = valueArray.GetLength(1);
                    for (int i = TableData.FirstRow; ; i++)//1 based array returned by excel
                    {
                        if (CheckIfBeyondLastRow(i, ref valueArray, DateFormats, ref DateSeparator) == true)
                        {
                            return SinglePrefixTableRows;
                        }

                        string Value = valueArray[i, TableData.IndexPrefix1].ToString().Replace(" ", "").Replace("\t", "");
                        Value = RangeToMultiplePrefix(Value);

                        SeparatedPrefixArray SepOldCodes = new SeparatedPrefixArray(Value);
                        string[] FinalPrefixesForThisRow = null;

                        if (ThisSheetType == ImportTaskType.RateChanges)
                        {
                            FinalPrefixesForThisRow = SepOldCodes.PrefixArray;
                        }
                        else//code changes,//code delete handling e.g. for tata
                        {
                            string ValNewCodes = valueArray[i, TableData.IndexPrefix2].ToString().Replace(" ", "").Replace("\t", "");
                            ValNewCodes = RangeToMultiplePrefix(ValNewCodes);
                            SeparatedPrefixArray SepNewCodes = new SeparatedPrefixArray(ValNewCodes);
                            FinalPrefixesForThisRow = (from s in SepOldCodes.PrefixArray.Except(SepNewCodes.PrefixArray)
                                                       where s != ""//exclude where old entries had only country, new code added, not significant for code deletion
                                                       select s).ToArray();
                        }

                        int Prefixcount = FinalPrefixesForThisRow.Count();

                        List<string[]> LstRows = new List<string[]>();//multi to single prefix normalized row

                        for (int k = 0; k < Prefixcount; k++)
                        {
                            string[] OneRow = new string[Dim2];
                            for (int j = 1; j <= Dim2; j++)
                            {
                                if (j != TableData.IndexPrefix1)
                                {
                                    OneRow[j - 1] = valueArray[i, j] != null ? valueArray[i, j].ToString().Trim() : "";
                                }
                            }
                            OneRow[TableData.IndexPrefix1 - 1] = FinalPrefixesForThisRow[k].Trim();//code deletes are normalized in index1 like rate changes
                            LstRows.Add(OneRow);
                        }

                        foreach (string[] Rstr in LstRows)//lstrows may contain one or multiple rows depending on
                                                          //single or multiple prefix
                        {
                            SinglePrefixTableRows.Add(Rstr);
                        }
                    }
                }//try
                catch (Exception e1)
                {
                    return null;
                }
                return SinglePrefixTableRows;
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
                NULL
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

                public Dictionary<int, ColumnMetaData> dicColData = new Dictionary<int, ColumnMetaData>();//keep
                                                                                                          //row column attribs for each column (key, 1 based) in case they are required 
            }

            class ColumnMetaData
            {
                public CellDataType ColumnType = CellDataType.NULL;
                public RowColumnAttributes ColAttribs = new RowColumnAttributes();
            }


            class RowColumnAttributes
            {
                public int CharacterCount = 0;
                public Dictionary<string, int> dicUnqWord = new Dictionary<string, int>();

                public int PossUndeterminedCount = 0;
                public int PossRateCount = 0;
                public int PossPrefixCount = 0;
                public int PossCountryCodeCount = 0;
                public int PossCountryNameCount = 0;
                public int PossMultipleWordCount = 0;
                public int PossDateCount = 0;




                public int GetAttributeCount()
                {
                    return PossRateCount + PossPrefixCount + PossCountryCodeCount + PossCountryNameCount + PossMultipleWordCount + PossDateCount;
                }

                public CellDataType GetCellDataTypeByMaxAttributeCount()
                {
                    int Max = PossUndeterminedCount;
                    CellDataType MaxLike = CellDataType.Undetermined;
                    if (PossRateCount > Max)
                    {
                        Max = PossRateCount;
                        MaxLike = CellDataType.Rate;
                    }
                    if (PossPrefixCount > Max)
                    {
                        Max = PossPrefixCount;
                        MaxLike = CellDataType.Prefix;//could be multipleprefix also, but set column type=prefix will do
                    }
                    if (PossCountryCodeCount > Max)
                    {
                        Max = PossCountryCodeCount;
                        MaxLike = CellDataType.CountryCode;
                    }
                    if (PossCountryNameCount > Max)
                    {
                        Max = PossCountryNameCount;
                        MaxLike = CellDataType.CountryName;
                    }
                    if (PossMultipleWordCount > Max)
                    {
                        Max = PossMultipleWordCount;
                        MaxLike = CellDataType.DescriptionMultipleWord;
                    }
                    if (PossDateCount > Max)
                    {
                        Max = PossDateCount;
                        MaxLike = CellDataType.Datetime;
                    }
                    return MaxLike;
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
                            CellDataType? ThisColumnLike = (objArray[i, j] != null ? CellDataType.Undetermined : CellDataType.NULL);
                            switch (ThisColumnLike)
                            {
                                case CellDataType.NULL:
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



            VendorFormat GetVendorFormat(ref object[,] objArray, int FirstRow, Workbook book)
            {
                int Dim2 = objArray.GetLength(1);
                string HeaderText = "";
                if ((FirstRow == 2 || FirstRow == 1) && Dim2 == 12)//1st row column header
                {
                    return VendorFormat.GenericExcel;
                }
                else if (FirstRow <= 0)//first worksheet does not contain rates and vendor information
                {
                    HeaderText = HeaderTextByAllSheet(book);
                }
                else
                {
                    int i = 1;
                    //concat all text before first rate row
                    StringBuilder sbstr = new StringBuilder();
                    for (i = 1; i < FirstRow; i++)//1 based array returned by excel
                    {
                        RowColumnAttributes RAttrib = new RowColumnAttributes();
                        int j = 1;
                        for (j = 1; j <= Dim2; j++)  //1 based array
                        {
                            CellDataType? ThisColumnLike = (objArray[i, j] != null ? CellDataType.Undetermined : CellDataType.NULL);
                            switch (ThisColumnLike)
                            {
                                case CellDataType.NULL:
                                    continue;
                                default:
                                    sbstr.Append(objArray[i, j].ToString().ToLower());
                                    break;
                            }
                        }
                    }
                    HeaderText = sbstr.ToString();
                }



                if (HeaderText.Contains("bharti airtel") && HeaderText.Contains("increase validity date")
                    && HeaderText.Contains("decrease validity date") && HeaderText.Contains("unchanged validity date"))
                {
                    //find bharti specific paramaters
                    for (int i = 1; i < FirstRow; i++)//1 based array returned by excel
                    {
                        int j = 1;
                        for (j = 1; j <= Dim2; j++)  //1 based array
                        {
                            DateTime TempDate = new DateTime(1, 1, 1);
                            string CellValue = objArray[i, j] == null ? "" : objArray[i, j].ToString().ToLower();
                            string DateValue = "";
                            if (j < Dim2)
                            {
                                DateValue = objArray[i, j + 1] == null ? "" : objArray[i, j + 1].ToString();
                            }
                            switch (CellValue)
                            {
                                case "increase validity date:":
                                    if (DateTime.TryParse(DateValue, out TempDate) == true)
                                    {
                                        VendorParams.paramBharti.DefaultIncreaseValidityDate = TempDate;
                                    }
                                    break;
                                case "decrease validity date:":
                                    if (DateTime.TryParse(DateValue, out TempDate) == true)
                                    {
                                        VendorParams.paramBharti.DefaultDecreaseValidityDate = TempDate;
                                    }
                                    break;
                                case "code update validity date:":
                                    if (DateTime.TryParse(DateValue, out TempDate) == true)
                                    {
                                        VendorParams.paramBharti.DefaultCodeUpdateValidityDate = TempDate;
                                    }
                                    break;
                                case "unchanged validity date:":
                                    if (DateTime.TryParse(DateValue, out TempDate) == true)
                                    {
                                        VendorParams.paramBharti.DefaultUnchangedValidityDate = TempDate;
                                    }
                                    break;
                            }
                        }
                    }
                    return VendorFormat.Bharti;
                }
                else if (HeaderText.Contains("tata communications") && HeaderText.Contains("billing entity")
                    && HeaderText.Contains("rate change amendment") && HeaderText.Contains("customer no"))
                {
                    return VendorFormat.Tata;
                }
                else if (HeaderText.Contains("a-z termination rates") && HeaderText.Contains("billing increment")
                    && HeaderText.Contains(" new = n") && HeaderText.Contains("no change = nc"))
                {
                    return VendorFormat.Mainberg;
                }
                else if (HeaderText.Contains("time zone") && HeaderText.Contains("date format")
                    && HeaderText.Contains("price stated") && HeaderText.Contains("ckri"))
                {
                    return VendorFormat.CKRI;
                }
                else if (HeaderText.Contains("dialog axiata plc") && HeaderText.Contains("increase notice period"))
                {
                    return VendorFormat.DialogAxiata;
                }
                else if (HeaderText.Contains("deutsche telekom ag") && HeaderText.Contains("price list content "))
                {
                    return VendorFormat.Deutsche;
                }
                else if (HeaderText.Contains("singapore telecommunications limited"))
                {
                    return VendorFormat.Singtel;
                }
                else if (HeaderText.Contains("confidential pricing to:") && HeaderText.Contains("pricing valid from: "))
                {
                    return VendorFormat.TM;
                }
                else if (HeaderText.Contains("the stipulated rates are effective") && HeaderText.Contains("starhub"))
                {
                    return VendorFormat.Starhub;
                }
                else
                {
                    return VendorFormat.None;
                }

            }




            int FindFirstRow(ref object[,] objArray, string[] DateFormats, ref string DateSeparator)
            {
                int RowIndex = -1;
                int Dim1 = objArray.GetLength(0);
                int Dim2 = objArray.GetLength(1);

                Dim1 = objArray.GetLength(0);
                Dim2 = objArray.GetLength(1);

                int i = 1;
                for (i = 1; i <= Dim1; i++)//1 based array returned by excel
                {
                    RowColumnAttributes RAttrib = new RowColumnAttributes();
                    int j = 1;

                    for (j = 1; j <= Dim2; j++)  //1 based array
                    {
                        CellDataType? ThisColumnLike = CellDataType.NULL;

                        ThisColumnLike = (objArray[i, j] != null ? FindCellDataType(objArray[i, j].ToString().Trim(), DateFormats, ref DateSeparator) : CellDataType.NULL);

                        switch (ThisColumnLike)
                        {
                            case CellDataType.NULL:
                                continue;
                                break;
                            case CellDataType.MultiplePrefix:
                            case CellDataType.Prefix:
                                RAttrib.PossPrefixCount++;
                                break;
                            case CellDataType.Datetime:
                                RAttrib.PossDateCount++;
                                break;
                            case CellDataType.Rate:
                                RAttrib.PossRateCount++;
                                break;
                            case CellDataType.CountryName:
                                RAttrib.PossCountryNameCount++;
                                break;
                            case CellDataType.CountryCode:
                                RAttrib.PossCountryCodeCount++;
                                break;
                        }
                    }

                    if (RAttrib.GetAttributeCount() >= 2)
                    {
                        RowIndex = i;
                        return RowIndex;
                    }
                }
                return RowIndex;
            }

            bool CheckIfBeyondLastRow(int Rowindex, ref object[,] objArray, string[] DateFormats, ref string DateSeparator)
            {
                int Dim1 = objArray.GetLength(0);
                if (Rowindex > Dim1)
                {
                    return true;
                }
                int i = Rowindex;
                int Dim2 = objArray.GetLength(1);

                RowColumnAttributes RAttrib = new RowColumnAttributes();
                int j = 1;

                for (j = 1; j <= Dim2; j++)  //1 based array
                {
                    CellDataType? ThisColumnLike = (objArray[i, j] != null ? FindCellDataType(objArray[i, j].ToString().Trim(), DateFormats, ref DateSeparator) : CellDataType.NULL);
                    switch (ThisColumnLike)
                    {
                        case CellDataType.NULL:
                            continue;
                            break;
                        case CellDataType.MultiplePrefix:
                        case CellDataType.Prefix:
                            RAttrib.PossPrefixCount++;
                            break;
                        case CellDataType.Datetime:
                            RAttrib.PossDateCount++;
                            break;
                        case CellDataType.Rate:
                            RAttrib.PossRateCount++;
                            break;
                        case CellDataType.CountryName:
                            RAttrib.PossCountryNameCount++;
                            break;
                        case CellDataType.CountryCode:
                            RAttrib.PossCountryCodeCount++;
                            break;
                    }
                }

                if (RAttrib.GetAttributeCount() < 2)
                {

                    return true;
                }


                return false;

            }

            int FindColumnAttributes(ref object[,] objArray, int FirstRow, int ColumnIndex, ref RowColumnAttributes RAttrib, string[] DateFormats, ref string DateSeparator)
            {

                try
                {
                    int i = FirstRow;
                    int j = ColumnIndex;
                    int Dim1 = objArray.GetLength(0);
                    for (i = FirstRow; i <= (Dim1- FirstRow) && i <= Dim1; i++)
                    {
                        CellDataType? ThisColumnLike = (objArray[i, j] != null ? FindCellDataType(objArray[i, j].ToString(), DateFormats, ref DateSeparator) : CellDataType.NULL);
                        switch (ThisColumnLike)
                        {
                            case CellDataType.NULL:
                                continue;
                                break;
                            case CellDataType.MultiplePrefix:
                            case CellDataType.Prefix:
                                RAttrib.PossPrefixCount++;
                                break;
                            case CellDataType.Datetime:
                                RAttrib.PossDateCount++;
                                break;
                            case CellDataType.Rate:
                                RAttrib.PossRateCount++;
                                break;
                            case CellDataType.CountryName:
                                RAttrib.PossCountryNameCount++;
                                break;
                            case CellDataType.CountryCode:
                                RAttrib.PossCountryCodeCount++;
                                break;
                            case CellDataType.DescriptionMultipleWord:
                                RAttrib.PossMultipleWordCount++;
                                string[] Words = objArray[i, j].ToString().Split(null);
                                foreach (string str in Words)
                                {
                                    int Count = 0;
                                    if (RAttrib.dicUnqWord.ContainsKey(str.Trim()) == true)
                                    {
                                        RAttrib.dicUnqWord[str] += 1;
                                    }
                                    else
                                    {
                                        RAttrib.dicUnqWord.Add(str, 1);
                                    }

                                }

                                break;
                        }
                        RAttrib.CharacterCount += objArray[i, j].ToString().Length;

                    }//for each row

                    //set charactercount for current column 


                    return FirstRow;
                }
                catch (Exception e1)
                {
                    return -1;
                }
            }

            CellDataType FindCellDataType(string Value, string[] DateFormats, ref string DateSeparator)//must send value after using tostring()
            {
                Value = Value.Trim();
                CellDataType ThisLike = CellDataType.Undetermined;
                try
                {

                    double rateDouble = -1;
                    if ((Value.Contains(".")||Value.StartsWith("-")) && double.TryParse(Value, out rateDouble) == true)//-1 support
                    {
                        ThisLike = CellDataType.Rate;
                        return ThisLike;
                    }

                    DateTime myDateTime = new DateTime(1, 1, 1);

                    if ((Value.Contains("/")) &&
                        DateTime.TryParseExact(Value, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out myDateTime))
                    {
                        ThisLike = CellDataType.Datetime;
                        DateSeparator = "/";
                        return ThisLike;
                    }
                    else if ((Value.Contains("-")) &&
                        DateTime.TryParseExact(Value, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out myDateTime))
                    {
                        ThisLike = CellDataType.Datetime;
                        DateSeparator = "-";
                        return ThisLike;
                    }
                    else if ((Value.Contains(".")) &&
                        DateTime.TryParseExact(Value, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out myDateTime))
                    {
                        ThisLike = CellDataType.Datetime;
                        DateSeparator = ".";
                        return ThisLike;
                    }
                    else if ((Value.Contains(" ")) &&
                        DateTime.TryParseExact(Value, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out myDateTime))
                    {
                        ThisLike = CellDataType.Datetime;
                        DateSeparator = " ";
                        return ThisLike;
                    }

                    //replace "/" with "-". If dates contain "/", datetime.tryparse didn't work with 
                    else if (DateTime.TryParse(Value.Replace("/", "-"), out myDateTime) && (Value.Contains("/") || Value.Contains("-")))
                    {
                        ThisLike = CellDataType.Datetime;
                        return ThisLike;
                    }

                    string WholeNumbers = "0123456789";
                    bool WholeNumber = true;
                    bool MostlyDigits = false;
                    //value could be character separated too
                    char[] SepArr = new char[] { ',', ':', '-', ';' };
                    char SepChar = ',';
                    //find separator chracter
                    foreach (char ch in SepArr)
                    {
                        if (Value.Contains(ch))
                        {
                            SepChar = ch;
                            break;
                        }
                    }

                    string[] VArr = null;
                    VArr = Value.Split(SepChar);

                    //trim all
                    for (int s = 0; s < VArr.GetLength(0); s++)
                    {
                        VArr[s] = VArr[s].Trim();
                    }
                    foreach (char ch in Value.ToCharArray())
                    {
                        if (WholeNumbers.Contains(ch) == false)
                        {
                            WholeNumber = false;
                            break;
                        }
                    }
                    if (WholeNumber == true)//prefix or country
                    {
                        long Num = 0;
                        if (Int64.TryParse(VArr[0], out Num))
                        {
                            if (Num > 0)
                            {
                                if (dicCountryCode.ContainsKey(Num.ToString()))
                                {
                                    ThisLike = CellDataType.CountryCode;
                                    return ThisLike;
                                }
                                else
                                {
                                    ThisLike = CellDataType.Prefix;
                                    return ThisLike;
                                }
                            }
                        }
                    }
                    else//not exactly numeric cell
                    {
                        if (dicCountryName.ContainsKey(VArr[0].ToString().ToLower()) && VArr.GetLength(0) == 1)
                        {
                            ThisLike = CellDataType.CountryName;
                            return ThisLike;
                        }
                        if (VArr.GetLength(0) > 1)//comma separated cell
                        {
                            //multiple values, comma separated
                            //cannot be single prefix
                            //could be Multiple prefix
                            //check against first 2 elements of array after split if they are numbers
                            long Num = -1;
                            long Num2 = -1;
                            if (Int64.TryParse(VArr[0].Trim(), out Num))
                            {
                                if (Num >= 0)
                                {
                                    //make another check with the 2nd element in the array
                                    if (Int64.TryParse(VArr[1].Trim(), out Num2))
                                    {
                                        if (Num2 >= 0)
                                        {
                                            //first two elements are numeric, so likely to be multiple prefix
                                            ThisLike = CellDataType.MultiplePrefix;
                                            return ThisLike;
                                        }
                                    }

                                }
                            }
                            else//comma separated but values not numeric
                            {
                                //one last check if this cell contains prefix
                                //check if mostly digits

                                int DigitCount = 0;
                                int NonDigitCount = 0;
                                foreach (char ch in Value.ToCharArray())
                                {
                                    if (WholeNumbers.Contains(ch))
                                    {
                                        DigitCount++;
                                    }
                                    else NonDigitCount++;
                                }
                                if (DigitCount > NonDigitCount) MostlyDigits = true;

                                if (MostlyDigits == true)
                                {
                                    ThisLike = CellDataType.MultiplePrefix;
                                }
                                else
                                {
                                    ThisLike = CellDataType.DescriptionMultipleWord;
                                }

                                return ThisLike;
                            }

                        }
                        if (VArr.GetLength(0) == 1)//single cell, not comma separated and also not numeric
                        {

                            //single value only
                            //can be single or multiple word description
                            if (VArr[0].Split(null).GetLength(0) > 1)//split on white space yields multiple values in array
                            {
                                ThisLike = CellDataType.DescriptionMultipleWord;
                                return ThisLike;
                            }
                            else
                            {
                                ThisLike = CellDataType.DescriptionSingleWord;
                                return ThisLike;
                            }
                        }
                    }//if numeric==false

                    //one last check if this cell contains prefix
                    //check if mostly digits
                    if (ThisLike != CellDataType.CountryCode && ThisLike != CellDataType.Prefix && ThisLike != CellDataType.MultiplePrefix)
                    {
                        int DigitCount = 0;
                        int NonDigitCount = 0;
                        foreach (char ch in Value.ToCharArray())
                        {
                            if (WholeNumbers.Contains(ch))
                            {
                                DigitCount++;
                            }
                            else NonDigitCount++;
                        }
                        if (DigitCount > NonDigitCount) MostlyDigits = true;

                        if (MostlyDigits == true)
                        {
                            ThisLike = CellDataType.MultiplePrefix;
                        }
                    }
                    return ThisLike;
                }
                catch (Exception e1)
                {
                    return CellDataType.Undetermined;
                }
            }

            string FindTableMetaData(ref object[,] valueArray, ref RateTableMetaData TableData, int FirstRow, VendorFormat VFormat, string[] DateFormats, ref string DateSeparator)
            {
                //try
                {

                    TableData.FirstRow = FirstRow;
                    if (VFormat == VendorFormat.GenericExcel || VFormat == VendorFormat.GenericText)
                    {
                        TableData.IndexPrefix1 = 1;
                        TableData.IndexPrefix2 = -1;//can be present e.g. code update sheet of Tata
                        TableData.IndexDescription = 2;
                        TableData.IndexRate = 3;
                        TableData.IndexPulse = 4;
                        TableData.IndexMinDuration = 5;
                        TableData.IndexCountryCode = 6;
                        TableData.IndexCountryName = -1;
                        TableData.IndexStartDate = 7;
                        TableData.IndexEndDate = 8;
                        TableData.IndexSurchargeTime = 9;
                        TableData.IndexSurchargeAmount = 10;
                        TableData.IndexServiceType = 11;
                        TableData.IndexSubServiceType = 12;
                        return "";
                    }

                    //scan by each column to find charctristic of a column
                    int Dim2 = valueArray.GetLength(1);
                    List<int> PossDateColsIndex = new List<int>();
                    List<int> PossDateMulPrefixIndex = new List<int>();

                    for (int j = 1; j <= Dim2; j++)  //1 based array, for each column
                    {
                        RowColumnAttributes RAttrib = new RowColumnAttributes();
                        FindColumnAttributes(ref valueArray, FirstRow, j, ref RAttrib, DateFormats, ref DateSeparator);

                        CellDataType? ThisColumnLike = RAttrib.GetCellDataTypeByMaxAttributeCount();
                        ColumnMetaData ThisColData = new ColumnMetaData();
                        ThisColData.ColumnType = (CellDataType)ThisColumnLike;
                        ThisColData.ColAttribs = RAttrib;
                        TableData.dicColData.Add(j, ThisColData);

                        switch (ThisColumnLike)
                        {
                            case CellDataType.MultiplePrefix:
                            case CellDataType.Prefix:
                                if (PossDateMulPrefixIndex.Count == 0)
                                {
                                    PossDateMulPrefixIndex.Add(j);
                                    TableData.IndexPrefix1 = j;
                                }
                                else
                                {
                                    if (PossDateMulPrefixIndex.Count == 1 && j > PossDateMulPrefixIndex[0])
                                    {
                                        PossDateMulPrefixIndex.Add(j);
                                        TableData.IndexPrefix2 = j;
                                    }
                                }
                                break;

                            case CellDataType.Datetime:

                                if (PossDateColsIndex.Count == 0)
                                {
                                    PossDateColsIndex.Add(j);
                                    TableData.IndexStartDate = j;
                                }
                                else
                                {
                                    if (PossDateColsIndex.Count == 1 && j > PossDateColsIndex[0])
                                    {
                                        PossDateColsIndex.Add(j);
                                        TableData.IndexEndDate = j;
                                    }
                                }
                                break;
                            case CellDataType.Rate:
                                TableData.IndexRate = j;
                                break;
                            case CellDataType.CountryName:
                                TableData.IndexCountryName = j;
                                break;
                            case CellDataType.CountryCode:
                                TableData.IndexCountryCode = j;
                                break;
                        }

                    }//for each column

                    //find out description column
                    int MaxColIndexChar = -1;
                    int MaxUniqueWordCount = 0;//prefix description column will have more unique word rather than increase/decrease/CLI route etc.
                    for (int c = 1; c <= TableData.dicColData.Count; c++)
                    {
                        ColumnMetaData cData = null;
                        TableData.dicColData.TryGetValue(c, out cData);
                        int Max = 0;
                        if (cData.ColumnType != CellDataType.MultiplePrefix && cData.ColumnType != CellDataType.Prefix
                            && cData.ColumnType != CellDataType.Datetime && cData.ColumnType != CellDataType.Rate
                            && cData.ColumnType != CellDataType.CountryName)
                        {
                            if (cData.ColumnType == CellDataType.DescriptionMultipleWord && cData.ColAttribs.CharacterCount > Max
                                && cData.ColAttribs.dicUnqWord.Count > MaxUniqueWordCount)
                            {
                                Max = cData.ColAttribs.CharacterCount;
                                MaxUniqueWordCount = cData.ColAttribs.dicUnqWord.Count;
                                MaxColIndexChar = c;
                            }
                        }
                    }

                    TableData.IndexDescription = MaxColIndexChar;
                    if (TableData.IndexPrefix1 == -1 & TableData.IndexCountryCode != -1)//sometime countrycode can be prefix...
                    {
                        TableData.IndexPrefix1 = TableData.IndexCountryCode;
                    }
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
