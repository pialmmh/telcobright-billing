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
    public class DialogicBorderNetGzip : DialogicBorderNet
    {
        public new string ToString() => this.RuleName;
        public new virtual string RuleName => GetType().Name;
        public new int Id => 27;
        public new string HelpText => "Decodes Dialogic BorderNet CSV CDR with Gzip";
        public new CompressionType CompressionType { get; set; } = CompressionType.Gzip;
        protected new CdrCollectorInputData Input { get; set; }

        public new List<string[]> DecodeFile(CdrCollectorInputData input, out List<cdrinconsistent> inconsistentCdrs)
        {
            this.Input = input;
            string fileName = this.Input.FullPath;
            List<string> tempLines = FileAndPathHelper.readLinesFromCompressedFile(fileName).ToList();
            List<string[]> lines = FileUtil.ParseLinesWithEnclosedAndUnenclosedFields(',', "\"",tempLines);
            return decodeLines(input, out inconsistentCdrs, fileName, lines);
        }
    }
}
