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
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Rating;

namespace InstallConfig
{
    public partial class MirAbstractConfigGenerator //quartz config part
    {
        public Dictionary<int, ServiceGroupConfiguration> GetServiceGroupConfigurations()
        {
            List<ServiceGroupConfiguration> serviceGroupConfigurations = new List<ServiceGroupConfiguration>()
            {
                new ServiceGroupConfiguration(idServiceGroup: 6) //LTFS
                {
                    Params = new Dictionary<string, string>() { {"prefixes","000" } },
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
                            new Duration2Gt0() {Data = .0M},
                            new OutPartnerCostGt0() {Data = .0M},
                            new BtrcRevShareTax1Gt0() {Data = .0M},
                        },
                    InvoiceGenerationConfig = new InvoiceGenerationConfig()
                    {
                        InvoiceGenerationRuleName = "InvoiceGenerationByLedgerSummary",
                        SectionGeneratorVsTemplateNames = new Dictionary<string, string>()
                        {
                            {"TollFreeInvoiceSection1Generator","LTFSToIPTSP" },
                            {"TollFreeInvoiceSection2Generator","LTFSToIPTSPDetails1" },
                            {"TollFreeInvoiceSection3Generator","LTFSToIPTSPDetails2" }
                        }
                    }
                },
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
                        new List<IValidationRule<cdr>>()
                        {
                            new DurationSecGtEq0(),
                            new OutgoingRouteNotEmpty(),
                            new InPartnerIdGt0(),
                            new OutPartnerIdGt0(),
                            new ServiceGroupGt0(),
                            new MatchedPrefixCustomerNotEmpty(),
                            new CountryCodeNotEmpty(),
                            new InPartnerCostGt0() {Data = 0M},
                            new CostIcxInGt0() {Data = .1M},
                            new BtrcRevShareTax1Gt0() {Data = .1M},
                            new AnsIdTermGt0(),
                            new RoundedDurationGt0(){Data = .1M},
                         },
                    InvoiceGenerationConfig = new InvoiceGenerationConfig()
                    {
                        InvoiceGenerationRuleName = "InvoiceGenerationByLedgerSummary",
                        SectionGeneratorVsTemplateNames = new Dictionary<string, string>()
                        {
                            {"A2ZInvoiceSection1Generator","InternationalIncomingToForeignCarrier" },
                            {"A2ZInvoiceSection2GeneratorMatchedPrefixCustomer","InternationalIncomingToForeignCarrierDetails1" },
                            {"A2ZInvoiceSection3Generator","InternationalIncomingToForeignCarrierDetails2" }
                        }
                    },
                    AccountActions = new List<IAutomationAction>
                    {
                        new SendAlertEmailAccountAction(),
                        new SMSAccountAction(),
                        new ActionBlockingAutomation()
                    }
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
                        new RatingRule()
                        {
                            IdServiceFamily = ServiceFamilyType.A2Z, AssignDirection = 2,
                            DigitRulesData = new DigitRulesData()
                            {
                                DigitRuleId = 1,
                                PhoneNumberLeg = PhoneNumberLeg.OriginatingCalledNumber,
                                PrefixToTrim = "00"
                            }
                        },
                        //keep xyz after a2z, there is logic in xyz sf rules for icx & igw that will change
                        //country code only if empty, or throw exception if different
                        new RatingRule() {IdServiceFamily = ServiceFamilyType.XyzIgw, AssignDirection = 0}
                    },
                    MediationChecklistForAnsweredCdrs =
                        new List<IValidationRule<cdr>>()
                        {
                            new DurationSecGtEq0(),
                            new CountryCodeNotEmpty(),
                            new InPartnerIdGt0(),
                            new OutPartnerIdGt0(),
                            new ServiceGroupGt0(),
                            new OutPartnerIdGt0(),
                            new MatchedPrefixYNotempty(),
                            new MatchedPrefixSupplierNotEmpty(),
                            new OutPartnerCostGt0() {Data = 0M},
                            new RevIgwOutGt0() {Data = .1M},
                            new XAmountGt0() {Data = .1M},
                            new YAmountGt0() {Data = .1M},
                            new RoundedDurationGt0() {Data = .1M}
                        },
                    InvoiceGenerationConfig = new InvoiceGenerationConfig()
                    {
                        InvoiceGenerationRuleName = "InvoiceGenerationByLedgerSummary",
                        SectionGeneratorVsTemplateNames = new Dictionary<string, string>()
                        {
                            {"XyzSection1GeneratorIgw","InternationalOutgoingToIOS" },
                            {"XyzSection2GeneratorIgw","InternationalOutgoingToIOSDetails1" },
                            {"XyzSection3GeneratorIgw","InternationalOutgoingToIOSDetails2" }
                        }
                    },
                    }
            };
            return serviceGroupConfigurations.ToDictionary(s => s.IdServiceGroup);
        }
    }
}
