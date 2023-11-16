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
    public class NokiaSiemensDecoderNGT : AbstractCdrDecoder
    {
        public override string ToString() => this.RuleName;
        public override string RuleName => GetType().Name;
        public override int Id => 75;
        public override string HelpText => "Decodes Nokia Siemens.";
        public override CompressionType CompressionType { get; set; }
        public override string UniqueEventTablePrefix { get; }
        public override string PartialTableStorageEngine { get; }
        public override string partialTablePartitionColName { get; }
        protected  CdrCollectorInputData Input { get; set; }
        private List<int> trailerSequence = new List<int>()
            {

                24,0,16,136,16,50,84,118,152
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

            List<byte> fileData = File.ReadAllBytes(filePath).ToList();

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


   
}
