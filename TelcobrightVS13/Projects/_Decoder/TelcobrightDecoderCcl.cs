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
using LibraryExtensions;

namespace Decoders
{

    [Export("Decoder", typeof(AbstractCdrDecoder))]
    public class TelcobrightDecoderCcl : AbstractCdrDecoder
    {
        public override string ToString() => this.RuleName;
        public override string RuleName => GetType().Name;
        public override int Id => 737;
        public override string HelpText => "Telcobright Decoder CCL TEST";
        public override CompressionType CompressionType { get; set; }
        public override string UniqueEventTablePrefix { get; }
        public override string PartialTableStorageEngine { get; }
        public override string partialTablePartitionColName { get; }
        protected CdrCollectorInputData Input { get; set; }


        //private static DateTime parseStringToDate(string timestamp)  //20181028051316400 yyyyMMddhhmmssfff
        //{
        //    DateTime dateTime = DateTime.ParseExact(timestamp, "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
        //    return dateTime;
        //}

        public static DateTime UnixTimeStampToDateTime(string unixTimeStampStr)
        {
            double unixTimeStamp = Convert.ToDouble(unixTimeStampStr);
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        public override List<string[]> DecodeFile(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs)
        {
            this.Input = input;
            string fileName = this.Input.FullPath;
            //List<string[]> lines = FileUtil.ParseCsvWithEnclosedAndUnenclosedFields(fileName, ',', 1, "\"", ";");
            List<string[]> lines = FileUtil.ParseCsvWithEnclosedAndUnenclosedFields(fileName, ',', 1, "\"", ",", true);

            inconsistentCdrs = new List<cdrinconsistent>();
            List<string[]> decodedRows = new List<string[]>();
            //this.Input = input;
            List<cdrfieldmappingbyswitchtype> fieldMappings = null;

            try
            {
                foreach (string[] lineAsArr in lines)

                {
                    string[] textCdr = new string[input.MefDecodersData.Totalfieldtelcobright];

                    double duretionSec = Convert.ToDouble(lineAsArr[7].Trim());

                    DateTime tmpstartTime;
                    string ansTime = "";
                    string startime = "";
                    if (DateTime.TryParseExact(lineAsArr[4], "yyyy-MM-dd HH:mm:ss",
                        CultureInfo.CurrentCulture, DateTimeStyles.None, out tmpstartTime) == true)
                    {
                        startime = tmpstartTime.ToMySqlFormatWithoutQuote();
                        ansTime = startime;
                    }
                    else
                    {
                        throw new Exception("Datetime is not a correct format");
                    }
                    string endTime = "";
                    if (!string.IsNullOrEmpty(ansTime))
                    {
                        DateTime ansTime1;
                        if (DateTime.TryParseExact(ansTime, "yyyy-MM-dd HH:mm:ss",
                        CultureInfo.CurrentCulture, DateTimeStyles.None, out ansTime1))
                        {
                            endTime = ansTime1.AddSeconds(duretionSec).ToMySqlFormatWithoutQuote() ;

                        }
                    }

                    string incomingRoute = lineAsArr[10];
                    if (!string.IsNullOrEmpty(incomingRoute))
                    {
                        incomingRoute = incomingRoute.Trim();

                    }

                    string outgoingRoute = lineAsArr[11];
                    if (!string.IsNullOrEmpty(outgoingRoute))
                    {
                        outgoingRoute = outgoingRoute.Trim();
                    }

                    textCdr[Fn.StartTime] = startime;
                    textCdr[Fn.AnswerTime] = ansTime;
                    textCdr[Fn.Endtime] = endTime;

                    textCdr[Fn.IncomingRoute] = incomingRoute;
                    textCdr[Fn.OutgoingRoute] = outgoingRoute;

                    textCdr[Fn.Filename] = fileName;
                    textCdr[Fn.Switchid] = Input.Ne.idSwitch.ToString();

                    textCdr[Fn.OriginatingCallingNumber] = lineAsArr[0].Trim(); 
                    textCdr[Fn.OriginatingCalledNumber] = lineAsArr[2].Trim();

                    textCdr[Fn.TerminatingCallingNumber] = lineAsArr[0].Trim();
                    textCdr[Fn.TerminatingCalledNumber] = lineAsArr[3].Trim();

                    textCdr[Fn.Originatingip] = incomingRoute;
                    textCdr[Fn.TerminatingIp] = outgoingRoute;

                    var channelCallUuid = lineAsArr[8].ToString();
                    textCdr[Fn.UniqueBillId] = channelCallUuid;
                    textCdr[Fn.Sequencenumber] = "0";
                    textCdr[Fn.DurationSec] = duretionSec.ToString();

                    textCdr[Fn.Validflag] = "1";
                    textCdr[Fn.Partialflag] = "0";
                    textCdr[Fn.ChargingStatus] = "1";

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
    }
}