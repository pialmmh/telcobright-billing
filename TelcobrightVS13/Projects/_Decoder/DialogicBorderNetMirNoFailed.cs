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
using System.Text;
using iTextSharp.awt.geom;
using TelcobrightMediation.Config;

namespace Decoders
{

    [Export("Decoder", typeof(AbstractCdrDecoder))]
    public class DialogicBorderNetMirNoFailed : AbstractCdrDecoder
    {
        public override string ToString() => this.RuleName;
        public override string RuleName => GetType().Name;
        public override int Id => 26;
        public override string HelpText => "Decodes Dialogic BorderNet CSV CDR (Mir Telecom)";
        public override CompressionType CompressionType { get; set; }
        public override string PartialTableStorageEngine { get; } = "innodb";
        public override string partialTablePartitionColName { get; } = "starttime";
        protected virtual CdrCollectorInputData Input { get; set; }
               
        private static string parseStringToDateWithoutMilliSec(string timestamp)  //20181028051316400 yyyyMMddhhmmssfff
        {
            string noMillis = timestamp.Split('.')[0];
            return string.Join(" ", noMillis.Split('+').Select(s => s.Trim()));
        }

        public override List<string[]> DecodeFile(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs)
        {
            this.Input = input;
            string fileName = this.Input.FullPath;
            List<string[]> lines = FileUtil.ParseCsvWithEnclosedAndUnenclosedFields(fileName, ',', 1, "\"", ";");
            return decodeLines(input, out inconsistentCdrs, fileName, lines);
        }


