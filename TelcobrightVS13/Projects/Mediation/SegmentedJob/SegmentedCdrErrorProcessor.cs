using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexValidation;
using MediationModel;
using Newtonsoft.Json;
using TelcobrightMediation;
using TelcobrightMediation.Cdr;

namespace TelcobrightMediation
{
    public class SegmentedCdrErrorProcessor : AbstractRowBasedSegmentedJobProcessor
    {
        private CdrCollectorInputData CdrCollectorInput { get; }

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
            DayWiseRowIdsCollection dayWiseRowsIdsCollection = base.DeserializeDayWiseRowIdsCollection(jobSegment);
            DbRowCollector<cdrerror> dbRowCollector =
                new DbRowCollector<cdrerror>(this.CdrCollectorInput.CdrJobInputData,
                    dayWiseRowsIdsCollection.GetSelectSql());
            List<string[]> txtRows = dbRowCollector.CollectAsTxtRows();
            NewCdrPreProcessor preProcessor =
                new NewCdrPreProcessor(txtRows, new List<cdrinconsistent>(), this.CdrCollectorInput);
            FlexValidator<string[]> inconsistentValidator = CreateValidatorInstance();
            Parallel.ForEach(txtRows, txtRow =>
            {
                preProcessor
                    .CheckAndConvertIfInconsistent(this.CdrCollectorInput.CdrJobInputData, inconsistentValidator, txtRow);
                preProcessor.ConvertToCdrOrInconsistentOnFailure(txtRow);
            });
            CdrCollectionResult newCollectionResult = null;
            CdrCollectionResult oldCollectionResult = null;
            preProcessor.GetCollectionResults(out newCollectionResult, out oldCollectionResult);
            CdrJobContext cdrJobContext = new CdrJobContext(this.CdrCollectorInput.CdrJobInputData,
                this.CdrCollectorInput.AutoIncrementManager, newCollectionResult.HoursInvolved);
            
            CdrProcessor cdrProcessor = new CdrProcessor(cdrJobContext, newCollectionResult);
            CdrEraser cdrEraser = oldCollectionResult != null ? new CdrEraser(cdrJobContext, oldCollectionResult) : null;
            int rawCount = preProcessor.TxtCdrRows.Count;
            CdrJob cdrJob = new CdrJob(cdrProcessor, cdrEraser, rawCount,partialCdrTesterData:null);
            return cdrJob;
        }

        private FlexValidator<string[]> CreateValidatorInstance()
        {
            return new FlexValidator<string[]>(continueOnError: false,
                throwExceptionOnFirstError: false,
                validationExpressionsWithErrorMessage: this.CdrCollectorInput.CdrJobInputData.MediationContext.Tbc.CdrSetting
                    .ValidationRulesForInconsistentCdrs);
        }
    }
}
