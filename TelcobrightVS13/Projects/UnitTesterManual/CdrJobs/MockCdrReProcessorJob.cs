using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jobs;
using MediationModel;
using TelcobrightMediation;

namespace UnitTesterManual
{
    public class MockCdrReProcessorJob:CdrReProcess
    {
        public override JobCompletionStatus Execute(ITelcobrightJobInput jobInputData)
        {
            CdrJobInputData input = (CdrJobInputData)jobInputData;
            AutoIncrementManager autoIncrementManager = new AutoIncrementManager(input.Context);
            CdrCollectorInputData cdrCollectorInput = new CdrCollectorInputData(input, "");
            SegmentedCdrReprocessor segmentedCdrReprocessJobProcessor =
                new SegmentedCdrReprocessor(cdrCollectorInput,
                    input.CdrSetting.BatchSizeWhenPreparingLargeSqlJob, "IdCall", "starttime");
            if (input.TelcobrightJob.Status != 2) //prepare job if not prepared already
                segmentedCdrReprocessJobProcessor.PrepareSegments();
            List<jobsegment> jobsegments = segmentedCdrReprocessJobProcessor.ExecuteIncompleteSegments();
            segmentedCdrReprocessJobProcessor.FinishJob(jobsegments); //mark job as complete
            return JobCompletionStatus.Complete;
        }
    }
}
