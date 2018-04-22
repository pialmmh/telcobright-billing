using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation
{
    public enum JobCompletionStatus
    {
        Incomplete = 0,
        Complete = 1,
        ProgressivelyComplete = 2,
    }

    public static class PartnerRuletype
    {
        public const int InPartnerByIncomingRoute=1;
        public const int OutPartnerByOutgoingRoute = 2;
    }
    public static class ServiceFamilyType
    {
        public const int A2Z =1;
        public const int IntlInIosT2 = 12;
        public const int IntlInIgwT1 = 3;
        public const int XyzIcx = 7;
        public const int XyzIgw = 4;
    }
    public static class IcxPartnerType
    {
        public const int ANS = 2;
        public const int IOS = 3;
    }
    public static class IgwPartnerType
    {
        public const int ANS = 1;
        public const int IOS = 2;
        public const int ForeignCarrier=3;
    }

    public static class RouteLocalityType
    {
        public const int National = 1;
        public const int International = 2;
    }

    public enum XyzRatingType
    {
        Igw,
        Icx
    }
}
