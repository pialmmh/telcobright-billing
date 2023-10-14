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
    public class CataleyaCsvDecoderGaziNoFailed : AbstractCdrDecoder
    {
        public override string ToString() => this.RuleName;
        public override string RuleName => GetType().Name;
        public override int Id => 32;
        public override string HelpText => "Decodes Cataleya CSV CDR. Gazi format, no failed calls";
        public override CompressionType CompressionType { get; set; }
        public override string PartialTablePrefix { get; }
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
            string fileName = this.Input.FullPath;
            string str = File.ReadAllText(fileName);
            List<string[]> lines = FileUtil.ParseCsvWithEnclosedAndUnenclosedFields(fileName, ',', 1, "\"", ";");
            inconsistentCdrs = new List<cdrinconsistent>();
            List<string[]> decodedRows = new List<string[]>();
            //this.Input = input;
            List<cdrfieldmappingbyswitchtype> fieldMappings = null;

            foreach (string[] lineAsArr in lines)
            {
                string chargingStatus = lineAsArr[3] == "S" ? "1" : "0"; //done
                if (chargingStatus != "1") continue;
                string[] textCdr = new string[input.MefDecodersData.Totalfieldtelcobright];
                textCdr[Fn.ChargingStatus] = chargingStatus; //done

                textCdr[Fn.Switchid] = Input.Ne.idSwitch.ToString();//done
                //cdr.SwitchId = 9;
                textCdr[Fn.Sequencenumber] = lineAsArr[1];//sequence num
                //cdr.SequenceNumber = Convert.ToInt64(lineAsArr[0]);
                textCdr[Fn.Filename] = fileName;//done
                textCdr[Fn.IncomingRoute] = lineAsArr[16];//ingress_call_info_zone_name --done
                textCdr[Fn.OutgoingRoute] = lineAsArr[31];//egress_call_info_inviting_ts --done
                textCdr[Fn.DurationSec] = lineAsArr[5];//duration --done
                //cdr.DurationSec = Convert.ToDecimal(lineAsArr[17]) / 1000;
                string ipAddr = lineAsArr[23];//7 ingress_call_info_sip_remote_address--done
                if (!string.IsNullOrEmpty(ipAddr))
                {
                    string[] ipPort = ipAddr.Split(':');
                    string ip = ipPort[1].Trim();
                    string port = ipPort[2].Split(';')[0].Trim();
                    textCdr[Fn.Originatingip] = ip + ":" + port;
                }
                ipAddr = lineAsArr[37];//egress_call_info_sip_remote_address-done

                if (!string.IsNullOrEmpty(ipAddr))
                {
                    string[] ipPort = ipAddr.Split(':');
                    string ip = ipPort[1].Trim();
                    string port = ipPort[2].Split(';')[0].Trim();
                    textCdr[Fn.TerminatingIp] = ip + ":" + port;
                }

                string startTime = lineAsArr[24];//ingress_call_info_inviting_ts --done
                if (!string.IsNullOrEmpty(startTime))
                {
                    startTime = parseStringToDate(startTime).ToString("yyyy-MM-dd HH:mm:ss");
                }

                string connectTime = lineAsArr[24];//ingress_call_info_inviting_ts-- done
                if (!string.IsNullOrEmpty(connectTime))
                {
                    connectTime = parseStringToDate(connectTime).ToString("yyyy-MM-dd HH:mm:ss");
                }

                string answerTime = lineAsArr[27];//ingress_call_info_answer_ts -done
                if (!string.IsNullOrEmpty(answerTime))
                {
                    answerTime = parseStringToDate(answerTime).ToString("yyyy-MM-dd HH:mm:ss");
                }

                string endTime = lineAsArr[28];//ingress_call_info_disconnect_ ts--done
                if (!string.IsNullOrEmpty(endTime))
                {
                    endTime = parseStringToDate(endTime).ToString("yyyy-MM-dd HH:mm:ss");
                }

                textCdr[Fn.StartTime] = startTime;
                //textCdr[Fn.ConnectTime] = connectTime;
                textCdr[Fn.AnswerTime] = answerTime;
                textCdr[Fn.Endtime] = endTime;

                textCdr[Fn.OriginatingCallingNumber] = lineAsArr[19].Trim();//ingress_call_info_calling_part y--done
                textCdr[Fn.OriginatingCalledNumber] = lineAsArr[20].Trim();//ingress_call_info_called_part          --done       y

                textCdr[Fn.TerminatingCallingNumber] = lineAsArr[33].Trim();//ingress_media_record_flow_c ommit_ts--done
                textCdr[Fn.TerminatingCalledNumber] = lineAsArr[34].Trim();//ingress_media_record_media_intf_name--done

                textCdr[Fn.ReleaseDirection] = lineAsArr[6].Trim();//release_direction --done
                textCdr[Fn.ReleaseCauseIngress] = lineAsArr[7].Trim();//sip_status_code --done
                textCdr[Fn.ReleaseCauseEgress] = lineAsArr[7].Trim();// --done
                textCdr[Fn.ReleaseCauseSystem] = lineAsArr[8].Trim();//internal_reason --done
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