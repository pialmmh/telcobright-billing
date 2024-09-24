using TelcobrightMediation;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using LibraryExtensions;
using TelcobrightFileOperations;
using MediationModel;
using TelcobrightMediation.Config;

namespace Jobs
{

    [Export("Job", typeof(ITelcobrightJob))]
    public class ErrorCdrProcessingJob : ITelcobrightJob
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;

        public string HelpText =>
            "Error Cdr processing Job, processes unprocessed CDRs in Error table in Database";

        public int Id => 2;

        public object Execute(ITelcobrightJobInput jobInputData)
        {
            CdrJobInputData input = (CdrJobInputData) jobInputData;
            CdrCollectorInputData cdrCollectorInput = new CdrCollectorInputData(input, "");
            SegmentedCdrErrorProcessor segmentedCdrErrorJobProcessor =
                new SegmentedCdrErrorProcessor(cdrCollectorInput,
                    input.CdrSetting.BatchSizeWhenPreparingLargeSqlJob, "IdCall", "starttime");
            openDbConAndStartTransaction(cdrCollectorInput.Context,cdrCollectorInput.MediationContext);
            if (input.Job.Status != 2) //prepare job if not prepared already
                segmentedCdrErrorJobProcessor.PrepareSegments();
            List<jobsegment> jobsegments = segmentedCdrErrorJobProcessor.ExecuteIncompleteSegments();
            segmentedCdrErrorJobProcessor.FinishJob(jobsegments,null); //mark job as complete
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


