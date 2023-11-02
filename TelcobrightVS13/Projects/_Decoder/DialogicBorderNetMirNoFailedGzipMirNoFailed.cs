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

    [Export("Decoder", typeof(AbstractCdrDecoder))]
    public class DialogicBorderNetMirNoFailedGzipMirNoFailed : DialogicBorderNetMirNoFailed
    {
        public override string ToString() => this.RuleName;
        public override string RuleName => GetType().Name;
        public override int Id => 27;
        public override string HelpText => "Decodes Dialogic BorderNet CSV CDR with Gzip (Mir Telecom)";
        public override CompressionType CompressionType { get; set; } = CompressionType.Gzip;
        protected override CdrCollectorInputData Input { get; set; }
        private static readonly object threadLocker = new object();
        public override List<string[]> DecodeFile(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs)
        {
            this.Input = input;
            string fileName = this.Input.FullPath;
            List<string> tempLines= new List<string>();
            lock (threadLocker)
            {
                CompressedFileLinesReader linesReader = new CompressedFileLinesReader(fileName);
                tempLines = linesReader.readLinesFromCompressedFile().ToList();
            }
            List<string[]> lines = FileUtil.ParseLinesWithEnclosedAndUnenclosedFields(',', "\"", tempLines)
                .Skip(1).ToList();
            return decodeLines(input, out inconsistentCdrs, fileName, lines);
        }
    }
}
