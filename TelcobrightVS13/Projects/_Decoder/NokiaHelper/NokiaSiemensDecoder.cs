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

    [Export("Decoder", typeof(IFileDecoder))]
    public class NokiaSiemensDecoder : IFileDecoder
    {
        public override string ToString() => this.RuleName;
        public virtual string RuleName => GetType().Name;
        public int Id => 74;
        public string HelpText => "Decodes Nokia Siemens.";
        public CompressionType CompressionType { get; set; }
        public string PartialTablePrefix { get; }
        public string PartialTableStorageEngine { get; }
        public string partialTablePartitionColName { get; }
        protected CdrCollectorInputData Input { get; set; }


     

        public virtual List<string[]> DecodeFile(CdrCollectorInputData decoderInputData, out List<cdrinconsistent> inconsistentCdrs)
        {
            this.Input = decoderInputData;
            string fileName = this.Input.FullPath;
            
            return decodeLine(decoderInputData, out inconsistentCdrs, fileName, decoderInputData.TelcobrightJob.JobName);
        }

        public string getTupleExpression(CdrCollectorInputData decoderInputData, string[] row)
        {
            throw new NotImplementedException();
        }

        public string getSqlWhereClauseForHourWiseSafeCollection(CdrCollectorInputData decoderInputData, DateTime day)
        {
            throw new NotImplementedException();
        }

        public string getCreateTableSqlForUniqueEvent(CdrCollectorInputData decoderInputData)
        {
            throw new NotImplementedException();
        }

        public string getDuplicateCollectionSql(CdrCollectorInputData decoderInputData, DateTime hourOfTheDay, List<string> tuples)
        {
            throw new NotImplementedException();
        }

        public string getPartialCollectionSql(CdrCollectorInputData decoderInputData, DateTime hourOfTheDay, List<string> tuples)
        {
            throw new NotImplementedException();
        }

        public string getSqlWhereClauseForDayWiseSafeCollection(CdrCollectorInputData decoderInputData, DateTime day)
        {
            throw new NotImplementedException();
        }



        public List<string[]> decodeLine(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs, string filePath, string jobName)
        {


            inconsistentCdrs = new List<cdrinconsistent>();
            List<string[]> decodedRows = new List<string[]>();

            List<byte> fileData = File.ReadAllBytes(filePath).ToList();
            
            int cdrRecordNumber = 0;
            int totalBytes = fileData.Count;
            Dictionary<string, PocAndPtc> callRefWiseLegs = new Dictionary<string, PocAndPtc>();


            for (int currentPosition = 0; currentPosition < totalBytes - 1;)
            {
                bool validPoint;
                while (currentPosition < totalBytes)
                {
                    validPoint = false;
                    if (fileData[currentPosition] == 190 && fileData[currentPosition + 2] == 17)
                        validPoint = true;

                    if (fileData[currentPosition] == 177 && fileData[currentPosition + 2] == 18)
                        validPoint = true;

                    if (validPoint)
                    {
                        cdrRecordNumber++;
                        CdrType cdrType = (fileData[currentPosition] == 177) ? CdrType.Ptc : CdrType.Poc;
                        string[] record = NokiaDecodeHelper.Decode(currentPosition, fileData, cdrType);
                        string callRef = record[5];
                        PocAndPtc pocAndPtc=null;
                        if (cdrType==CdrType.Ptc)
                        {
                            NokiaCdr finalRecord = new NokiaCdr(record, null);
                            decodedRows.Add(finalRecord.Row);

                        }

                        //if (callRefWiseLegs.TryGetValue(callRef, out pocAndPtc) == false)
                        //{
                        //    pocAndPtc = new PocAndPtc();
                        //    callRefWiseLegs.Add(callRef, pocAndPtc);
                        //    if (cdrType == CdrType.Ptc) pocAndPtc.Ptc = record;
                        //    else pocAndPtc.Poc = record;
                        //}
                        //else
                        //{
                        //    if (cdrType == CdrType.Ptc) callRefWiseLegs[callRef].Ptc = record;
                        //    else callRefWiseLegs[callRef].Poc = record;
                        //}

                        

                        currentPosition += fileData[currentPosition];
                    }
                    else currentPosition++;
                }
            }

            //foreach (KeyValuePair<string, PocAndPtc> callRefWiseLeg in callRefWiseLegs)
            //{
            //    var ptc = callRefWiseLeg.Value.Ptc;
            //    var poc = callRefWiseLeg.Value.Poc;

            //    NokiaCdr finalRecord = new NokiaCdr(ptc, poc);
            //    if (finalRecord.Row[2]=="")
            //    {
            //        decodedRows.Add(finalRecord.Row);

            //    }
            //}


            return decodedRows;
        }
    }


    public enum CdrType
    {
        Ptc,
        Poc
    }

    public class PocAndPtc
    {
        public string[] Poc { get; set; }
        public string[] Ptc { get; set; }
    }

    public class NokiaCdr
    {
        public string[] Row { get; } = new string[104];
        private string[] Ptc { get; }
        private string[] Poc { get; }

        public NokiaCdr(string[] ptc, string[] poc)
        {
            this.Ptc = ptc;
            this.Poc = poc;

            if (this.Ptc == null) return;

             Row[Fn.Sequencenumber] = ptc[2];
           
            

          


            Row[Fn.OriginatingCallingNumber] = new string (ptc[11].ToCharArray().TakeWhile(c=>c!='F').ToArray());
            Row[Fn.OriginatingCalledNumber] = new string(ptc[13].ToCharArray().TakeWhile(c => c != 'F').ToArray());
            Row[Fn.TerminatingCallingNumber] = new string(ptc[11].ToCharArray().TakeWhile(c => c != 'F').ToArray());
            Row[Fn.TerminatingCalledNumber] = new string(ptc[13].ToCharArray().TakeWhile(c => c != 'F').ToArray());
           
            Row[Fn.DurationSec] = Convert.ToString(Convert.ToInt32(ptc[51]) * 10 / 1000);
            string toChec = "ABCDEF";
            if (ptc[50].Any(c => toChec.Contains(c)) == false) Row[Fn.IncomingRoute] = ptc[50].Trim().TrimStart('0');
            else Row[Fn.IncomingRoute] = "Incoming Route painai";
            if (ptc[14].Any(c => toChec.Contains(c)) == false) Row[Fn.OutgoingRoute] = ptc[14].Trim().TrimStart('0');
            else Row[Fn.OutgoingRoute] = "Outgoing Route Painai";

            string[] formats = new string[] { "MddyyyyHHmmss", "MMddyyyyHHmmss" };

            string startTimestr = ptc[16].Trim();
            DateTime startTime = startTimestr.ConvertToDateTimeFromCustomFormats(formats);
            Row[Fn.StartTime] = startTime.ToMySqlFormatWithoutQuote();

            string ansTime = ptc[17].Trim();
            DateTime startTime1 = ansTime.ConvertToDateTimeFromCustomFormats(formats);
            Row[Fn.AnswerTime] = startTime1.ToMySqlFormatWithoutQuote();


            string endTime = ptc[18].Trim();
            DateTime startTime2 = endTime.ConvertToDateTimeFromCustomFormats(formats);
            Row[Fn.Endtime] = startTime2.ToMySqlFormatWithoutQuote();

            //if (poc != null) Row[Fn.OriginatingCallingNumber] = new string(poc[11].ToCharArray().TakeWhile(c => c != 'F').ToArray());
            //else Row[Fn.OriginatingCallingNumber] = new string(ptc[11].ToCharArray().TakeWhile(c => c != 'F').ToArray());
            //if (poc != null) Row[Fn.OriginatingCalledNumber] = new string(poc[13].ToCharArray().TakeWhile(c => c != 'F').ToArray());
            //else Row[Fn.OriginatingCalledNumber] = new string(ptc[13].ToCharArray().TakeWhile(c => c != 'F').ToArray());
            



            //if (ptc != null) Row[Fn.TerminatingCallingNumber] = new string(ptc[11].ToCharArray().TakeWhile(c => c != 'F').ToArray());
            //else Row[Fn.TerminatingCallingNumber] = new string(poc[11].ToCharArray().TakeWhile(c => c != 'F').ToArray());
            //if (ptc != null) Row[Fn.TerminatingCalledNumber] = new string(ptc[13].ToCharArray().TakeWhile(c => c != 'F').ToArray());
            //else Row[Fn.TerminatingCalledNumber] = new string(poc[13].ToCharArray().TakeWhile(c => c != 'F').ToArray());
            //if (ptc != null) Row[Fn.DurationSec] = Convert.ToString(Convert.ToInt32(ptc[51]) * 10 / 1000);

            //string toChec = "ABCDEF";
            //if (ptc != null  && ptc[50].Any(c =>toChec.Contains(c))==false) Row[Fn.IncomingRoute] = ptc[50].Trim().TrimStart('0');
            //else Row[Fn.IncomingRoute] = poc[50].Trim().TrimStart('0'); 
            //if (ptc != null && ptc[14].Any(c => toChec.Contains(c)) == false) Row[Fn.OutgoingRoute] = ptc[14].Trim().TrimStart('0'); 
            //else Row[Fn.OutgoingRoute] = poc[14].Trim().TrimStart('0'); 

            //string[] formats = new string[] { "MddyyyyHHmmss", "MMddyyyyHHmmss" };

            //if (ptc != null)
            //{
            //    string startTimestr = ptc[16].Trim();
            //    DateTime startTime = startTimestr.ConvertToDateTimeFromCustomFormats(formats);
            //    Row[Fn.StartTime] = startTime.ToMySqlFormatWithoutQuote();
               
            //}
            //else
            //{
            //    string startTimestr = poc[16].Trim();
            //    DateTime startTime = startTimestr.ConvertToDateTimeFromCustomFormats(formats);
            //    Row[Fn.StartTime] = startTime.ToMySqlFormatWithoutQuote();

            //}
            //if (ptc != null)
            //{
                
            //    string ansTime = ptc[17].Trim();
            //    DateTime startTime = ansTime.ConvertToDateTimeFromCustomFormats(formats);
            //    Row[Fn.AnswerTime] = startTime.ToMySqlFormatWithoutQuote();
            //}
            //else
            //{

            //    string ansTime = poc[17].Trim();
            //    DateTime startTime = ansTime.ConvertToDateTimeFromCustomFormats(formats);
            //    Row[Fn.AnswerTime] = startTime.ToMySqlFormatWithoutQuote();
            //}
            //if (ptc != null)
            //{

               
            //    string endTime = ptc[18].Trim();
            //    DateTime startTime = endTime.ConvertToDateTimeFromCustomFormats(formats);
            //    Row[Fn.Endtime] = startTime.ToMySqlFormatWithoutQuote();
            //}
            //else
            //{

            //    string endTime = poc[18].Trim();
            //    DateTime startTime = endTime.ConvertToDateTimeFromCustomFormats(formats);
            //    Row[Fn.Endtime] = startTime.ToMySqlFormatWithoutQuote();
            //}

            Row[Fn.Validflag] = "1";
            Row[Fn.ChargingStatus] = "1";
        }
    }
    public class FieldInfo
    {
        public int Offset;
        public int Length;
        public DataType DataType;

        public FieldInfo(int offset, int length, DataType dataType)
        {
            this.Offset = offset;
            this.Length = length;
            this.DataType = dataType;
        }
        
    }
}
