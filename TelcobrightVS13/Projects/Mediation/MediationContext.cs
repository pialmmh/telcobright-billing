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
using static System.String;

namespace TelcobrightMediation
{
    public class MediationContext
    {
        public TelcobrightConfig Tbc { get; }
        public CdrSetting CdrSetting => this.Tbc.CdrSetting;
        public PartnerEntities Context { get; }
        public AutoIncrementManager AutoIncrementManager { get; }
        private AutomationContainer AutomationContainer { get; }= new AutomationContainer();
        public MefDecoderContainer MefDecoderContainer { get; set; }
        public MefServiceGroupsContainer MefServiceGroupContainer = new MefServiceGroupsContainer();
        public MefPartnerRulesContainer MefPartnerRuleContainer = new MefPartnerRulesContainer();
        public MefJobContainer MefJobContainer = new MefJobContainer();
        public MefNerRulesContainer MefNerRulesContainer = new MefNerRulesContainer();
        public MefValidator<string[]> InconsistentCdrCheckListValidator { get; private set; }
        public MefValidator<cdr> CommonMediationCheckListValidator { get; private set; }
        public MefServiceFamilyContainer MefServiceFamilyContainer { get; }
        public MefExceptionalCdrPreProcessorContainer MefExceptionalCdrPreProcessorContainer = 
            new MefExceptionalCdrPreProcessorContainer();
        public Dictionary<string, enumbillingspan> BillingSpans { get; }
        public Dictionary<ValueTuple<int,string>, route> Routes { get; }
        public Dictionary<ValueTuple<int, string>, bridgedroute> BridgedRoutes { get; }
        public List<ansprefixextra> LstAnsPrefixExtra{get;private set;} //required for failed intl in calls where term number might be missing
        public Dictionary<string, partnerprefix>
            DictAnsOrig { get; private set; } //ANSTermprefix partner dictionary with AnsPrefix as Key
        public Dictionary<int, cdrfieldlist> CdrFieldLists { get; private set; }
        public Dictionary<int, Dictionary<int, ServiceGroupConfiguration>>
            ServiceGroupConfigurations { get; } //<switchid,dic<servicegroupID,medruleassignment>>
        public Dictionary<int, SwitchWiseLookup> SwitchWiseLookups { get; }
        public Dictionary<string, ne> Nes { get; } //load only nes for corresponding telcobright partner
        public Dictionary<int,partner> Partners { get; }
        public MediationContext(TelcobrightConfig tbc, PartnerEntities context)
        {
            StaticExtInsertColumnParsedDic.Parse();
            this.Tbc = tbc;
            this.Context = context;
            this.AutoIncrementManager = new AutoIncrementManager(
                counter=>(int)AutoIncrementTypeDictionary.EnumTypes[counter.tableName],
                counter=>counter.GetExtInsertValues(),
                counter=>counter.GetUpdateCommand(
                    c=>$@" where tableName='{AutoIncrementTypeDictionary.EnumTypes[counter.tableName]}'"),
                null, this.Context.Database.Connection.CreateCommand(),this.CdrSetting.SegmentSizeForDbWrite);
            this.AutoIncrementManager.PopulateCache(() => context.autoincrementcounters
            .ToDictionary(c => (int)AutoIncrementTypeDictionary.EnumTypes[c.tableName]));

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
            this.Routes = context.routes.Include(r=>r.partner)
                .ToDictionary(r => new ValueTuple<int,string>(r.SwitchId,r.RouteName));
            this.BridgedRoutes = context.bridgedroutes.Include(r => r.partner).Include(r=>r.partner1)
                .ToDictionary(r => new ValueTuple<int, string>(r.switchId, r.routeName));
            this.DictAnsOrig = new Dictionary<string, partnerprefix>();
            this.DictAnsOrig = PopulateANSPrefix();
            this.LstAnsPrefixExtra = context.ansprefixextras.OrderByDescending(c => c.PrefixBeforeAnsNumber.Length)
                .ToList();
            this.CdrFieldLists = context.cdrfieldlists.ToDictionary(c => c.fieldnumber);
            //load extensions
            this.MefServiceFamilyContainer = new MefServiceFamilyContainer
            {
                DicRouteIncludingPartner = this.Routes,
                DicRateplans = context.rateplans.ToDictionary(c => c.id),
                BillingRules = context.jsonbillingrules.ToList()
                    .Select(c => JsonBillingRuleToBillingRuleConverter.Convert(c)).ToDictionary(c => c.Id)
            };
            this.MefServiceFamilyContainer.PopulateCachedUsdBcs(this.Context);
            this.InconsistentCdrCheckListValidator = CreateValidatorInstanceFromRules<string[]>(this.Tbc
                .CdrSetting.ValidationRulesForInconsistentCdrs);
            this.CommonMediationCheckListValidator =
                CreateValidatorInstanceFromRules<cdr>(this.Tbc.CdrSetting.ValidationRulesForCommonMediationCheck);
            ComposeMefExtensions(this.Tbc);
            CreateServiceGroupWiseValidatorInstances();
            this.MefServiceGroupContainer.SwitchWiseRoutes =
                this.Routes; //assign the route dic to servicegroupdata
            this.MefPartnerRuleContainer.SwitchWiseRoutes =
                this.Routes; //assign the route dic to servicegroupdata

            //assign the route dic to servicegroupdata
            DateRange dRange = new DateRange() {StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1)};
            this.MefServiceFamilyContainer.RateCache =
                new RateCache(
                    context.telcobrightpartners.First(c => c.databasename == this.Tbc.Telcobrightpartner.databasename)
                        .RateDictionaryMaxRecords, this.Context)
                {
                    DicRatePlan = context.rateplans.Include("enumbillingspan")
                        .ToDictionary(c => c.id.ToString())
                };

