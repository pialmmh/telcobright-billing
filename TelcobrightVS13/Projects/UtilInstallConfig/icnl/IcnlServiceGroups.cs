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
        public Dictionary<int, ServiceGroupConfiguration> GetServiceGroupConfigurations()
        {


            List<ServiceGroupConfiguration> serviceGroupConfigurations = new List<ServiceGroupConfiguration>()
            {
                new ServiceGroupConfiguration(idServiceGroup: 20) //local
                {
                    Params = new Dictionary<string, string>()
                        { { "idCdrRules", "3" } },//LocalCallByTgTypeAndPrefix=3
                    PartnerRules = new List<int>()
                    {
                        PartnerRuletype.InPartnerByIncomingRoute,
                        PartnerRuletype.OutPartnerByOutgoingRoute
                    },
                    Ratingtrules = new List<RatingRule>()
                    {
                        new RatingRule() {IdServiceFamily = ServiceFamilyType.A2Z, AssignDirection = 1},
                        new RatingRule() {IdServiceFamily = ServiceFamilyType.A2Z, AssignDirection = 2},
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
                            new Duration1Gt0(){Data = 0M},
                            new MatchedPrefixSupplierNotEmpty(),
                            new OutPartnerCostGt0() {Data = 0M},
                            new Duration2Gt0(){Data = 0M},
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
            };
            return serviceGroupConfigurations.ToDictionary(s => s.IdServiceGroup);
        }
    }
}
