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
using TelcobrightInfra.PerformanceAndOptimization;

namespace Decoders
{

    [Export("Decoder", typeof(AbstractCdrDecoder))]
    public class NokiaSiemensDecoderMother : AbstractCdrDecoder
    {
        public override string ToString() => this.RuleName;
        public override string RuleName => GetType().Name;
        public override int Id => 74;
        public override string HelpText => "Decodes Nokia Siemens.";
        public override CompressionType CompressionType { get; set; }
        public override string UniqueEventTablePrefix { get; }
        public override string PartialTableStorageEngine { get; }
        public override string partialTablePartitionColName { get; }
        protected  CdrCollectorInputData Input { get; set; }
        private List<int> trailerSequence = new List<int>()
            {

                24,0,16,136,16,50,84,118,137
            };

        public  override List<string[]> DecodeFile(CdrCollectorInputData decoderInputData, out List<cdrinconsistent> inconsistentCdrs)
        {
            this.Input = decoderInputData;
            string fileName = this.Input.FullPath;

            return decodeLine(decoderInputData, out inconsistentCdrs, fileName, decoderInputData.TelcobrightJob.JobName);
        }

        public string getTupleExpression(CdrCollectorInputData decoderInputData, string[] row)
        {
            throw new NotImplementedException();
        }

        public string getCreateTableSqlForUniqueEvent(CdrCollectorInputData decoderInputData)
        {
            throw new NotImplementedException();
        }

        public string getSelectExpressionForUniqueEvent(CdrCollectorInputData decoderInputData)
        {
            throw new NotImplementedException();
        }

        public string getWhereForHourWiseUniqueEventCollection(CdrCollectorInputData decoderInputData, DateTime hourOfDay)
        {
            throw new NotImplementedException();
        }

        public string getSelectExpressionForPartialCollection(CdrCollectorInputData decoderInputData)
        {
            throw new NotImplementedException();
        }

        public string getWhereForHourWisePartialCollection(CdrCollectorInputData decoderInputData, DateTime hourOfDay)
        {
            throw new NotImplementedException();
        }

        public override string getTupleExpression(Object data)
        {
            throw new NotImplementedException();
        }

        public override string getCreateTableSqlForUniqueEvent(object data)
        {
            throw new NotImplementedException();
        }

        public override string getSelectExpressionForUniqueEvent(object data)
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

        public override DateTime getEventDatetime(Object data)
        {
            throw new NotImplementedException();
        }


        public List<string[]> decodeLine(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs, string filePath, string jobName)
        {


            inconsistentCdrs = new List<cdrinconsistent>();
            List<string[]> decodedRows = new List<string[]>();

            List<byte> fileData= new List<byte>();
            try
            {
                fileData = File.ReadAllBytes(filePath).ToList();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("OutOfMemoryException"))
                {
                    GarbageCollectionHelper.CompactGCNowForOnce();
                    fileData = File.ReadAllBytes(filePath).ToList();
                }
                else
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            int cdrRecordNumber = 0;
            int totalBytes = fileData.Count;
            //Dictionary<string, PocAndPtc> callRefWiseLegs = new Dictionary<string, PocAndPtc>();


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
                    if (fileData[currentPosition] == 177)
                        cdrRecordNumber++;

                    if (validPoint)
                    {
                        //cdrRecordNumber++;


                        CdrType cdrType = (fileData[currentPosition] == 177) ? CdrType.Ptc : CdrType.Poc;

                        if (cdrType == CdrType.Ptc)
                        {
                            string[] record = NokiaDecodeHelper.Decode(currentPosition, fileData, cdrType,trailerSequence);
                            NokiaCdr finalRecord = new NokiaCdr(record);
                            decodedRows.Add(finalRecord.Row);
                        }

                        //string callRef = record[5];
                        //PocAndPtc pocAndPtc=null;

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
            //    if (ptc!= null)
            //    {
            //        NokiaCdr finalRecord = new NokiaCdr(ptc, poc);
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

    //public class PocAndPtc
    //{
    //    public string[] Poc { get; set; }
    //    public string[] Ptc { get; set; }
    //}

    public class NokiaCdr
    {
        public string[] Row { get; } = new string[104];


        public NokiaCdr(string[] record)
        {



            this.Row[Fn.Sequencenumber] = record[2];
            this.Row[Fn.OriginatingCallingNumber] = new string(record[11].ToCharArray().TakeWhile(c => c != 'F').ToArray());
            this.Row[Fn.OriginatingCalledNumber] = new string(record[13].ToCharArray().TakeWhile(c => c != 'F').ToArray());
            this.Row[Fn.TerminatingCallingNumber] = new string(record[11].ToCharArray().TakeWhile(c => c != 'F').ToArray());
            this.Row[Fn.TerminatingCalledNumber] = new string(record[13].ToCharArray().TakeWhile(c => c != 'F').ToArray());


            this.Row[Fn.IncomingRoute] = record[record.Length - 2].Trim().TrimStart('0');

            this.Row[Fn.OutgoingRoute] = record[14].Trim().TrimStart('0');

            string[] formats = new string[] { "MddyyyyHHmmss", "MMddyyyyHHmmss" };

            string startTimestr = record[17].Trim();
            DateTime startTime = startTimestr.ConvertToDateTimeFromCustomFormats(formats);
            this.Row[Fn.StartTime] = startTime.ToMySqlFormatWithoutQuote();

            string ansTime = record[17].Trim();
            DateTime startTime1 = ansTime.ConvertToDateTimeFromCustomFormats(formats);
            this.Row[Fn.AnswerTime] = startTime1.ToMySqlFormatWithoutQuote();


            string endTime = record[18].Trim();
            DateTime startTime2 = endTime.ConvertToDateTimeFromCustomFormats(formats);
            this.Row[Fn.Endtime] = startTime2.ToMySqlFormatWithoutQuote();


            string durationStr = record[51].Trim();
            double duration = Convert.ToDouble(durationStr) / 100;
            var tmp = record[23].Trim().TrimStart('0') == "" ? "0" : record[23].Trim().TrimStart('0');
            double duration2 = Convert.ToDouble(tmp);

            this.Row[Fn.Duration4] = duration2.ToString();

            this.Row[Fn.DurationSec] = duration.ToString();


            this.Row[Fn.Validflag] = "1";
            this.Row[Fn.ChargingStatus] = "1";
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
