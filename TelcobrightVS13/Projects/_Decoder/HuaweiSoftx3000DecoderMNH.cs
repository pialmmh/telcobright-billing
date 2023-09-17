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
    public class HuaweiSoftx3000DecoderMNH : HuaweiSoftx3000Decoder
    {
        public override string ToString() => this.RuleName;
        public override string RuleName => GetType().Name;
        public override int Id => 51;
        public override string HelpText => "Decodes Huawei Softx3000 CDR (MNH version)";
        public override CompressionType CompressionType { get; set; }

        public override List<string[]> DecodeFile(CdrCollectorInputData input,out List<cdrinconsistent> inconsistentCdrs)
        {
            List<string[]> decodedRows = base.DecodeFile(input, out inconsistentCdrs);
            foreach (string[] thisRow in decodedRows)
            {
                try
                {
                    string originatingCalledNumber = thisRow[Fn.OriginatingCalledNumber]?.Trim();
                    string originatingCallingNumber = thisRow[Fn.OriginatingCallingNumber]?.Trim();
                    string terminatingCalledNumber = thisRow[Fn.TerminatingCalledNumber]?.Trim();
                    string terminatingCallingNumber = thisRow[Fn.TerminatingCallingNumber]?.Trim();
                    if (originatingCalledNumber.IsNullOrEmptyOrWhiteSpace())
                    {
                        thisRow[Fn.OriginatingCalledNumber] = terminatingCalledNumber;
                    }
                    if (originatingCallingNumber.IsNullOrEmptyOrWhiteSpace())
                    {
                        thisRow[Fn.OriginatingCallingNumber] = terminatingCallingNumber;
                    }
                    thisRow[Fn.StartTime] = thisRow[Fn.AnswerTime];
                    thisRow[Fn.SignalingStartTime] = thisRow[Fn.StartTime];
                }
                catch (Exception e1)
                {//if error found for one row, add this to inconsistent

                    Console.WriteLine(e1);
                    inconsistentCdrs.Add(CdrConversionUtil.ConvertTxtRowToCdrinconsistent(thisRow));
                    ErrorWriter wr = new ErrorWriter(e1, "DecodeCdr", null,
                        this.RuleName + " encounterd error during decoding and an Inconsistent cdr has been generated."
                        , input.Tbc.Telcobrightpartner.CustomerName);
                    continue;//with next row
                }
            }//try for each row in byte array
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
