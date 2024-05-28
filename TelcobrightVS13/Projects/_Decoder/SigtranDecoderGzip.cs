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
    public class SigtranDecoderGzip : SigtranDecoder
    {
        public override string ToString() => this.RuleName;
        public override string RuleName => GetType().Name;
        public override int Id => 780;
        public override string HelpText => "Sigtarn Decoder Gzip";
        public override CompressionType CompressionType { get; set; } = CompressionType.Gzip;
        protected override CdrCollectorInputData Input { get; set; }

        public override List<string[]> DecodeFile(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs)
        {
            this.Input = input;
            string fileName = this.Input.FullPath;
            CompressedFileLinesReader linesReader= new CompressedFileLinesReader(fileName);
            string tempFile = linesReader.readFromTempCompressedFile();
            //string[] l = linesReader.readLinesFromCompressedFile();
            List<string[]> lines = base.decodeLine(input, out inconsistentCdrs, tempFile, input.TelcobrightJob.JobName);
            Directory.Delete(linesReader.ExtractedTempDir.FullName, true);
            return lines;
        }
    }
}
