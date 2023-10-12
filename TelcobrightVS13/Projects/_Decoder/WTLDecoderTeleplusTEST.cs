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

    [Export("Decoder", typeof(IFileDecoder))]
    public class WTLDecoderTeleplusTEST : IFileDecoder
    {
        public override string ToString() => this.RuleName;
        public virtual string RuleName => GetType().Name;
        public int Id => 73;
        public string HelpText => "WTL Decoder Teleplus TEST";
        public CompressionType CompressionType { get; set; }
        public string PartialTablePrefix { get; }
        public string PartialTableStorageEngine { get; }
        public string partialTablePartitionColName { get; }
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

        public List<string[]> DecodeFile(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs)
        {
            this.Input = input;
            string fileName = this.Input.FullPath;
            //List<string[]> lines = FileUtil.ParseCsvWithEnclosedAndUnenclosedFields(fileName, ',', 1, "\"", ";");
            List<string[]> lines = FileUtil.ParseCsvWithEnclosedAndUnenclosedFields(fileName, ',', 0, "\"", ",", true);

            inconsistentCdrs = new List<cdrinconsistent>();
            List<string[]> decodedRows = new List<string[]>();
            //this.Input = input;
            List<cdrfieldmappingbyswitchtype> fieldMappings = null;

            foreach (string[] lineAsArr in lines)

            {
                //string chargingStatus = lineAsArr[3] == "S" ? "1" : "0"; //done
                //if (chargingStatus != "1") continue;

                string chargingstatus = "1";
                if (lineAsArr[70].Trim() == "0")
                    chargingstatus = "0";
                if (chargingstatus == "0")
                    continue;





                string[] textCdr = new string[input.MefDecodersData.Totalfieldtelcobright];


                var terminatingCallingNumber = lineAsArr[8].Trim();
                textCdr[Fn.TerminatingCallingNumber] = terminatingCallingNumber;
                var terminatingCalledNumber = lineAsArr[27].Trim();
                textCdr[Fn.TerminatingCalledNumber] = terminatingCalledNumber;

                if (string.IsNullOrEmpty(terminatingCalledNumber) && string.IsNullOrEmpty(terminatingCalledNumber))
                {
                    double durSec = Convert.ToDouble(lineAsArr[70].Trim());
                    if (durSec > 0)
                    {
                        throw new ArgumentException("call duration should not greater than 0");
                    }
                    continue;
                }

                string startTime = lineAsArr[1];
                string ansTime = "";

                if (Convert.ToInt32(lineAsArr[70].Trim()) == 0)
                    continue;

                if (!string.IsNullOrEmpty(lineAsArr[1]))
                {
                    startTime = UnixTimeStampToDateTime(startTime).ToString("yyyy-MM-dd HH:mm:ss");
                    ansTime = startTime;

                }
                //endTime   = UnixTimeStampToDateTime(startTime).AddSeconds(Convert.ToInt32(lineAsArr[60])).ToString("yyyy-MM-dd HH:mm:ss");
                string endTime = "";
                if (!string.IsNullOrEmpty(ansTime))
                {
                    DateTime ansTime1 = DateTime.ParseExact(ansTime, "yyyy-MM-dd HH:mm:ss",
                                       System.Globalization.CultureInfo.InvariantCulture);
                    double durSec = Convert.ToDouble(lineAsArr[60]);
                    endTime = ansTime1.AddSeconds(durSec).ToString("yyyy-MM-dd HH:mm:ss");
                }

                string incomingRoute = lineAsArr[7];
                //string finalIncRoute = "";
                if (!string.IsNullOrEmpty(incomingRoute))
                {
                    incomingRoute = incomingRoute.Trim();
                    //string[] incomingRouteArr = incomingRoute.Split('.');
                    ////

                    //int[] intArray = new int[incomingRouteArr.Length];

                    //for (int i = 0; i < incomingRouteArr.Length; i++)
                    //{

                    //    intArray[i] = Convert.ToInt32(incomingRouteArr[i]);

                    //}
                    //finalIncRoute = string.Join(".", intArray);

                }
                string outgoingRoute = lineAsArr[28];
                if (!string.IsNullOrEmpty(outgoingRoute))
                {
                    outgoingRoute = outgoingRoute.Trim();
                }


                textCdr[Fn.StartTime] = startTime;
                textCdr[Fn.AnswerTime] = ansTime;
                textCdr[Fn.Endtime] = endTime;

                textCdr[Fn.IncomingRoute] = incomingRoute;
                textCdr[Fn.OutgoingRoute] = outgoingRoute;

                textCdr[Fn.Filename] = fileName;
                textCdr[Fn.Switchid] = Input.Ne.idSwitch.ToString();

                textCdr[Fn.OriginatingCallingNumber] = lineAsArr[8].Trim(); ;
                textCdr[Fn.OriginatingCalledNumber] = lineAsArr[27].Trim(); ;



                textCdr[Fn.Originatingip] = incomingRoute;
                textCdr[Fn.TerminatingIp] = outgoingRoute;

                //textCdr[Fn.Originatingip] = lineAsArr[61].Trim();

                textCdr[Fn.UniqueBillId] = lineAsArr[68].Trim();

                textCdr[Fn.DurationSec] = lineAsArr[60].Trim();



                textCdr[Fn.Sequencenumber] = lineAsArr[1].Trim();
                textCdr[Fn.Validflag] = "1";
                textCdr[Fn.ChargingStatus] = "1";





                decodedRows.Add(textCdr);
            }

            return decodedRows;

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

        public string getWhereForHourWiseCollection(Object data)
        {
            throw new NotImplementedException();
        }

        public string getSelectExpressionForPartialCollection(Object data)
        {
            throw new NotImplementedException();
        }

        public DateTime getEventDatetime(Object data)
        {
            throw new NotImplementedException();
        }
    }
}