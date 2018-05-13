using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using TelcobrightMediation;
using TelcobrightMediation.Scheduler.Quartz;
using System.ComponentModel.Composition;
using LibraryExtensions;
using LibraryExtensions.ConfigHelper;
using QuartzTelcobright;
using TelcobrightMediation.Config;

namespace InstallConfig
{
    public partial class JslConfigGenerator //quartz config part
    {
        public Dictionary<int, ServiceGroupConfiguration> GetServiceGroupConfigurations()
        {
            List<ServiceGroupConfiguration> serviceGroupConfigurations = new List<ServiceGroupConfiguration>();

            serviceGroupConfigurations.Add(new ServiceGroupConfiguration(idServiceGroup: 6) //LTFS
            {
                Params = new Dictionary<string, string>() { {"prefixes","0800" } },
                PartnerRules = new List<int>()
                {
                    PartnerRuletype.InPartnerByIncomingRoute,
                    PartnerRuletype.OutPartnerByOutgoingRoute
                },
                Ratingtrules = new List<RatingRule>()
                {
                    new RatingRule() {IdServiceFamily = ServiceFamilyType.SfTollFreeEgressCharging, AssignDirection = 2},
                },
                MediationChecklistForAnsweredCdrs =
                    new Dictionary<string, string>()
                    {
                        {
                            "obj.DurationSec >= 0",
                            "DurationSec must be >=  0"
                        },
                        {
                            "!String.IsNullOrEmpty(obj.OutgoingRoute) and !String.IsNullOrWhiteSpace(obj.OutgoingRoute)",
                            "OutgoingRoute cannot be empty"
                        },
                        {
                            "obj.InPartnerId!=null and obj.InPartnerId > 0",
                            "InPartnerId must be > 0"
                        },

                        {
                            "obj.OutPartnerId!=null and obj.OutPartnerId > 0",
                            "OutPartnerId must be > 0"
                        },
                        {
                            "!String.IsNullOrEmpty(obj.matchedprefixsupplier) and !String.IsNullOrWhiteSpace(obj.matchedprefixsupplier)",
                            "matchedprefixsupplier cannot be empty"
                        },
                        {
                            "obj.DurationSec >= 0.1M ? obj.duration2 > 0 : obj.duration2 == 0 ",
                            "duration2 must be > 0 when DurationSec >= 0.1"
                        },
                        {
                            "obj.DurationSec >= 0.1M ? obj.OutPartnerCost > 0 : obj.OutPartnerCost == 0 ",
                            "OutPartnerCost must be > 0 when DurationSec >= 0.1"
                        },
                        {
                            "obj.DurationSec >= 0.1M ? obj.Tax1 > 0 : obj.Tax1 == 0 ",
                            "BTRC RevShare (Tax1) must be > 0 when DurationSec >= 0.1"
                        },
                    },
            });
            serviceGroupConfigurations.Add(new ServiceGroupConfiguration(idServiceGroup: 1) //domestic
            {
                PartnerRules = new List<int>()
                {
                    PartnerRuletype.InPartnerByIncomingRoute,
                    PartnerRuletype.OutPartnerByOutgoingRoute
                },
                Ratingtrules = new List<RatingRule>()
                {
                    new RatingRule() {IdServiceFamily = ServiceFamilyType.A2Z, AssignDirection = 1},
                },
                MediationChecklistForAnsweredCdrs =
                    new Dictionary<string, string>()
                    {
                        {
                            "obj.DurationSec >= 0",
                            "DurationSec must be >=  0"
                        },
                        {
                            "!String.IsNullOrEmpty(obj.OutgoingRoute) and !String.IsNullOrWhiteSpace(obj.OutgoingRoute)",
                            "OutgoingRoute cannot be empty"
                        },
                        {
                            "obj.InPartnerId!=null and obj.InPartnerId > 0",
                            "InPartnerId must be > 0"
                        },

                        {
                            "obj.OutPartnerId!=null and obj.OutPartnerId > 0",
                            "OutPartnerId must be > 0"
                        },
                        {
                            "!String.IsNullOrEmpty(obj.matchedprefixcustomer) and !String.IsNullOrWhiteSpace(obj.matchedprefixcustomer)",
                            "matchedprefixcustomer cannot be empty"
                        },
                        {
                            "obj.DurationSec >= 0.1M ? obj.duration1 > 0 : obj.duration1 == 0 ",
                            "duration1 must be > 0 when DurationSec >= 0.1"
                        },
                        {
                            "obj.DurationSec >= 0.1M ? obj.InPartnerCost > 0 : obj.InPartnerCost == 0 ",
                            "InPartnerCost must be > 0 when DurationSec >= 0.1"
                        },
                        {
                            "obj.DurationSec >= 0.1M ? obj.Tax1 > 0 : obj.Tax1 == 0 ",
                            "BTRC RevShare (Tax1) must be > 0 when DurationSec >= 0.1"
                        },
                    },
            });
            serviceGroupConfigurations.Add(
                new ServiceGroupConfiguration(idServiceGroup: 3) //intlInIcx
                {
                    PartnerRules = new List<int>()
                    {
                        PartnerRuletype.InPartnerByIncomingRoute,
                        PartnerRuletype.OutPartnerByOutgoingRoute
                    },
                    Ratingtrules = new List<RatingRule>()
                    {
                        new RatingRule() {IdServiceFamily = ServiceFamilyType.A2Z, AssignDirection = 1},
                    },
                    MediationChecklistForAnsweredCdrs =
                        new Dictionary<string, string>()
                        {
                            {
                                "obj.DurationSec >= 0",
                                "DurationSec must be >=  0"
                            },
                            {
                                "!String.IsNullOrEmpty(obj.OutgoingRoute) and !String.IsNullOrWhiteSpace(obj.OutgoingRoute)",
                                "OutgoingRoute cannot be empty"
                            },
                            {
                                "obj.InPartnerId!=null and obj.InPartnerId > 0",
                                "InPartnerId must be > 0"
                            },
                            {
                                "obj.OutPartnerId!=null and obj.OutPartnerId > 0",
                                "OutPartnerId must be > 0"
                            },
                            {
                                "!String.IsNullOrEmpty(obj.matchedprefixcustomer) and !String.IsNullOrWhiteSpace(obj.matchedprefixcustomer)",
                                "matchedprefixcustomer cannot be empty"
                            },
                            {
                                "obj.DurationSec >= 0.1M ? obj.duration1 > 0 : obj.duration1 == 0 ",
                                "duration1 must be > 0 when DurationSec >= 0.1"
                            },
                            {
                                "obj.DurationSec >= 0.1M ? obj.InPartnerCost > 0 : obj.InPartnerCost == 0 ",
                                "InPartnerCost must be > 0 when DurationSec >= 0.1"
                            },
                            {
                                "obj.DurationSec >= 0.1M ? obj.Tax1 > 0 : obj.Tax1 == 0 ",
                                "BTRC RevShare (Tax1) must be > 0 when DurationSec >= 0.1"
                            },
                        },
                });
            serviceGroupConfigurations.Add(new ServiceGroupConfiguration(idServiceGroup: 2) //intlOutIgw
            {
                PartnerRules = new List<int>()
                {
                    PartnerRuletype.InPartnerByIncomingRoute,
                    PartnerRuletype.OutPartnerByOutgoingRoute
                },
                Ratingtrules = new List<RatingRule>()
                {
                    new RatingRule() {IdServiceFamily = ServiceFamilyType.XyzIcx, AssignDirection = 0}
                },
                MediationChecklistForAnsweredCdrs =
                    new Dictionary<string, string>()
                    {
                        {
                            "obj.DurationSec >= 0",
                            "DurationSec must be >=  0"
                        },
                        {
                            "!String.IsNullOrEmpty(obj.CountryCode) and !String.IsNullOrWhiteSpace(obj.CountryCode)",
                            "CountryCode cannot be empty"
                        },
                        {
                            "!String.IsNullOrEmpty(obj.OutgoingRoute) and !String.IsNullOrWhiteSpace(obj.OutgoingRoute)",
                            "OutgoingRoute cannot be empty"
                        },
                        {
                            "obj.OutPartnerId!=null and obj.OutPartnerId > 0",
                            "OutPartnerId must be > 0"
                        },
                        {
                            "!String.IsNullOrEmpty(obj.MatchedPrefixY) and !String.IsNullOrWhiteSpace(obj.MatchedPrefixY)",
                            "MatchedPrefixY cannot be empty"
                        },
                        {
                            "obj.DurationSec >= 0.1M ? obj.RevenueIcxOut != 0 : obj.RevenueIcxOut == 0 ", //negative allowed
                            "RevenueIcxOut must be > 0 when DurationSec >= 0.1"
                        },
                        {
                            "obj.DurationSec >= 0.1M ? obj.Tax2 != 0 : obj.Tax2 == 0 ",
                            "BTRC RevShare (Tax2) must be > 0 when DurationSec >= 0.1"
                        },
                        {
                            "obj.DurationSec >= 0.1M ? obj.XAmount > 0 : obj.XAmount == 0 ",
                            "XAmount must be > 0 when DurationSec >= 0.1"
                        },
                        {
                            "obj.DurationSec >= 0.1M ? obj.YAmount > 0 : obj.YAmount == 0 ",
                            "YAmount must be > 0 when DurationSec >= 0.1"
                        },
                        {
                            "obj.PartialFlag >= 0",
                            "PartialFlag must be >=  0"
                        },
                        {
                            "obj.DurationSec >= 0.1M ? obj.roundedduration > 0 : obj.roundedduration == 0 ",
                            "roundedduration must be > 0 when DurationSec >= 0.1"
                        },
                    },
            });
            return serviceGroupConfigurations.ToDictionary(s => s.IdServiceGroup);
        }
    }
}
