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
    public partial class PlatinumConfigGenerator //quartz config part
    {
        public Dictionary<int, ServiceGroupConfiguration> GetServiceGroupConfigurations()
        {
            List<ServiceGroupConfiguration> serviceGroupConfigurations = new List<ServiceGroupConfiguration>()
            {
                new ServiceGroupConfiguration(idServiceGroup: 4) //intlInIgw
                {
                    PartnerRules = new List<int>()
                    {
                        PartnerRuletype.InPartnerByIncomingRoute,
                        PartnerRuletype.OutPartnerByOutgoingRoute
                    },
                    Ratingtrules = new List<RatingRule>()
                    {
                        new RatingRule() {IdServiceFamily = ServiceFamilyType.A2Z, AssignDirection = 1},
                        new RatingRule() {IdServiceFamily = ServiceFamilyType.IntlInIgwT1, AssignDirection = 0}
                    },
                    MediationChecklistForAnsweredCdrs =
                        new Dictionary<string, string>()
                        {
                            {
                                "obj.DurationSec >= 0",
                                "DurationSec must be >=  0"
                            },
                            {
                                "!String.IsNullOrEmpty(obj.outgoingroute) and !String.IsNullOrWhiteSpace(obj.outgoingroute)",
                                "outgoingroute cannot be empty"
                            },
                            {
                                "obj.outPartnerId > 0",
                                "outPartnerId must be > 0"
                            },
                            {
                                "!String.IsNullOrEmpty(obj.CountryCode) and !String.IsNullOrWhiteSpace(obj.CountryCode)",
                                "CountryCode cannot be empty"
                            },
                            {
                                "!String.IsNullOrEmpty(obj.matchedprefixcustomer) and !String.IsNullOrWhiteSpace(obj.matchedprefixcustomer)",
                                "matchedprefixcustomer cannot be empty"
                            },
                            {
                                "obj.durationsec > 0 ? obj.CustomerCost > 0 : obj.CustomerCost == 0 ",
                                "CustomerCost must be > 0 when durationsec > 0  , otherwise == 0 "
                            },
                            {
                                "obj.CostICXIn >= 0",
                                "CostICXIn must be >=  0"
                            },
                            {
                                "obj.CostVATCommissionIn >= 0",
                                "CostVATCommissionIn must be >=  0"
                            },
                            {
                                "obj.AnsIdTerm>0",
                                "AnsIdTerm must be > 0"
                            },
                            {
                                "obj.durationsec > 0?obj.roundedduration > 0: obj.roundedduration >= 0",
                                "roundedduration must be > 0 when durationsec >0 , otherwise >= 0 "
                            },
                        },
                }, //end dictionary item
                new ServiceGroupConfiguration(idServiceGroup: 5) //intlOutIgw
                {
                    PartnerRules = new List<int>()
                    {
                        PartnerRuletype.InPartnerByIncomingRoute,
                        PartnerRuletype.OutPartnerByOutgoingRoute
                    },
                    Ratingtrules = new List<RatingRule>()
                    {
                        new RatingRule() {IdServiceFamily = ServiceFamilyType.A2Z, AssignDirection = 2},
                        //keep xyz after a2z, there is logic in xyz sf rules for icx & igw that will change
                        //country code only if empty, or throw exception if different
                        new RatingRule() {IdServiceFamily = ServiceFamilyType.XyzIgw, AssignDirection = 0}
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
                                "!String.IsNullOrEmpty(obj.outgoingroute) and !String.IsNullOrWhiteSpace(obj.outgoingroute)",
                                "outgoingroute cannot be empty"
                            },
                            {
                                "obj.outPartnerId > 0",
                                "outPartnerId must be > 0"
                            },
                            {
                                "!String.IsNullOrEmpty(obj.MatchedPrefixY) and !String.IsNullOrWhiteSpace(obj.MatchedPrefixY)",
                                "MatchedPrefixY cannot be empty"
                            },
                            {
                                "!String.IsNullOrEmpty(obj.matchedprefixsupplier) and !String.IsNullOrWhiteSpace(obj.matchedprefixsupplier)",
                                "matchedprefixsupplier cannot be empty"
                            },
                            {
                                "obj.durationsec > 0 ? obj.SupplierCost > 0 : obj.SupplierCost == 0 ",
                                "SupplierCost must be > 0 when durationsec > 0 , otherwise == 0"
                            },
                            {
                                "obj.durationsec > 0 ? obj.RevenueIGWOut > 0 : obj.RevenueIGWOut == 0 ",
                                "RevenueIGWOut must be > 0 when durationsec > 0 , otherwise == 0"
                            },
                            {
                                "obj.durationsec > 0 ? obj.SubscriberChargeXOut > 0 : obj.SubscriberChargeXOut == 0 ",
                                "SubscriberChargeXOut must be > 0 when durationsec > 0 , otherwise == 0"
                            },
                            {
                                "obj.durationsec > 0 ? obj.CarrierCostYIGWOut > 0 : obj.CarrierCostYIGWOut == 0 ",
                                "CarrierCostYIGWOut must be > 0 when durationsec > 0 , otherwise == 0"
                            },
                            {
                                "obj.PartialFlag >= 0",
                                "PartialFlag must be >=  0"
                            },
                            {
                                "obj.field1 >= 0",
                                "field1 must be >=  0"
                            },
                            {
                                "obj.field2 >= 0",
                                "field2 must be >=  0"
                            },
                            {
                                "obj.durationsec > 0 ? obj.roundedduration > 0 : obj.roundedduration == 0 ",
                                "roundedduration must be > 0 when durationsec > 0 , otherwise == 0"
                            },

                        },
                }
            };
            return serviceGroupConfigurations.ToDictionary(s => s.IdServiceGroup);
        }
    }
}
