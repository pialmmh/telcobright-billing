using TelcobrightMediation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using TelcobrightFileOperations;
using MediationModel;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Mediation.Cdr;
using System.Linq;
using LibraryExtensions;

namespace Decoders
{

    [Export("Decoder", typeof(IFileDecoder))]
    public class DialogicControlSwitchDecoder : IFileDecoder
    {
        public override string ToString() => this.RuleName;
        public virtual string RuleName => GetType().Name;
        public int Id => 2;
        public string HelpText => "Decodes Dialogic (Veraz) Control Switch CDR.";
        public CompressionType CompressionType { get; set; }
        public string PartialTablePrefix { get; }
        public string PartialTableStorageEngine { get; }
        public string partialTablePartitionColName { get; }
        protected CdrCollectorInputData Input { get; set; }
        protected virtual List<string[]> GetTxtCdrs()
        {
            return FileUtil.ParseTextFileToListOfStrArray(this.Input.FullPath, ';', 0);
        }
        public List<string[]> DecodeFile(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs)
        {
            inconsistentCdrs = new List<cdrinconsistent>();
            List<string[]> decodedRows = new List<string[]>();
            this.Input = input;
            List<cdrfieldmappingbyswitchtype> lstFieldMapping = null;
            input.MefDecodersData.DicFieldMapping.TryGetValue(this.Id, out lstFieldMapping);
            List<string[]> tempTable = GetTxtCdrs();
            
            int rowCnt = 0;
            string strThisField = "";   
            string[] normalizedRow = null;
            int currentFieldNo = 0;
            foreach (string[] csvRow in tempTable) //for each row
            {
                if (csvRow[6].Trim() == "I")
                    continue;
                try
                {
                    normalizedRow = null;
                    normalizedRow = new string[input.MefDecodersData.Totalfieldtelcobright];
                    int fldCount = 0;
                    foreach (cdrfieldmappingbyswitchtype thisField in lstFieldMapping) //for each field
                    {
                        currentFieldNo = thisField.FieldNumber;
                        strThisField = "";
                        if (thisField.FieldPositionInCDRRow == null) //field is not present in cdr
                        {
                            normalizedRow[fldCount] = "";
                        }
                        else //field is present in CDR and get the string representation by Dialogic CDR spec
                        {
                            switch (thisField.FieldNumber)
                            {
                                case 2: //sequence number
                                    strThisField = csvRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)]
                                        .ToString();
                                    break;
                                case 3: //SwitchCallId
                                    strThisField = csvRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)]
                                        .ToString();
                                    break;
                                case 5: //Incoming Route
                                    strThisField = csvRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)]
                                        .ToString();
                                    break;
                                case 6: //Originating IP
                                    strThisField = csvRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)]
                                        .ToString();
                                    break;
                                case 8: //Originating CIC
                                    strThisField = csvRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)]
                                        .ToString();
                                    break;
                                case 9: //Originating Called Number
                                    strThisField = csvRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)]
                                        .ToString();
                                    break;
                                case 10: //Terminating Called Number
                                    strThisField = csvRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)]
                                        .ToString();
                                    break;
                                case 11: //Originating Calling Number
                                    strThisField = csvRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)]
                                        .ToString();
                                    break;
                                case 12: //Terminating Calling Number
                                    strThisField = csvRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)]
                                        .ToString();
                                    break;
                                case 14: //DurationSec
                                    strThisField = csvRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)]
                                        .ToString();
                                    break;
                                case 15: //EndTime
                                    string tempStr = csvRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)]
                                        .ToString();
                                    string yy = "";
                                    string MM = "";
                                    string dd = "";
                                    string hh = "";
                                    string mm = "";
                                    string ss = "";

                                    if (tempStr.Length > 4) //at least 4
                                    {
                                        //debugging found intremediate CDRs...for these there would be no end time.
                                        yy = tempStr.Substring(0, 4);
                                        MM = tempStr.Substring(5, 2);
                                        dd = tempStr.Substring(8, 2);
                                        hh = tempStr.Substring(11, 2);
                                        mm = tempStr.Substring(14, 2);
                                        ss = tempStr.Substring(17, 2);
                                        strThisField = yy + "-" + MM + "-" + dd + " " + hh + ":" + mm + ":" + ss;
                                    }
                                    else
                                    {
                                        strThisField = "";
                                    }
                                    break;
                                case 16: //ConnectTime, connect time may not be present for failed call
                                    tempStr = csvRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString();
                                    if (tempStr.Length > 4) //at least 4
                                    {
                                        yy = tempStr.Substring(0, 4);
                                        MM = tempStr.Substring(5, 2);
                                        dd = tempStr.Substring(8, 2);
                                        hh = tempStr.Substring(11, 2);
                                        mm = tempStr.Substring(14, 2);
                                        ss = tempStr.Substring(17, 2);
                                        strThisField = yy + "-" + MM + "-" + dd + " " + hh + ":" + mm + ":" + ss;
                                    }
                                    break;
                                case 17: //AnswerTime, may not be present for failed calls

                                    tempStr = csvRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString();
                                    if (tempStr.Length > 4) //at least 4
                                    {
                                        yy = tempStr.Substring(0, 4);
                                        MM = tempStr.Substring(5, 2);
                                        dd = tempStr.Substring(8, 2);
                                        hh = tempStr.Substring(11, 2);
                                        mm = tempStr.Substring(14, 2);
                                        ss = tempStr.Substring(17, 2);
                                        strThisField = yy + "-" + MM + "-" + dd + " " + hh + ":" + mm + ":" + ss;
                                    }
                                    break;
                                case 18: //charging status
                                    string str = csvRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)]
                                        .ToString();
                                    switch (str)
                                    {
                                        case "S":
                                            strThisField = "1"; //answered
                                            normalizedRow[Fn.Partialflag] = "0";
                                            break;
                                        case "U": //unsuccessful
                                            strThisField = "0"; //unanswered
                                            normalizedRow[Fn.Partialflag] = "0";
                                            break;
                                        case "I": //ignore while generating rating info for partners
                                            strThisField = "2";
                                            normalizedRow[Fn.Partialflag] = "1";
                                            break;
                                        default:
                                            strThisField = "3"; //undefined
                                            break;
                                    } //switchch str
                                    break;
                                case 22: //release direction
                                    int ingressReleaseDirection =
                                        int.Parse(csvRow[137].ToString()); //field 138=Ingress Release Direction
                                    int egresseReleaseDirection =
                                        int.Parse(csvRow[138].ToString()); //field 139=Engress Release Direction
                                    //3=network initiated, 2=remote half call
                                    //in our billing system we follow:
                                    //0: caller party release; 
                                    //1: called party release;
                                    //2: inter release 
                                    //3: peer caller release  
                                    //4: peer called release
                                    //5: Maintenance release
                                    //6: Both party release at same time
                                    //7: inter release due to a problem in caller side
                                    //8: inter release due to a problem in called side
                                    if ((ingressReleaseDirection == 3) && (egresseReleaseDirection == 2))
                                    {
                                        //calling party release
                                        strThisField = "0";
                                    }
                                    else if ((ingressReleaseDirection == 2) && (egresseReleaseDirection == 3))
                                    {
                                        //called party release
                                        strThisField = "1";
                                    }
                                    else if ((ingressReleaseDirection == 3) && (egresseReleaseDirection == 3))
                                    {
                                        //both party release at same time
                                        strThisField = "6";
                                    }
                                    else if ((ingressReleaseDirection == 0) && (egresseReleaseDirection == 2))
                                    {
                                        //Internal + Remote Half Call
                                        //A problem is encountered at the ingress Call Control Element which caused 
                                        //the call to be released
                                        strThisField = "7"; //internal release in dialogic system
                                    }
                                    else if ((ingressReleaseDirection == 2) && (egresseReleaseDirection == 0))
                                    {
                                        //Remote Half Call+internal
                                        //A problem is encountered at the engress Call Control Element which caused 
                                        //the call to be released
                                        strThisField = "8"; //internal release in dialogic system
                                    }
                                    else if ((ingressReleaseDirection == 2) && (egresseReleaseDirection == 2))
                                    {
                                        //Either the ingress media gateway or the egress media gateway encountered a problem.
                                        strThisField = "9"; //internal release in dialogic system
                                    }
                                    break;
                                case 24: //release cause egress
                                    strThisField = csvRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)]
                                        .ToString();
                                    break;
                                case 25: //outgoing route
                                    strThisField = csvRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)]
                                        .ToString();
                                    break;
                                case 26: //terminating IP
                                    strThisField = csvRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)]
                                        .ToString();
                                    break;
                                case 28: //terminating CIC
                                    strThisField = csvRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)]
                                        .ToString();
                                    break;
                                case 29: //StartTime
                                    tempStr = csvRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString();
                                    tempStr = csvRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString();
                                    if (tempStr.Length >= 18)
                                    {
                                        yy = tempStr.Substring(0, 4);
                                        MM = tempStr.Substring(5, 2);
                                        dd = tempStr.Substring(8, 2);
                                        hh = tempStr.Substring(11, 2);
                                        mm = tempStr.Substring(14, 2);
                                        ss = tempStr.Substring(17, 2);
                                        strThisField = yy + "-" + MM + "-" + dd + " " + hh + ":" + mm + ":" + ss;
                                    }
                                    else strThisField = "";

                                    break;
                                case 56: //release cause ingress
                                    strThisField = csvRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)]
                                        .ToString();
                                    break;
                                case 59: //CalledPartyNOA
                                    int tempInt = 0;
                                    if (csvRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString().Length >
                                        0)
                                        tempInt = int.Parse(
                                            csvRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)]);
                                    //noa normalizing matching huawei
                                    //0:subscriber number
                                    //1:spared
                                    //2:national valid number
                                    //3:international number  
                                    //255: Unknown
                                    switch (tempInt)
                                    {
                                        case 0: //unknown
                                            strThisField = "255";
                                            break;
                                        case 1: //subscriber
                                            strThisField = "0";
                                            break;
                                        case 2: //not available or not provided
                                            strThisField = "255";
                                            break;
                                        case 3: //national
                                            strThisField = "2";
                                            break;
                                        case 4: //international
                                            strThisField = "3";
                                            break;
                                    }

                                    break;
                                case 60: //CallingPartyNOA
                                    tempInt = 0;
                                    if (csvRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)].ToString().Length >
                                        0)
                                        tempInt = int.Parse(
                                            csvRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)]);
                                    //noa normalizing matching huawei
                                    //0:subscriber number
                                    //1:spared
                                    //2:national valid number
                                    //3:international number  
                                    //255: Unknown
                                    switch (tempInt)
                                    {
                                        case 0: //unknown
                                            strThisField = "255";
                                            break;
                                        case 1: //subscriber
                                            strThisField = "0";
                                            break;
                                        case 2: //not available or not provided
                                            strThisField = "255";
                                            break;
                                        case 3: //national
                                            strThisField = "2";
                                            break;
                                        case 4: //international
                                            strThisField = "3";
                                            break;
                                    }
                                    break;
                                case 98: //unique billid
                                    strThisField = csvRow[Convert.ToInt32(thisField.FieldPositionInCDRRow - 1)]
                                        .ToString();
                                    break;
                            } //switch
                        } //if field is present in cdr
                        normalizedRow[fldCount] = strThisField;
                        fldCount++;
                    } // for each field
                    rowCnt++;
                    //exclude rows having charging status other than successful or failed...(e.g. intermediate in dialogic cdr)
                    if ((normalizedRow[18] == "0") || (normalizedRow[18] == "1"))
                    {
                        //add valid flag for this type of switch, valid flag comes from cdr for zte
                        normalizedRow[Fn.Validflag] = "1";
                        normalizedRow[Fn.Partialflag] = "0";
                        normalizedRow[Fn.FinalRecord] = "1";
                        decodedRows.Add(normalizedRow);
                    }
                }
                catch (Exception e1)
                {
                    //if error found for one row, add this to inconsistent
                    Console.WriteLine(e1);
                    inconsistentCdrs.Add(CdrConversionUtil.ConvertTxtRowToCdrinconsistent(csvRow));
                    ErrorWriter wr = new ErrorWriter(e1, "DecodeCdr", null,
                        this.RuleName + " encounterd error during decoding and an Inconsistent cdr has been generated."
                        , input.Tbc.Telcobrightpartner.CustomerName);
                }
            }//for each row
            return decodedRows;
        }

        public string getTupleExpression(CdrCollectorInputData decoderInputData, string[] row)
        {
            throw new NotImplementedException();
        }

        public string getSqlWhereClauseForHourWiseSafeCollection(CdrCollectorInputData decoderInputData, DateTime day)
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
