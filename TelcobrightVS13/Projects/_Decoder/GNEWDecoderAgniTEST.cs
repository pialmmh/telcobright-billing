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
    public class GNEWDecoderAgniTEST : AbstractCdrDecoder
    {
        public override string ToString() => this.RuleName;
        public override string RuleName => GetType().Name;
        public override int Id => 58;
        public override string HelpText => "GNEW Decoder Agni TEST";
        public override CompressionType CompressionType { get; set; }
        public override string UniqueEventTablePrefix { get; }
        public override string PartialTableStorageEngine { get; }
        public override string partialTablePartitionColName { get; }
        protected CdrCollectorInputData Input { get; set; }


        private static DateTime parseStringToDate(string timestamp)  //20181028051316400 yyyyMMddhhmmssfff
        {
            DateTime dateTime;
            if (DateTime.TryParseExact(timestamp, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None,
                out dateTime)) ;

                
            return dateTime;
        }

        public override List<string[]> DecodeFile(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs)
        {
            this.Input = input;
            string fileName = this.Input.FullPath;
            //List<string[]> lines = FileUtil.ParseCsvWithEnclosedAndUnenclosedFields(fileName, ',', 1, "\"", ";");
            List<string[]> lines = FileUtil.ParseCsvWithEnclosedAndUnenclosedFields(fileName, ',', 0, "\"", ",",true);

            inconsistentCdrs = new List<cdrinconsistent>();
            List<string[]> decodedRows = new List<string[]>();
            //this.Input = input;
            List<cdrfieldmappingbyswitchtype> fieldMappings = null;

            try
            {
                foreach (string[] lineAsArr in lines)

                {
                
                    //string chargingStatus = lineAsArr[3] == "S" ? "1" : "0"; //done
                    //if (chargingStatus != "1") continue;
                    string[] textCdr = new string[input.MefDecodersData.Totalfieldtelcobright];



                    textCdr[Fn.Sequencenumber] = "1";
                    //string startTime = lineAsArr[11].Trim().Split('/')[1];
                    string startTime = lineAsArr[11].Trim();
                    if (!string.IsNullOrEmpty(startTime))
                    {
                        startTime = parseStringToDate(startTime).ToString("yyyy-MM-dd HH:mm:ss");
                    }

                    //string endTime = lineAsArr[12].Trim().Split('/')[1];
                    string endTime = lineAsArr[12].Trim();
                    if (!string.IsNullOrEmpty(endTime))
                    {
                        endTime = parseStringToDate(endTime).ToString("yyyy-MM-dd HH:mm:ss");
                    }




                    textCdr[Fn.Filename] = fileName;
                    textCdr[Fn.Switchid] = Input.Ne.idSwitch.ToString();

                    if (lineAsArr[9].Any(t => t == '/') == true)
                    {
                        string tempstr = lineAsArr[9].Split('/')[1];
                        textCdr[Fn.OriginatingCallingNumber] = tempstr.Trim();
                        textCdr[Fn.TerminatingCallingNumber] = tempstr.Trim();

                    }
                    else
                    {
                        textCdr[Fn.OriginatingCallingNumber] = lineAsArr[9].Trim();
                        textCdr[Fn.TerminatingCallingNumber] = lineAsArr[9].Trim();
                    }


                    if (lineAsArr[10].Any(t => t == '/') == true)
                    {
                        string tempstr = lineAsArr[10].Split('/')[1];
                        textCdr[Fn.OriginatingCalledNumber] = tempstr.Trim();
                        textCdr[Fn.TerminatingCalledNumber] = tempstr.Trim();

                    }
                    else
                    {
                        textCdr[Fn.OriginatingCalledNumber] = lineAsArr[10].Trim();
                        textCdr[Fn.TerminatingCalledNumber] = lineAsArr[10].Trim();
                    }
                



                    if (lineAsArr[66].Any(t => t == '/')==true)
                    {
                        string tempstr = lineAsArr[66].Split('/')[1];
                        textCdr[Fn.DurationSec] = tempstr.Trim();

                    }
                    else
                    {
                        textCdr[Fn.DurationSec] = lineAsArr[66].Trim();
                    }



                    if (lineAsArr[88].Any(t => t == '/') == true)
                    {
                        string tempstr = lineAsArr[88].Split('/')[1];
                        textCdr[Fn.IncomingRoute] = tempstr.Trim();

                    }
                    else
                    {
                        textCdr[Fn.IncomingRoute] = lineAsArr[88].Trim();
                    }



                    if (lineAsArr[89].Any(t => t == '/') == true)
                    {
                        string tempstr = lineAsArr[89].Split('/')[1];
                        textCdr[Fn.OutgoingRoute] = tempstr.Trim();

                    }
                    else
                    {
                        textCdr[Fn.OutgoingRoute] = lineAsArr[89].Trim();
                    }
                
                

                    textCdr[Fn.StartTime] = startTime;
                    textCdr[Fn.AnswerTime] = startTime;
                    textCdr[Fn.Endtime] = endTime.Trim();

                

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