using System;
using System.Collections.Generic;
using System.Linq;
using MediationModel;
using MediationModel.enums;
using TelcobrightMediation.Cdr;

namespace TelcobrightMediation
{
    class IncrementalDurationVerifier
    {
        private CdrSummaryType SummaryTableName { get; }
        private decimal prevDurationInSummaryTable = 0;
        private decimal receivedDurationFromNewCdr = 0;
        private decimal receivedDurationFromNewSummary = 0;
        private CdrJobContext CdrJobContext { get; }
        private CdrProcessor cdrProcessor { get; }
        public decimal MergedDurationInSummaryContext { get; private set; }

        public IncrementalDurationVerifier(CdrProcessor cdrProcessor, CdrSummaryType summaryTableName)
        {
            this.cdrProcessor = cdrProcessor;
            this.CdrJobContext = cdrProcessor.CdrJobContext;
            this.SummaryTableName = summaryTableName;
            string sql = $@"SELECT sum(actualduration) actualduration
                        FROM {summaryTableName}
                        where tup_starttime in ('2017-12-01 00:00:00')";

            var cmd = this.CdrJobContext.DbCmd;
            cmd.CommandText = sql;
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (reader.IsDBNull(0) == false)
                    this.prevDurationInSummaryTable = reader.GetDecimal(0);
            }
            reader.Close();
        }

        public void Verify(CdrExt cdrExt)
        {
            decimal durationInCdr = cdrExt.Cdr.DurationSec;
            AbstractCdrSummary cdrSummary;
            cdrExt.TableWiseSummaries.TryGetValue(this.SummaryTableName,out cdrSummary);
            decimal durationInSummary = cdrSummary.actualduration;
            if (durationInCdr != durationInSummary)
                throw new Exception("duration in cdr & summary does not match.");
            this.receivedDurationFromNewCdr += durationInCdr;
            this.receivedDurationFromNewSummary += durationInSummary;
            var summaryCache = this.CdrJobContext.CdrSummaryContext.TableWiseSummaryCache[this.SummaryTableName];
            this.MergedDurationInSummaryContext = summaryCache.GetItems().Sum(s => s.actualduration);
            if ((this.MergedDurationInSummaryContext - this.receivedDurationFromNewSummary) !=
                this.prevDurationInSummaryTable)
                throw new Exception(
                    "Increment of merged duration in summaryContext & prev duration from summary does not match.");
            decimal differenceMergedVsPrevDurationInSummary =
                this.MergedDurationInSummaryContext - this.prevDurationInSummaryTable;
            decimal insertedItemsSum = summaryCache.GetInsertedItems().Sum(s => s.actualduration);
            if (differenceMergedVsPrevDurationInSummary != this.receivedDurationFromNewSummary)
                throw new Exception("differenceMergedVsPrevDurationInSummary != this.accumulatedNewSummaryTotal");

            var insertedkeys = summaryCache.GetInsertedItems().Select(item => item.GetTupleKey());
            decimal nonInsertedItemsSum = summaryCache.GetItems().Where(i => !insertedkeys.Contains(i.GetTupleKey()))
                .Sum(c => c.actualduration);
            decimal sumOfInsertedVsNonInsertedItem = insertedItemsSum + nonInsertedItemsSum;
            decimal totalInCache = summaryCache.GetItems().Sum(c => c.actualduration);
            if (totalInCache != sumOfInsertedVsNonInsertedItem)
                throw new Exception(
                    "Sum of total & inserted+nonInserted item must match after each transaction in cache.");
        }
    }
}