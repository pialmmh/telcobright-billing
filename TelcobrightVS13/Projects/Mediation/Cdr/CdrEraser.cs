using LibraryExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TelcobrightMediation.Accounting;
using MediationModel;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web.ApplicationServices;
using FlexValidation;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;
using TelcobrightMediation.Mediation.Cdr;
using TransactionTuple = System.ValueTuple<int, int, long, int>;
using CdrSummaryTuple = System.ValueTuple<int, int, int, string, string, decimal, decimal, System.ValueTuple<string, string, string, string, string, string, string, System.ValueTuple<string, string, string, string, string, string>>>;

namespace TelcobrightMediation
{
    public class CdrEraser
    {
        public CdrJobContext CdrJobContext { get; }
        public CdrCollectionResult CollectionResult { get; }
        private List<CdrExt> OldCdrExts { get; }
        private Dictionary<string, List<acc_transaction>> BillIdWisePrevTransactions { get; set; } =
            new Dictionary<string, List<acc_transaction>>();

        private MediationContext MediationContext => this.CdrJobContext.MediationContext;

        public CdrEraser(CdrJobContext cdrJobContext, CdrCollectionResult newCollectionResult)
        {
            this.CdrJobContext = cdrJobContext;
            this.CollectionResult = newCollectionResult;
            this.CollectionResult.ConcurrentCdrExts.Values.ToList()
                .ForEach(c =>
                {
                    if (c.CdrNewOldType != CdrNewOldType.OldCdr)
                        throw new Exception("OldCdrs must have CdrNewOldtype status set to old.");
                    this.CollectionResult.ProcessedCdrExts.Add(c);
                });
            if (!this.CollectionResult.ProcessedCdrExts.Any())
                throw new Exception("ProcessedCdrExts cannot be empty in Cdr Erasing job.");
            this.OldCdrExts= this.CollectionResult.ProcessedCdrExts.ToList();
        }
        public void RegenerateOldSummaries()
        {
            //todo: change to parallel
            //Parallel.ForEach(oldCdrExts,oldCdrExt=>
            this.OldCdrExts.ForEach(oldCdrExt =>
            {
                if (oldCdrExt.CdrNewOldType != CdrNewOldType.OldCdr)
                    throw new Exception("OldCdrs must have CdrNewOldtype status set to old.");
                List<string> summaryTargetTables = this.MediationContext.MefServiceGroupContainer
                    .IdServiceGroupWiseServiceGroups[Convert.ToInt32(oldCdrExt.Cdr.ServiceGroup)]
                    .GetSummaryTargetTables().Keys.ToList();
                summaryTargetTables.ForEach(targetTableName =>
                {
                    AbstractCdrSummary regeneratedSummary = (AbstractCdrSummary) this.CdrJobContext.CdrSummaryContext
                        .TargetTableWiseSummaryFactory[targetTableName].CreateNewInstance(oldCdrExt);
                    oldCdrExt.TableWiseSummaries.Add(targetTableName, regeneratedSummary);
                });
            });
        }

        public void ValidateSummaryReGeneration()
        {
            decimal regeneratedSummaryDurationTotal = this.OldCdrExts.SelectMany(c => c.TableWiseSummaries.Values)
                .Sum(s => s.actualduration);
            var summaryDurationTotalFromCdrMetaData =
                Convert.ToDecimal(this.OldCdrExts.Sum(c => c.Cdr.SummaryMetaTotal));
            if (Math.Abs(regeneratedSummaryDurationTotal - summaryDurationTotalFromCdrMetaData) >
                this.CdrJobContext.MediationContext.Tbc.CdrSetting.FractionalNumberComparisonTollerance)
            {
                throw new Exception("Regenerated summary total do not match ");
            }
        }

        public void UndoOldSummaries()
        {
            //todo: change to parallel
            //Parallel.ForEach(oldCdrExts,oldCdrExt=>
            this.OldCdrExts.ForEach(oldCdrExt =>
            {
                foreach (var kv in oldCdrExt.TableWiseSummaries)
                {
                    string summaryTargetTable = kv.Key;
                    this.CdrJobContext.CdrSummaryContext.MergeSubstractSummary(summaryTargetTable, kv.Value);
                }
            });
        }

        public void UndoOldChargeables()
        {
            if (!this.CollectionResult.ProcessedCdrExts.Any())
                throw new Exception("ProcessedCdrExts cannot be empty in Cdr Erasing job.");
            var oldChargeables = this.CollectionResult.ProcessedCdrExts.SelectMany(c => c.Chargeables.Values).ToList();
            this.CdrJobContext.AccountingContext.ChargeableCache
                .PopulateCache(() => oldChargeables.ToDictionary(chargeable => chargeable.id.ToString()));
            this.CdrJobContext.AccountingContext.ChargeableCache.DeleteAll();
        }

        public void DeleteOldCdrs()
        {
            int delCount = OldCdrDeleter.DeleteOldCdrs("cdr", this.CollectionResult.ProcessedCdrExts
                    .Select(c => new KeyValuePair<long, DateTime>(c.Cdr.IdCall, c.StartTime)).ToList(),
                this.CdrJobContext.SegmentSizeForDbWrite, this.CdrJobContext.DbCmd);
            if (delCount != this.CollectionResult.RawCount)
                throw new Exception("Deleted number of cdrs do not match raw count in collection result.");
        }
    }
}
