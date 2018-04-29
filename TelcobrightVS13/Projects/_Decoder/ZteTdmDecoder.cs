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
    public class ZteTdmDecoder:ZteWireLineSwitchDecoder
    {
        public override string ToString() => this.RuleName;
        public override string RuleName => GetType().Name;
        public override int Id => 17;
        public override string HelpText => "Decodes ZTE TDM CDR.";
        public override List<string[]> DecodeFile(CdrCollectorInputData input,out List<cdrinconsistent> inconsistentCdrs)
        {
            inconsistentCdrs = new List<cdrinconsistent>();
            List<string[]> decodedRows = base.DecodeFile(input, out inconsistentCdrs);
            decodedRows.ForEach(row =>
            {
                row[Fn.IncomingRoute] = row[Fn.InTrunkAdditionalInfo];
                row[Fn.OutgoingRoute] = row[Fn.OutTrunkAdditionalInfo];
            });
            return decodedRows;
        }
    }
}
