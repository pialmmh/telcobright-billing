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
using CdrValidationRules;
using CdrValidationRules.CdrValidationRules.CommonCdrValidationRules;
using LibraryExtensions;
using LibraryExtensions.ConfigHelper;
using MediationModel;
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
                    new List<IValidationRule<cdr>>()
                    {
                        new DurationSecGtEq0(),
                        new OutgoingRouteNotEmpty(),
                        new InPartnerIdGt0(),
                        new OutPartnerIdGt0(),
                        new ServiceGroupGt0(),
                        new MatchedPrefixSupplierNotEmpty(),
                        new Duration2Gt0() {Data = .09M},
                        new OutPartnerCostGt0() {Data = .09M},
                        new BtrcRevShareTax1Gt0() {Data = .09M},
                     },
            });

            serviceGroupConfigurations.Add(new ServiceGroupConfiguration(idServiceGroup: 2) //intlOutIgw
            {
                Params = new Dictionary<string, string>()
                { { "idCdrRules", "2" } },//IcxOutgoingCallByInOutTg=2, IcxOutgoingCallByInTgType=1
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
                    new List<IValidationRule<cdr>>()
                    {
                        new DurationSecGtEq0(),
                        new CountryCodeNotEmpty(),
                        new OutgoingRouteNotEmpty(),
                        new InPartnerIdGt0(),
                        new OutPartnerIdGt0(),
                        new ServiceGroupGt0(),
                        new MatchedPrefixYNotempty(),
                        new RevIcxOutNot0() {Data = .1M},
                        new BtrcRevShareTax2Gt0(){Data = .1M},
                        new XAmountGt0(){Data = .1M},
                        new YAmountGt0(){Data = .1M},
                        new RoundedDurationGt0(){Data = .1M}
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
                    new List<IValidationRule<cdr>>()
                    {
                        new DurationSecGtEq0(),
                        new OutgoingRouteNotEmpty(),
                        new InPartnerIdGt0(),
                        new OutPartnerIdGt0(),
                        new ServiceGroupGt0(),
                        new MatchedPrefixCustomerNotEmpty(),
                        new Duration1Gt0() {Data = .09M},
                        new InPartnerCostGt0() {Data = .09M},
                        new BtrcRevShareTax1Gt0(){Data = .09M},
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
                        new List<IValidationRule<cdr>>()
                        {
                            new DurationSecGtEq0(),
                            new OutgoingRouteNotEmpty(),
                            new InPartnerIdGt0(),
                            new OutPartnerIdGt0(),
                            new ServiceGroupGt0(),
                            new MatchedPrefixCustomerNotEmpty(),
                            new Duration1Gt0() {Data = .09M},
                            new InPartnerCostGt0() {Data = .09M},
                            new BtrcRevShareTax1Gt0() {Data = .09M},
                        },
                });
            return serviceGroupConfigurations.ToDictionary(s => s.IdServiceGroup);
        }
    }
}
