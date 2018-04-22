using LibraryExtensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using MediationModel;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Entity;
using System.Data.Common;
using TelcobrightMediation.Config;
using FlexValidation;
using TelcobrightMediation.Accounting;

namespace TelcobrightMediation
{
    public class MediationContext
    {
        public TelcobrightConfig Tbc { get; }
        public PartnerEntities Context { get; }
        public MefDecoderContainer MefDecoderContainer { get; set; }
        public MefServiceGroupsContainer MefServiceGroupContainer = new MefServiceGroupsContainer();
        public MefPartnerRulesContainer MefPartnerRuleContainer = new MefPartnerRulesContainer();
        public MefJobContainer MefJobContainer = new MefJobContainer();
        public MefNerRulesContainer MefNerRulesContainer = new MefNerRulesContainer();
        public FlexValidator<string[]> InconsistentCdrCheckListValidator { get; private set; }
        public FlexValidator<cdr> CommonMediationCheckListValidator { get; private set; }
        public MefServiceFamilyContainer MefServiceFamilyContainer { get; }
        public Dictionary<string, enumbillingspan> BillingSpans { get; }
        public Dictionary<ValueTuple<int,string>, route> Routes { get; }

        public List<ansprefixextra>
            LstAnsPrefixExtra
        {
            get;
            private set;
        } //required for failed intl in calls where term number might be missing

        public Dictionary<string, partnerprefix>
            DictAnsOrig { get; private set; } //ANSTermprefix partner dictionary with AnsPrefix as Key

        public Dictionary<int, cdrfieldlist> CdrFieldLists { get; private set; }
        public Dictionary<string, partner> Partners { get; set; }

        public Dictionary<int, Dictionary<int, ServiceGroupConfiguration>>
            ServiceGroupConfigurations { get; } //<switchid,dic<servicegroupID,medruleassignment>>

        public Dictionary<int, SwitchWiseLookup> SwitchWiseLookups { get; }
        public Dictionary<string, ne> Nes { get; } //load only nes for corresponding telcobright partner

