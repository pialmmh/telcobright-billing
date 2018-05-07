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
    public class MockCdrEraserJob:CdrReProcess
    {
        public override JobCompletionStatus Execute(ITelcobrightJobInput jobInputData)
        {
            CdrJobInputData input = (CdrJobInputData)jobInputData;
            CdrCollectorInputData cdrCollectorInput = new CdrCollectorInputData(input, "");
            SegmentedCdrEraser segmentedCdrEraser =
                new SegmentedCdrEraser(cdrCollectorInput,
                    input.CdrSetting.BatchSizeWhenPreparingLargeSqlJob, "IdCall", "starttime");
            if (input.TelcobrightJob.Status != 2) //prepare job if not prepared already
                segmentedCdrEraser.PrepareSegments();
            List<jobsegment> jobsegments = segmentedCdrEraser.ExecuteIncompleteSegments();
            segmentedCdrEraser.FinishJob(jobsegments); //mark job as complete
            return JobCompletionStatus.Complete;
        }
    }
}
