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
    public class CataleyaCsvDecoderSRNoFailed : IFileDecoder
    {
        public override string ToString() => this.RuleName;
        public virtual string RuleName => GetType().Name;
        public int Id => 25;
        public string HelpText => "Decodes Cataleya CSV CDR. SR Telecom format, no failed calls";
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
                textCdr[Fn.Sequencenumber] = lineAsArr[0];//acc_rec_num
                //cdr.SequenceNumber = Convert.ToInt64(lineAsArr[0]);
                textCdr[Fn.Filename] = fileName;
                textCdr[Fn.IncomingRoute] = lineAsArr[25];//ingress_call_info_zone_name
                textCdr[Fn.OutgoingRoute] = lineAsArr[56];//egress_call_info_inviting_ts
                textCdr[Fn.DurationSec] = lineAsArr[17];//duration 
                //cdr.DurationSec = Convert.ToDecimal(lineAsArr[17]) / 1000;
                string ipAddr= lineAsArr[36];//7 ingress_call_info_sip_remote_address
                if (!string.IsNullOrEmpty(ipAddr))
                {
                    string[] ipPort = ipAddr.Split(':');
                    string ip = ipPort[1].Trim();
                    string port = ipPort[2].Split(';')[0].Trim();
                    textCdr[Fn.Originatingip] = ip + ":"+ port;
                }
                ipAddr = lineAsArr[67];//ingress_media_record_remot e_address

                if (!string.IsNullOrEmpty(ipAddr))
                {
                    string[] ipPort = ipAddr.Split(':');
                    string ip = ipPort[1].Trim();
                    string port = ipPort[2].Split(';')[0].Trim();
                    textCdr[Fn.TerminatingIp] = ip + ":" + port;
                }
                
                string startTime = lineAsArr[37];//ingress_call_info_inviting_ts
                if (!string.IsNullOrEmpty(startTime))
                {
                    startTime= parseStringToDate(startTime).ToString("yyyy-MM-dd HH:mm:ss");
                }

                string connectTime = lineAsArr[38];//ingress_call_info_inviting_ts
                if (!string.IsNullOrEmpty(connectTime))
                {
                    connectTime= parseStringToDate(connectTime).ToString("yyyy-MM-dd HH:mm:ss");
                }

                string answerTime = lineAsArr[39];//ingress_call_info_answer_ts 
                if (!string.IsNullOrEmpty(answerTime))
                {
                    answerTime= parseStringToDate(answerTime).ToString("yyyy-MM-dd HH:mm:ss");
                }

                string endTime = lineAsArr[40];//ingress_call_info_disconnect_ ts
                if (!string.IsNullOrEmpty(endTime))
                {
                    endTime= parseStringToDate(endTime).ToString("yyyy-MM-dd HH:mm:ss");
                }

                textCdr[Fn.StartTime] = startTime;
                textCdr[Fn.ConnectTime] = connectTime;
                textCdr[Fn.AnswerTime] = answerTime;
                textCdr[Fn.Endtime] = endTime;

                textCdr[Fn.OriginatingCallingNumber] = lineAsArr[30].Trim();//ingress_call_info_calling_part y
                textCdr[Fn.OriginatingCalledNumber] = lineAsArr[31].Trim();//ingress_call_info_called_part                 y

                textCdr[Fn.TerminatingCallingNumber] = lineAsArr[61].Trim();//ingress_media_record_flow_c ommit_ts
                textCdr[Fn.TerminatingCalledNumber] = lineAsArr[62].Trim();//ingress_media_record_media_intf_name

                textCdr[Fn.ReleaseDirection] = lineAsArr[8].Trim();//release_direction 
                textCdr[Fn.ReleaseCauseIngress] = lineAsArr[9].Trim();//sip_status_code 
                textCdr[Fn.ReleaseCauseEgress] = lineAsArr[9].Trim();
                textCdr[Fn.ReleaseCauseSystem] = lineAsArr[10].Trim();//internal_reason
                //textCdr[Fn.UniqueBillId] = lineAsArr[10].Trim();
                textCdr[Fn.Validflag] = "1";
                decodedRows.Add(textCdr);
            }

            return decodedRows;
            
        }

        public string getTupleExpression(CdrCollectorInputData decoderInputData, string[] row)
        {
            throw new NotImplementedException();
        }

        public DateTime getEventDatetime(Object data)
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
    }
}
