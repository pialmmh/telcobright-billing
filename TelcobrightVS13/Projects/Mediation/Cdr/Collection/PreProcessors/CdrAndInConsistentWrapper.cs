using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;

namespace TelcobrightMediation
{
    public class CdrAndInconsistentWrapper
    {
        public cdr Cdr { get; }
        public cdrinconsistent Cdrinconsistent { get; }

        public CdrAndInconsistentWrapper(cdr cdr, cdrinconsistent cdrinconsistent)
        {
            Cdr = cdr;
            Cdrinconsistent = cdrinconsistent;
        }
    }
}
