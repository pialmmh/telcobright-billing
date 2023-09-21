using TelcobrightMediation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using TelcobrightFileOperations;
using System.Globalization;
using LibraryExtensions;
using MediationModel;
using TelcobrightMediation.Mediation.Cdr;


namespace Decoders
{

    [Export("Decoder", typeof(IFileDecoder))]
    public class HuaweiSoftx3000DecoderSheba : HuaweiSoftx3000Decoder
    {
        public override string ToString() => this.RuleName;
        public override string RuleName => GetType().Name;
        public override int Id => 52;
        public override string HelpText => "Decodes Huawei Softx3000 (Sheba) CDR.";
        public override CompressionType CompressionType { get; set; }

        public override List<string[]> DecodeFile(CdrCollectorInputData input,out List<cdrinconsistent> inconsistentCdrs)
        {
            List<string[]> decodedRows = base.DecodeFile(input, out inconsistentCdrs);
            foreach (string[] row in decodedRows )
            {
                try
                {
                    string originatingCalledNumber = row[Fn.OriginatingCalledNumber]?.Trim();
                    string originatingCallingNumber = row[Fn.OriginatingCallingNumber]?.Trim();
                    string terminatingCalledNumber = row[Fn.TerminatingCalledNumber]?.Trim();
                    string terminatingCallingNumber = row[Fn.TerminatingCallingNumber]?.Trim();
                    if (originatingCalledNumber.IsNullOrEmptyOrWhiteSpace())
                    {
                        row[Fn.OriginatingCalledNumber] = terminatingCalledNumber;
                    }
                    if (originatingCallingNumber.IsNullOrEmptyOrWhiteSpace())
                    {
                        row[Fn.OriginatingCallingNumber] = terminatingCallingNumber;
                    }
                    row[Fn.StartTime] = row[Fn.AnswerTime];
                    row[Fn.SignalingStartTime] = row[Fn.StartTime];

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    inconsistentCdrs.Add(CdrConversionUtil.ConvertTxtRowToCdrinconsistent(row));
                    ErrorWriter wr = new ErrorWriter(e, "DecodeCdr", null,
                        this.RuleName + " encounterd error during decoding and an Inconsistent cdr has been generated."
                        , input.Tbc.Telcobrightpartner.CustomerName);
                    continue;//with next row
                }
            }

            return decodedRows;
        }

        public string getTupleExpression(CdrCollectorInputData decoderInputData, string[] row)
        {
            throw new NotImplementedException();
        }

        public string getSqlWhereClauseForDayWiseSafeCollection(CdrCollectorInputData decoderInputData, DateTime day)
        {
            throw new NotImplementedException();
        }
    }
}
