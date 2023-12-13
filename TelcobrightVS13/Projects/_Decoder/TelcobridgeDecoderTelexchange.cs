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
        public override string UniqueEventTablePrefix { get; }
        public override string PartialTableStorageEngine { get; }
        public override string partialTablePartitionColName { get; }
        protected CdrCollectorInputData Input { get; set; }


        private static DateTime parseStringToDate(string timestamp)  //20181028051316400 yyyyMMddhhmmssfff
        {
            DateTime dateTime;
            if (DateTime.TryParseExact(timestamp, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime));
                
            return dateTime;
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

            try
            {
                foreach (string ln in linesAsString)
                {
                    lineAsArr = ln.Split(',');
                    string[] textCdr = new string[input.MefDecodersData.Totalfieldtelcobright];

                    if (lineAsArr.Length < 5)
                        continue;
                    if (!string.IsNullOrEmpty(lineAsArr[5]))
                    {
                        continue;
                    }
                    if (lineAsArr.Length < 5)
                        continue;
                    //string durationStr = lineAsArr[7].Trim();
                    //double durationSec = 0;
                    //double.TryParse(durationStr, out durationSec);
                    //if (durationSec <= 0) continue;
                    string callStatus = lineAsArr[1].Trim().ToLower();
                    if (!string.Equals(callStatus, "End")) continue;

                    textCdr[Fn.Partialflag] = "1";// all telcobridge cdrs are partial
                    //textCdr[Fn.DurationSec] = lineAsArr[7].Trim();

                    //originate or answer, intrunk taken from "originate" let, outtrunk from "answer" leg
                    string inTrunkAdditionalInfo = lineAsArr[14].Trim().ToLower();
                    textCdr[Fn.InTrunkAdditionalInfo] = inTrunkAdditionalInfo;

                    string startTimeStr = lineAsArr[4].Trim();
                    DateTime startTime = new DateTime();
                    if (!string.IsNullOrEmpty(startTimeStr))
                    {
                        startTime = DateTime.ParseExact(startTimeStr, "yyyy-MM-dd HH.mm.ss.fff", CultureInfo.InvariantCulture);
                        startTimeStr = startTime.ToString("yyyy-MM-dd HH:mm:ss");
                    }

                    string connectTimeStr = lineAsArr[5].Trim();
                    DateTime connectTime = new DateTime();
                    if (!string.IsNullOrEmpty(connectTimeStr))
                    {
                        connectTime = DateTime.ParseExact(connectTimeStr, "yyyy-MM-dd HH.mm.ss.fff", CultureInfo.InvariantCulture);
                        connectTimeStr = connectTime.ToString("yyyy-MM-dd HH:mm:ss");
                    }

                    string endTimeStr = lineAsArr[6].Trim();
                    DateTime endTime = new DateTime();
                    if (!string.IsNullOrEmpty(endTimeStr))
                    {
                        endTime = DateTime.ParseExact(endTimeStr, "yyyy-MM-dd HH.mm.ss.fff", CultureInfo.InvariantCulture);
                        endTimeStr = endTime.ToString("yyyy-MM-dd HH:mm:ss");
                    }


                    TimeSpan DurationSec = connectTime - startTime;
                    string DurationSecStr = DurationSec.TotalSeconds.ToString();

                    textCdr[Fn.Filename] = fileName;
                    textCdr[Fn.Switchid] = Input.Ne.idSwitch.ToString();

                    textCdr[Fn.OriginatingCallingNumber] = lineAsArr[9].Trim();
                    textCdr[Fn.OriginatingCalledNumber] = lineAsArr[10].Trim();

                    textCdr[Fn.TerminatingCallingNumber] = lineAsArr[9].Trim();
                    textCdr[Fn.TerminatingCalledNumber] = lineAsArr[10].Trim();

                    if(inTrunkAdditionalInfo == "originate")
                    {
                        textCdr[Fn.IncomingRoute] = lineAsArr[12].Trim();
                    }
                    else
                    {
                        textCdr[Fn.OutgoingRoute] = lineAsArr[12].Trim();
                    }
                     

                    textCdr[Fn.StartTime] = startTimeStr;
                    textCdr[Fn.Endtime] = endTimeStr;
                    textCdr[Fn.AnswerTime] = connectTimeStr;
                    textCdr[Fn.DurationSec] = DurationSecStr;
                


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
                return decodedRows;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                e.Data.Add("customError", "Possibly Corrupted");
                e.Data.Add("jobId", input.TelcobrightJob.id);
                throw e;
            }
        }

        public override EventAggregationResult Aggregate(object data)
        {
            return TelcobridgeAggregationHelper.Aggregate(data);
        }

        public override string getCreateTableSqlForUniqueEvent(Object data)
        {
            throw new NotImplementedException();
        }

        public override string getSelectExpressionForUniqueEvent(Object data)
        {
            throw new NotImplementedException();
        }

        public override string getWhereForHourWiseCollection(Object data)
        {
            throw new NotImplementedException();
        }

        public override string getSelectExpressionForPartialCollection(Object data)
        {
            throw new NotImplementedException();
        }

        public override DateTime getEventDatetime(Object data)
        {
            throw new NotImplementedException();
        }
    }
}