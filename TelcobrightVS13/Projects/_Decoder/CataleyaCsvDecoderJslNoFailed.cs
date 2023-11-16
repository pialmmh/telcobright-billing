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
    public class CataleyaCsvDecoderJslNoFailed : AbstractCdrDecoder
    {
        public override string ToString() => this.RuleName;
        public override string RuleName => GetType().Name;
        public override int Id => 30;
        public override string HelpText => "Decodes Cataleya CSV CDR. SR Telecom format, no failed calls";
        public override CompressionType CompressionType { get; set; }
        public override string UniqueEventTablePrefix { get; }
        public override string PartialTableStorageEngine { get; }
        public override string partialTablePartitionColName { get; }
        protected CdrCollectorInputData Input { get; set; }


        private static DateTime parseStringToDate(string timestamp)  //20181028051316400 yyyyMMddhhmmssfff
        {
            DateTime dateTime = DateTime.ParseExact(timestamp, "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
            return dateTime;
        }

        public override List<string[]> DecodeFile(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs)
        {
            this.Input = input;
            string fileName = this.Input.FullPath; ;
            List<string[]> lines = FileUtil.ParseCsvWithEnclosedAndUnenclosedFields(fileName, ',', 1, "\"", ";");
            inconsistentCdrs = new List<cdrinconsistent>();
            List<string[]> decodedRows = new List<string[]>();
            //this.Input = input;
            List<cdrfieldmappingbyswitchtype> fieldMappings = null;

            foreach (string[] lineAsArr in lines)
            {
                string chargingStatus = lineAsArr[2] == "S" ? "1" : "0";
                if (chargingStatus != "1") continue;
                string[] textCdr = new string [input.MefDecodersData.Totalfieldtelcobright];
                textCdr[Fn.ChargingStatus] = chargingStatus;

                textCdr[Fn.Switchid] = Input.Ne.idSwitch.ToString();
                //cdr.SwitchId = 9;
                textCdr[Fn.Sequencenumber] = lineAsArr[0];
                //cdr.SequenceNumber = Convert.ToInt64(lineAsArr[0]);
                textCdr[Fn.Filename] = fileName;
                textCdr[Fn.IncomingRoute] = lineAsArr[24].Trim();
                textCdr[Fn.OutgoingRoute] = lineAsArr[55].Trim();
                textCdr[Fn.DurationSec] = lineAsArr[17];
                //cdr.DurationSec = Convert.ToDecimal(lineAsArr[17]) / 1000;
                string ipAddr= lineAsArr[36];
                if (!string.IsNullOrEmpty(ipAddr))
                {
                    string[] ipPort = ipAddr.Split(':');
                    string ip = ipPort[1].Trim();
                    string port = ipPort[2].Split(';')[0].Trim();
                    textCdr[Fn.Originatingip] = ip + ":"+ port;
                }
                ipAddr = lineAsArr[67];
                if (!string.IsNullOrEmpty(ipAddr))
                {
                    string[] ipPort = ipAddr.Split(':');
                    string ip = ipPort[1].Trim();
                    string port = ipPort[2].Split(';')[0].Trim();
                    textCdr[Fn.TerminatingIp] = ip + ":" + port;
                }
                
                string startTime = lineAsArr[37];//SignalStart
                if (!string.IsNullOrEmpty(startTime))
                {
                    startTime= parseStringToDate(startTime).ToString("yyyy-MM-dd HH:mm:ss");
                }

                string connectTime = lineAsArr[38];//ConnectTime
                if (!string.IsNullOrEmpty(connectTime))
                {
                    connectTime= parseStringToDate(connectTime).ToString("yyyy-MM-dd HH:mm:ss");
                }

                string answerTime = lineAsArr[39];//AnswerTime
                if (!string.IsNullOrEmpty(answerTime))
                {
                    answerTime= parseStringToDate(answerTime).ToString("yyyy-MM-dd HH:mm:ss");
                }

                string endTime = lineAsArr[40];//EndTime
                if (!string.IsNullOrEmpty(endTime))
                {
                    endTime= parseStringToDate(endTime).ToString("yyyy-MM-dd HH:mm:ss");
                }

                textCdr[Fn.StartTime] = startTime;
                textCdr[Fn.ConnectTime] = connectTime;
                textCdr[Fn.AnswerTime] = answerTime;
                textCdr[Fn.Endtime] = endTime;

                textCdr[Fn.OriginatingCallingNumber] = lineAsArr[30].Trim();
                textCdr[Fn.OriginatingCalledNumber] = lineAsArr[31].Trim();
                textCdr[Fn.TerminatingCallingNumber] = lineAsArr[61].Trim();
                textCdr[Fn.TerminatingCalledNumber] = lineAsArr[62].Trim();
                textCdr[Fn.ReleaseDirection] = lineAsArr[8].Trim();
                textCdr[Fn.ReleaseCauseIngress] = lineAsArr[9].Trim();
                textCdr[Fn.ReleaseCauseEgress] = lineAsArr[9].Trim();
                textCdr[Fn.ReleaseCauseSystem] = lineAsArr[10].Trim();
                //textCdr[Fn.UniqueBillId] = lineAsArr[10].Trim();
                textCdr[Fn.Validflag] = "1";
                decodedRows.Add(textCdr);
            }

            return decodedRows;
            
        }

        public override string getTupleExpression(Object data)
        {
            throw new NotImplementedException();
        }

        public override DateTime getEventDatetime(Object data)
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
    }
}
