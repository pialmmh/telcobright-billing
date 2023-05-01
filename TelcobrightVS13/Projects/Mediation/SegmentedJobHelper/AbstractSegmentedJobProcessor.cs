﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging.Configuration;
using LibraryExtensions;
using MediationModel;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using TelcobrightFileOperations;
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;

namespace TelcobrightMediation
{

    public abstract class AbstractSegmentedJobProcessor : ISegmentedJobProcessor
    {
        
        public job TelcobrightJob { get; }
        public PartnerEntities Context { get; }
        public abstract ISegmentedJob CreateJobSegmentInstance(jobsegment jobSegment);
        public abstract void PrepareSegments();
        protected AbstractSegmentedJobProcessor(job telcobrightJob, PartnerEntities context)
        {
            this.TelcobrightJob = telcobrightJob;
            this.Context = context;
        }

        protected void SaveSegmentsToDb(List<jobsegment> jobSegments)
        {
            using (DbCommand cmd = this.Context.Database.Connection.CreateCommand())
            {
                try
                {
                    //cmd.ExecuteCommandText("set autocommit=0;"); //start transaction
                    foreach (jobsegment s in jobSegments)
                    {
                        cmd.ExecuteCommandText($@"insert into jobsegment(idjob,segmentNumber,stepsCount,status,segmentdetail) values(
                                        {s.idJob},{s.segmentNumber},{s.stepsCount},
                                        {s.status},'{JsonConvert.SerializeObject(s.SegmentDetail)}')");
                    }
                    //cmd.ExecuteCommandText(" update job set " +
                    //                       " status=2," + "NoOfSteps=" + jobSegments.Sum(s => s.stepsCount) +
                    //                       " where id= " + this.TelcobrightJob.id);
                    //cmd.ExecuteCommandText("commit;");
                    CdrJobCommiter.Commit(cmd);
                }
                catch (Exception e)
                {
                    try
                    {
                        Console.WriteLine(e);
                        cmd.CommandText = ("rollback;");
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                        throw;
                    }
                    throw;
                }
            }
        }

        public virtual List<jobsegment> ExecuteIncompleteSegments()
        {
            List<long> incompleteSegmentIds = this.Context.jobsegments
                                                  .Where(c => c.idJob == this.TelcobrightJob.id
                                                   && c.status == 6).Select(js=>js.id).ToList();
            using (DbCommand cmd = ConnectionManager.CreateCommandFromDbContext(this.Context))
            {
                foreach (int idJobSegment in incompleteSegmentIds)
                {
                    jobsegment jobSegment = this.Context.jobsegments.Where(js => js.id == idJobSegment).ToList().First();
                    string sqlProgress = " select ifnull(progress,0) as progress from job " +
                                         " where id=" + this.TelcobrightJob.id;
                    cmd.CommandText = sqlProgress;
                    object retVal= cmd.ExecuteScalar();
                    int progressSoFar = Convert.ToInt32(retVal);
                    if (progressSoFar > this.TelcobrightJob.NoOfSteps)
                        throw new Exception("Progress cannot be > total no of steps for a job.");
                    Console.WriteLine("Processing Segment:" + (jobSegment.segmentNumber) + " for job "
                                      + this.TelcobrightJob.JobName + ". Progress="+progressSoFar +"/"
                                      + this.TelcobrightJob.NoOfSteps.ToString());
                    ISegmentedJob segmentedJob = null;
                    try
                    {
                        cmd.ExecuteCommandText("set autocommit=0;");
                        segmentedJob = this.CreateJobSegmentInstance(jobSegment);
                        segmentedJob.Execute();//execute segment
                        jobSegment.status = 1; //finished
                        cmd.ExecuteCommandText($@"update jobsegment set status=1 where id={jobSegment.id}");
                                                cmd.ExecuteCommandText(
                            $" update job set lastexecuted=\'{DateTime.Now:yyyy-MM-dd HH:mm:ss}\', " +
                            $" progress=ifnull(progress,0)+{segmentedJob.ActualStepsCount} " +
                            $" where id={TelcobrightJob.id}");
                        //cmd.ExecuteCommandText(" commit; ");
                        CdrJobCommiter.Commit(cmd);
                    }
                    catch (Exception e)
                    {
                        //try
                        {
                            Console.WriteLine(e);
                            cmd.ExecuteCommandText("rollback;"); //rollback immediately
                            ErrorWriter wr = new ErrorWriter(e, $@"Segmented Job Processor", this.TelcobrightJob,
                                "Error Processing Segments of batch job " + this.TelcobrightJob.JobName, 
                                "", this.Context);
                            throw; // do not continue on error other than rateCache exception for cdrJobs
                        }
                    }
                } //for each segment
            }
            List<jobsegment> incompleteSegments = this.Context.jobsegments
                                                  .Where(c => c.idJob == this.TelcobrightJob.id
                                                   && c.status == 6).ToList();
            return incompleteSegments;
        }

        public virtual void FinishJob(List<jobsegment> jobsegments,Action<object> additionalJobFinalizingTask)
        {
            using (DbCommand cmd = ConnectionManager.CreateCommandFromDbContext(this.Context))
            {
                additionalJobFinalizingTask?.Invoke(cmd);
                if (jobsegments.Any(c => c.status != 1) == false) //no incomplete segment
                {
                    cmd.ExecuteCommandText(" set autocommit=0; ");
                    cmd.ExecuteCommandText(" update job set completiontime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                                           " status=1, Error='' " +
                                           " where id=" +this.TelcobrightJob.id);
                    //delete job segments which can hold large amount of data
                    cmd.ExecuteCommandText(" delete from jobsegment where idjob=" + this.TelcobrightJob.id);
                    //cmd.ExecuteCommandText(" commit; ");
                    CdrJobCommiter.Commit(cmd);
                }
            }
        }
    }
}
