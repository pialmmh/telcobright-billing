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
    public partial class IcnlConfigGenerator //quartz config part
    {
        public List<int> GetServiceGroupPreProcessingRules()
        {
            return new List<int>(1);//copy orig call number to redirect num field
        }
        public Dictionary<int, ServiceGroupConfiguration> GetServiceGroupConfigurations()
        {
            List<ServiceGroupConfiguration> serviceGroupConfigurations = new List<ServiceGroupConfiguration>()
            {
                new ServiceGroupConfiguration(idServiceGroup: 20) //local
                {
                    Disabled = false,
                    Params = new Dictionary<string, string>()
                        {{"idCdrRules", "3"}}, //LocalCallByTgTypeAndPrefix=3
                    InPartnerFirstMatchRules = new List<int>()
                    {
                        PartnerRuletype.InPartnerByIncomingRoute,
                        PartnerRuletype.InPartnerByBridgeRoute,
                    },
                    OutPartnerFirstMatchRules = new List<int>()
                    {
                        PartnerRuletype.OutPartnerByOutgoingRoute,
                        PartnerRuletype.OutPartnerByBridgeRoute
                    },
                    Ratingtrules = new List<RatingRule>()
                    {
                        new RatingRule() {IdServiceFamily = ServiceFamilyType.A2Z, AssignDirection = 1},
                        //new RatingRule() {IdServiceFamily = ServiceFamilyType.A2Z, AssignDirection = 2},
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
                            //new CountryCodeNotEmpty(),
                            new InPartnerCostGt0() {Data = 0M},
                            new Duration1Gt0() {Data = 0M},
                            //new MatchedPrefixSupplierNotEmpty(),
                            //new OutPartnerCostGt0() {Data = 0M},
                            //new Duration2Gt0(){Data = 0M},
                        },
                    InvoiceGenerationConfig = new InvoiceGenerationConfig()
                    {
                        InvoiceGenerationRuleName = "InvoiceGenerationByLedgerSummary",
                        SectionGeneratorVsTemplateNames = new Dictionary<string, string>()
                        {
                            {"A2ZInvoiceSection1Generator", "InternationalIncomingToForeignCarrier"},
                            {
                                "A2ZInvoiceSection2GeneratorMatchedPrefixCustomer",
                                "InternationalIncomingToForeignCarrierDetails1"
                            },
                            {"A2ZInvoiceSection3Generator", "InternationalIncomingToForeignCarrierDetails2"}
                        }
                    },
                    AccountActions = new List<IAutomationAction>
                    {
                        new SendAlertEmailAccountAction(),
                        new SMSAccountAction(),
                        new ActionBlockingAutomation()
                    }
                }, //end dictionary item
                new ServiceGroupConfiguration(idServiceGroup: 21) //outgoing
                {
                    Disabled = false,
                    Params = new Dictionary<string, string>() {{"idCdrRules", "4"}},
                    InPartnerFirstMatchRules = new List<int>()
                    {
                        PartnerRuletype.InPartnerByIncomingRoute,
                        PartnerRuletype.InPartnerByBridgeRoute,
                    },
                    OutPartnerFirstMatchRules = new List<int>()
                    {
                        PartnerRuletype.OutPartnerByOutgoingRoute,
                        PartnerRuletype.OutPartnerByBridgeRoute
                    },
                    Ratingtrules = new List<RatingRule>()
                    {
                        new RatingRule() {IdServiceFamily = ServiceFamilyType.A2Z, AssignDirection = 1},
                        //new RatingRule() {IdServiceFamily = ServiceFamilyType.A2Z, AssignDirection = 2},
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
                            //new InPartnerCostGt0() {Data = 0M},
                            new Duration1Gt0() {Data = 0M},
                            //new MatchedPrefixSupplierNotEmpty(),
                            //new OutPartnerCostGt0() {Data = 0M},
                            //new Duration2Gt0(){Data = 0M},
                        },
                    InvoiceGenerationConfig = new InvoiceGenerationConfig()
                    {
                        InvoiceGenerationRuleName = "InvoiceGenerationByLedgerSummary",
                        SectionGeneratorVsTemplateNames = new Dictionary<string, string>()
                        {
                            {"A2ZInvoiceSection1Generator", "InternationalIncomingToForeignCarrier"},
                            {
                                "A2ZInvoiceSection2GeneratorMatchedPrefixCustomer",
                                "InternationalIncomingToForeignCarrierDetails1"
                            },
                            {"A2ZInvoiceSection3Generator", "InternationalIncomingToForeignCarrierDetails2"}
                        }
                    },
                    AccountActions = new List<IAutomationAction>
                    {
                        new SendAlertEmailAccountAction(),
                        new SMSAccountAction(),
                        new ActionBlockingAutomation()
                    }
                }, //end dictionary item
                new ServiceGroupConfiguration(idServiceGroup: 22) //incoming
                {
                    Disabled = false,
                    Params = new Dictionary<string, string>() {{"idCdrRules", "5"}},
                    PartnerRules = new List<int>()
                    {
                        PartnerRuletype.InPartnerByIncomingRoute,
                        //PartnerRuletype.OutPartnerByOutgoingRoute
                    },
                    Ratingtrules = new List<RatingRule>()
                    {
                        new RatingRule() {IdServiceFamily = ServiceFamilyType.A2Z, AssignDirection = 1},
                        //new RatingRule() {IdServiceFamily = ServiceFamilyType.A2Z, AssignDirection = 2},
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
                            new Duration1Gt0() {Data = 0M},
                            //new MatchedPrefixSupplierNotEmpty(),
                            //new OutPartnerCostGt0() {Data = 0M},
                            //new Duration2Gt0(){Data = 0M},
                        },
                    InvoiceGenerationConfig = new InvoiceGenerationConfig()
                    {
                        InvoiceGenerationRuleName = "InvoiceGenerationByLedgerSummary",
                        SectionGeneratorVsTemplateNames = new Dictionary<string, string>()
                        {
                            {"A2ZInvoiceSection1Generator", "InternationalIncomingToForeignCarrier"},
                            {
                                "A2ZInvoiceSection2GeneratorMatchedPrefixCustomer",
                                "InternationalIncomingToForeignCarrierDetails1"
                            },
                            {"A2ZInvoiceSection3Generator", "InternationalIncomingToForeignCarrierDetails2"}
                        }
                    },
                    AccountActions = new List<IAutomationAction>
                    {
                        new SendAlertEmailAccountAction(),
                        new SMSAccountAction(),
                        new ActionBlockingAutomation()
                    }
                }, //end dictionary item
                new ServiceGroupConfiguration(idServiceGroup: 23) //toll free
                {
                    Disabled = true,
                    Params = new Dictionary<string, string>() {{"prefixes", "622"}},
                    InPartnerFirstMatchRules = new List<int>()
                    {
                        PartnerRuletype.InPartnerByIncomingRoute,
                        PartnerRuletype.InPartnerByBridgeRoute,
                    },
                    OutPartnerFirstMatchRules = new List<int>()
                    {
                        PartnerRuletype.OutPartnerByOutgoingRoute,
                        PartnerRuletype.OutPartnerByBridgeRoute
                    },
                    Ratingtrules = new List<RatingRule>()
                    {
                        new RatingRule()
                        {
                            IdServiceFamily = ServiceFamilyType.SfTollFreeEgressCharging,
                            AssignDirection = 2
                        },
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
                            new Duration2Gt0() {Data = 0M},
                            new OutPartnerCostGt0() {Data = 0M},
                            
                        },
                    InvoiceGenerationConfig = new InvoiceGenerationConfig()
                    {
                        InvoiceGenerationRuleName = "InvoiceGenerationByLedgerSummary",
                        SectionGeneratorVsTemplateNames = new Dictionary<string, string>()
                        {
                            {"TollFreeInvoiceSection1GeneratorWithTax", "LTFSToIPTSP"},
                            {"TollFreeInvoiceSection2GeneratorWithTax", "LTFSToIPTSPDetails1"},
                            {"TollFreeInvoiceSection3GeneratorWithTax", "LTFSToIPTSPDetails2"}
                        }
                    }
                },
                new ServiceGroupConfiguration(idServiceGroup: 24) //toll free premium
                {
                    Disabled = true,
                    Params = new Dictionary<string, string>() {{"prefixes", "800"}},
                    InPartnerFirstMatchRules = new List<int>()
                    {
                        PartnerRuletype.InPartnerByIncomingRoute,
                        PartnerRuletype.InPartnerByBridgeRoute,
                    },
                    OutPartnerFirstMatchRules = new List<int>()
                    {
                        PartnerRuletype.OutPartnerByOutgoingRoute,
                        PartnerRuletype.OutPartnerByBridgeRoute
                    },
                    Ratingtrules = new List<RatingRule>()
                    {
                        new RatingRule()
                        {
                            IdServiceFamily = ServiceFamilyType.SfTollFreeEgressCharging,
                            AssignDirection = 2
                        },
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
                            new Duration2Gt0() {Data = 0M},
                            new OutPartnerCostGt0() {Data = 0M},
                            
                        },
                    InvoiceGenerationConfig = new InvoiceGenerationConfig()
                    {
                        InvoiceGenerationRuleName = "InvoiceGenerationByLedgerSummary",
                        SectionGeneratorVsTemplateNames = new Dictionary<string, string>()
                        {
                            {"TollFreeInvoiceSection1GeneratorWithTax", "LTFSToIPTSP"},
                            {"TollFreeInvoiceSection2GeneratorWithTax", "LTFSToIPTSPDetails1"},
                            {"TollFreeInvoiceSection3GeneratorWithTax", "LTFSToIPTSPDetails2"}
                        }
                    }
                }
            };
            return serviceGroupConfigurations.ToDictionary(s => s.IdServiceGroup);
        }
    }
}
