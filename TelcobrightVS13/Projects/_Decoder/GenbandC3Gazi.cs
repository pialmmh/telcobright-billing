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
    public class GenbandC3Gazi : AbstractCdrDecoder
    {
        public override string ToString() => this.RuleName;
        public override string RuleName => GetType().Name;
        public override int Id => 70;
        public override string HelpText => "Decodes GenbandC3 CSV CDR For Gazi";
        public override CompressionType CompressionType { get; set; }
        public override string UniqueEventTablePrefix { get; }
        public override string PartialTableStorageEngine { get; }
        public override string partialTablePartitionColName { get; }
        protected CdrCollectorInputData Input { get; set; }


        private static DateTime parseStringToDate(string timestamp)  //20181028051316400 yyyyMMddhhmmssfff
        {
            DateTime dateTime = DateTime.ParseExact(timestamp, "MMddyyyyHHmmssfff", CultureInfo.InvariantCulture);
            return dateTime;
        }

        public override List<string[]> DecodeFile(CdrCollectorInputData decoderInputData, out List<cdrinconsistent> inconsistentCdrs)
        {
            this.Input = decoderInputData;
            string fileName = this.Input.FullPath;

            List<string[]> lines = FileUtil.ParseCsvWithEnclosedAndUnenclosedFields(fileName, ',', 1, "\"", ";");
            return decodeLine(decoderInputData, out inconsistentCdrs, fileName, lines);
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


        public List<string[]> decodeLine(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs, string fileName, List<string[]> lines)
        {
            
           
            inconsistentCdrs = new List<cdrinconsistent>();
            List<string[]> decodedRows = new List<string[]>();

            foreach (string[] lineAsArr in lines)
            {
                if (lineAsArr.Length < 100 ) continue;


                string[] textCdr= new  string[input.MefDecodersData.Totalfieldtelcobright];

                string durationStr = lineAsArr[135];
                double durationIn10sOfMillis=0;
                if (double.TryParse(durationStr, out durationIn10sOfMillis) && durationIn10sOfMillis <= 0) continue;



                textCdr[Fn.DurationSec] = ((durationIn10sOfMillis*10)/1000).ToString();
                textCdr[Fn.Sequencenumber] = lineAsArr[0];
                textCdr[Fn.Filename] = fileName;
                textCdr[Fn.IncomingRoute] = lineAsArr[45];
                textCdr[Fn.OriginatingCallingNumber] = lineAsArr[24];
                textCdr[Fn.OriginatingCalledNumber] = lineAsArr[27];
                textCdr[Fn.TerminatingCalledNumber] = lineAsArr[27];
                textCdr[Fn.TerminatingCallingNumber] = lineAsArr[24];
                textCdr[Fn.OutgoingRoute] = lineAsArr[38];

                string[] formats = new string[] { "MddyyyyHHmmssfff", "MMddyyyyHHmmssfff" };


                if (!string.IsNullOrEmpty(lineAsArr[16]))
                {
                    string startTimestr = lineAsArr[16].Trim();
                    DateTime startTime = startTimestr.ConvertToDateTimeFromCustomFormats(formats);
                    textCdr[Fn.StartTime] = startTime.ToMySqlFormatWithoutQuote();
                }

                string ansTimestr = lineAsArr[18].Trim();
                DateTime ansTime = ansTimestr.ConvertToDateTimeFromCustomFormats(formats);

                if (!string.IsNullOrEmpty(lineAsArr[18]))
                {
                    textCdr[Fn.AnswerTime] = ansTime.ToMySqlFormatWithoutQuote();
                }
                string endTimestr = lineAsArr[19].Trim();
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
