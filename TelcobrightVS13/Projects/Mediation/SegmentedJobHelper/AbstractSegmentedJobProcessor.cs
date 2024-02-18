using System;
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
using TelcobrightInfra.PerformanceAndOptimization;
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
        private CdrSetting CdrSetting { get; set; }

        protected AbstractSegmentedJobProcessor(CdrSetting cdrSetting, job telcobrightJob, PartnerEntities context)
        {
            this.TelcobrightJob = telcobrightJob;
            this.Context = context;
            this.CdrSetting = cdrSetting;
        }

        protected void SaveSegmentsToDb(List<jobsegment> jobSegments)
        {
            try
            {
                using (DbCommand cmd = this.Context.Database.Connection.CreateCommand())
                {
                    //cmd.ExecuteCommandText("set autocommit=0;"); //start transaction
                    foreach (jobsegment s in jobSegments)
                    {
                        cmd.ExecuteCommandText(
                            $@"insert into jobsegment(idjob,segmentNumber,stepsCount,status,segmentdetail) values(
                                        {s.idJob},{s.segmentNumber},{s.stepsCount},
                                        {s.status},'{JsonConvert.SerializeObject(s.SegmentDetail)}')");
                    }
                    //cmd.ExecuteCommandText(" update job set " +
                    //                       " status=2," + "NoOfSteps=" + jobSegments.Sum(s => s.stepsCount) +
                    //                       " where id= " + this.TelcobrightJob.id);
                    //cmd.ExecuteCommandText("commit;");
                    CdrJobCommiter.Commit(cmd, this.CdrSetting);
                }
            }
            catch (Exception e)
            {
                try
                {
                    using (DbCommand cmd = this.Context.Database.Connection.CreateCommand())
                    {
                        Console.WriteLine(e);
                        cmd.CommandText = ("rollback;");
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    throw;
                }
                throw;
            }
        }

        public virtual List<jobsegment> ExecuteIncompleteSegments()
        {
            List<long> incompleteSegmentIds = this.Context.jobsegments
                .Where(c => c.idJob == this.TelcobrightJob.id
                            && c.status == 6).Select(js => js.id).ToList();
            using (DbCommand cmd = ConnectionManager.CreateCommandFromDbContext(this.Context))
            {
                foreach (int idJobSegment in incompleteSegmentIds)
                {
                    int noOfSteps = getNoOfSteps(cmd);

                    //jobsegment jobSegment = this.Context.jobsegments.Where(js => js.id == idJobSegment).ToList()
                    //  .First();
                    jobsegment jobSegment = getJobSegment(cmd, idJobSegment);
                    int progressSoFar = getJobProgressSoFar(cmd);
                    if (progressSoFar > noOfSteps)
                        throw new Exception("Progress cannot be > total no of steps for a job.");
                    Console.WriteLine("Processing Segment:" + (jobSegment.segmentNumber) + " for job "
                                      + this.TelcobrightJob.JobName + ". Progress=" + progressSoFar + "/"
                                      + this.TelcobrightJob.NoOfSteps.ToString());
                    ISegmentedJob segmentedJob = null;
                    try
                    {
                        cmd.ExecuteCommandText("set autocommit=0;");
                        segmentedJob = this.CreateJobSegmentInstance(jobSegment);
                        segmentedJob.Execute(); //execute segment
                        //progressSoFar = getJobProgressSoFar(cmd);
                        //if (progressSoFar + segmentedJob.ActualStepsCount > noOfSteps)
                        //{
                        //    Console.WriteLine("Processing Segment:" + (jobSegment.segmentNumber) + " for job "
                        //                      + this.TelcobrightJob.JobName + ". Progress=" + progressSoFar + "/"
                        //                      + this.TelcobrightJob.NoOfSteps.ToString());
                        //}

                        jobSegment.status = 1; //finished
                        cmd.ExecuteCommandText($@"update jobsegment set status=1 where id={jobSegment.id}");
                        cmd.ExecuteCommandText(
                            $" update job set lastexecuted=\'{DateTime.Now:yyyy-MM-dd HH:mm:ss}\', " +
                            $" progress=ifnull(progress,0)+{segmentedJob.ActualStepsCount} " +
                            $" where id={TelcobrightJob.id}");
                        //cmd.ExecuteCommandText(" commit; ");
                        progressSoFar = getJobProgressSoFar(cmd);
                        if (progressSoFar > noOfSteps)
                            throw new Exception("Progress cannot be > total no of steps for a job.");
                        CdrJobCommiter.Commit(cmd, this.CdrSetting);
                    }
                    catch (Exception e)
                    {
                        //try
                        {
                            Console.WriteLine(e);
                            cmd.ExecuteCommandText("rollback;"); //rollback immediately
                            ErrorWriter.WriteError(e, $@"Segmented Job Processor", this.TelcobrightJob,
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

        private int getNoOfSteps(DbCommand cmd)
        {
            cmd.CommandText = $"select noofsteps from job where id={this.TelcobrightJob.id}";
            var reader = cmd.ExecuteReader();
            int noOfSteps = 0;
            try
            {
                while (reader.Read())
                {
                    // Format the date to "Month-Day-Year" format
                    noOfSteps = Convert.ToInt32(reader["noofsteps"].ToString());
                }
                reader.Close();
            }
            catch (Exception e)
            {
                reader.Close();
                Console.WriteLine(e);
                throw;
            }

            return noOfSteps;
        }

        private int getJobProgressSoFar(DbCommand cmd)
        {
            string sqlProgress = " select ifnull(progress,0) as progress from job " +
                                 " where id=" + this.TelcobrightJob.id;
            cmd.CommandText = sqlProgress;
            object retVal = cmd.ExecuteScalar();
            int progressSoFar = Convert.ToInt32(retVal);
            return progressSoFar;
        }

        private jobsegment getJobSegment(DbCommand cmd,int idJobSegment)
        {
            string sqlProgress = " select id,idjob,segmentnumber,stepscount,status,segmentdetail from jobsegment " +
                                 " where id=" + idJobSegment.ToString();
            cmd.CommandText = sqlProgress;
            DbDataReader reader=null;
            try
            {
                reader = cmd.ExecuteReader();
                List<jobsegment> jobsegments= new List<jobsegment>();
                while (reader.Read())
                {
                    jobsegment segment= new jobsegment();
                    segment.id = Convert.ToInt64(reader["id"].ToString());
                    segment.idJob = Convert.ToInt64(reader["idjob"].ToString());
                    segment.segmentNumber= Convert.ToInt32(reader["segmentnumber"].ToString());
                    segment.stepsCount= Convert.ToInt32(reader["stepscount"].ToString());
                    segment.status= Convert.ToInt32(reader["status"].ToString());
                    segment.SegmentDetail= reader["segmentdetail"].ToString();
                    reader.Close();
                    return segment;
                }
                throw new Exception("Job segment not found while processing error cdrs.");
            }
            catch (Exception e)
            {
                reader.Close();
                Console.WriteLine(e);
                throw;
            }
        }

        public virtual void FinishJob(List<jobsegment> jobsegments, Action<object> additionalJobFinalizingTask)
        {
            using (DbCommand cmd = ConnectionManager.CreateCommandFromDbContext(this.Context))
            {
                additionalJobFinalizingTask?.Invoke(cmd);
                if (jobsegments.Any(c => c.status != 1) == false) //no incomplete segment
                {
                    cmd.ExecuteCommandText(" set autocommit=0; ");
                    cmd.ExecuteCommandText(" update job set completiontime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                                           " status=1, Error='' " +
                                           " where id=" + this.TelcobrightJob.id);
                    //delete job segments which can hold large amount of data
                    cmd.ExecuteCommandText(" delete from jobsegment where idjob=" + this.TelcobrightJob.id);
                    //cmd.ExecuteCommandText(" commit; ");
                    CdrJobCommiter.Commit(cmd, this.CdrSetting);
                }
            }
        }
    }
}
