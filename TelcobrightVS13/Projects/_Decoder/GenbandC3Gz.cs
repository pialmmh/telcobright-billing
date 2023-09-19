using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text;
using LibraryExtensions;
using MediationModel;
using TelcobrightFileOperations;
using TelcobrightMediation;

namespace Decoders
{
    [Export("Decoder", typeof(IFileDecoder))]
    class GenbandC3Gz: GenbandC3
    {
        public override string ToString() => this.RuleName;
        public override string RuleName => GetType().Name;
        public override int Id => 31;
        public override string HelpText => "Decodes GenbandC3 Gz CDR.";
        public override CompressionType CompressionType { get; set; }
        protected  override CdrCollectorInputData Input { get; set; }


        public override List<string[]> DecodeFile(CdrCollectorInputData decoderInputData, out List<cdrinconsistent> inconsistentCdrs)
        {
            this.Input = decoderInputData;
            string fileName = this.Input.FullPath;
            List<string> tempLines = FileAndPathHelper.readLinesFromCompressedFile(fileName).ToList();
            List<string[]> lines = FileUtil.ParseLinesWithEnclosedAndUnenclosedFields(',', "\"", tempLines);
            return decodeLine(decoderInputData, out inconsistentCdrs, fileName, lines);
        }
    }
}
