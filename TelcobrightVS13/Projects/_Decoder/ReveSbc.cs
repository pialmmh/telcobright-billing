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
    public class ReveSbc : IFileDecoder
    {
        public override string ToString() => this.RuleName;
        public virtual string RuleName => GetType().Name;
        public int Id => 29;
        public string HelpText => "Decodes ReveSBC CSV CDR.";
        public CompressionType CompressionType { get; set; }
        protected CdrCollectorInputData Input { get; set; }


        private static DateTime parseStringToDate(string timestamp)  //20181028051316400 yyyyMMddhhmmssfff
        {
            DateTime dateTime = DateTime.ParseExact(timestamp, "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
            return dateTime;
        }

        public static string ConvertToUnixTimestamp(DateTime date)
        {

            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds).ToString();
        }

        public List<string[]> DecodeFile(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs)
        {

            string fileName = "cdr_success_192.168.1.105_From_2023-05-16_00_01_To_2023-05-17_00_00.csv";
            List<string[]> lines = FileUtil.ParseCsvWithEnclosedAndUnenclosedFields(fileName, ',', 5, "\"", ";");
            inconsistentCdrs = new List<cdrinconsistent>();
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
                textCdr[Fn.IncomingRoute] = lineAsArr[4];
                textCdr[Fn.DurationSec] = lineAsArr[1];
                //cdr.DurationSec = Convert.ToDecimal(lineAsArr[17]) / 1000;
                textCdr[Fn.Originatingip] = lineAsArr[4];
                textCdr[Fn.TerminatingIp] = lineAsArr[12];
                //cdr.OriginatingIP = lineAsArr[70];
                //cdr.TerminatingIP = lineAsArr[81];
                textCdr[Fn.Mediaip1] = lineAsArr[71];
                textCdr[Fn.Mediaip2] = lineAsArr[82];
                //cdr.MediaIp1 = lineAsArr[71];
                //cdr.MediaIp2 = lineAsArr[82];

                //string dt = lineAsArr[103];//SignalStart
                ////if (!string.IsNullOrEmpty(dt)) cdr.SignalingStartTime = parseStringToDate(dt);

                //string diff = ConvertToUnixTimestamp(Convert.ToDateTime(lineAsArr[82])) - ConvertToUnixTimestamp((DateTime)textCdr[Fn.AnswerTime]);
                //textCdr[Fn.DurationSec] = diff;


                //OriginCalledId
                if (!string.IsNullOrEmpty(lineAsArr[6].Trim())) textCdr[Fn.OriginatingCalledNumber] = lineAsArr[6].Trim();

                //OriginCalling 
                if (!string.IsNullOrEmpty(lineAsArr[8].Trim())) textCdr[Fn.OriginatingCallingNumber] = lineAsArr[8].Trim();


                //TerminatingCalledId
                if (!string.IsNullOrEmpty(lineAsArr[7].Trim())) textCdr[Fn.TerminatingCalledNumber] = lineAsArr[7].Trim();

                //TerminatingCalling
                if (!string.IsNullOrEmpty(lineAsArr[17].Trim())) textCdr[Fn.OriginatingCallingNumber] = lineAsArr[17].Trim();



           


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

