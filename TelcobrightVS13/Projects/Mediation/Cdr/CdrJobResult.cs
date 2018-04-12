using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;
using TelcobrightMediation.Mediation.Cdr;

namespace TelcobrightMediation.Cdr
{
    public class CdrJobResult
    {
        public int cdrCount { get; set; }
        public int cdrErrorCount { get; set; }
        public int cdrInConsistent { get; set; }

    }
}
