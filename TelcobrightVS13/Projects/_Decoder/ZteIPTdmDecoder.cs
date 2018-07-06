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
    public class ZteIpTdmDecoder : ZteWireLineSwitchDecoder
    {
        public override string ToString() => this.RuleName;
        public override string RuleName => GetType().Name;
        private int id = 18;
        public override int Id => this.id;
        public override string HelpText => "Decodes ZTE IP TDM CDR.";
        public override List<string[]> DecodeFile(CdrCollectorInputData input,out List<cdrinconsistent> inconsistentCdrs)
        {
            inconsistentCdrs = new List<cdrinconsistent>();
            //this.id = base.Id;
            List<string[]> decodedRows = base.DecodeFile(input, out inconsistentCdrs);
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
        }
    }
}
