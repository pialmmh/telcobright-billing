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
    public class GNEWDecoderRingTechTEST : IFileDecoder
    {
        public override string ToString() => this.RuleName;
        public virtual string RuleName => GetType().Name;
        public int Id => 56;
        public string HelpText => "WTL Decoder Teleplus TEST";
        public CompressionType CompressionType { get; set; }
        protected CdrCollectorInputData Input { get; set; }


        private static DateTime parseStringToDate(string timestamp)  //20181028051316400 yyyyMMddhhmmssfff
        {
            DateTime dateTime;
            if (DateTime.TryParseExact("20230904123527", "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime));
                
            return dateTime;
        }

        public List<string[]> DecodeFile(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs)
        {
            this.Input = input;
            string fileName = this.Input.FullPath;
            //List<string[]> lines = FileUtil.ParseCsvWithEnclosedAndUnenclosedFields(fileName, ',', 1, "\"", ";");
            List<string[]> lines = FileUtil.ParseCsvWithEnclosedAndUnenclosedFields(fileName, ',', 1, "\"", ",");

            inconsistentCdrs = new List<cdrinconsistent>();
            List<string[]> decodedRows = new List<string[]>();
            //this.Input = input;
            List<cdrfieldmappingbyswitchtype> fieldMappings = null;

            foreach (string[] lineAsArr in lines)

            {
                //string chargingStatus = lineAsArr[3] == "S" ? "1" : "0"; //done
                //if (chargingStatus != "1") continue;
                string[] textCdr = new string[input.MefDecodersData.Totalfieldtelcobright];


                string startTime = lineAsArr[11].Trim();
                if (!string.IsNullOrEmpty(startTime))
                {
                    startTime = parseStringToDate(startTime).ToString("yyyy-MM-dd HH:mm:ss");
                }

                string endTime = lineAsArr[11].Trim(); 
                if (!string.IsNullOrEmpty(endTime))
                {
                    endTime = parseStringToDate(endTime).ToString("yyyy-MM-dd HH:mm:ss");
                }




                textCdr[Fn.Filename] = fileName;
                textCdr[Fn.Switchid] = Input.Ne.idSwitch.ToString();

                textCdr[Fn.OriginatingCallingNumber] = lineAsArr[9].Trim();
                textCdr[Fn.OriginatingCalledNumber] = lineAsArr[10].Trim();

                textCdr[Fn.TerminatingCallingNumber] = lineAsArr[9].Trim();
                textCdr[Fn.TerminatingCalledNumber] = lineAsArr[10].Trim();

                textCdr[Fn.DurationSec] = lineAsArr[53].Trim();

                textCdr[Fn.IncomingRoute] = lineAsArr[66].Trim();
                textCdr[Fn.OutgoingRoute] = lineAsArr[67].Trim();

                textCdr[Fn.Originatingip] = lineAsArr[13].Trim();
                textCdr[Fn.TerminatingIp] = lineAsArr[14].Trim();

                textCdr[Fn.StartTime] = startTime.Trim();
                textCdr[Fn.Endtime] = endTime.Trim();

                

                textCdr[Fn.Validflag] = "1";

                

                decodedRows.Add(textCdr);
            }

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