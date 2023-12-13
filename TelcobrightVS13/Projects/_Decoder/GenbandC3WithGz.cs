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
using System.IO;

namespace Decoders
{

    [Export("Decoder", typeof(AbstractCdrDecoder))]
    public class GenbandC3WithGz : GenbandC3
    {
        public override string ToString() => this.RuleName;
        public override string RuleName => GetType().Name;
        public override int Id => 31;
        public override string HelpText => "Decodes GenbandC3 CSV CDR.";
        public override CompressionType CompressionType { get; set; }
        public override string UniqueEventTablePrefix { get; }
        public override string PartialTableStorageEngine { get; }
        public override string partialTablePartitionColName { get; }
        protected CdrCollectorInputData Input { get; set; }
        private int TotalRecordCount { get; set; }
        private List<string> MetaDataKeyWordsAtLineBeginning { get; } = new List<string>()
        {
            "FILENAME=",
            "CREATION_TIME=",
            "DCT_ID=",
            "DCT_VERSION=",
            "DCT_DEF=",
            "CLOSE_TIME=",
            "SEQNUM_FIRST=",
            "SEQNUM_LAST=",
            "RECORD_COUNT="
        };
        private bool IsMetaDataLine(string line)
        {
            return this.MetaDataKeyWordsAtLineBeginning.Any(md => line.StartsWith(md));
        }

        private static DateTime parseStringToDate(string timestamp)  //20181028051316400 yyyyMMddhhmmssfff
        {
            DateTime dateTime = DateTime.ParseExact(timestamp, "MMddyyyyHHmmssfff", CultureInfo.InvariantCulture);
            return dateTime;
        }

        public override List<string[]> DecodeFile(CdrCollectorInputData decoderInputData, out List<cdrinconsistent> inconsistentCdrs)
        {
            this.Input = decoderInputData;
            string fileName = this.Input.FullPath;
            //CompressedFileLinesReader linesReader= new CompressedFileLinesReader(fileName);
            //List<string> tempLines = linesReader.readLinesFromCompressedFile().ToList();
            List<string> tempLines = File.ReadAllLines(fileName).ToList();

            int recordCount = tempLines.Last().StartsWith("RECORD_COUNT") ? Convert.ToInt32(tempLines.Last().Split('=')[1]) : -1;
            tempLines = tempLines.Where(l => IsMetaDataLine(l) == false).ToList();
            List<string[]> lines = FileUtil.ParseLinesWithEnclosedAndUnenclosedFields(',', "\"", tempLines);
            int failedAndSuccessCount = 0;
            List<string[]> decodedLines = decodeLine(decoderInputData, out inconsistentCdrs, fileName, lines, out failedAndSuccessCount);
            if (recordCount == -1)
            {
                return decodedLines;
            }
            if (recordCount == failedAndSuccessCount)
            {
                return decodedLines;
            }
            var e = new Exception("Record count does not match RECORD_COUNT meta data.");
            Console.WriteLine(e);
            e.Data.Add("customError", "Possibly Corrupted");
            e.Data.Add("jobId", this.Input.TelcobrightJob.id);
            throw e;
        }

        public override string getTupleExpression(Object data)
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

        public override DateTime getEventDatetime(Object data)
        {
            throw new NotImplementedException();
        }
        

    }
}
