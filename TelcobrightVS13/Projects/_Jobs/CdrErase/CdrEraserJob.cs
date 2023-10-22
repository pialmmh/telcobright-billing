using TelcobrightMediation;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using TelcobrightFileOperations;
using MediationModel;
using TelcobrightMediation.Config;

namespace Jobs
{

    [Export("Job", typeof(ITelcobrightJob))]
    public class CdrEraserJob : ITelcobrightJob
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;

        public string HelpText => "Cdr erasing job.";

        public int Id => 4;

        public object Execute(ITelcobrightJobInput jobInputData)
        {
            CdrJobInputData input = (CdrJobInputData) jobInputData;
            CdrCollectorInputData cdrCollectorInput = new CdrCollectorInputData(input, "");
            SegmentedCdrErrorProcessor segmentedCdrErrorJobProcessor =
                new SegmentedCdrErrorProcessor(cdrCollectorInput,
                    input.CdrSetting.BatchSizeWhenPreparingLargeSqlJob, "IdCall", "starttime");
            if (input.TelcobrightJob.Status != 2) //prepare job if not prepared already
                segmentedCdrErrorJobProcessor.PrepareSegments();
            List<jobsegment> jobsegments = segmentedCdrErrorJobProcessor.ExecuteIncompleteSegments();
            segmentedCdrErrorJobProcessor.FinishJob(jobsegments,null); //mark job as complete
            return JobCompletionStatus.Complete;
        }

        public object PreprocessJob(ITelcobrightJobInput jobInputData)
        {
            throw new NotImplementedException();
        }

        public object PostprocessJob(ITelcobrightJobInput jobInputData)
        {
            throw new NotImplementedException();
        }
    }
}


