using TelcobrightMediation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Text;
using TelcobrightFileOperations;
using MediationModel;
using TelcobrightMediation.Mediation.Cdr;


namespace Decoders
{

    [Export("Decoder", typeof(IFileDecoder))]
    public class SansaySostSwitchDecoder : IFileDecoder
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public int Id => 4;
        public string HelpText => "Decodes Sansay CDR.";
        protected CdrCollectorInputData Input { get; set; }//required for testing with derived mock collector class
        protected virtual List<string[]> GetTxtCdrs()
        {
            return FileUtil.ParseTextFileToListOfStrArray(this.Input.FullPath, ';', 0);
        }

        public List<string[]> DecodeFile(CdrCollectorInputData input,out List<cdrinconsistent> inconsistentCdrs)
        {
            inconsistentCdrs=new List<cdrinconsistent>();
            List<string[]> decodedRows = new List<string[]>();

            this.Input = input;
            List<cdrfieldmappingbyswitchtype> lstFieldMapping = null;
            input.MefDecodersData.DicFieldMapping.TryGetValue(this.Id, out lstFieldMapping);
            List<string[]> tempTable = GetTxtCdrs();
            if (tempTable == null)
            {
                return new List<string[]>();
            }
            foreach (string[] thisRow in tempTable) //for each row
            {
                if (thisRow[10].Trim()=="998") continue;
                try
                {//sansay was set to try next field in case of error with one field
                    //the code has been changed to make exception handling per row basis like other decoding formats
                    string[] thisNormalizedRow = null;
                    thisNormalizedRow = new string[input.MefDecodersData.Totalfieldtelcobright];
                    int fldCount = 0;
                    foreach (cdrfieldmappingbyswitchtype thisField in lstFieldMapping) //for each field
                    {
                        //sansay often gives inconsistent row e.g. once found only 38 fields in a row
                        //use try catch if error occurs with any field..
                        //try
                        {
                            var strThisField = "";
                            if (thisField.FieldPositionInCDRRow == null)//field is not present in cdr
                            {
                                thisNormalizedRow[fldCount] = "";
                                //continue;//dont use continue
                            }
                            else //field is present in CDR and get the string representation by Dialogic CDR spec
                            {
                                switch (thisField.FieldNumber)
                                {
                                    case 2://sequence number
                                        strThisField = thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString();
                                        break;
                                    case 3://SwitchCallId
                                        strThisField = thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString();
                                        break;
                                    case 5://Incoming Route
                                        strThisField = Convert.ToInt32(thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString()).ToString();
                                        break;
                                    case 6://Originating IP
                                        strThisField = thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString();
                                        break;
                                    case 8://Originating CIC
                                        strThisField = thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString();
                                        break;
                                    case 9://Originating Called Number
                                        strThisField = thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString();
                                        break;
                                    case 10://Terminating Called Number
                                        strThisField = thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString();
                                        break;
                                    case 11://Originating Calling Number
                                        strThisField = thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString();
                                        break;
                                    case 12://Terminating Calling Number
                                        strThisField = thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString();
                                        break;
                                    case 14://DurationSec
                                        strThisField = (Convert.ToDouble(thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString()) / 1000).ToString();

                                        break;
                                    case 15://EndTime
                                    case 17://answertime
                                        string tempStr = thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString().Split('.')[0];//remove ms part, not required, duration will have it.
                                        if (tempStr.Length == 24) //e.g. Mon Jul 27 18:40:47 2015
                                        {
                                            string[] dateArr = tempStr.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                                            string thisDay = dateArr[2].PadLeft(2, '0');
                                            StringBuilder sbDate = new StringBuilder().Append(dateArr[4]).Append("-")
                                                .Append(dateArr[1]).Append("-")
                                                .Append(thisDay).Append(" ")
                                                .Append(dateArr[3]);
                                            DateTime tempDate = new DateTime();
                                            DateTime.TryParseExact(sbDate.ToString(), "yyyy-MMM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate);
                                            strThisField = tempDate.ToString("yyyy-MM-dd HH:mm:ss");

                                        }
                                        else
                                        {
                                            strThisField = tempStr;//keep as it is
                                        }


                                        break;
                                    case 16://ConnectTime, connect time may not be present for failed call
                                            //has been set later by PDD
                                        break;

                                    case 18://charging status
                                            ////has been set later by duration
                                        break;
                                    case 22://release direction

                                        break;
                                    case 24://release cause egress
                                        strThisField = thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString();
                                        break;
                                    case 25://outgoing route
                                        strThisField = Convert.ToInt32(thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString()).ToString();
                                        break;
                                    case 26://terminating IP
                                        strThisField = thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString();
                                        break;
                                    case 28://terminating CIC
                                        strThisField = thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString();
                                        break;
                                    case 29://StartTime
                                        tempStr = thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString().Split('.')[0];//remove ms part, not required, duration will have it.
                                        if (tempStr.Length == 24) //e.g. Mon Jul 27 18:40:47 2015
                                        {
                                            string[] dateArr = tempStr.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                                            string thisDay = dateArr[2].PadLeft(2, '0');
                                            StringBuilder sbDate = new StringBuilder().Append(dateArr[4]).Append("-")
                                                .Append(dateArr[1]).Append("-")
                                                .Append(thisDay).Append(" ")
                                                .Append(dateArr[3]);
                                            DateTime tempDate = new DateTime();
                                            DateTime.TryParseExact(sbDate.ToString(), "yyyy-MMM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate);
                                            strThisField = tempDate.ToString("yyyy-MM-dd HH:mm:ss");

                                        }
                                        else
                                        {
                                            strThisField = tempStr;//keep as it is
                                        }
                                        break;

                                    case 19://PDD
                                        string strPdd = thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString();
                                        double dblPdd = -1;
                                        double tempDouble = 0;
                                        if (double.TryParse(strPdd, out tempDouble) == true)//isnumeric
                                        {
                                            strThisField = tempDouble.ToString();
                                        }
                                        else//pdd not present
                                        {
                                            strThisField = "";
                                        }

                                        break;
                                    //strThisField = ThisRow[Convert.ToInt32(ThisField.FieldPositionInCDRRow - 1)].ToString();
                                    case 54://valid flag based on record type. R – Normal CDR record, A Audit
                                        strThisField = thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString();
                                        break;
                                    case 56://release cause ingress
                                        strThisField = thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString();
                                        break;
                                    case 59://CalledPartyNOA

                                        break;
                                    case 60://CallingPartyNOA

                                        break;
                                    case 65://cdr-type interim, end1 etc.
                                        strThisField = thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString();
                                        break;
                                    case 98://unique bill id
                                        strThisField = thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString();
                                        break;
                                }//switch
                            }//if field is present in cdr
                            thisNormalizedRow[fldCount] = (strThisField == "NA" ? "" : strThisField.Replace("'", ""));
                            fldCount++;

                        }
                        
                    } // for each field
                      //set charging status
                    double tmpDuration = 0;
                    double.TryParse(thisNormalizedRow[Fn.DurationSec], out tmpDuration);
                    if (tmpDuration > 0)
                    {
                        thisNormalizedRow[Fn.ChargingStatus] = "1";
                        //set connect time based on PDD
                        double pddSec = 0;
                        double tempDbl = -1;
                        if (double.TryParse(thisNormalizedRow[Fn.Pdd], out tempDbl))
                        {
                            pddSec = tempDbl;//pdd found in sec here.
                            if (pddSec > 0)
                            {
                                DateTime connectTime = new DateTime();
                                DateTime startTime = new DateTime();
                                if (DateTime.TryParseExact(thisNormalizedRow[Fn.StartTime], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime))
                                {
                                    connectTime = startTime.AddSeconds(pddSec);
                                    thisNormalizedRow[Fn.ConnectTime] = connectTime.ToString("yyyy-MM-dd HH:mm:ss");
                                }
                            }
                        }
                    }
                    else
                    {
                        thisNormalizedRow[Fn.ChargingStatus] = "0";
                    }
                    
                    //exclude rows having charging status other than successful or failed...(e.g. intermediate in dialogic cdr)
                    if ((thisNormalizedRow[18] == "0") || (thisNormalizedRow[18] == "1"))
                    {
                        if (thisNormalizedRow[54] == "R")
                        {
                            thisNormalizedRow[54] = "1";//add valid flag for this type of switch, valid flag comes from cdr for zte
                            thisNormalizedRow[55] = "0";//for now mark as non-partial, single cdr
                            thisNormalizedRow[Fn.FinalRecord] = "1";
                            decodedRows.Add(thisNormalizedRow);
                        }
                    }
                }
                catch (Exception e1)
                {//if error found for one row, add this to inconsistent

                    Console.WriteLine(e1);
                    inconsistentCdrs.Add(CdrConversionUtil.ConvertTxtRowToCdrinconsistent(thisRow));
                    ErrorWriter wr = new ErrorWriter(e1, "DecodeCdr", null,
                        this.RuleName + " encounterd error during decoding and an Inconsistent cdr has been generated."
                        , input.Tbc.DatabaseSetting.GetOperatorName);
                    continue;//with next switch
                }
            }//for each row
            return decodedRows;
        }
    }
}
