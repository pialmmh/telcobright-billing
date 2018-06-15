using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;

namespace TelcobrightMediation.Cdr
{
    public class PartialCdrWriter
    {
        public int WrittenNewRawInstances { get; private set; }
        public int WrittenCdrPartialReferences { get; private set; }
        public int WrittenNewAggregatedRawInstances { get; private set; }
        public List<PartialCdrContainer> PartialCdrContainers { get; }
        private CdrJobContext CdrJobContext { get; }

        public PartialCdrWriter(List<PartialCdrContainer> partialCdrContainers, CdrJobContext cdrJobContext)
        {
            this.PartialCdrContainers = partialCdrContainers ?? new List<PartialCdrContainer>();
            this.CdrJobContext = cdrJobContext;
        }

        public void Write()
        {
            this.WrittenNewRawInstances = WriteNewRawPartialInstances();
            //there will always be a partialCdrReference for a partial cdr, no need to filter the following list for null
            List<cdrpartialreference> cdrPartialReferences =
                this.PartialCdrContainers.Select(c => c.CdrPartialReference).ToList();
            DeletePrevCdrPartialReferences(cdrPartialReferences); //deleted count could be 0 because partialcdrref may not exist before, so counting is useless
            this.WrittenCdrPartialReferences= WriteCdrPartialReferences(cdrPartialReferences);
            DeletePrevAggregatedRawInstances();
            this.WrittenNewAggregatedRawInstances = WriteNewAggregatedRawInstances();
        }

        private int WriteNewRawPartialInstances()
        {
            List<cdrpartialrawinstance> newRawPartialInstances =
                this.PartialCdrContainers.SelectMany(c => c.NewRawInstances).ToList();
            int insertCount = 0;
            if (newRawPartialInstances.Any())
            {
                int startAt = 0;
                CollectionSegmenter<cdrpartialrawinstance> collectionSegmenter =
                    new CollectionSegmenter<cdrpartialrawinstance>(newRawPartialInstances, startAt);
                collectionSegmenter.ExecuteMethodInSegments(this.CdrJobContext.SegmentSizeForDbWrite,
                    segment =>
                    {
                        this.CdrJobContext.DbCmd.CommandText = new StringBuilder(StaticExtInsertColumnHeaders.cdrpartialrawinstance)
                            .Append(string.Join(",",
                                segment.Select(c => c.GetExtInsertValues()).ToList())).ToString();
                        insertCount += this.CdrJobContext.DbCmd.ExecuteNonQuery(); //write cdr loaded
                    });
            }
            return insertCount;
        }
        private int WriteCdrPartialReferences(List<cdrpartialreference> cdrPartialReferences)
        {
            int insertCount = 0;
            if (cdrPartialReferences.Any())
            {
                int startAt = 0;
                CollectionSegmenter<cdrpartialreference> collectionSegmenter =
                    new CollectionSegmenter<cdrpartialreference>(cdrPartialReferences, startAt);
                collectionSegmenter.ExecuteMethodInSegments(this.CdrJobContext.SegmentSizeForDbWrite,
                    segment =>
                    {
                        this.CdrJobContext.DbCmd.CommandText = new StringBuilder(StaticExtInsertColumnHeaders.cdrpartialreference)
                            .Append(string.Join(",",
                                segment.Select(c => c.GetExtInsertValues()).ToList())).ToString();
                        insertCount += this.CdrJobContext.DbCmd.ExecuteNonQuery(); //write cdr loaded
                    });
            }
            if (insertCount != cdrPartialReferences.Count)
                throw new Exception("Inserted cdrpartialreference count does not match collected number of unique partialcdrs");
            return insertCount;
        }

        private void DeletePrevAggregatedRawInstances()
        {
            List<cdrpartiallastaggregatedrawinstance> prevAggregatedRawInstances =
                this.PartialCdrContainers.Select(c => c.LastProcessedAggregatedRawInstance).Where(c => c != null).ToList();
            int deletedCount = 0;
            if (prevAggregatedRawInstances.Any())
            {
                int startAt = 0;
                CollectionSegmenter<cdrpartiallastaggregatedrawinstance> collectionSegmenter =
                    new CollectionSegmenter<cdrpartiallastaggregatedrawinstance>(prevAggregatedRawInstances, startAt);
                collectionSegmenter.ExecuteMethodInSegments(this.CdrJobContext.SegmentSizeForDbWrite,
                    segment =>
                    {
                        this.CdrJobContext.DbCmd.CommandText = new StringBuilder()
                            .Append(string.Join(";", segment.Select(c => $@" delete from cdrpartiallastaggregatedrawinstance where 
                                                                             uniquebillid={c.UniqueBillId.EncloseWith("'")}
                                                                             and starttime={c.StartTime.ToMySqlStyleDateTimeStrWithQuote()} 
                                                                           "))).ToString();
                        deletedCount += this.CdrJobContext.DbCmd.ExecuteNonQuery(); //write cdr loaded
                    });
            }
            if(deletedCount!=prevAggregatedRawInstances.Count)
                throw new Exception("No of deleted prev cdrpartiallastaggregatedrawinstance does not match count in collection.");
        }

        private int WriteNewAggregatedRawInstances()
        {
            List<cdrpartiallastaggregatedrawinstance> newAggregatedRawInstances =
                this.PartialCdrContainers.Select(c => c.NewAggregatedRawInstance).ToList();
            int insertCount = 0;
            if (newAggregatedRawInstances.Any())
            {
                int startAt = 0;
                CollectionSegmenter<cdrpartiallastaggregatedrawinstance> collectionSegmenter =
                    new CollectionSegmenter<cdrpartiallastaggregatedrawinstance>(newAggregatedRawInstances,
                        startAt);
                collectionSegmenter.ExecuteMethodInSegments(this.CdrJobContext.SegmentSizeForDbWrite,
                    segment =>
                    {
                        this.CdrJobContext.DbCmd.CommandText =
                            new StringBuilder(StaticExtInsertColumnHeaders.cdrpartiallastaggregatedrawinstance)
                                .Append(string.Join(",",
                                    segment.Select(c => c.GetExtInsertValues()).ToList())).ToString();
                        insertCount += this.CdrJobContext.DbCmd.ExecuteNonQuery(); //write cdr loaded
                    });
            }
            if (insertCount != newAggregatedRawInstances.Count)
                throw new Exception(
                    "No of inserted newAggregatedRawInstances does not match count in collection.");
            return insertCount;
        }


        private int DeletePrevCdrPartialReferences(List<cdrpartialreference> cdrPartialReferences)
        {
            //deleted count could be 0 because partialcdrref may not exist before, so counting is useless
            int delCount = 0;
            if (cdrPartialReferences.Any())
            {
                int startAt = 0;
                CollectionSegmenter<cdrpartialreference> collectionSegmenter =
                    new CollectionSegmenter<cdrpartialreference>(cdrPartialReferences, startAt);
                collectionSegmenter.ExecuteMethodInSegments(this.CdrJobContext.SegmentSizeForDbWrite,
                    segment =>
                    {
                        this.CdrJobContext.DbCmd.CommandText = new StringBuilder()
                            .Append(string.Join(";", segment.Select(c => $@" delete from cdrpartialreference where 
                                                                             uniquebillid={
                                    c.UniqueBillId.EncloseWith("'")
                                }
                                                                             and calldate=
                                                                             {
                                    c.CallDate.ToMySqlStyleDateTimeStrWithQuote()
                                } 
                                                                           "))).ToString();
                        delCount = this.CdrJobContext.DbCmd.ExecuteNonQuery();
                    });
            }
            return delCount;
        }
    }
}
