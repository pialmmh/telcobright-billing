using TelcobrightMediation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using LibraryExtensions;
using TelcobrightFileOperations;
using MediationModel;
using TelcobrightMediation.Mediation.Cdr;


namespace Decoders
{

    [Export("Decoder", typeof(IFileDecoder))]
    public class GenbandS3Decoder : IFileDecoder
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public int Id => 1;
        public string HelpText => "Decodes Genband S3 CDR.";
        public CompressionType CompressionType { get; set; }
        public string PartialTablePrefix { get; }
        public string PartialTableStorageEngine { get; }
        public string partialTablePartitionColName { get; }

        public List<string[]> DecodeFile(CdrCollectorInputData input,out List<cdrinconsistent> inconsistentCdrs)
        {
            List<string[]> decodedRows = new List<string[]>();
            inconsistentCdrs = new List<cdrinconsistent>();

            List<cdrfieldmappingbyswitchtype> lstFieldMapping = null;
            input.MefDecodersData.DicFieldMapping.TryGetValue(this.Id, out lstFieldMapping);
            List<string[]> tempTable = FileUtil.ParseTextFileToListOfStrArray(input.FullPath, ';', 0);
            
            if (tempTable == null)
            {
                return decodedRows;
            }
            int rowCnt = 0;
            string strThisField = "";
            string[] thisNormalizedRow = null;
            int currentFieldNo = 0;
            foreach (string[] thisRow in tempTable) //for each row
            {
                try
                {
                    thisNormalizedRow = null;
                    thisNormalizedRow = new string[input.MefDecodersData.Totalfieldtelcobright];
                    int fldCount = 0;
                    foreach (cdrfieldmappingbyswitchtype thisField in lstFieldMapping) //for each field
                    {
                        currentFieldNo = thisField.FieldNumber;
                        strThisField = "";
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
                                    strThisField = thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString();
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
                                    strThisField = thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString();

                                    break;
                                case 15://EndTime
                                    string tempStr = thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString().Split('.')[0];//remove ms part, not required, duration will have it.

                                    if (tempStr.Length == 19) //at least 19, genband s3 is yyyy-MM-dd HH:mm:ss format
                                    {
                                        strThisField = tempStr;

                                    }
                                    else
                                    {
                                        strThisField = "";
                                    }


                                    break;
                                case 16://ConnectTime, connect time may not be present for failed call
                                        //has been set later by PDD
                                    break;
                                case 17://AnswerTime, may not be present for failed calls
                                        //has been set later by duration and charging status
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
                                    strThisField = thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString();
                                    break;
                                case 26://terminating IP
                                    strThisField = thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString();
                                    break;
                                case 28://terminating CIC
                                    strThisField = thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString();
                                    break;
                                case 29://StartTime
                                    tempStr = thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString().Split('.')[0];//remove ms part, not required, duration will have it.

                                    if (tempStr.Length == 19) //at least 19, genband s3 is yyyy-MM-dd HH:mm:ss format
                                    {
                                        strThisField = tempStr;

                                    }
                                    else
                                    {
                                        strThisField = "";
                                    }

                                    break;

                                case 19://PDD
                                    string strPdd = thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString();
                                    double dblPdd = -1;
                                    double tempDouble = 0;
                                    if (double.TryParse(strPdd, out tempDouble) == true)//isnumeric
                                    {
                                        dblPdd = (double)tempDouble / 1000;//convert from micro to milli, s3 doc says micro, but actually found milli
                                        strThisField = dblPdd.ToString();
                                    }
                                    else//pdd not present
                                    {
                                        strThisField = "";
                                    }

                                    break;
                                //strThisField = ThisRow[Convert.ToInt32(ThisField.FieldPositionInCDRRow - 1)].ToString();
                                case 56://release cause ingress
                                    strThisField = thisRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString();
                                    thisNormalizedRow[23] = strThisField;//keep the sip codes to release cause system field
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
                        thisNormalizedRow[fldCount] = strThisField;
                        fldCount++;
                    } // for each field

                    //NER is calculated as: (Answers + User Busy + Ring No Answer +Terminal Rejects) / Total call attempts (seizures)
                    //For NER reports while calculating network efficiency for terminal reject calls, 
                    //RSM considers CDRS with SIP code as 403 and 404, and isdn codes 21, 26.So it would be basically
                    //(Answers + User Busy + Ring No Answer + sip code 403 + sip code 404 + isdn code 21 + isdn code 26) / total call attempts.
                    //convert ISDN cause codes in field 30 to ingresscause code
                    //65021=isdn 21, 65026=isdn 26 

                    //even later...
                    //GENBAND is not sharing the actual formula of NER calculation on RSM as it is their design related issue.But the suggestions so far we have received from Genband we came to a conclusion that the below formula will give us the best possible accurate NER value.
                    //NER ={ N(field 13) + A(field 13) + B(field 13) + sip code 403 + sip code 404 + isdn code 21 + isdn code 26} / total call attempts %
                    thisNormalizedRow[24] = thisRow[29];//keep the isdn codes to release cause egress field
                    int tempIsdnCode = 0;
                    if (Int32.TryParse(thisRow[29], out tempIsdnCode) == true)//isdn cause code part
                    {
                        tempIsdnCode = (65000 + tempIsdnCode);
                        List<int> isdnCodesForNer = new List<int>() { 65021, 65026 };
                        if (isdnCodesForNer.Contains(tempIsdnCode))
                        {
                            //sip codes are already in field 23, releasecausesystem
                            thisNormalizedRow[23] = tempIsdnCode.ToString();//overwrite sip codes in releasecausesystem with isdn for ner
                        }
                    }
                    //field12 response code part for NER
                    //let N=65016, A=65017, B=65018
                    switch (thisRow[12])
                    {
                        case "N":
                            thisNormalizedRow[23] = "65016";
                            break;
                        case "A":
                            thisNormalizedRow[23] = "65017";
                            break;
                        case "B":
                            thisNormalizedRow[23] = "65018";
                            break;
                    }

                    //set charging status
                    double tmpDuration = 0;
                    double.TryParse(thisNormalizedRow[Fn.DurationSec], out tmpDuration);
                    if (thisRow[12] == "N" || tmpDuration > 0)
                    {
                        thisNormalizedRow[Fn.ChargingStatus] = "1";
                        //set answer time based on duration
                        DateTime endTime = new DateTime();
                        DateTime answerTime = new DateTime();
                        if (DateTime.TryParseExact(thisNormalizedRow[Fn.Endtime], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out endTime))
                        {
                            answerTime = endTime.AddSeconds(tmpDuration * (-1));//substract
                            thisNormalizedRow[Fn.AnswerTime] = answerTime.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                    }
                    else
                    {
                        thisNormalizedRow[Fn.ChargingStatus] = "0";
                    }

                    //set connect time based on PDD
                    double pddSec = 0;
                    double tempDbl = -1;
                    if (double.TryParse(thisNormalizedRow[Fn.Pdd], out tempDbl))
                    {
                        pddSec = tempDbl;//pdd found in sec here.
                        DateTime connectTime = new DateTime();
                        DateTime startTime = new DateTime();
                        if (DateTime.TryParseExact(thisNormalizedRow[Fn.StartTime], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime))
                        {
                            connectTime = startTime.AddSeconds(pddSec);
                            thisNormalizedRow[Fn.ConnectTime] = connectTime.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                    }

                    rowCnt++;
                    //exclude rows having charging status other than successful or failed...(e.g. intermediate in dialogic cdr)
                    if ((thisNormalizedRow[18] == "0") || (thisNormalizedRow[18] == "1"))
                    {
                        if (thisNormalizedRow[65] == "end1")
                        {
                            //add valid flag for this type of switch, valid flag comes from cdr for zte
                            thisNormalizedRow[54] = "1";
                            thisNormalizedRow[55] = "0";//for now mark as non-partial, single cdr
                                                        //remove the text "end1", casue that will throw error for this field in cdr
                            thisNormalizedRow[65] = "0";//a numeric value is ok as per cdrfieldlist
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
                        , input.Tbc.Telcobrightpartner.CustomerName);
                    continue;//with next switch
                }
            }//for each row
            return decodedRows;
        }

        public string getTupleExpression(CdrCollectorInputData decoderInputData, string[] row)
        {
            throw new NotImplementedException();
        }

        public string getSqlWhereClauseForHourWiseSafeCollection(CdrCollectorInputData decoderInputData,DateTime hourOfDay, int minusHoursForSafeCollection, int plusHoursForSafeCollection)
        {
            throw new NotImplementedException();
        }

        public string getCreateTableSqlForUniqueEvent(CdrCollectorInputData decoderInputData)
        {
            throw new NotImplementedException();
        }

        public string getDuplicateCollectionSql(CdrCollectorInputData decoderInputData, DateTime hourOfTheDay, List<string> tuples)
        {
            throw new NotImplementedException();
        }

        public string getPartialCollectionSql(CdrCollectorInputData decoderInputData, DateTime hourOfTheDay, List<string> tuples)
        {
            throw new NotImplementedException();
        }
    }
}
