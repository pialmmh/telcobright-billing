using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexValidation;
using LibraryExtensions;
using MediationModel;
using Newtonsoft.Json;
using TelcobrightMediation;
using TelcobrightMediation.Cdr;

namespace TelcobrightMediation
{
    public class SegmentedCdrErrorProcessor : AbstractRowBasedSegmentedJobProcessor
    {
        private CdrCollectorInputData CdrCollectorInput { get; }
        protected int RawCount, NonPartialCount, UniquePartialCount, RawPartialCount, DistinctPartialCount = 0;
        protected decimal RawDurationTotalOfConsistentCdrs = 0;

        protected bool PartialCollectionEnabled => this.CdrCollectorInput.MediationContext.Tbc.CdrSetting
            .PartialCdrEnabledNeIds.Contains(this.CdrCollectorInput.Ne.idSwitch);

        public SegmentedCdrErrorProcessor(CdrCollectorInputData cdrCollectorInput,
            int batchSizeWhenPreparingLargeSqlJob,
            string indexedColumnName, string dateColumnName)
            : base(cdrCollectorInput.TelcobrightJob, cdrCollectorInput.Context, batchSizeWhenPreparingLargeSqlJob,
                indexedColumnName, dateColumnName)
        {
            this.CdrCollectorInput = cdrCollectorInput;
        }

        public override ISegmentedJob CreateJobSegmentInstance(jobsegment jobSegment)
        {
            RowIdsCollectionForSingleDay dayWiseRowsIdsCollection = base.DeserializeDayWiseRowIdsCollection(jobSegment);
            CdrRowCollector<cdrerror> dbRowCollector =
                new CdrRowCollector<cdrerror>(this.CdrCollectorInput.CdrJobInputData,
                    dayWiseRowsIdsCollection.GetSelectSql());
            List<string[]> txtRows = dbRowCollector.CollectAsTxtRows();
            NewCdrPreProcessor preProcessor =
                new NewCdrPreProcessor(txtRows, new List<cdrinconsistent>(), this.CdrCollectorInput);
            MefValidator<string[]> inconsistentValidator = CreateValidatorInstance();
            Parallel.ForEach(txtRows, txtRow =>
            {
                preProcessor
                    .CheckAndConvertIfInconsistent(this.CdrCollectorInput.CdrJobInputData, inconsistentValidator,
                        txtRow);
                cdrinconsistent cdrInconsistent = null;
                preProcessor.ConvertToCdr(txtRow, out cdrInconsistent);
                if (cdrInconsistent != null) preProcessor.InconsistentCdrs.Add(cdrInconsistent);
            });
            CdrCollectionResult newCollectionResult = null, oldCollectionResult = null;
            preProcessor.GetCollectionResults(out newCollectionResult, out oldCollectionResult);

            PartialCdrTesterData partialCdrTesterData =
                OrganizeTestDataForPartialCdrs(preProcessor, newCollectionResult);
            CdrJob cdrJob =
                (new CdrJobFactory(this.CdrCollectorInput.CdrJobInputData, this.RawCount)).CreateCdrJob(preProcessor,
                    newCollectionResult, oldCollectionResult, partialCdrTesterData);
            return cdrJob;
        }

        public PartialCdrTesterData OrganizeTestDataForPartialCdrs(NewCdrPreProcessor preProcessor,
            CdrCollectionResult newCollectionResult)
        {
            this.RawCount = preProcessor.RawCount;
            newCollectionResult.RawDurationTotalOfConsistentCdrs =
                preProcessor.NonPartialCdrs.Sum(c => c.DurationSec) + preProcessor.PartialCdrContainers
                    .SelectMany(pc => pc.NewRawInstances).Sum(r => r.DurationSec);
            this.RawDurationTotalOfConsistentCdrs = newCollectionResult.RawDurationTotalOfConsistentCdrs;
            PartialCdrTesterData partialCdrTesterData = null;
            if (this.PartialCollectionEnabled)
            {
                this.NonPartialCount = preProcessor.TxtCdrRows.Count(r => r[Fn.Partialflag] == "0");
                List<string[]> partialRows = preProcessor.TxtCdrRows.Where(r =>
                    this.CdrCollectorInput.CdrSetting.PartialCdrFlagIndicators.Contains(r[Fn.Partialflag])).ToList();
                this.RawPartialCount = partialRows.Count;
                if (preProcessor.TxtCdrRows.Count != this.NonPartialCount + this.RawPartialCount)
                    throw new Exception(
                        "TxtCdr rows with partial & non-partial flag do not match total decoded text rows");
                this.DistinctPartialCount = partialRows.GroupBy(r => r[Fn.UniqueBillId]).Count();
                partialCdrTesterData = new PartialCdrTesterData(this.NonPartialCount, this.RawCount,
                    newCollectionResult.RawDurationTotalOfConsistentCdrs, this.RawPartialCount);
            }
            return partialCdrTesterData;
        }

        private MefValidator<string[]> CreateValidatorInstance()
        {
            List<IValidationRule<string[]>> rules = this.CdrCollectorInput.CdrJobInputData.MediationContext.Tbc
                .CdrSetting.ValidationRulesForInconsistentCdrs;
            rules = rules.Where(rule => rule.ValidationMessage.StartsWith("SequenceNumber") == false
                                        || rule.ValidationMessage.StartsWith("StartTime") == false).ToList();
            var validator = new MefValidator<string[]>(continueOnError: false,
                throwExceptionOnFirstError: false, rules: rules);
            return validator;
        }
    }
}
