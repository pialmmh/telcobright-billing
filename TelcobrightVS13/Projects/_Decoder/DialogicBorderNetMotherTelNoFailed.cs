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

    [Export("Decoder", typeof(IFileDecoder))]
    public class DialogicBorderNetMotherTelNoFailed : IFileDecoder
    {
        public override string ToString() => this.RuleName;
        public virtual string RuleName => GetType().Name;
        public virtual int Id => 33;
        public virtual string HelpText => "Decodes Dialogic BorderNet CSV CDR (Mother Telecom)";
        public virtual CompressionType CompressionType { get; set; }
        public string PartialTablePrefix { get; }
        public string PartialTableStorageEngine { get; }
        public string partialTablePartitionColName { get; }
        protected virtual CdrCollectorInputData Input { get; set; }
               
        private static string parseStringToDateWithoutMilliSec(string timestamp)  //20181028051316400 yyyyMMddhhmmssfff
        {
            string noMillis = timestamp.Split('.')[0];
            return string.Join(" ", noMillis.Split('+').Select(s => s.Trim()));
        }

        public virtual List<string[]> DecodeFile(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs)
        {
            this.Input = input;
            string fileName = this.Input.FullPath;
            List<string[]> lines = FileUtil.ParseCsvWithEnclosedAndUnenclosedFields(fileName, ',', 1, "\"", ";");
            return decodeLines(input, out inconsistentCdrs, fileName, lines);

        }

        public string getTupleExpression(CdrCollectorInputData decoderInputData, string[] row)
        {
            CdrSetting cdrSetting = decoderInputData.CdrSetting;
            int switchId = decoderInputData.Ne.idSwitch;
            string startTimeFieldName = "";
            DateTime startTime = getStartTime(cdrSetting, row,out startTimeFieldName);
            string sessionId = getSessionId(row);
            string separator = "/";
            return new StringBuilder(switchId.ToString()).Append(separator)
                .Append(startTime.ToMySqlFormatWithoutQuote()).Append(separator)
                .Append(sessionId).ToString();
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
            Dictionary<string, object> dataAsDic = (Dictionary<string, object>)data;
            CdrSetting cdrSetting = (CdrSetting)dataAsDic["cdrSetting"];
            string[] row = (string[])dataAsDic["row"];
            int timeFieldNo = EventDateTimeHelper.getTimeFieldNo(cdrSetting, row);
            DateTime dateTime = row[timeFieldNo].ConvertToDateTimeFromMySqlFormat();
            return dateTime;
        }

       

        private static string getSessionId(string[] row)
        {
            string sessionId = row[Fn.UniqueBillId];
            long sessionIdNum = 0;
            if (sessionId.IsNullOrEmptyOrWhiteSpace() || Int64.TryParse(sessionId, out sessionIdNum) == false)
            {
                throw new Exception("UniquebillId is not in correct format.");
            }
            return sessionId;
        }

        private static DateTime getStartTime(CdrSetting cdrSettings, string[] row,out string timeFieldName)
        {
            DateTime startTime;
            switch (cdrSettings.SummaryTimeField)
            {
                case SummaryTimeFieldEnum.StartTime:
                    startTime = row[Fn.StartTime].ConvertToDateTimeFromMySqlFormat();
                    timeFieldName = "starttime";
                    break;
                case SummaryTimeFieldEnum.AnswerTime:
                    startTime = row[Fn.AnswerTime].ConvertToDateTimeFromMySqlFormat();
                    timeFieldName = "answertime";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return startTime;
        }

        private static string getTimeFieldName(CdrSetting cdrSettings)
        {
            return cdrSettings.SummaryTimeField == SummaryTimeFieldEnum.AnswerTime
                ? "answertime"
                : "starttime";
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

                string accountStatusType = lineAsArr[86].Trim();// AccountStatusType = field 7, we keep it in calledPartyNoa 
                string accountEventReason = lineAsArr[87].Trim();//AccountEventReason = field 8, we keep it in callingPartyNoa

                string chargingStatus = lineAsArr[93];//SDRSessionStatus = field 16
                string durationSec = lineAsArr[13];
                double duration = 0;
                double.TryParse(durationSec, out duration);
                if (accountStatusType != "2" || duration <= 0)
                {
                    continue;
                }

                string[] textCdr = new string[input.MefDecodersData.Totalfieldtelcobright];
                textCdr[Fn.ChargingStatus] = chargingStatus == "1" ? "1" : "0";
                textCdr[Fn.CalledpartyNOA] = accountStatusType;
                textCdr[Fn.CallingPartyNOA] = accountEventReason;

                textCdr[Fn.Sequencenumber] = lineAsArr[4];
                textCdr[Fn.UniqueBillId] = lineAsArr[5];
                textCdr[Fn.DurationSec] = durationSec.IsNullOrEmptyOrWhiteSpace() == false
                    ? durationSec : string.Empty;


                string ingressSigRemoteAddress = lineAsArr[16];//IPv4 192.168.130.63:5060
                string ipAndPort = ingressSigRemoteAddress.Replace("IPv4 ", "");
                string ingressRequestLine = lineAsArr[57]; //"sip:00918860086409@192.168.130.63:5060";
                if (ingressRequestLine.IsNullOrEmptyOrWhiteSpace() == false)
                {
                    string[] tempArr = ingressRequestLine.Split(':');
                    string originatingCalledNumber = tempArr[1].Split('@')[0];
                    var originatingIp = ipAndPort;
                    textCdr[Fn.Originatingip] = originatingIp;
                    //use media ip1 as own signaling ip
                    string ingressSigLocalAddress = lineAsArr[17].Split(null)[1];
                    textCdr[Fn.Mediaip1] = ingressSigLocalAddress;
                    textCdr[Fn.IncomingRoute] = new StringBuilder(originatingIp).Append('-').Append(ingressSigLocalAddress)
                        .ToString();
                    textCdr[Fn.OriginatingCalledNumber] = originatingCalledNumber.Trim();
                }

                string ingressSipFromHeader = lineAsArr[58]; //"From: <sip:1111111@192.168.130.63>;tag=5228fc25a2c34aa7ba35e565aeda1457";
                if (ingressSipFromHeader.IsNullOrEmptyOrWhiteSpace() == false)
                {
                    string originatingCallingNumber = ingressSipFromHeader.Replace(" ", string.Empty).Split(':')[2]
                        .Split('@')[0];
                    originatingCallingNumber = originatingCallingNumber.Split('>')[0].Trim();
                    textCdr[Fn.OriginatingCallingNumber] = originatingCallingNumber.Trim();
                }

                string outSigReqLine = lineAsArr[63].Replace(" ",""); //sip: 00918860086409@10.10.234.8:5060; transport = UDP
                if (outSigReqLine.IsNullOrEmptyOrWhiteSpace() == false)
                {
                    string[] tempArr = outSigReqLine.Split(':');
                    string[] calledNoAndIp = (new StringBuilder(tempArr[1]).Append(":").Append(tempArr[2])).ToString()
                        .Split('@').Select(s => s.Trim()).ToArray();
                    var terminatingCalledNumber = calledNoAndIp[0];
                    var terminatingIp = calledNoAndIp[1].Split(';')[0];
                    //media ip 2 as egress sig remote address OutSigLocalAddr v
                    string outSigLocalAddr = lineAsArr[18].Split(null)[1];
                    textCdr[Fn.Mediaip2] = outSigLocalAddr;
                    textCdr[Fn.OutgoingRoute] = new StringBuilder(terminatingIp).Append("-").Append(outSigLocalAddr)
                        .ToString();
                    textCdr[Fn.TerminatingIp] = terminatingIp;
                    textCdr[Fn.TerminatingCalledNumber] = terminatingCalledNumber.Trim();
                }

                string outSigFrom = lineAsArr[64];//From: <sip:1111111@192.168.130.63>;tag=5228fc25a2c34aa7ba35e565aeda1457
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

                string dt = lineAsArr[34];//SignalStart
                if (!string.IsNullOrEmpty(dt)) textCdr[Fn.StartTime] = parseStringToDateWithoutMilliSec(dt);

                //dt = lineAsArr[38];//ConnectTime
                //if (!string.IsNullOrEmpty(dt)) cdr.ConnectTime = parseStringToDateWithoutMilliSec(dt);


                dt = lineAsArr[35];//ConnectTime
                if (!string.IsNullOrEmpty(dt)) textCdr[Fn.ConnectTime] = parseStringToDateWithoutMilliSec(dt);

                //dt = lineAsArr[129];//AnswerTime
                //if (!string.IsNullOrEmpty(dt)) cdr.AnswerTime = parseStringToDateWithoutMilliSec(dt);

                dt = lineAsArr[41];//AnswerTime
                if (!string.IsNullOrEmpty(dt)) textCdr[Fn.AnswerTime] = parseStringToDateWithoutMilliSec(dt);

                //dt = lineAsArr[130];//EndTime
                //if (!string.IsNullOrEmpty(dt)) cdr.EndTime = parseStringToDateWithoutMilliSec(dt);

                string endTimeStr="";
                if (lineAsArr[42].IsNullOrEmptyOrWhiteSpace() == false)
                {
                    endTimeStr = lineAsArr[42];
                }
                else if (lineAsArr[43].IsNullOrEmptyOrWhiteSpace() == false)
                {
                    endTimeStr = lineAsArr[43];
                }
                else if (lineAsArr[12].IsNullOrEmptyOrWhiteSpace() == false)
                {
                    endTimeStr = lineAsArr[12];
                }
                else if (lineAsArr[37].IsNullOrEmptyOrWhiteSpace() == false)
                {
                    endTimeStr = lineAsArr[37];
                }
                if (!string.IsNullOrEmpty(dt)) textCdr[Fn.Endtime] = parseStringToDateWithoutMilliSec(endTimeStr);

                textCdr[Fn.ReleaseCauseIngress] = lineAsArr[47];
                textCdr[Fn.ReleaseCauseEgress] = lineAsArr[48];
                textCdr[Fn.Validflag] = "1";
                decodedRows.Add(textCdr.ToArray());
            }

            return decodedRows;
        }
    }
}

