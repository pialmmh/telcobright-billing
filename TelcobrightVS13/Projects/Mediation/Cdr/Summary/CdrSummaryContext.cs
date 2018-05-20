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
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Cache;
using TelcobrightMediation.Config;
using CdrSummaryTuple = System.ValueTuple<int, int, int, string, string, decimal, decimal, System.ValueTuple<string, string, string, string, string, string, string, System.ValueTuple<string, string, string, string, string, string>>>;

namespace TelcobrightMediation.Cdr
{
    public class CdrSummaryContext
    {
        public ConcurrentDictionary<string, SummaryCache<AbstractCdrSummary, CdrSummaryTuple>> TableWiseSummaryCache { get; }
            = new ConcurrentDictionary<string, SummaryCache<AbstractCdrSummary, CdrSummaryTuple>>();
        public Dictionary<string, CdrSummaryFactory<CdrExt>> TargetTableWiseSummaryFactory { get; }
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
            List<string> summaryTargetTables = this.MediationContext.MefServiceGroupContainer
                .IdServiceGroupWiseServiceGroups[cdrExt.Cdr.ServiceGroup]
                .GetSummaryTargetTables().Keys.ToList();
            summaryTargetTables.ForEach(targetTableName =>
            {
                AbstractCdrSummary cdrSummary = (AbstractCdrSummary)this
                    .TargetTableWiseSummaryFactory[targetTableName].CreateNewInstance(cdrExt);
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
                    Dictionary<string, Type> summaryTargetTables = serviceGroup.GetSummaryTargetTables();
                    foreach (string summaryTableName in summaryTargetTables.Keys)
                    {
                        var summaryCache = CreateSummaryCacheInstance(summaryTableName);
                        if(this.TableWiseSummaryCache.TryAdd(summaryTableName, summaryCache)==false)
                            throw  new Exception("Could not add to concurrent dictionary TableWiseSummary " +
                                                 "in CdrSummaryContext");
                        List<DateTime> selectedDateTimes;
                        if (summaryTableName.Contains("day"))
                        {
                            selectedDateTimes = this.DatesInvolved;
                        }
                        else if (summaryTableName.Contains("hr"))
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
            var daySummaryCaches = this.TableWiseSummaryCache.Where(kv => kv.Key.Contains("_day_"))
                .ToDictionary(kv => kv.Key, kv => kv.Value);
            //todo: remove temp code
            var prevSummarySumInCache = daySummaryCaches.Values.SelectMany(c => c.GetItems())
                .Sum(s => s.actualduration);
            //temp code
            var hourSummaryCaches = this.TableWiseSummaryCache.Where(kv => kv.Key.Contains("_hr_"))
                .ToDictionary(kv => kv.Key, kv => kv.Value);
            foreach (var kv in daySummaryCaches)
            {
                SummaryCache<AbstractCdrSummary, CdrSummaryTuple> dayWiseSummaryCache = kv.Value;
                SummaryCache<AbstractCdrSummary, CdrSummaryTuple> hourWiseSummaryCache =
                    hourSummaryCaches[kv.Key.Replace("day", "hr")];
                var daySumOfDuration = dayWiseSummaryCache.GetItems().Sum(s => s.actualduration);
                var hourWiseSumOfDuration = hourWiseSummaryCache.GetItems().Sum(s => s.actualduration);
                //if (Math.Abs(daySumOfDuration - hourWiseSumOfDuration) > this.MediationContext.Tbc.CdrSetting.FractionalNumberComparisonTollerance)
                  //  throw new Exception("Collected day & hour wise summary duration do not match.");
            }
        }
        private SummaryCache<AbstractCdrSummary, CdrSummaryTuple>
            CreateSummaryCacheInstance(string summaryTableName)
        {
            Func<AbstractCdrSummary,string> whereClauseBuilder = summary =>
                $@" where id={summary.id} and tup_starttime={summary.tup_starttime.ToMySqlField()}";
            SummaryCache<AbstractCdrSummary, CdrSummaryTuple> cachedSummary =
                new SummaryCache<AbstractCdrSummary, CdrSummaryTuple>
                (summaryTableName,this.AutoIncrementManager, e => e.GetTupleKey(), e => e.GetExtInsertValues(),
                    e => e.GetUpdateCommand(whereClauseBuilder).Replace("AbstractCdrSummary",summaryTableName), 
                    null);
            return cachedSummary;
        }

        public AbstractCdrSummary CreateSummaryInstanceForSingleTable(CdrExt cdrExt, string targetTableName)
        {
            AbstractCdrSummary newSummary =
                (AbstractCdrSummary) this.TargetTableWiseSummaryFactory[targetTableName].CreateNewInstance(cdrExt);
            return newSummary;
        }

        public void MergeAddSummary(string summaryTableName, AbstractCdrSummary cdrSummary)
        {
            this.TableWiseSummaryCache[summaryTableName].Merge(cdrSummary, SummaryMergeType.Add, sum => sum.id > 0);
        }
        public void MergeSubstractSummary(string summaryTableName, AbstractCdrSummary cdrSummary)
        {
            this.TableWiseSummaryCache[summaryTableName].Merge(cdrSummary, SummaryMergeType.Substract, sum => sum.id > 0);
        }
    }
}
