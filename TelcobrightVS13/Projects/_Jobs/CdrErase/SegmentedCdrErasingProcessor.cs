﻿using System;
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
    public class SegmentedCdrErasingProcessor : AbstractRowBasedSegmentedJobProcessor
    {
        private CdrCollectorInputData CdrCollectorInput { get; }

        public SegmentedCdrErasingProcessor(CdrCollectorInputData cdrCollectorInput, int batchSizeWhenPreparingLargeSqlJob,
            string indexedColumnName, string dateColumnName)
            : base(cdrCollectorInput.TelcobrightJob, cdrCollectorInput.Context, batchSizeWhenPreparingLargeSqlJob,
                indexedColumnName, dateColumnName)
        {
            this.CdrCollectorInput = cdrCollectorInput;
        }

        public override ISegmentedJob CreateJobSegmentInstance(jobsegment jobSegment)
        {
            RowIdsCollectionForSingleDay dayWiseRowsIdsCollection = base.DeserializeDayWiseRowIdsCollection(jobSegment);
            string selectSql = dayWiseRowsIdsCollection.GetSelectSql();
            DbRowCollector<cdr> dbRowCollector =
                new DbRowCollector<cdr>(this.CdrCollectorInput.CdrJobInputData, selectSql);
            List<cdr> finalCdrs = dbRowCollector.Collect();
            CdrErasingPreProcessor reprocessingPreProcessor =
                new CdrErasingPreProcessor(this.CdrCollectorInput, finalCdrs);

            CdrCollectionResult newCollectionResult = null, oldCollectionResult = null;
            reprocessingPreProcessor.GetCollectionResults(out newCollectionResult, out oldCollectionResult);

            CdrJobContext cdrJobContext = new CdrJobContext(this.CdrCollectorInput.CdrJobInputData,
                oldCollectionResult.HoursInvolved);
            CdrEraser cdrEraser = new CdrEraser(cdrJobContext, oldCollectionResult);
            CdrJob cdrJob = new CdrJob(null, cdrEraser, oldCollectionResult.RawCount,partialCdrTesterData:null);
            return cdrJob;
        }
    }
}
