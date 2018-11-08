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
            string baseSql = batchParamJ.GetSqlForRowIdAndDate(); //select IdCall,starttime from...
            List<DateTime> partitionDates = new List<DateTime>();
            DateTime startPartitionDate = batchParamJ.StartPartitionDate;
            DateTime endPartitionDate = batchParamJ.EndPartitionDate;
            string partitionColName = batchParamJ.PartitionColName;
            string rowIdColName = batchParamJ.RowIdColName;
            int noOfDates = (endPartitionDate - startPartitionDate).Days + 1;
            for (int i = 0; i < noOfDates; i++)
            {
                partitionDates.Add(startPartitionDate.AddDays(i));
            }
            List<jobsegment> jobSegments = new List<jobsegment>();
            long totalOfAllSegments = 0;
            int currentSegmentNumber = 0;
            foreach (var partitionDate in partitionDates)
            {
                bool recordExists = true;
                long maxRowIdForThisDate = 0;
                                while (recordExists)
                {
                    string sql = baseSql + " and " + partitionDate.ToMySqlWhereClauseForOneDay(partitionColName) +
                                 " and " + rowIdColName + "> " + maxRowIdForThisDate +
                                 " order by " + this.IndexedRowIdColumnName + " limit 0" +
                                 "," + this.BatchSizeWhenPreparingLargeSqlJob;
                    List<RowIdVsDate<long>> idCallAndDates =
                        this.Context.Database.SqlQuery<RowIdVsDate<long>>(sql).ToList();
                    recordExists = idCallAndDates.Any();
                    if (recordExists)
                    {
                        maxRowIdForThisDate = idCallAndDates.Max(c => c.RowId);
                        totalOfAllSegments += idCallAndDates.Count;
                        Console.WriteLine("Preparing job, "+totalOfAllSegments + " records fetched.");
                        var collectionSegmenter = new CollectionSegmenter<RowIdVsDate<long>>(idCallAndDates, 0);
                        var segmentedCollection = collectionSegmenter.GetNextSegment(batchSize).ToList();
                        while(segmentedCollection.Any() == true)
                        {
                            var dayWiseRowIds = segmentedCollection.GroupBy(c => c.RowDateTime.Date)
                                .ToDictionary(g => g.Key,
                                    g => g.Select(rowIdVsDate => rowIdVsDate.RowId.ToString()).ToList());
                            foreach (var kv in dayWiseRowIds)
                            {
                                jobsegment js = new jobsegment()
                                {
                                    idJob = this.TelcobrightJob.id,
                                    segmentNumber = ++currentSegmentNumber,
                                    stepsCount = kv.Value.Count,
                                    status = 6,//6=create, consistent to job status
                                    SegmentDetail = JsonConvert.SerializeObject(
                                        new RowIdsCollectionForSingleDay(kv.Key, kv.Value,
                                            sourceTable: sourceTable, indexedRowIdColName: this.IndexedRowIdColumnName,
                                            dateColName: this.DateColumnName,
                                            quoteCharToEncloseNonNumericRowIdValues: string.Empty))
                                };
                                jobSegments.Add(js);
                            }
                            segmentedCollection = collectionSegmenter.GetNextSegment(batchSize).ToList();
                        } //while to to create smaller segments of 5000-10000 or whatever is defined
                    }
                } //while to to create smaller segments of 5000-10000 or whatever is defined
            }

            if (totalOfAllSegments != jobSegments.Sum(s => s.stepsCount))
                throw new Exception("Sum of all segments count do not match total raw records received.");
            base.SaveSegmentsToDb(jobSegments);
        }
        public RowIdsCollectionForSingleDay DeserializeDayWiseRowIdsCollection(jobsegment segment)
        {
            return JsonConvert.DeserializeObject<RowIdsCollectionForSingleDay>
                (segment.SegmentDetail.Trim(Convert.ToChar("\"")));
        }
    }

}