            List<rateplanassignmenttuple> rateplanassignmenttuples
                = context.rateplanassignmenttuples.Include("billingruleassignment").ToList();
            this.MefServiceFamilyContainer.IdWiseRateplanAssignmenttuplesIncludingBillingRules
                = rateplanassignmenttuples.ToDictionary(c => c.id);
            Dictionary<int, List<rateplanassignmenttuple>> serviceGroupWiseRatePlanAssignmentTuples =
                rateplanassignmenttuples.GroupBy(c => c.billingruleassignment.idServiceGroup)
                    .ToDictionary(g => g.Key, g => g.ToList());
            foreach (var kv in serviceGroupWiseRatePlanAssignmentTuples)
            {
                this.MefServiceFamilyContainer.ServiceGroupWiseTupDefs.Add(kv.Key, new TupleDefinitions(kv.Value));
            }
            CdrSummaryTypeDictionary.Initialize();
            CreateTemporaryTables();
            this.Partners = context.partners.ToDictionary(c => c.idPartner);
            this.MefPartnerRuleContainer.MediationContext = this;
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
            this.AutomationContainer.Compose();

            this.MefDecoderContainer.CmpDecoder.Compose();
            foreach (var ext in this.MefDecoderContainer.CmpDecoder.Decoders)
                this.MefDecoderContainer.DicExtensions.Add(ext.Id, ext);
            this.MefServiceGroupContainer.CmpServiceGroupPre.Compose();
            foreach (var serviceGroupPreProcessor 
                in this.MefServiceGroupContainer.CmpServiceGroupPre.ServiceGroupPreProcessors)
            {
                this.MefServiceGroupContainer.ServiceGroupPreProcessors.Add(serviceGroupPreProcessor.Id,
                    serviceGroupPreProcessor);
            }
            this.MefServiceGroupContainer.CmpServiceGroup.Compose();
            CdrRuleComposer cdrRuleComposer = new CdrRuleComposer();
            cdrRuleComposer.Compose();
            foreach (var serviceGroup in this.MefServiceGroupContainer.CmpServiceGroup.ServiceGroups)
            {
                ServiceGroupConfiguration serviceGroupConfiguration = null;
                tbc.CdrSetting.ServiceGroupConfigurations.TryGetValue(serviceGroup.Id,
                    out serviceGroupConfiguration);
                if (serviceGroupConfiguration != null)
                {
                    Dictionary<string, object> additionalParams = serviceGroupConfiguration.Params
                        ?.ToDictionary(kv => kv.Key, kv => (object)kv.Value);
                    object configuredCdrRules = "";
                    additionalParams?.TryGetValue("idCdrRules", out configuredCdrRules);
                    string cdrRulesAsStr = configuredCdrRules as string;
                    List<ICdrRule> cdrRules = new List<ICdrRule>();
                    if (!IsNullOrEmpty(cdrRulesAsStr) && !IsNullOrWhiteSpace(cdrRulesAsStr))
                    {
                        List<int> configuredIdCdrRules = cdrRulesAsStr.Split(',')
                            .Select(str => Convert.ToInt32(str)).ToList();
                        cdrRuleComposer.CdrRules
                            .Where(c => configuredIdCdrRules.Contains(c.Id)).ToList().ForEach(cdrRule =>
                            {
                                cdrRule.Prepare(this);
                                cdrRules.Add(cdrRule);
                            });
                    }
                    additionalParams?.Add("cdrRules",cdrRules);
                    serviceGroup.SetAdditionalParams(additionalParams);
                }

                this.MefServiceGroupContainer.DicExtensions.Add(serviceGroup.RuleName, serviceGroup);
                this.MefServiceGroupContainer.IdServiceGroupWiseServiceGroups
                    .Add(serviceGroup.Id, serviceGroup); //this is required during summary generation
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
            DigitRuleComposer digitRuleComposer=new DigitRuleComposer();
            digitRuleComposer.Compose();
            this.MefServiceFamilyContainer.DigitRules = digitRuleComposer.DigitRules.ToDictionary(r => r.Id);
            this.MefExceptionalCdrPreProcessorContainer.composer.Compose();
            foreach(var ext in this.MefExceptionalCdrPreProcessorContainer.composer.ExceptionalCdrPreProcessors) {
                ext.Prepare(this);
                this.MefExceptionalCdrPreProcessorContainer.DicExtensions.Add(ext.RuleName, ext);
            }
            
        }