        public MediationContext(TelcobrightConfig tbc, PartnerEntities context)
        {
            StaticExtInsertColumnParsedDic.Parse();
            this.Tbc = tbc;
            this.Context = context;
            this.MefDecoderContainer = new MefDecoderContainer(this.Context);
            this.MefServiceFamilyContainer = new MefServiceFamilyContainer();
            this.Nes = context.nes.Include(n=>n.telcobrightpartner)
                .Where(n => n.telcobrightpartner.databasename == tbc.DatabaseSetting.DatabaseName)
                .ToDictionary(c => c.idSwitch.ToString());
            this.SwitchWiseLookups = new Dictionary<int, SwitchWiseLookup>();
            this.ServiceGroupConfigurations = new Dictionary<int, Dictionary<int, ServiceGroupConfiguration>>();
            Dictionary<int, ServiceGroupConfiguration> dicmedInner = new Dictionary<int, ServiceGroupConfiguration>();
            foreach (ne ne in this.Nes.Values)
            {
                this.SwitchWiseLookups.Add(ne.idSwitch, new SwitchWiseLookup(context, ne.idSwitch));
                this.ServiceGroupConfigurations.Add(ne.idSwitch, dicmedInner);
            }
            this.BillingSpans = context.enumbillingspans.ToDictionary(c => c.ofbiz_uom_Id); //route data
            this.Routes = context.routes.ToDictionary(r => new ValueTuple<int,string>(r.SwitchId,r.RouteName));
            this.Partners = context.partners.ToDictionary(c => c.idPartner.ToString());
            this.DictAnsOrig = new Dictionary<string, partnerprefix>();
            this.DictAnsOrig = PopulateANSPrefix();
            this.LstAnsPrefixExtra = context.ansprefixextras.OrderByDescending(c => c.PrefixBeforeAnsNumber.Length)
                .ToList();
            this.CdrFieldLists = context.cdrfieldlists.ToDictionary(c => c.fieldnumber);
            //load extensions
            this.MefServiceFamilyContainer = new MefServiceFamilyContainer
            {
                DicRouteIncludingPartner = this.Routes,
                DicCountryCode = context.countrycodes.ToDictionary(c => c.Code),
                DicRateplans = context.rateplans.ToDictionary(c => c.id.ToString()),
                BillingRules = context.jsonbillingrules.ToList()
                    .Select(c => JsonBillingRuleToBillingRuleConverter.Convert(c)).ToDictionary(c => c.Id.ToString())
            };
            this.MefServiceFamilyContainer.PopulateCachedUsdBcs(this.Context);
            this.InconsistentCdrCheckListValidator = CreateValidatorInstanceWithValidationExpressions<string[]>(this.Tbc
                .CdrSetting.ValidationRulesForInconsistentCdrs);
            this.CommonMediationCheckListValidator =
                CreateValidatorInstanceWithValidationExpressions<cdr>(this.Tbc.CdrSetting.CommonMediationChecklist);
            ComposeMefExtensions(this.Tbc);
            CreateServiceGroupWiseFlexValidatorInstances();
            this.MefServiceGroupContainer.SwitchWiseRoutes =
                this.Routes; //assign the route dic to servicegroupdata
            this.MefPartnerRuleContainer.SwitchWiseRoutes =
                this.Routes; //assign the route dic to servicegroupdata

            //assign the route dic to servicegroupdata
            DateRange dRange = new DateRange() {StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1)};
            this.MefServiceFamilyContainer.RateCache =
                new RateCache(
                    context.telcobrightpartners.First(c => c.databasename == this.Tbc.DatabaseSetting.DatabaseName)
                        .RateDictionaryMaxRecords, this.Context)
                {
                    DicRatePlan = context.rateplans.Include("enumbillingspan")
                        .ToDictionary(c => c.id.ToString())
                };

            List<rateplanassignmenttuple> rateplanassignmenttuples
                = context.rateplanassignmenttuples.Include("billingruleassignment").ToList();
            this.MefServiceFamilyContainer.IdWiseRateplanAssignmenttuplesIncludingBillingRules
                = rateplanassignmenttuples.ToDictionary(c => c.id.ToString());
            Dictionary<string, List<rateplanassignmenttuple>> serviceGroupWiseRatePlanAssignmentTuples =
                rateplanassignmenttuples.GroupBy(c => c.billingruleassignment.idServiceGroup)
                    .ToDictionary(g => g.Key.ToString(), g => g.ToList());
            foreach (var kv in serviceGroupWiseRatePlanAssignmentTuples)
            {
                this.MefServiceFamilyContainer.ServiceGroupWiseTupDefs
                    .Add(kv.Key, new TupleDefinitions(kv.Value));
            }
        }

        Dictionary<string, partnerprefix> PopulateANSPrefix()
        {
            //load ans prefix from partnerprefix
            Dictionary<string, partnerprefix> dictAnsOrig = new Dictionary<string, partnerprefix>();
            string sql = " select idpartner as partner,prefix from partnerprefix where prefixtype=3 ";
            using (DbCommand commandOrig = ConnectionManager.CreateCommandFromDbContext(this.Context, sql))
            {
                commandOrig.CommandType = CommandType.Text;
                DbDataReader myReader = commandOrig.ExecuteReader();
                while (myReader.Read())
                {
                    partnerprefix thisPrefix = new partnerprefix();
                    if (myReader[1].ToString() != "") thisPrefix.Prefix = myReader[1].ToString();
                    thisPrefix.idPartner = int.Parse(myReader[0].ToString());
                    dictAnsOrig.Add(thisPrefix.Prefix, thisPrefix);
                }
                myReader.Close();
            }
            return dictAnsOrig;
        }

