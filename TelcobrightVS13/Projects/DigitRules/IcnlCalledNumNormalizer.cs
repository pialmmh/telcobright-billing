using System;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation.Rating;

namespace PartnerRules
{

    [Export("DigitRule", typeof(IDigitRule))]
    public class IcnlCalledNumNormalizer : IDigitRule
    {
        public override string ToString() => this.GetType().Name;
        public string RuleName => this.GetType().Name;
        public string HelpText => this.RuleName;
        public int Id => 2;
        public string GetModifiedDigits(DigitRulesData digitRulesData, string phoneNumber)
        {
            if (phoneNumber.StartsWith("234234"))
                phoneNumber = phoneNumber.Substring(3);//remove first 234

            if (phoneNumber.StartsWith("234"))
            {
                phoneNumber = phoneNumber.Substring(3);
                if (phoneNumber.Length >= 12 && phoneNumber.Length <= 13) //234-4081412019895 or 234-408141201989
                {
                    phoneNumber = phoneNumber.Substring(2);
                }
            }
            else if (phoneNumber.StartsWith("0"))
            {
                phoneNumber = phoneNumber.Substring(1);
            }
            return phoneNumber;
        }
    }
}
