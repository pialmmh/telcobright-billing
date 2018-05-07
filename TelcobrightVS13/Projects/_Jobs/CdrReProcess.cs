using TelcobrightMediation;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using TelcobrightFileOperations;
using MediationModel;
using LibraryExtensions;
using TelcobrightMediation.Config;

namespace Jobs
{

    [Export("Job", typeof(ITelcobrightJob))]
    public class CdrReProcess : ITelcobrightJob
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string HelpText => "Cdr Re-Processing Job, processes already processed CDRs in cdr table in Database";
        public int Id => 3;

        public virtual JobCompletionStatus Execute(ITelcobrightJobInput jobInputData)
        {
            CdrJobInputData input = (CdrJobInputData) jobInputData;
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


