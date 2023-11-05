using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;
using TelcobrightMediation.Mediation.Cdr;

namespace TelcobrightMediation.Cdr
{
    [Serializable]
    public class CdrMergedJobError
    {
        public string Filename { get; set; }
        public job Job { get; set; }
        public string UniqueBillid { get; set; }
        public string Starttime { get; set; }
        public string Answertime { get; set; }
        public string CalledNumber { get; set; }
        public string CallingNumber { get; set; }
        public string Duration { get; set; }
    }
}
