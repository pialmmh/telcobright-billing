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
using TelcobrightMediation.Mediation.Cdr;

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
            : base(cdrCollectorInput.CdrSetting, cdrCollectorInput.TelcobrightJob, cdrCollectorInput.Context, batchSizeWhenPreparingLargeSqlJob,
                indexedColumnName, dateColumnName)
        {
            this.CdrCollectorInput = cdrCollectorInput;
        }

        public override ISegmentedJob CreateJobSegmentInstance(jobsegment jobSegment)
        {
            RowIdsCollectionForSingleDay rowIdsCollectionForSingleDay =
                base.DeserializeDayWiseRowIdsCollection(jobSegment);
            CdrRowCollector<cdrerror> dbRowCollector =
                new CdrRowCollector<cdrerror>(this.CdrCollectorInput.CdrJobInputData,
                    rowIdsCollectionForSingleDay.GetSelectSql());
            List<string[]> txtRows = dbRowCollector.CollectAsTxtRows();
            if (this.CdrCollectorInput.CdrSetting.AutoCorrectDuplicateBillIdBeforeErrorProcess == true)
            {
                txtRows = AbstractCdrJobPreProcessor.ChangeDuplicateBillIds(txtRows);
            }
            List<cdr> cdrs = new List<cdr>();
            txtRows.ForEach(c =>
            {
                cdrinconsistent inconsistentCdr = null;
                List<IExceptionalCdrPreProcessor> exceptionalCdrPreProcessor = new List<IExceptionalCdrPreProcessor>();
                cdr convertedCdr =
                    CdrConversionUtil.ConvertTxtRowToCdrOrInconsistentOnFailure(c,exceptionalCdrPreProcessor, out inconsistentCdr);
                cdrs.Add(convertedCdr);
            });
            
            CdrErrorPreProcessor preProcessor =
                new CdrErrorPreProcessor(this.CdrCollectorInput, cdrs);
            CdrCollectionResult newCollectionResult = null, oldCollectionResult = null;
            preProcessor.GetCollectionResults(out newCollectionResult, out oldCollectionResult);
            newCollectionResult.RawDurationTotalOfConsistentCdrs =
                preProcessor.NonPartialCdrs.Sum(c => c.DurationSec) + preProcessor.PartialCdrContainers
                    .SelectMany(pc => pc.NewRawInstances).Sum(r => r.DurationSec);
            CdrJobContext cdrJobContext = new CdrJobContext(this.CdrCollectorInput.CdrJobInputData,
                newCollectionResult.HoursInvolved);
            CdrProcessor cdrProcessor = new CdrProcessor(cdrJobContext, newCollectionResult);
            CdrEraser cdrEraser = null;
            CdrJob cdrJob = new CdrJob(cdrProcessor, cdrEraser, cdrProcessor.CollectionResult.RawCount,
                partialCdrTesterData: null);
            return cdrJob;

        }
    }
}
