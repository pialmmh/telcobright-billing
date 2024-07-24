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
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Config;

namespace InstallConfig
{
    public partial class SmsHubAbstractConfigGenerator //quartz config part
    {
        public Dictionary<int, ServiceGroupConfiguration> GetServiceGroupConfigurations()
        {
            List<ServiceGroupConfiguration> serviceGroupConfigurations = new List<ServiceGroupConfiguration>();
            serviceGroupConfigurations.Add(new ServiceGroupConfiguration(idServiceGroup: 1) //domestic
            {
                PartnerRules = new List<int>()
                {
                    PartnerRuletype.InAnsByPrefixSmsHub,
                    PartnerRuletype.OutAnsByPrefixSmsHub
                },
                Ratingtrules = new List<RatingRule>()
                {
                    new RatingRule() {IdServiceFamily = ServiceFamilyType.A2Z, AssignDirection = 1},
                },
                MediationChecklistForAnsweredCdrs =
                    new List<IValidationRule<cdr>>()
                    {
                        new DurationSecGtEq0(),
                        //new OutgoingRouteNotEmpty(),
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
            
            return serviceGroupConfigurations.ToDictionary(s => s.IdServiceGroup);
        }
    }
}
