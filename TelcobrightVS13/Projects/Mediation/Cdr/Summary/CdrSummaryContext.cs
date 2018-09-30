using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;
using MediationModel.enums;
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Cache;
using TelcobrightMediation.Config;
using CdrSummaryTuple = System.ValueTuple<int, int, int, string, string, decimal, decimal, System.ValueTuple<string, string, string, string, string, string, string, System.ValueTuple<string, string, string, string, string, string>>>;

namespace TelcobrightMediation.Cdr
{
    public class CdrSummaryContext
    {
        public ConcurrentDictionary<CdrSummaryType, SummaryCache<AbstractCdrSummary, CdrSummaryTuple>> TableWiseSummaryCache { get; }
            = new ConcurrentDictionary<CdrSummaryType, SummaryCache<AbstractCdrSummary, CdrSummaryTuple>>();
        public Dictionary<CdrSummaryType, CdrSummaryFactory<CdrExt>> TargetTableWiseSummaryFactory { get; }
        private MediationContext MediationContext { get; }
        private AutoIncrementManager AutoIncrementManager { get; }
        public List<DateTime> DatesInvolved { get; } //set by new cdrs only, which covers old cdr case as well.
        public List<DateTime> HoursInvolved { get; } //set by new cdrs only, which covers old cdr case as well.
        private PartnerEntities Context { get; }

        public CdrSummaryContext(MediationContext mediationContext,
            AutoIncrementManager autoIncrementManager, PartnerEntities context, List<DateTime> hoursInvolved)
        {
            this.MediationContext = mediationContext;
            this.AutoIncrementManager = autoIncrementManager;
            this.HoursInvolved = hoursInvolved;
            this.DatesInvolved = hoursInvolved.Select(c => c.Date).Distinct().ToList();
            this.Context = context;
            this.TargetTableWiseSummaryFactory = this.MediationContext.MefServiceGroupContainer
                .IdServiceGroupWiseServiceGroups
                .Values.SelectMany(sg => sg.GetSummaryTargetTables().Keys.ToList())
                .Distinct()
                .Select(tableName => new
                {
                    SummaryTableName = tableName,
                    CdrSummaryFactory =
                    CdrSummaryFactoryFactory.Create(tableName, this.MediationContext.MefServiceGroupContainer)
                }).ToDictionary(annonymous => annonymous.SummaryTableName, annonymous => annonymous.CdrSummaryFactory);
        }
        public void GenerateSummary(CdrExt cdrExt)
        {
            List<CdrSummaryType> summaryTargetTables = this.MediationContext.MefServiceGroupContainer
                .IdServiceGroupWiseServiceGroups[cdrExt.Cdr.ServiceGroup]
                .GetSummaryTargetTables().Keys.ToList();
            summaryTargetTables.ForEach(targetTableName =>
            {
                CdrSummaryFactory<CdrExt> cdrSummaryFactory;
                if(!this.TargetTableWiseSummaryFactory.TryGetValue(targetTableName,out cdrSummaryFactory))
                    throw new Exception("Cdrsummary factory not found for table name="+targetTableName);
                AbstractCdrSummary cdrSummary = (AbstractCdrSummary)cdrSummaryFactory.CreateNewInstance(cdrExt);
                cdrExt.TableWiseSummaries.Add(targetTableName, cdrSummary);
            });
        }
        public void PopulatePrevSummary()
        {
            foreach (int serviceGroupNumber in this.MediationContext.Tbc.CdrSetting.ServiceGroupConfigurations.Keys)
            {
                IServiceGroup serviceGroup = null;
                this.MediationContext.MefServiceGroupContainer.IdServiceGroupWiseServiceGroups.TryGetValue(
                    serviceGroupNumber, out serviceGroup);
                if (serviceGroup != null)
                {
                    Dictionary<CdrSummaryType, Type> summaryTargetTables = serviceGroup.GetSummaryTargetTables();
                    foreach (CdrSummaryType summaryTableName in summaryTargetTables.Keys)
                    {
                        var summaryCache = CreateSummaryCacheInstance(summaryTableName);
                        if(this.TableWiseSummaryCache.TryAdd(summaryTableName, summaryCache)==false)
                            throw  new Exception("Could not add to concurrent dictionary TableWiseSummary " +
                                                 "in CdrSummaryContext");
                        List<DateTime> selectedDateTimes;
                        if (summaryTableName.ToString().Contains("day"))
                        {
                            selectedDateTimes = this.DatesInvolved;
                        }
                        else if (summaryTableName.ToString().Contains("hr"))
                        {
                            selectedDateTimes = this.HoursInvolved;
                        }
                        else
                            throw new Exception(
                                "Cdrsummary type must contain 'day' or 'hr' in service group configuration");
                        TimeWiseSummaryCachePopulator<AbstractCdrSummary, CdrSummaryTuple> timeWiseSummaryCachePopulator
                            = new TimeWiseSummaryCachePopulator<AbstractCdrSummary, CdrSummaryTuple>
                                (summaryCache, this.Context, "tup_starttime", selectedDateTimes);
                        timeWiseSummaryCachePopulator.Populate();
                    }
                }
            }
        }
        public void ValidateDayVsHourWiseSummaryCollection()
        {
            var daySummaryCaches = this.TableWiseSummaryCache.Where(kv => kv.Key.ToString().Contains("_day_"))
                .ToDictionary(kv => kv.Key, kv => kv.Value);
            var hourSummaryCaches = this.TableWiseSummaryCache.Where(kv => kv.Key.ToString().Contains("_hr_"))
                .ToDictionary(kv => kv.Key, kv => kv.Value);
            foreach (var kv in daySummaryCaches)
            {
                SummaryCache<AbstractCdrSummary, CdrSummaryTuple> dayWiseSummaryCache = kv.Value;
                string cdrSummaryTypeStr = kv.Key.ToString();
                SummaryCache<AbstractCdrSummary, CdrSummaryTuple> hourWiseSummaryCache;
                CdrSummaryType cdrSummaryType = CdrSummaryTypeDictionary.GetTypes()[cdrSummaryTypeStr.Replace("day", "hr")];
                hourSummaryCaches.TryGetValue(cdrSummaryType,out hourWiseSummaryCache);
                var daySumOfDuration = dayWiseSummaryCache.GetItems().Sum(s => s.actualduration);
                var hourWiseSumOfDuration = hourWiseSummaryCache.GetItems().Sum(s => s.actualduration);
                //if (Math.Abs(daySumOfDuration - hourWiseSumOfDuration) > this.MediationContext.Tbc.CdrSetting.FractionalNumberComparisonTollerance)
                  //  throw new Exception("Collected day & hour wise summary duration do not match.");
            }
        }
        private SummaryCache<AbstractCdrSummary, CdrSummaryTuple>
            CreateSummaryCacheInstance(CdrSummaryType summaryTableName)
        {
            Func<AbstractCdrSummary,string> whereClauseBuilder = summary =>
                $@" where id={summary.id} and tup_starttime={summary.tup_starttime.ToMySqlField()}";
            SummaryCache<AbstractCdrSummary, CdrSummaryTuple> cachedSummary =
                new SummaryCache<AbstractCdrSummary, CdrSummaryTuple>
                (summaryTableName.ToString(),this.AutoIncrementManager, e => e.GetTupleKey(), e => e.GetExtInsertValues(),
                    e => e.GetUpdateCommand(whereClauseBuilder).Replace("AbstractCdrSummary",summaryTableName.ToString()), 
                    null);
            return cachedSummary;
        }

