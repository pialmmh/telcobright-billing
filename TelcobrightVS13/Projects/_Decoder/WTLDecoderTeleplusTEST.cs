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


        private static DateTime parseStringToDate(string timestamp)  //20181028051316400 yyyyMMddhhmmssfff
        {
            DateTime dateTime = DateTime.ParseExact(timestamp, "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
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

                textCdr[Fn.Filename] = fileName;
                textCdr[Fn.Switchid] = Input.Ne.idSwitch.ToString();

                textCdr[Fn.OriginatingCallingNumber] = lineAsArr[66];
                textCdr[Fn.OriginatingCalledNumber] = lineAsArr[68];

                textCdr[Fn.TerminatingCallingNumber] = lineAsArr[82];
                textCdr[Fn.TerminatingCalledNumber] = lineAsArr[84];

                textCdr[Fn.DurationSec] = lineAsArr[60];




                textCdr[Fn.Validflag] = "1";





                decodedRows.Add(textCdr);
            }

            return decodedRows;

        }

        public string getTupleExpression(CdrCollectorInputData decoderInputData, string[] row)
        {
            throw new NotImplementedException();
        }

        public string getSqlWhereClauseForHourWiseSafeCollection(CdrCollectorInputData decoderInputData,DateTime hourOfDay, int minusHoursForSafeCollection, int plusHoursForSafeCollection)
        {
            throw new NotImplementedException();
        }

        public string getCreateTableSqlForUniqueEvent(CdrCollectorInputData decoderInputData)
        {
            throw new NotImplementedException();
        }

        public string getDuplicateCollectionSql(CdrCollectorInputData decoderInputData, DateTime hourOfTheDay, List<string> tuples)
        {
            throw new NotImplementedException();
        }

        public string getPartialCollectionSql(CdrCollectorInputData decoderInputData, DateTime hourOfTheDay, List<string> tuples)
        {
            throw new NotImplementedException();
        }
    }
}