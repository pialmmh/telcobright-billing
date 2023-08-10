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
    public class ReveSbc : IFileDecoder
    {
        public override string ToString() => this.RuleName;
        public virtual string RuleName => GetType().Name;
        public int Id => 29;
        public string HelpText => "Decodes ReveSBC CSV CDR.";
        public CompressionType CompressionType { get; set; }
        protected CdrCollectorInputData Input { get; set; }




        public static double ConvertToUnixTimestamp(DateTime date)
        {

            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        public virtual List<string[]> DecodeFile(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs)
        {
            this.Input = input;
            string fileName = this.Input.FullPath;
            List<string[]> lines = FileUtil.ParseCsvWithEnclosedAndUnenclosedFields(fileName, ',', 1, "\"", ";");
            return decodeLines(Input, out inconsistentCdrs, fileName, lines);
        }

        public string getTupleExpression(CdrCollectorInputData decoderInputData, string[] row)
        {
            throw new NotImplementedException();
        }

        public string getSqlWhereClauseForDayWiseSafeCollection(CdrCollectorInputData decoderInputData, DateTime day)
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


                textCdr[Fn.Switchid] = "9";
                textCdr[Fn.Sequencenumber] = lineAsArr[0];

                textCdr[Fn.Filename] = fileName;
                string originatingIp = lineAsArr[5];
                string originatingPort = lineAsArr[6];

                string orignatingIpPort = originatingIp + ":" + originatingPort;
                textCdr[Fn.IncomingRoute] = orignatingIpPort;


                textCdr[Fn.Originatingip] = orignatingIpPort;
                string terminitingIp = lineAsArr[12];
                string terminitingPort = lineAsArr[13];
                string terminitingIpPort = terminitingIp + ":" + terminitingPort;
                textCdr[Fn.TerminatingIp] = terminitingIpPort;


                textCdr[Fn.Mediaip1] = lineAsArr[18];
                textCdr[Fn.Mediaip2] = lineAsArr[21];


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
                textCdr[Fn.DurationSec] = durationSec.ToString();



                if (!string.IsNullOrEmpty(lineAsArr[7].Trim())) textCdr[Fn.OriginatingCalledNumber] = lineAsArr[7].Trim();
                if (!string.IsNullOrEmpty(lineAsArr[9].Trim())) textCdr[Fn.OriginatingCallingNumber] = lineAsArr[9].Trim();
                if (!string.IsNullOrEmpty(lineAsArr[8].Trim())) textCdr[Fn.TerminatingCalledNumber] = lineAsArr[8].Trim();
                if (!string.IsNullOrEmpty(lineAsArr[20].Trim())) textCdr[Fn.TerminatingCallingNumber] = lineAsArr[20].Trim();


                textCdr[Fn.Validflag] = "1";
                decodedRows.Add(textCdr.ToArray());
            }
            return decodedRows;
        }
    }
}

