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
    public class GenbandC3DecoderBangla : IFileDecoder
    {
        public override string ToString() => this.RuleName;
        public virtual string RuleName => GetType().Name;
        public int Id => 49;
        public string HelpText => "Decodes GenbandC3 (Bangla Version) CSV CDR.";
        public CompressionType CompressionType { get; set; }
        public string PartialTablePrefix { get; }
        public string PartialTableStorageEngine { get; }
        public string partialTablePartitionColName { get; }
        protected CdrCollectorInputData Input { get; set; }      

        private static DateTime parseStringToDate(string timestamp)  //20181028051316400 yyyyMMddhhmmssfff
        {
            DateTime dateTime = DateTime.ParseExact(timestamp, "MMddyyyyHHmmssfff", CultureInfo.InvariantCulture);
            return dateTime;
        }

        public virtual List<string[]> DecodeFile(CdrCollectorInputData decoderInputData, out List<cdrinconsistent> inconsistentCdrs)
        {
            this.Input = decoderInputData;
            string fileName = this.Input.FullPath;
            List<string[]> lines = FileUtil.ParseCsvWithEnclosedAndUnenclosedFields(fileName, ',',5, "\"", ";");
            return decodeLine(decoderInputData, out inconsistentCdrs, fileName, lines);
        }

        public string getTupleExpression(CdrCollectorInputData decoderInputData, string[] row)
        {
            throw new NotImplementedException();
        }

        public string getCreateTableSqlForUniqueEvent(Object data)
        {
            throw new NotImplementedException();
        }

        public string getSelectExpressionForUniqueEvent(Object data)
        {
            throw new NotImplementedException();
        }

        public string getWhereForHourWiseUniqueEventCollection(Object data)
        {
            throw new NotImplementedException();
        }

        public string getSelectExpressionForPartialCollection(CdrCollectorInputData decoderInputData)
        {
            throw new NotImplementedException();
        }

        public string getWhereForHourWisePartialCollection(CdrCollectorInputData decoderInputData, DateTime hourOfDay)
        {
            throw new NotImplementedException();
        }

        public List<string[]> decodeLine(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs, string fileName, List<string[]> lines)
        {
            inconsistentCdrs = new List<cdrinconsistent>();
            List<string[]> decodedRows = new List<string[]>();

            foreach (string[] lineAsArr in lines)
            {
                if (lineAsArr.Length < 15) continue;

                string[] textCdr = new string[input.MefDecodersData.Totalfieldtelcobright];

                string durationStr = lineAsArr[16];
                double durationIn10sOfMillis = 0;
                if (double.TryParse(durationStr, out durationIn10sOfMillis) && durationIn10sOfMillis <= 0) continue;

                textCdr[Fn.DurationSec] = ((durationIn10sOfMillis * 10) / 1000).ToString();
                textCdr[Fn.Sequencenumber] = lineAsArr[0];
                textCdr[Fn.ReleaseCauseSystem] = lineAsArr[49];
                textCdr[Fn.Filename] = fileName;
                textCdr[Fn.IncomingRoute] = lineAsArr[26];
                textCdr[Fn.OriginatingCallingNumber] = lineAsArr[4];
                textCdr[Fn.OriginatingCalledNumber] = lineAsArr[8];
                textCdr[Fn.TerminatingCalledNumber] = lineAsArr[8];
                textCdr[Fn.TerminatingCallingNumber] = lineAsArr[4];
                textCdr[Fn.OutgoingRoute] = lineAsArr[33];

                string[] formats = new string[] { "MddyyyyHHmmssfff", "MMddyyyyHHmmssfff" };

                if (!string.IsNullOrEmpty(lineAsArr[13]))
                {
                    string startTimestr = lineAsArr[13].Trim();
                    DateTime startTime = startTimestr.ConvertToDateTimeFromCustomFormats(formats);
                    textCdr[Fn.StartTime] = startTime.ToMySqlFormatWithoutQuote();
                }
                string ansTimestr = lineAsArr[14].Trim();
                DateTime ansTime = ansTimestr.ConvertToDateTimeFromCustomFormats(formats);

                if (!string.IsNullOrEmpty(lineAsArr[14]))
                {
                    textCdr[Fn.AnswerTime] = ansTime.ToMySqlFormatWithoutQuote();
                }
                string endTimestr = lineAsArr[15].Trim();
                if (!string.IsNullOrEmpty(endTimestr))
                {
                    DateTime endTime = endTimestr.ConvertToDateTimeFromCustomFormats(formats);
                    textCdr[Fn.Endtime] = endTime.ToMySqlFormatWithoutQuote();
                }
                else
                {
                    textCdr[Fn.Endtime] = ansTime.ToMySqlFormatWithoutQuote();
                }
                textCdr[Fn.Validflag] = "1";
                textCdr[Fn.ChargingStatus] = "1";
                decodedRows.Add(textCdr.ToArray());
            }
            return decodedRows;
        }
        
    }
}
