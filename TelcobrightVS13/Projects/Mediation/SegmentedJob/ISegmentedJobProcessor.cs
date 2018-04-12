using System.Collections.Generic;
using MediationModel;
using TelcobrightMediation.Cdr;

namespace TelcobrightMediation
{
    public interface ISegmentedJobProcessor
    {
        job TelcobrightJob { get; }
        PartnerEntities Context { get; }
        void PrepareSegments();
        ISegmentedJob CreateJobSegmentInstance(jobsegment jobSegment);
        List<jobsegment> ExecuteIncompleteSegments();
        void FinishJob(List<jobsegment> jobsegments);
    }
}