        private void ComposeMefExtensions(TelcobrightConfig tbc)
        {
            this.MefDecoderContainer.CmpDecoder.Compose();
            foreach (var ext in this.MefDecoderContainer.CmpDecoder.Decoders)
                this.MefDecoderContainer.DicExtensions.Add(ext.Id, ext);
            this.MefServiceGroupContainer.CmpServiceGroup.Compose();
            foreach (var ext in this.MefServiceGroupContainer.CmpServiceGroup.ServiceGroups)
            {
                this.MefServiceGroupContainer.DicExtensions.Add(ext.RuleName.ToString(), ext);
                this.MefServiceGroupContainer.IdServiceGroupWiseServiceGroups
                    .Add(ext.Id, ext); //this is required during summary generation
            }
            this.MefPartnerRuleContainer.CmpPartner.Compose();
            foreach (var ext in this.MefPartnerRuleContainer.CmpPartner.Partners)
                this.MefPartnerRuleContainer.DicExtensions.Add(ext.Id, ext);
            this.MefServiceFamilyContainer.CmpServiceFamily.Compose();
            foreach (var ext in this.MefServiceFamilyContainer.CmpServiceFamily.ServiceFamilys)
                this.MefServiceFamilyContainer.DicExtensions.Add(ext.Id, ext);
            this.MefJobContainer.CmpJob.Compose();
            foreach (var ext in this.MefJobContainer.CmpJob.Jobs)
            {
                this.MefJobContainer.DicExtensions.Add(ext.RuleName, ext);
                this.MefJobContainer.DicExtensionsIdJobWise.Add(ext.Id.ToString(), ext);
            }
            this.MefNerRulesContainer.NerComposer.Compose();
            foreach (var ext in this.MefNerRulesContainer.NerComposer.NerRules)
                this.MefNerRulesContainer.DicExtensions.Add(ext.RuleName, ext);
        }

        private void CreateServiceGroupWiseFlexValidatorInstances()
        {
            foreach (KeyValuePair<int, IServiceGroup> kv in this.MefServiceGroupContainer.IdServiceGroupWiseServiceGroups)
            {
                if (this.Tbc.CdrSetting.ServiceGroupConfigurations.ContainsKey(kv.Key))
                {
                    this.MefServiceGroupContainer.MediationChecklistValidatorForAnsweredCdrs.Add(kv.Key,
                        CreateValidatorInstanceWithValidationExpressions<cdr>(this.Tbc.CdrSetting
                            .ServiceGroupConfigurations[kv.Key]
                            .MediationChecklistForAnsweredCdrs));
                    this.MefServiceGroupContainer.MediationChecklistValidatorForUnAnsweredCdrs.Add(kv.Key,
                        CreateValidatorInstanceWithValidationExpressions<cdr>(this.Tbc.CdrSetting
                            .ServiceGroupConfigurations[kv.Key]
                            .MediationChecklistForUnAnsweredCdrs));
                }
            }
        }

        private FlexValidator<T> CreateValidatorInstanceWithValidationExpressions<T>(
            Dictionary<string, string> validationExpressions)
        {
            var flexValidator = new FlexValidator<T>(continueOnError: false,
                throwExceptionOnFirstError: false,
                validationExpressionsWithErrorMessage: validationExpressions);
            flexValidator.DateParsers.Add("strToMySqlDtConverter", str => str.ConvertToDateTimeFromMySqlFormat());
            flexValidator.DoubleParsers.Add("doubleConverterProxy", str => Convert.ToDouble(str));
            flexValidator.IntParsers.Add("intConverterProxy", str => Convert.ToInt32(str));
            flexValidator.BooleanParsers.Add("isDateTimeChecker",
                str => str.IsDateTime(StringExtensions.MySqlDateTimeFormat));
            flexValidator.BooleanParsers.Add("isNumericChecker", str => str.IsNumeric());
            return flexValidator;
        }
    }
}