        public AbstractCdrSummary CreateSummaryInstanceForSingleTable(CdrExt cdrExt, CdrSummaryType targetTableName)
        {
            CdrSummaryFactory<CdrExt> cdrSummaryFactory;
            if (!this.TargetTableWiseSummaryFactory.TryGetValue(targetTableName, out cdrSummaryFactory))
                throw new Exception("Could not find cdrSummaryFactory");
            return (AbstractCdrSummary) cdrSummaryFactory.CreateNewInstance(cdrExt);
        }

        public void MergeAddSummary(CdrSummaryType summaryTableName, AbstractCdrSummary cdrSummary)
        {
            SummaryCache<AbstractCdrSummary, CdrSummaryTuple> targetSummaryCacheForMergeSubstract;
            if (!this.TableWiseSummaryCache.TryGetValue(summaryTableName, out targetSummaryCacheForMergeSubstract))
            {
                throw new Exception("Target summary cache not found for summary merge substraction.");
            }
            targetSummaryCacheForMergeSubstract.Merge(cdrSummary, SummaryMergeType.Add, sum => sum.id > 0);
        }
        public void MergeSubstractSummary(CdrSummaryType summaryTableName, AbstractCdrSummary cdrSummary)
        {
            SummaryCache<AbstractCdrSummary, CdrSummaryTuple> targetSummaryCacheForMergeAdd;
            if (!this.TableWiseSummaryCache.TryGetValue(summaryTableName, out targetSummaryCacheForMergeAdd))
            {
                throw new Exception("Target summary cache not found for summary merge addition.");
            }
            targetSummaryCacheForMergeAdd.Merge(cdrSummary, SummaryMergeType.Substract, sum => sum.id > 0);
        }
    }
}
