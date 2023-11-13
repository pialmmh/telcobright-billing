using System.Collections.Generic;
using System.Linq;
using CdrValidationRules;
using CdrValidationRules.CdrValidationRules.CommonCdrValidationRules;
using MediationModel;
using TelcobrightMediation;
using TelcobrightMediation.Accounting;

namespace InstallConfig.config._helper
{
    public class CasServiceGroupHelper
    {
        public static Dictionary<int, ServiceGroupConfiguration> GetServiceGroupConfigurations()
        {
            List<ServiceGroupConfiguration> serviceGroupConfigurations = new List<ServiceGroupConfiguration>();

            serviceGroupConfigurations.Add(new ServiceGroupConfiguration(idServiceGroup: 6) //LTFS
            {
                Params = new Dictionary<string, string>() { { "prefixes", "0800" } },
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
                        new Duration2Gt0() {Data = .1M},
                        new OutPartnerCostGt0() {Data = .1M},
                        new BtrcRevShareTax1Gt0() {Data = .1M},
                    },
                InvoiceGenerationConfig = new InvoiceGenerationConfig()
                {
                    InvoiceGenerationRuleName = "InvoiceGenerationByLedgerSummary",
                    SectionGeneratorVsTemplateNames = new Dictionary<string, string>()
                    {
                        {"TollFreeInvoiceSection1GeneratorWithTax","LTFSToIPTSP" },
                        {"TollFreeInvoiceSection2GeneratorWithTax","LTFSToIPTSPDetails1" },
                        {"TollFreeInvoiceSection3GeneratorWithTax","LTFSToIPTSPDetails2" }
                    }
                }
            });

            serviceGroupConfigurations.Add(new ServiceGroupConfiguration(idServiceGroup: 2) //intlOutIcx
            {
                Params = new Dictionary<string, string>()
                    { { "idCdrRules", "2" } },//IcxOutgoingCallByInOutTg=2, IcxOutgoingCallByInTgType=1
                PartnerRules = new List<int>()
                {
                    PartnerRuletype.InPartnerByIncomingRoute,
                    PartnerRuletype.OutPartnerByOutgoingRoute,
                },
                Ratingtrules = new List<RatingRule>()
                {
                    new RatingRule() {IdServiceFamily = ServiceFamilyType.XyzIcx, AssignDirection = 0}
                },
                MediationChecklistForAnsweredCdrs =
                    new List<IValidationRule<cdr>>()
                    {
                        new DurationSecGtEq0(),
                        new OutgoingRouteNotEmpty(),
                        new InPartnerIdGt0(),
                        new OutPartnerIdGt0(),
                        new ServiceGroupGt0(),
                        new MatchedPrefixYNotempty(),
                        new CountryCodeNotEmpty(),
                        new RevIcxOutNot0() {Data = .1M},
                        new BtrcRevShareTax2Gt0(){Data = .1M},
                        new XAmountGt0(){Data = .1M},
                        new YAmountGt0(){Data = .1M},
                        new RoundedDurationGt0(){Data = .1M}
                    },
                InvoiceGenerationConfig = new InvoiceGenerationConfig()
                {
                    InvoiceGenerationRuleName = "InvoiceGenerationByLedgerSummary",
                    SectionGeneratorVsTemplateNames = new Dictionary<string, string>()
                    {
                        {"XyzSection1GeneratorWithTax","InternationalOutgoingToANS" },
                        {"XyzSection2GeneratorWithTax","InternationalOutgoingToANSDetails1" },
                        {"XyzSection3GeneratorWithTax","InternationalOutgoingToANSDetails2" }
                    }
                }
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
                        new Duration1Gt0() {Data = .1M},//all was .09
                        new InPartnerCostGt0() {Data = .1M},
                        new BtrcRevShareTax1Gt0(){Data = .1M},
                    },
                InvoiceGenerationConfig = new InvoiceGenerationConfig()
                {
                    InvoiceGenerationRuleName = "InvoiceGenerationByLedgerSummary",
                    SectionGeneratorVsTemplateNames = new Dictionary<string, string>()
                    {
                        {"A2ZInvoiceSection1GeneratorWithTax","DomesticToANS" },
                        {"A2ZInvoiceSection2GeneratorWithTax","DomesticToANSDetails1" },
                        {"A2ZInvoiceSection3GeneratorWithTax","DomesticToANSDetails2" }
                    }
                }
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
                            new Duration1Gt0() {Data = .1M},
                            new InPartnerCostGt0() {Data = .1M},
                            new BtrcRevShareTax1Gt0() {Data = .1M},
                        },
                    InvoiceGenerationConfig = new InvoiceGenerationConfig()
                    {
                        InvoiceGenerationRuleName = "InvoiceGenerationByLedgerSummary",
                        SectionGeneratorVsTemplateNames = new Dictionary<string, string>()
                        {
                            {"A2ZInvoiceSection1GeneratorWithCurrencyConversion","InternationalToIOS" },
                            {"A2ZInvoiceSection2GeneratorWithCurrencyConversion","InternationalToIOSDetails1" },
                            {"A2ZInvoiceSection3GeneratorWithCurrencyConversion","InternationalToIOSDetails2" }
                        }
                    }
                });
            return serviceGroupConfigurations.ToDictionary(s => s.IdServiceGroup);
        }

    }
}