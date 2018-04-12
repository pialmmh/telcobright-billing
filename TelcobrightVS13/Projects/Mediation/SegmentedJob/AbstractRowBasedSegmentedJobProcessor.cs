using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using TelcobrightFileOperations;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;
using MediationModel;
namespace TelcobrightMediation
{
    public abstract class AbstractRowBasedSegmentedJobProcessor : AbstractSegmentedJobProcessor
    {
        protected string IndexedRowIdColumnName { get; }
        protected string DateColumnName { get; }
        protected int BatchSizeWhenPreparingLargeSqlJob { get; }

        protected AbstractRowBasedSegmentedJobProcessor(job telcobrightJob, PartnerEntities context
            , int batchSizeWhenPreparingLargeSqlJob,
            string indexedRowIdColumnName, string dateColumnName) : base(telcobrightJob, context)
        {
            this.BatchSizeWhenPreparingLargeSqlJob = batchSizeWhenPreparingLargeSqlJob;
            this.IndexedRowIdColumnName = indexedRowIdColumnName;
            this.DateColumnName = dateColumnName;
        }

        public override void PrepareSegments()
        {
            BatchSqlJobParamJson batchParamJ =
                JsonConvert.DeserializeObject<BatchSqlJobParamJson>(this.TelcobrightJob.JobParameter);
            string sourceTable = batchParamJ.TableName;
            int batchSize = batchParamJ.BatchSize < 1000 ? 1000 : batchParamJ.BatchSize;
            string sql = batchParamJ.GetSqlForRowIdAndDate(); //select idCall,starttime from...
            int limitStart = 0;
            bool keepLooping = true;
            List<jobsegment> jobSegments = new List<jobsegment>();
            //there could be millions of rows e.g. cdrs, FETCH rowId+starttime in batches of fixed size, 
            //default 1M, this should be a bigger chunk for performance, looping limit in blocks of 5000 will be slow
            while (keepLooping)
            {
                sql += "order by " + this.IndexedRowIdColumnName + " limit " +
                       limitStart + //idcall is indexed for performance
                       "," + this.BatchSizeWhenPreparingLargeSqlJob;
                limitStart += this.BatchSizeWhenPreparingLargeSqlJob;
                List<RowIdVsDate<long>> idCallAndDates =
                    this.Context.Database.SqlQuery<RowIdVsDate<long>>(sql).ToList();

                //segment the job to defined batch size e.g. 5000-10000 configured in the config file
                var collectionSegmenter = new CollectionSegmenter<RowIdVsDate<long>>(idCallAndDates, 0);
                List<RowIdVsDate<long>> segment = new List<RowIdVsDate<long>>();
                int currentSegmentNumber = 0;
                segment = collectionSegmenter.GetNextSegment(batchSize).ToList();
                while ((keepLooping = segment.Any()) == true)
                {
                    jobsegment js = new jobsegment()
                    {
                        idJob = this.TelcobrightJob.id,
                        segmentNumber = ++currentSegmentNumber,
                        stepsCount = segment.Count,
                        status = 6,//6=create, consistent to job status
                        SegmentDetail = JsonConvert.SerializeObject(
                            new DayWiseRowIdsCollection(segment.GroupBy(c => c.RowDateTime.Date)
                                    .ToDictionary(g => g.Key,
                                        g => g.Select(rowIdVsDate => rowIdVsDate.RowId.ToString()).ToList()),
                                sourceTable, this.IndexedRowIdColumnName, this.DateColumnName,
                                string.Empty))
                    };
                    jobSegments.Add(js);
                    segment = collectionSegmenter.GetNextSegment(batchSize).ToList();
                } //while to to create smaller segments of 5000-10000 or whatever is defined
            } //while for Millions of idcalls & dates
            base.SaveSegmentsToDb(jobSegments);
        }
        public DayWiseRowIdsCollection DeserializeDayWiseRowIdsCollection(jobsegment segment)
        {
            return JsonConvert.DeserializeObject<DayWiseRowIdsCollection>
                (segment.SegmentDetail.Trim(Convert.ToChar("\"")));
        }
    }
    
}
