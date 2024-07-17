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
    public class CataleyaCsvDecoderGaziNoFailedGip : CataleyaCsvDecoderGaziNoFailedV2
    {
        public override string ToString() => this.RuleName;
        public override string RuleName => GetType().Name;
        public override int Id => 48;
        public override string HelpText => "Decodes Cataleya CSV CDR. Gazi format, no failed calls";
        public override CompressionType CompressionType { get; set; }
        public override string UniqueEventTablePrefix { get; }
        public override string PartialTableStorageEngine { get; }
        public override string partialTablePartitionColName { get; }
        protected CdrCollectorInputData Input { get; set; }
        private static readonly object threadLocker = new object();

        public override List<string[]>DecodeFile(CdrCollectorInputData input,
            out List<cdrinconsistent> inconsistentCdrs)
        {
            this.Input = input;
            string fileName = this.Input.FullPath;
            List<string> tempLines = new List<string>();
            lock (threadLocker)
            {
                CompressedFileLinesReader linesReader = new CompressedFileLinesReader(fileName);
                tempLines = linesReader.readLinesFromCompressedFile().ToList();
            }
            List<string[]> lines = FileUtil.ParseLinesWithEnclosedAndUnenclosedFields(',', "\"", tempLines)
                .Skip(1).ToList();
            return DecodeLines(input, out inconsistentCdrs, fileName, lines);
        }
    }
}