using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;

namespace TelcobrightMediation.Rating
{
    public static class PhoneNumberSelector
    {
        public static string GetPhoneNumberForRating(cdr cdr,PhoneNumberLeg phoneNumberLeg)
        {
            switch (phoneNumberLeg)
            {
                case PhoneNumberLeg.OriginatingCalledNumber:
                    return cdr.OriginatingCalledNumber;
                case PhoneNumberLeg.OriginatingCallingNumber:
                        return cdr.OriginatingCallingNumber;
                case PhoneNumberLeg.TerminatingCalledNumber:
                    return cdr.TerminatingCalledNumber;
                case PhoneNumberLeg.TerminatingCallingNumber:
                    return cdr.TerminatingCallingNumber;
                default:
                    throw new ArgumentOutOfRangeException(nameof(phoneNumberLeg), phoneNumberLeg, null);
            }
        }
    }
}
