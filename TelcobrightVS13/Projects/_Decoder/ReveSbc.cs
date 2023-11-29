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

    [Export("Decoder", typeof(AbstractCdrDecoder))]
    public class ReveSbc : AbstractCdrDecoder
    {
        public override string ToString() => this.RuleName;
        public override string RuleName => GetType().Name;
        public override int Id => 29;
        public override string HelpText => "Decodes ReveSBC CSV CDR.";
        public override CompressionType CompressionType { get; set; }
        public override string UniqueEventTablePrefix { get; }
        public override string PartialTableStorageEngine { get; }
        public override string partialTablePartitionColName { get; }
        protected CdrCollectorInputData Input { get; set; }




        public static double ConvertToUnixTimestamp(DateTime date)
        {

            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        public override List<string[]> DecodeFile(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs)
        {
            this.Input = input;
            string fileName = this.Input.FullPath;
            List<string[]> lines = FileUtil.ParseCsvWithEnclosedAndUnenclosedFields(fileName, ',', 1, "\"", ";");
            return decodeLines(Input, out inconsistentCdrs, fileName, lines);
        }

        public override string getTupleExpression(Object data)
        {
            throw new NotImplementedException();
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


        protected static List<string[]> decodeLines(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs, string fileName, List<string[]> lines)
        {
            inconsistentCdrs = new List<cdrinconsistent>();
            List<string[]> decodedRows = new List<string[]>();

            foreach (string[] lineAsArr in lines)
            {
                string[] textCdr = new string[input.MefDecodersData.Totalfieldtelcobright];
                try
                {
                    string startTimestr = lineAsArr[10].Trim();
                    string answerTimeStr = lineAsArr[11].Trim();
                    string endTimeStr = lineAsArr[12].Trim();

                    DateTime startTime = startTimestr.ConvertToDateTimeFromCustomFormat("yyyyMMddHHmmss.fff");
                    DateTime anstime = answerTimeStr.ConvertToDateTimeFromCustomFormat("yyyyMMddHHmmss.fff");
                    DateTime endtime = endTimeStr.ConvertToDateTimeFromCustomFormat("yyyyMMddHHmmss.fff");

                    textCdr[Fn.StartTime] = startTime.ToMySqlFormatWithoutQuote();
                    textCdr[Fn.AnswerTime] = anstime.ToMySqlFormatWithoutQuote();
                    textCdr[Fn.Endtime] = endtime.ToMySqlFormatWithoutQuote();
                    TimeSpan duration = endtime - anstime;

                    double durationSec = (duration.TotalMilliseconds) / 1000;
                    if (durationSec <= 0)//skip failed calls
                    {
                        continue;
                    }
                    textCdr[Fn.ChargingStatus] = "1";
                    textCdr[Fn.DurationSec] = durationSec.ToString();


                    textCdr[Fn.Switchid] = "2";
                    textCdr[Fn.Sequencenumber] = lineAsArr[0];

                    string ownSignalingAddress = lineAsArr[1].Trim() + ":5060";//as discussed with Agni Romel, they will always maintain 5060 for own sip signaling

                    textCdr[Fn.Filename] = fileName;
                    string originatingIp = lineAsArr[5];
                    string originatingPort = lineAsArr[6];

                    string orignatingIpPort = originatingIp + ":" + originatingPort;
                    textCdr[Fn.IncomingRoute] = orignatingIpPort;


                    textCdr[Fn.Originatingip] = orignatingIpPort;
                    string terminitingIp = lineAsArr[15];
                    string terminitingPort = lineAsArr[16];
                    string terminitingIpPort = terminitingIp + ":" + terminitingPort;
                    textCdr[Fn.TerminatingIp] = terminitingIpPort;
                    textCdr[Fn.OutgoingRoute] = terminitingIpPort;

                    textCdr[Fn.Mediaip1] = lineAsArr[18];
                    textCdr[Fn.Mediaip2] = lineAsArr[21];

                    if (!string.IsNullOrEmpty(lineAsArr[7].Trim())) textCdr[Fn.OriginatingCalledNumber] = lineAsArr[7].Trim();
                    if (!string.IsNullOrEmpty(lineAsArr[9].Trim())) textCdr[Fn.OriginatingCallingNumber] = lineAsArr[9].Trim();
                    if (!string.IsNullOrEmpty(lineAsArr[8].Trim())) textCdr[Fn.TerminatingCalledNumber] = lineAsArr[8].Trim();
                    if (!string.IsNullOrEmpty(lineAsArr[9].Trim())) textCdr[Fn.TerminatingCallingNumber] = lineAsArr[9].Trim();


                    textCdr[Fn.Validflag] = "1";
                    textCdr[Fn.Partialflag] = "0";
                    decodedRows.Add(textCdr.ToArray());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    var cdrInconsistent = CdrConversionUtil.ConvertTxtRowToCdrinconsistent(textCdr);
                    inconsistentCdrs.Add(cdrInconsistent);
                }
            }
            return decodedRows;
        }
    }
}

