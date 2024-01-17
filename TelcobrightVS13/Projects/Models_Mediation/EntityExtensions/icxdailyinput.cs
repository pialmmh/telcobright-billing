using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediationModel
{
    public partial class icxdailyinput
    {
            // icx daily input table
            public string callDateICX { get; set; }
            public decimal DomesticICX { get; set; }
            public decimal LtfsICX { get; set; }
            public decimal IntInICX { get; set; }
            public decimal IntOutICX { get; set; }

            // cas sumvoice day
            public string callDateCalc { get; set; }
            public decimal? DomesticDurationCalc { get; set; }
            public decimal? LtfsDurationCalc { get; set; }
            public decimal? IntlInDurationCalc { get; set; }
            public decimal? IntlOutDurationCalc { get; set; }

            // submitted data
            public string callDateSub { get; set; }
            public string CasDomesticSub { get; set; }
            public string CasLtfsSub { get; set; }  
            public string CasIntInSub { get; set; }
            public string CasIntOutSub { get; set; }

            public string submitted { get; set; }


    }
 }