        private void CreateServiceGroupWiseValidatorInstances()
        {
            foreach (KeyValuePair<int, IServiceGroup> kv in this.MefServiceGroupContainer.IdServiceGroupWiseServiceGroups)
            {
                if (this.Tbc.CdrSetting.ServiceGroupConfigurations.ContainsKey(kv.Key))
                {
                    this.MefServiceGroupContainer.MediationChecklistValidatorForAnsweredCdrs.Add(kv.Key,
                        CreateValidatorInstanceFromRules<cdr>(this.Tbc.CdrSetting
                            .ServiceGroupConfigurations[kv.Key]
                            .MediationChecklistForAnsweredCdrs));
                    this.MefServiceGroupContainer.MediationChecklistValidatorForUnAnsweredCdrs.Add(kv.Key,
                        CreateValidatorInstanceFromRules<cdr>(this.Tbc.CdrSetting
                            .ServiceGroupConfigurations[kv.Key]
                            .MediationChecklistForUnAnsweredCdrs));
                }
            }
        }

        private MefValidator<T> CreateValidatorInstanceFromRules<T>(List<IValidationRule<T>> rules)
        {
            var mefValidator = new MefValidator<T>(continueOnError: false,
                                throwExceptionOnFirstError: false,
                                rules:rules);
            return mefValidator;
        }
        private void CreateTemporaryTables()
        {
            DbCommand cmd = this.Context.Database.Connection.CreateCommand();
            cmd.CommandType = CommandType.Text;

            cmd.CommandText = "drop table if exists temp_sql_statement;";
            cmd.ExecuteNonQuery();

            bool windowsDb = this.Tbc.DatabaseSetting.UseVarcharInsteadOfTextForMemoryEngine;
            string engine = windowsDb == false ? "memory" : "innodb";
            cmd.CommandText = $@"create temporary table temp_sql_statement(
                                        id int primary key auto_increment,
                                        statement text not null) engine={engine};";
            
            cmd.ExecuteNonQuery();
            DropAndCreateTempRateTable(cmd);
        }

        public static void DropAndCreateTempRateTable(DbCommand cmd)
        {
            cmd.CommandText = "drop table if exists temp_rate;";
            cmd.ExecuteNonQuery();

            cmd.CommandText = $@"create temporary table temp_rate  engine=memory
                                     select * from rate where 1=2;";
            cmd.ExecuteNonQuery();
            cmd.CommandText =
                "alter table temp_rate add index ind_rateplan_startdate_enddate (idrateplan,startdate,enddate);" +
                "alter table temp_rate add index ind_prefix_startdate (Prefix,startdate);" +
                "alter table temp_rate add index `ind_enddate` (`enddate`);";
            cmd.ExecuteNonQuery();
        }
    }
}
