using TelcobrightMediation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using TelcobrightFileOperations;
using MediationModel;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Mediation.Cdr;
using System.Linq;
using System.Globalization;
using LibraryExtensions;

namespace Decoders
{

    [Export("Decoder", typeof(IFileDecoder))]
    public class AsteriskDecoder : IFileDecoder
    {
        public override string ToString() => this.RuleName;
        public virtual string RuleName => GetType().Name;
        public int Id => 20;
        public string HelpText => "Decodes Asterisk CDR.";
        public CompressionType CompressionType { get; set; }
        protected CdrCollectorInputData Input { get; set; }
        protected virtual List<string[]> GetTxtCdrs()
        {
            return FileUtil.ParseCsvWithEnclosedAndUnenclosedFields(this.Input.FullPath, ',', 0,"\"",";");
        }


        public List<string[]> DecodeFile(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs)
        {
            inconsistentCdrs = new List<cdrinconsistent>();
            List<string[]> decodedRows = new List<string[]>();
            this.Input = input;
            List<cdrfieldmappingbyswitchtype> fieldMappings = null;
            input.MefDecodersData.DicFieldMapping.TryGetValue(this.Id, out fieldMappings);
            int maxFieldPositionInInputCsv = fieldMappings.Max(
                cdrFldMapping => Convert.ToInt32(cdrFldMapping.FieldPositionInCDRRow));
            List<string[]> tempTable = GetTxtCdrs();
            string[] replaceChars = new string[] { "'", "<", ">" };
            foreach (var rowCsv in tempTable) //for each row
            {
                //if (rowCsv.Length < 15) {
                //    Console.WriteLine("hello");
                //    continue;
                //}
                try
                {
                    string[] normalizedRow = new string[input.MefDecodersData.Totalfieldtelcobright];
                    foreach (cdrfieldmappingbyswitchtype thisField in fieldMappings) //for each field
                    {
                        string strThisField = "";
                        int fieldPosInCsv = thisField.FieldPositionInCDRRow != null &&
                            thisField.FieldPositionInCDRRow > -1 
                            ? Convert.ToInt32(thisField.FieldPositionInCDRRow) : -1;
                        string val = fieldPosInCsv>-1?rowCsv[fieldPosInCsv]:"";
                        foreach (string c in replaceChars) {
                            val = val.Replace(c, "");
                        }
                        //temp code
                        
                        if (val.Contains("<")|| val.Contains("'")|| val.Contains(">")) {
                            Console.WriteLine("found");
                        }
                        if (val.Contains("0034711303061432"))
                        {
                            Console.WriteLine("found");
                        }
                        switch (thisField.FieldNumber) {
                            case Fn.Sequencenumber:
                                strThisField= val.Replace(".", "");
                                break;
                            case Fn.IncomingRoute:
                            case Fn.OutgoingRoute:
                                if (string.IsNullOrEmpty(val))
                                {
                                    strThisField= "";
                                }
                                strThisField= val.Split('/')[1].Split('-')[0];
                                break;
                            case Fn.ChargingStatus:
                                strThisField = (val == "ANSWERED" ? "1" : "0");
                                break;
                            case Fn.TerminatingCalledNumber:
                                if (val.Contains("/"))
                                {
                                    strThisField = val.Split('/')[1].Split('@')[0];
                                }
                                else {
                                    strThisField = val;
                                }
                                break;
                            default:
                                strThisField = val;
                                break;
                        }
                        string replaceChar = "'";
                        if (strThisField.Contains(replaceChar)) {
                            strThisField.Replace(replaceChar, "");
                        }
                        normalizedRow[thisField.FieldNumber] = strThisField;
                    } // for each field
                    
                    //add valid flag for this type of switch, valid flag comes from cdr for zte
                    normalizedRow[Fn.Validflag] = "1";
                    normalizedRow[Fn.Partialflag] = "0";
                    normalizedRow[Fn.FinalRecord] = "1";
                    normalizedRow[Fn.ConnectTime] = normalizedRow[Fn.StartTime];
                    //ignore 0 duration
                    if (Convert.ToDouble(normalizedRow[Fn.DurationSec]) <= 0 &&
                        normalizedRow[Fn.ChargingStatus]=="1") {
                        normalizedRow[Fn.ChargingStatus] = "0";
                    }
                    decodedRows.Add(normalizedRow);
                    
                }
                catch (Exception e1)
                {
                    //if error found for one row, add this to inconsistent
                    Console.WriteLine(e1);
                    var inconsistentCdr = CdrConversionUtil.ConvertTxtRowToCdrinconsistent(rowCsv);
                    inconsistentCdr.SwitchId = input.Ne.idSwitch.ToString();
                    inconsistentCdr.FileName = input.TelcobrightJob.JobName;
                    inconsistentCdrs.Add(inconsistentCdr);
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

        public string getSqlWhereClauseForDayWiseSafeCollection(CdrCollectorInputData decoderInputData, DateTime day)
        {
            throw new NotImplementedException();
        }
    }
}
