using TelcobrightMediation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using TelcobrightFileOperations;
using MediationModel;
using TelcobrightMediation.Mediation.Cdr;


namespace Decoders
{

    [Export("Decoder", typeof(IFileDecoder))]
    public class ZteIpTdmDecoder : IFileDecoder
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public int Id => 18;
        public string HelpText => "Decodes ZTE IP TDM CDR.";
        public List<string[]> DecodeFile(CdrCollectorInputData input,out List<cdrinconsistent> inconsistentCdrs)
        {
            inconsistentCdrs = new List<cdrinconsistent>();
            //this.id = base.Id;
            List<string[]> decodedRows = ZteDecoderHelper.DecodeFile(input.Ne.idcdrformat,input, out inconsistentCdrs);
            decodedRows.ForEach(row =>
            {
                SetIncomingRoute(row);
                SetOutgoingRoute(row);
            });
            return decodedRows;
        }

        private static void SetIncomingRoute(string[] row)
        {
            if (row[Fn.InTrunkAdditionalInfo] == "0")
            {
                row[Fn.IncomingRoute] = row[Fn.InMgwId];
            }
            else if (Convert.ToInt32(row[Fn.InTrunkAdditionalInfo]) > 0 && row[Fn.InMgwId] == "1")
            {
                row[Fn.IncomingRoute] = row[Fn.InTrunkAdditionalInfo];
            }
            else throw new ArgumentOutOfRangeException($"Unexpected values of InMgw & InTrunkAdditionalInfo while setting incoming route.");
        }
        private static void SetOutgoingRoute(string[] row)
        {
            if (row[Fn.OutTrunkAdditionalInfo] == "0")
            {
                row[Fn.OutgoingRoute] = row[Fn.OutMgwId];
            }
            else if (Convert.ToInt32(row[Fn.OutTrunkAdditionalInfo]) > 0 && row[Fn.OutMgwId] == "1")
            {
                row[Fn.OutgoingRoute] = row[Fn.OutTrunkAdditionalInfo];
            }
            //else throw new ArgumentOutOfRangeException($"Unexpected values of OutMgw & OutTrunkAdditionalInfo while setting oucoming route.");
        }
    }
}
