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

namespace Decoders
{

    [Export("Decoder", typeof(IFileDecoder))]
    public class CataleyaCsvDecoder : IFileDecoder
    {
        public override string ToString() => this.RuleName;
        public virtual string RuleName => GetType().Name;
        public int Id => 25;
        public string HelpText => "Decodes Cataleya CSV CDR.";
        protected CdrCollectorInputData Input { get; set; }


        private static DateTime parseStringToDate(string timestamp)  //20181028051316400 yyyyMMddhhmmssfff
        {
            DateTime dateTime = DateTime.ParseExact(timestamp, "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
            return dateTime;
        }

        public List<string[]> DecodeFile(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs)
        {

            string fileName = "esdr_sample.csv";
            List<string[]> lines = FileUtil.ParseCsvWithEnclosedAndUnenclosedFields(fileName, ',', 1, "\"", ";");
            //inconsistentCdrs = new List<cdrinconsistent>();
            List<string[]> decodedRows = new List<string[]>();
            //this.Input = input;
            List<cdrfieldmappingbyswitchtype> fieldMappings = null;

            foreach (string[] lineAsArr in lines)
            {
                var textCdr = new List<string>();

                textCdr[Fn.Switchid] = Convert.ToString(9);
                //cdr.SwitchId = 9;
                textCdr[Fn.Sequencenumber] = lineAsArr[0];
                //cdr.SequenceNumber = Convert.ToInt64(lineAsArr[0]);
                textCdr[Fn.Filename] = fileName;
                textCdr[Fn.IncomingRoute] = lineAsArr[25];
                textCdr[Fn.DurationSec] = lineAsArr[17];
                //cdr.DurationSec = Convert.ToDecimal(lineAsArr[17]) / 1000;
                textCdr[Fn.Originatingip] = lineAsArr[70];
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


                dt = lineAsArr[38];//ConnectTime
                if (!string.IsNullOrEmpty(dt)) textCdr[Fn.ConnectTime] = parseStringToDate(dt).ToString();

                //dt = lineAsArr[129];//AnswerTime
                //if (!string.IsNullOrEmpty(dt)) cdr.AnswerTime = parseStringToDate(dt);

                dt = lineAsArr[129];//AnswerTime
                if (!string.IsNullOrEmpty(dt)) textCdr[Fn.AnswerTime] = parseStringToDate(dt).ToString();

                //dt = lineAsArr[130];//EndTime
                //if (!string.IsNullOrEmpty(dt)) cdr.EndTime = parseStringToDate(dt);

                dt = lineAsArr[130];//EndTime
                if (!string.IsNullOrEmpty(dt)) textCdr[Fn.Endtime] = parseStringToDate(dt).ToString();


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
