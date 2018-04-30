using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;
using Newtonsoft.Json;
using TelcobrightMediation;
using TelcobrightMediation.Cdr;

namespace TelcobrightMediation
{
    public class SegmentedCdrReprocessor : AbstractRowBasedSegmentedJobProcessor
    {
        private CdrCollectorInputData CdrCollectorInput { get; }
        public SegmentedCdrReprocessor(CdrCollectorInputData cdrCollectorInput,int batchSizeWhenPreparingLargeSqlJob, 
            string indexedColumnName, string dateColumnName)
            : base(cdrCollectorInput.TelcobrightJob, cdrCollectorInput.Context, batchSizeWhenPreparingLargeSqlJob,
                indexedColumnName, dateColumnName)
        {
            this.CdrCollectorInput = cdrCollectorInput;
        }

        public override ISegmentedJob CreateJobSegmentInstance(jobsegment jobSegment)
        {
            DayWiseRowIdsCollection dayWiseRowsIdsCollection = base.DeserializeDayWiseRowIdsCollection(jobSegment);
            string selectSql = dayWiseRowsIdsCollection.GetSelectSql();
            DbRowCollector<cdr> dbRowCollector =
                new DbRowCollector<cdr>(this.CdrCollectorInput.CdrJobInputData, selectSql);
            List<cdr> finalCdrs = dbRowCollector.Collect();
            CdrReProcessingPreProcessor preProcessor =
                new CdrReProcessingPreProcessor(this.CdrCollectorInput, finalCdrs);
            CdrCollectionResult newCollectionResult = null;
            CdrCollectionResult oldCollectionResult = null;
            preProcessor.GetCollectionResults(out newCollectionResult,out oldCollectionResult);
            CdrJobContext cdrJobContext = new CdrJobContext(this.CdrCollectorInput.CdrJobInputData,
                newCollectionResult.HoursInvolved);
            CdrProcessor cdrProcessor = new CdrProcessor(cdrJobContext, newCollectionResult);
            CdrEraser cdrEraser = new CdrEraser(cdrJobContext, oldCollectionResult);
            CdrJob cdrJob = new CdrJob(cdrProcessor, cdrEraser, cdrProcessor.CollectionResult.RawCount,
                partialCdrTesterData:null);
            return cdrJob;
        }

    }
}
