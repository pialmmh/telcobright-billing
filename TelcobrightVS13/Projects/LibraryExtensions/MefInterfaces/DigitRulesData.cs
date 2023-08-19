using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;
using TelcobrightMediation.Rating;

namespace LibraryExtensions
{
    public class DigitRulesData
    {
        public string PrefixToTrim { get; set; }
        public int Position { get; set; }
        public PhoneNumberLeg PhoneNumberLeg { get; set; }
        public int DigitRuleId { get; set; }
    }
}