        protected static List<string[]> decodeLines(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs, string fileName, List<string[]> lines)
        {
            inconsistentCdrs = new List<cdrinconsistent>();
            List<string[]> decodedRows = new List<string[]>();
            //this.Input = input;
            List<cdrfieldmappingbyswitchtype> fieldMappings = null;

            foreach (string[] lineAsArr in lines)
            {
                //take only final cdrs 
                //AccountStatusType = field 7
                //AccountEventReason = field 8
                //SDRSessionStatus = field 16

                string accountStatusType = lineAsArr[6].Trim();// AccountStatusType = field 7, we keep it in calledPartyNoa 
                string accountEventReason =lineAsArr[7].Trim(); //AccountEventReason = field 8, we keep it in callingPartyNoa

                string chargingStatus = lineAsArr[15];//SDRSessionStatus = field 16
                string durationSec = lineAsArr[14]; // 
                double duration = 0;
                double.TryParse(durationSec, out duration);
                if (accountStatusType != "2" || duration<=0)
                {
                    continue;
                }

                string[] textCdr = new string[input.MefDecodersData.Totalfieldtelcobright];
                textCdr[Fn.ChargingStatus] = chargingStatus == "1" ? "1" : "0";
                textCdr[Fn.CalledpartyNOA] = accountStatusType;
                textCdr[Fn.CallingPartyNOA] = accountEventReason;

                textCdr[Fn.Sequencenumber] = lineAsArr[1];
                textCdr[Fn.UniqueBillId] = lineAsArr[10];
                textCdr[Fn.DurationSec] = durationSec.IsNullOrEmptyOrWhiteSpace() == false
                    ? durationSec : string.Empty;

                string ingressSigRemoteAddress = lineAsArr[69];//IPv4 192.168.130.63:5060
                string ipAndPort = ingressSigRemoteAddress.Replace("IPv4 ", "");
                string ingressRequestLine = lineAsArr[71]; //"sip:00918860086409@192.168.130.63:5060";
                if (ingressRequestLine.IsNullOrEmptyOrWhiteSpace() == false)
                {
                    string[] tempArr = ingressRequestLine.Split(':');
                    string originatingCalledNumber = tempArr[1].Split('@')[0];
                    var originatingIp = ipAndPort;
                    textCdr[Fn.Originatingip] = originatingIp;
                    //use media ip1 as own signaling ip
                    string ingressSigLocalAddress = lineAsArr[70].Split(null)[1];
                    textCdr[Fn.Mediaip1] = ingressSigLocalAddress;
                    textCdr[Fn.IncomingRoute] = new StringBuilder(originatingIp).Append('-').Append(ingressSigLocalAddress)
                        .ToString();

                    textCdr[Fn.OriginatingCalledNumber] = originatingCalledNumber.Replace("+","");
                }

                string ingressSipFromHeader = lineAsArr[72]; //"From: <sip:1111111@192.168.130.63>;tag=5228fc25a2c34aa7ba35e565aeda1457";
                if (ingressSipFromHeader.IsNullOrEmptyOrWhiteSpace() == false)
                {
                    string originatingCallingNumber = ingressSipFromHeader.Replace(" ", string.Empty).Split(':')[2]
                        .Split('@')[0];
                    originatingCallingNumber = originatingCallingNumber.Split('>')[0].Trim();
                    textCdr[Fn.OriginatingCallingNumber] = originatingCallingNumber.Trim();
                }

                string outSigReqLine = lineAsArr[82].Replace(" ",""); //sip: 00918860086409@10.10.234.8:5060; transport = UDP
                if (outSigReqLine.IsNullOrEmptyOrWhiteSpace() == false)
                {
                    string[] tempArr = outSigReqLine.Split(':');
                    string[] calledNoAndIp = (new StringBuilder(tempArr[1]).Append(":").Append(tempArr[2])).ToString()
                        .Split('@').Select(s => s.Trim()).ToArray();
                    var terminatingCalledNumber = calledNoAndIp[0];
                    var terminatingIp = calledNoAndIp[1].Split(';')[0];
                    //media ip 2 as egress sig remote address OutSigLocalAddr v
                    string outSigLocalAddr = lineAsArr[80].Split(null)[1];
                    textCdr[Fn.Mediaip2] = outSigLocalAddr;
                    textCdr[Fn.OutgoingRoute] = new StringBuilder(terminatingIp).Append("-").Append(outSigLocalAddr)
                        .ToString();
                    textCdr[Fn.TerminatingIp] = terminatingIp;
                    textCdr[Fn.TerminatingCalledNumber] = terminatingCalledNumber.Trim();
                }

                string outSigFrom = lineAsArr[83];//From: <sip:1111111@192.168.130.63>;tag=5228fc25a2c34aa7ba35e565aeda1457
                if (outSigFrom.IsNullOrEmptyOrWhiteSpace() == false)
                {
                    string terminatingCallingNumber = outSigFrom.Split(':')[2].Split('@')[0];
                    terminatingCallingNumber = terminatingCallingNumber.Split('>')[0].Trim();
                    textCdr[Fn.TerminatingCallingNumber] = terminatingCallingNumber.Trim();
                }

                
                //textCdr[Fn.Mediaip1] = lineAsArr[71];
                //textCdr[Fn.Mediaip2] = lineAsArr[82];
                //cdr.MediaIp1 = lineAsArr[71];
                //cdr.MediaIp2 = lineAsArr[82];

                //string dt = lineAsArr[103];//SignalStart
                ////if (!string.IsNullOrEmpty(dt)) cdr.SignalingStartTime = parseStringToDateWithoutMilliSec(dt);

                string dt = lineAsArr[102];//SignalStart
                if (!string.IsNullOrEmpty(dt)) textCdr[Fn.StartTime] = parseStringToDateWithoutMilliSec(dt);

                //dt = lineAsArr[38];//ConnectTime
                //if (!string.IsNullOrEmpty(dt)) cdr.ConnectTime = parseStringToDateWithoutMilliSec(dt);


                dt = lineAsArr[103];//ConnectTime
                if (!string.IsNullOrEmpty(dt)) textCdr[Fn.ConnectTime] = parseStringToDateWithoutMilliSec(dt);

                //dt = lineAsArr[129];//AnswerTime
                //if (!string.IsNullOrEmpty(dt)) cdr.AnswerTime = parseStringToDateWithoutMilliSec(dt);

                dt = lineAsArr[105];//AnswerTime
                if (!string.IsNullOrEmpty(dt)) textCdr[Fn.AnswerTime] = parseStringToDateWithoutMilliSec(dt);

                //dt = lineAsArr[130];//EndTime
                //if (!string.IsNullOrEmpty(dt)) cdr.EndTime = parseStringToDateWithoutMilliSec(dt);

                dt = lineAsArr[106];//EndTime
                if (!string.IsNullOrEmpty(dt)) textCdr[Fn.Endtime] = parseStringToDateWithoutMilliSec(dt);

                textCdr[Fn.ReleaseCauseIngress] = lineAsArr[110];
                textCdr[Fn.ReleaseCauseEgress] = lineAsArr[133];
                textCdr[Fn.Validflag] = "1";
                textCdr[Fn.Partialflag] = "0";
                decodedRows.Add(textCdr.ToArray());
            }

            return decodedRows;
        }
    }
}

