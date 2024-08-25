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
    public class CdrReProcessingJob : ITelcobrightJob
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string HelpText => "Cdr Re-Processing Job, processes already processed CDRs in cdr table in Database";
        public int Id => 3;

        public virtual object Execute(ITelcobrightJobInput jobInputData)
        {
            CdrJobInputData input = (CdrJobInputData) jobInputData;
            CdrCollectorInputData cdrCollectorInput = new CdrCollectorInputData(input, "");
            openDbConAndStartTransaction(jobInputData.Context, cdrCollectorInput.MediationContext);
            SegmentedCdrReprocessJobProcessor segmentedCdrReprocessJobProcessor =
                new SegmentedCdrReprocessJobProcessor(cdrCollectorInput,
                    input.CdrSetting.BatchSizeWhenPreparingLargeSqlJob, "IdCall", "starttime");
            if (input.Job.Status != 2) //prepare job if not prepared already
                segmentedCdrReprocessJobProcessor.PrepareSegments();
            List<jobsegment> jobsegments = segmentedCdrReprocessJobProcessor.ExecuteIncompleteSegments();
            segmentedCdrReprocessJobProcessor.FinishJob(jobsegments,null); //mark job as complete
            return JobCompletionStatus.Complete;
        }

        private void openDbConAndStartTransaction(PartnerEntities context, MediationContext mediationContext)
        {
            DbCommand cmd = context.Database.Connection.CreateCommand();
            if (cmd.Connection.State == ConnectionState.Open)
            {
                throw new Exception("Connection should only be open after preprocessing new cdr job.");
            }
            cmd.Connection.Open();
            mediationContext.CreateTemporaryTables();
            cmd.ExecuteCommandText("set autocommit=0;");
        }

        public object PreprocessJob(object data)
        {
            throw new NotImplementedException();
        }

        public object PostprocessJobBeforeCommit(object data)
        {
            throw new NotImplementedException();
        }

        public object PostprocessJobAfterCommit(object data)
        {
            throw new NotImplementedException();
        }

        public ITelcobrightJob createNewNonSingletonInstance()
        {
            throw new NotImplementedException();
        }
    }
}


