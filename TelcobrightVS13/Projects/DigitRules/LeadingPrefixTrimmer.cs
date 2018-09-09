using System;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation.Rating;

namespace PartnerRules
{

    [Export("DigitRule", typeof(IDigitRule))]
    public class LeadingPrefixTrimmer : IDigitRule
    {
        public override string ToString() => this.GetType().Name;
        public string RuleName => this.GetType().Name;
        public string HelpText => "Removes mentioned prefix from start of phone number.";
        public int Id => 1;
        public string GetModifiedDigits(DigitRulesData digitRulesData, string phoneNumber)
        {
            string prefixToTrim = digitRulesData.PrefixToTrim;
            if (phoneNumber.StartsWith(prefixToTrim))
            {
                return phoneNumber.Substring(prefixToTrim.Length);
            }
            return phoneNumber;
        }
    }
}
