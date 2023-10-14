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
    public class TelcobridgeDecodingGaziTEST : AbstractCdrDecoder
    {
        public override string ToString() => this.RuleName;
        public override string RuleName => GetType().Name;
        public override int Id => 57;
        public override string HelpText => "WTL Decoder Teleplus TEST";
        public override CompressionType CompressionType { get; set; }
        public override string PartialTablePrefix { get; }
        public override string PartialTableStorageEngine { get; }
        public override string partialTablePartitionColName { get; }
        protected CdrCollectorInputData Input { get; set; }


        private static DateTime parseStringToDate(string timestamp)  //20181028051316400 yyyyMMddhhmmssfff
        {
            DateTime dateTime;
            if (DateTime.TryParseExact("20230904123527", "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime));
                
            return dateTime;
        }

        public override List<string[]> DecodeFile(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs)
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

            foreach (string ln in linesAsString)

            {
                lineAsArr = ln.Split(',');
                if(lineAsArr.Length < 5)
                    continue;             
                //string chargingStatus = lineAsArr[3] == "S" ? "1" : "0"; //done
                //if (chargingStatus != "1") continue;
                string[] textCdr = new string[input.MefDecodersData.Totalfieldtelcobright];


                string startTime = lineAsArr[4].Trim();
                if (!string.IsNullOrEmpty(startTime))
                {
                    startTime = parseStringToDate(startTime).ToString("yyyy-MM-dd HH:mm:ss");
                }

                string endTime = lineAsArr[5].Trim(); 
                if (!string.IsNullOrEmpty(endTime))
                {
                    endTime = parseStringToDate(endTime).ToString("yyyy-MM-dd HH:mm:ss");
                }

                string connectTime = lineAsArr[5].Trim();
                if (!string.IsNullOrEmpty(connectTime))
                {
                    connectTime = parseStringToDate(connectTime).ToString("yyyy-MM-dd HH:mm:ss");
                }               

                
                textCdr[Fn.Filename] = fileName;
                textCdr[Fn.Switchid] = Input.Ne.idSwitch.ToString();

                textCdr[Fn.OriginatingCallingNumber] = lineAsArr[8].Trim();
                textCdr[Fn.OriginatingCalledNumber] = lineAsArr[9].Trim();

                textCdr[Fn.TerminatingCallingNumber] = lineAsArr[8].Trim();
                textCdr[Fn.TerminatingCalledNumber] = lineAsArr[9].Trim();

                textCdr[Fn.DurationSec] = lineAsArr[7].Trim();

                textCdr[Fn.IncomingRoute] = lineAsArr[12].Trim();
                textCdr[Fn.OutgoingRoute] = lineAsArr[11].Trim();

                textCdr[Fn.Originatingip] = lineAsArr[13].Trim();
                textCdr[Fn.TerminatingIp] = lineAsArr[14].Trim();


                textCdr[Fn.StartTime] = startTime;
                textCdr[Fn.Endtime] = endTime;
                textCdr[Fn.ConnectTime] = connectTime;

                textCdr[Fn.UniqueBillId] = lineAsArr[2].Trim();

                lineAsArr[0].Trim();


                string status = lineAsArr[0].Trim().ToLower();
                if(status == "end")
                    textCdr[Fn.Partialflag] = "0";
                else
                    textCdr[Fn.Partialflag] = "1";


                textCdr[Fn.Validflag] = "1";

                

                decodedRows.Add(textCdr);
            }

            return decodedRows;

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