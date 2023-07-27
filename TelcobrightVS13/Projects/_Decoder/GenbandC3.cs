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
    public class GenbandC3 : IFileDecoder
    {
        public override string ToString() => this.RuleName;
        public virtual string RuleName => GetType().Name;
        public int Id => 28;
        public string HelpText => "Decodes GenbandC3 CSV CDR.";
        public CompressionType CompressionType { get; set; }
        protected CdrCollectorInputData Input { get; set; }


        private static DateTime parseStringToDate(string timestamp)  //20181028051316400 yyyyMMddhhmmssfff
        {
            DateTime dateTime = DateTime.ParseExact(timestamp, "MMddyyyyHHmmssfff", CultureInfo.InvariantCulture);
            return dateTime;
        }

        public virtual List<string[]> DecodeFile(CdrCollectorInputData decoderInputData, out List<cdrinconsistent> inconsistentCdrs)
        {
            this.Input = decoderInputData;
            string fileName = this.Input.FullPath;
            List<string> tempLines = FileAndPathHelper.readLinesFromCompressedFile(fileName).ToList();
            List<string[]> lines = FileUtil.ParseLinesWithEnclosedAndUnenclosedFields(',', "\"", tempLines);
            return decodeLine(decoderInputData, out inconsistentCdrs, fileName, lines);
        }

        public List<string[]> decodeLine(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs, string fileName, List<string[]> lines)
        {
            
           
            inconsistentCdrs = new List<cdrinconsistent>();
            List<string[]> decodedRows = new List<string[]>();
            
            List<cdrfieldmappingbyswitchtype> fieldMappings = null;

            foreach (string[] lineAsArr in lines)
            {
                if(lineAsArr.Length<15)continue;
                string[] textCdr= new  string[input.MefDecodersData.Totalfieldtelcobright];

                textCdr[Fn.Switchid] = Convert.ToString(9);
                //cdr.SwitchId = 9;
                textCdr[Fn.Sequencenumber] = lineAsArr[0];
                //cdr.SequenceNumber = Convert.ToInt64(lineAsArr[0]);
                textCdr[Fn.Filename] = fileName;
                textCdr[Fn.IncomingRoute] = lineAsArr[22];
                textCdr[Fn.DurationSec] = lineAsArr[52];
                //cdr.DurationSec = Convert.ToDecimal(lineAsArr[17]) / 1000;
                textCdr[Fn.Originatingip] = lineAsArr[21];
                textCdr[Fn.TerminatingIp] = lineAsArr[25];
                //cdr.OriginatingIP = lineAsArr[70];
                //cdr.TerminatingIP = lineAsArr[81];
                //textCdr[Fn.Mediaip1] = lineAsArr[71];//
                //textCdr[Fn.Mediaip2] = lineAsArr[82];//
                //cdr.MediaIp1 = lineAsArr[71];
                //cdr.MediaIp2 = lineAsArr[82];

                //string dt = lineAsArr[103];//SignalStart
                ////if (!string.IsNullOrEmpty(dt)) cdr.SignalingStartTime = parseStringToDate(dt);

                string dt = lineAsArr[8];//SignalStart
                if (!string.IsNullOrEmpty(dt)) textCdr[Fn.SignalingStartTime] = parseStringToDate(dt).ToString();

                //dt = lineAsArr[38];//ConnectTime
                //if (!string.IsNullOrEmpty(dt)) cdr.ConnectTime = parseStringToDate(dt);


                dt = lineAsArr[8];//ConnectTime
                if (!string.IsNullOrEmpty(dt)) textCdr[Fn.ConnectTime] = parseStringToDate(dt).ToString();

                //dt = lineAsArr[129];//AnswerTime
                //if (!string.IsNullOrEmpty(dt)) cdr.AnswerTime = parseStringToDate(dt);

                dt = lineAsArr[9];//AnswerTime
                if (!string.IsNullOrEmpty(dt)) textCdr[Fn.AnswerTime] = parseStringToDate(dt).ToString();

                //dt = lineAsArr[130];//EndTime
                //if (!string.IsNullOrEmpty(dt)) cdr.EndTime = parseStringToDate(dt);

                dt = lineAsArr[11];//EndTime
                if (!string.IsNullOrEmpty(dt)) textCdr[Fn.Endtime] = parseStringToDate(dt).ToString();



                string phoneNumber = lineAsArr[17]; //OriginCalled 
                if (!string.IsNullOrEmpty(phoneNumber)) textCdr[Fn.OriginatingCalledNumber] = phoneNumber;

                phoneNumber = lineAsArr[14]; //OriginCalling 
                if (!string.IsNullOrEmpty(phoneNumber)) textCdr[Fn.OriginatingCallingNumber] = phoneNumber;

                
                textCdr[Fn.Validflag] = "1";
                decodedRows.Add(textCdr.ToArray());
            }

            return decodedRows;

        }




    }
}
