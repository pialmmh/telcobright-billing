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
    public class DialogicBorderNet : IFileDecoder
    {
        public override string ToString() => this.RuleName;
        public virtual string RuleName => GetType().Name;
        public virtual int Id => 26;
        public virtual string HelpText => "Decodes Dialogic BorderNet CSV CDR.";
        public virtual CompressionType CompressionType { get; set; } 
        protected virtual CdrCollectorInputData Input { get; set; }
               
        private static DateTime parseStringToDate(string timestamp)  //20181028051316400 yyyyMMddhhmmssfff
        {
            DateTime dateTime = string.Join(" ", timestamp.Split('+').Select(s => s.Trim()))
                .ConvertToDateTimeFromMySqlFormat();
            return dateTime;
        }

        public virtual List<string[]> DecodeFile(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs)
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
                string[] textCdr = new string[input.MefDecodersData.Totalfieldtelcobright];
                textCdr[Fn.Sequencenumber] = lineAsArr[1];

                string durationSec = lineAsArr[7];
                textCdr[Fn.DurationSec] = durationSec.IsNullOrEmptyOrWhiteSpace() == false
                    ? durationSec : string.Empty;

                string ingressRequestLine = lineAsArr[70]; //"sip:00918860086409@192.168.130.63:5060";
                if (ingressRequestLine.IsNullOrEmptyOrWhiteSpace() == false)
                {
                    string[] calledNoAndIp = ingressRequestLine.Split(':')[1].Split('@').Select(s=>s.Trim()).ToArray();
                    var originatingCalledNumber = calledNoAndIp[0];
                    var originatingIp = calledNoAndIp[1];
                    textCdr[Fn.IncomingRoute] = originatingIp;
                    textCdr[Fn.Originatingip] = originatingIp;
                    textCdr[Fn.OriginatingCalledNumber] = originatingCalledNumber;
                }

                string ingressSipFromHeader = lineAsArr[71]; //"From: <sip:1111111@192.168.130.63>;tag=5228fc25a2c34aa7ba35e565aeda1457";
                if (ingressSipFromHeader.IsNullOrEmptyOrWhiteSpace() == false)
                {
                    textCdr[Fn.OriginatingCallingNumber] = ingressSipFromHeader.Replace(" ", string.Empty).Split(':')[2]
                        .Split('@')[0];
                }

                string outSigReqLine = lineAsArr[81]; //sip: 00918860086409@10.10.234.8:5060; transport = UDP
                if (outSigReqLine.IsNullOrEmptyOrWhiteSpace() == false)
                {
                    string[] calledNoAndIp = outSigReqLine.Replace(" ",string.Empty)
                        .Split(':')[1].Split('@').Select(s => s.Trim()).ToArray();
                    var terminatingCalledNumber = calledNoAndIp[0];
                    var terminatingIp = calledNoAndIp[1];
                    textCdr[Fn.OutgoingRoute] = terminatingIp;
                    textCdr[Fn.TerminatingIp] = terminatingIp;
                    textCdr[Fn.TerminatingCalledNumber] = terminatingCalledNumber;
                }
                textCdr[Fn.TerminatingIp] = lineAsArr[81];
                //cdr.OriginatingIP = lineAsArr[70];
                //cdr.TerminatingIP = lineAsArr[81];
                textCdr[Fn.Mediaip1] = lineAsArr[71];
                textCdr[Fn.Mediaip2] = lineAsArr[82];
                //cdr.MediaIp1 = lineAsArr[71];
                //cdr.MediaIp2 = lineAsArr[82];

                //string dt = lineAsArr[103];//SignalStart
                ////if (!string.IsNullOrEmpty(dt)) cdr.SignalingStartTime = parseStringToDate(dt);

                string dt = lineAsArr[103];//SignalStart
                if (!string.IsNullOrEmpty(dt)) textCdr[Fn.SignalingStartTime] = parseStringToDate(dt).ToString();

                //dt = lineAsArr[38];//ConnectTime
                //if (!string.IsNullOrEmpty(dt)) cdr.ConnectTime = parseStringToDate(dt);


                dt = lineAsArr[104];//ConnectTime
                if (!string.IsNullOrEmpty(dt)) textCdr[Fn.ConnectTime] = parseStringToDate(dt).ToString();

                //dt = lineAsArr[129];//AnswerTime
                //if (!string.IsNullOrEmpty(dt)) cdr.AnswerTime = parseStringToDate(dt);

                dt = lineAsArr[129];//AnswerTime
                if (!string.IsNullOrEmpty(dt)) textCdr[Fn.AnswerTime] = parseStringToDate(dt).ToString();

                //dt = lineAsArr[130];//EndTime
                //if (!string.IsNullOrEmpty(dt)) cdr.EndTime = parseStringToDate(dt);

                dt = lineAsArr[130];//EndTime
                if (!string.IsNullOrEmpty(dt)) textCdr[Fn.Endtime] = parseStringToDate(dt).ToString();

                textCdr[Fn.ReleaseCauseIngress] = lineAsArr[109];
                textCdr[Fn.ReleaseCauseEgress] = lineAsArr[78];


                //string phoneNumber = lineAsArr[74]; //OriginCalled 
                //if (!string.IsNullOrEmpty(phoneNumber))
                //{
                //    string Contact = phoneNumber.Split(':')[1].Split('<')[0].Trim();
                //    cdr.OriginatingCalledNumber = Contact;
                //}

                //phoneNumber = lineAsArr[73]; //OriginCalling 
                //if (!string.IsNullOrEmpty(phoneNumber))
                //{
                //    string Contact = phoneNumber.Split(':')[1].Split('<')[0].Trim();
                //    cdr.OriginatingCallingNumber = Contact;
                //}


                //phoneNumber = lineAsArr[85]; //TerminatingCalled 
                //if (!string.IsNullOrEmpty(phoneNumber))
                //{
                //    string Contact = phoneNumber.Split(':')[1].Split('<')[0].Trim();
                //    cdr.TerminatingCalledNumber = Contact;
                //}

                //phoneNumber = lineAsArr[84];
                //if (!string.IsNullOrEmpty(phoneNumber))//TerminatingCalling 
                //{
                //    string Contact = phoneNumber.Split(':')[1].Split('<')[0].Trim();
                //    cdr.TerminatingCallingNumber = Contact;
                //}
                
                decodedRows.Add(textCdr.ToArray());
            }

            return decodedRows;
        }
    }
}

