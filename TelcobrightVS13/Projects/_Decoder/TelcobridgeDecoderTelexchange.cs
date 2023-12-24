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
using System.IO;
using System.Text;
using Decoders;
using LibraryExtensions;
using TelcobrightMediation.Cdr.Collection.PreProcessors;

namespace Decoders
{

    [Export("Decoder", typeof(AbstractCdrDecoder))]
    public class TelcobridgeDecoderTelexchange : AbstractCdrDecoder
    {
        public override string ToString() => this.RuleName;
        public override string RuleName => GetType().Name;
        public override int Id => 570;
        public override string HelpText => "Teleexchange Telcobridge";
        public override CompressionType CompressionType { get; set; }
        protected CdrCollectorInputData Input { get; set; }


        private static DateTime? parseStringToDate(string timestamp)
        {
            DateTime dateTime;
            if (DateTime.TryParseExact(timestamp, "yyyy-MM-dd HH.mm.ss.fff", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out dateTime))
            {
                return dateTime;
            }
            return null;
        }

        public override List<string[]> DecodeFile(CdrCollectorInputData input,
            out List<cdrinconsistent> inconsistentCdrs)
        {
            this.Input = input;
            string fileName = this.Input.FullPath;
            //List<string[]> lines = FileUtil.ParseCsvWithEnclosedAndUnenclosedFields(fileName, ',', 1, "\"", ";");
            //List<string[]> lines = FileUtil.ParseCsvWithEnclosedAndUnenclosedFields(fileName, ',', 0, "\"", ",");

            // my parser
            string[] linesAsString = File.ReadAllLines(fileName);
            string[] lineAsArr;


            //

            inconsistentCdrs = new List<cdrinconsistent>();
            List<string[]> decodedRows = new List<string[]>();
            //this.Input = input;
            List<cdrfieldmappingbyswitchtype> fieldMappings = null;
            bool emptyLineFound = false;
            try
            {
                foreach (string ln in linesAsString)
                {
                    if (ln.IsNullOrEmptyOrWhiteSpace())
                    {
                        emptyLineFound = true;
                        continue;
                    }
                    if (emptyLineFound)
                    {
                        throw new Exception("Empty line found between Telcobridge rows, file may be inconsistent.");
                    }
                    lineAsArr = ln.Split(',');
                    string[] textCdr = new string[input.MefDecodersData.Totalfieldtelcobright];

                    
                    textCdr[Fn.Partialflag] = "1";// all telcobridge cdrs are partial
                    textCdr[Fn.DurationSec] = "0";


                    //originate or answer, intrunk taken from "originate" let, outtrunk from "answer" leg
                    string inTrunkAdditionalInfo = "";
                    if (lineAsArr.Length < 14)
                    {
                        textCdr[Fn.IncomingRoute] = lineAsArr[8].Trim();
                        textCdr[Fn.OutgoingRoute] = lineAsArr[8].Trim();

                        inTrunkAdditionalInfo = lineAsArr[9].Trim();

                        textCdr[Fn.OriginatingCallingNumber] = lineAsArr[6].Trim();
                        textCdr[Fn.OriginatingCalledNumber] = lineAsArr[7].Trim();

                        textCdr[Fn.TerminatingCallingNumber] = lineAsArr[6].Trim();
                        textCdr[Fn.TerminatingCalledNumber] = lineAsArr[7].Trim();

                        if (inTrunkAdditionalInfo == "originate")
                        {
                            textCdr[Fn.IncomingRoute] = lineAsArr[8].Trim();
                        }
                        else if (inTrunkAdditionalInfo == "answer")
                        {
                            textCdr[Fn.OutgoingRoute] = lineAsArr[8].Trim();
                        }
                        else
                        {
                            Console.WriteLine("except");
                        }
                    }
                    else
                    {
                        textCdr[Fn.IncomingRoute] = lineAsArr[12].Trim();
                        textCdr[Fn.OutgoingRoute] = lineAsArr[12].Trim();

                        inTrunkAdditionalInfo = lineAsArr[14];

                        textCdr[Fn.OriginatingCallingNumber] = lineAsArr[10].Trim();
                        textCdr[Fn.OriginatingCalledNumber] = lineAsArr[11].Trim();

                        textCdr[Fn.TerminatingCallingNumber] = lineAsArr[10].Trim();
                        textCdr[Fn.TerminatingCalledNumber] = lineAsArr[11].Trim();

                        if (inTrunkAdditionalInfo == "originate")
                        {
                            textCdr[Fn.IncomingRoute] = lineAsArr[12].Trim();
                        }
                        else if (inTrunkAdditionalInfo == "answer")
                        {
                            textCdr[Fn.OutgoingRoute] = lineAsArr[12].Trim();
                        }
                    }

                    textCdr[Fn.InTrunkAdditionalInfo] = inTrunkAdditionalInfo;
                    string outTrunkAdditionalInfo = lineAsArr[1].Trim().ToLower();
                    textCdr[Fn.OutTrunkAdditionalInfo] = outTrunkAdditionalInfo;//start,update,end          
                    string startTimeStr = lineAsArr[4].Trim();
                    DateTime? startTime = null;
                    if (!string.IsNullOrEmpty(startTimeStr))
                    {
                        startTime = parseStringToDate(startTimeStr);
                        startTimeStr = startTime == null
                            ? ""
                            : Convert.ToDateTime(startTime).ToMySqlFormatWithoutQuote();
                    }

                    string connectTimeStr = lineAsArr[5].Trim();
                    DateTime? connectTime = null;
                    if (!string.IsNullOrEmpty(connectTimeStr))
                    {
                        connectTime= parseStringToDate(connectTimeStr);
                        connectTimeStr = connectTime == null
                            ? ""
                            : Convert.ToDateTime(connectTime).ToMySqlFormatWithoutQuote();
                    }

                    string endTimeStr = lineAsArr[6].Trim();
                    DateTime? endTime = null;
                    if (!string.IsNullOrEmpty(endTimeStr))
                    {
                        endTime = parseStringToDate(endTimeStr);
                        endTimeStr = endTime == null
                            ? ""
                            : Convert.ToDateTime(endTime).ToMySqlFormatWithoutQuote();
                    }

                    string durationSecStr="0";
                    double durationSec = 0;
                    if (connectTime != null && endTime != null)
                    {
                        TimeSpan timeDiff = Convert.ToDateTime(endTime)- Convert.ToDateTime(connectTime);
                        durationSec = timeDiff.TotalSeconds;
                        durationSecStr = durationSec.ToString();
                    }
                    textCdr[Fn.ChargingStatus] = durationSec > 0 ? "1" : "0";

                    textCdr[Fn.Filename] = fileName;
                    textCdr[Fn.Switchid] = Input.Ne.idSwitch.ToString();

                    

                    textCdr[Fn.StartTime] = startTimeStr;
                    textCdr[Fn.AnswerTime] = connectTimeStr.IsNullOrEmptyOrWhiteSpace() ? startTimeStr : connectTimeStr;
                    textCdr[Fn.Endtime] = endTimeStr.IsNullOrEmptyOrWhiteSpace() ? textCdr[Fn.AnswerTime] : endTimeStr;
                    textCdr[Fn.DurationSec] = durationSecStr;

                  
                    textCdr[Fn.Validflag] = "1";
                    string seqNumber = lineAsArr[3].Remove(0, 2);
                    seqNumber = Int64.Parse(seqNumber, NumberStyles.HexNumber).ToString();
                    textCdr[Fn.Sequencenumber] = seqNumber;

                    textCdr[Fn.UniqueBillId] = lineAsArr[2].Trim();
                    string customUniqueBillId = this.getTupleExpression(new Dictionary<string, object>()
                    {
                        { "collectorInput", this.Input},
                        {"row", textCdr }
                    });
                    textCdr[Fn.UniqueBillId] = customUniqueBillId;
                    decodedRows.Add(textCdr);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                e.Data.Add("customError", "Possibly Corrupted");
                throw e;
            }
            return decodedRows;
        }

        public override string getTupleExpression(Object data)
        {
            Dictionary<string, object> dataAsDic = (Dictionary<string, object>)data;
            CdrCollectorInputData collectorInput = (CdrCollectorInputData)dataAsDic["collectorInput"];
            CdrSetting cdrSetting = collectorInput.CdrSetting;
            string[] row = (string[])dataAsDic["row"];
            int switchId = collectorInput.Ne.idSwitch;
            DateTime startTime = getEventDatetime(new Dictionary<string, object>
            {
                {"cdrSetting", cdrSetting},
                {"row", row}
            });
            //23:00 hours eventid to be rounded up as 00:00 next hour in uniqueEventTupleId                        
            //aggregation logic checks cdr for +-1 hour, so collection and aggregation will be possible            
            if (startTime.Hour == 23)
            {
                startTime = startTime.Date.AddDays(1);
            }
            else
            {
                //startTime = startTime.Date.AddHours(startTime.Hour); prev logic
                startTime = startTime.Date; //new logic
            }
            string sessionId = row[Fn.UniqueBillId];
            string separator = "/";
            return new StringBuilder(switchId.ToString()).Append(separator)
                .Append(startTime.ToMySqlFormatDateOnlyWithoutTimeAndQuote()).Append(separator)
                .Append(sessionId).ToString();
        }

        public override EventAggregationResult Aggregate(object data)
        {
            return TelcobridgeAggregationHelper.Aggregate((NewAndOldEventsWrapper<string[]>)data);
        }
    }
}