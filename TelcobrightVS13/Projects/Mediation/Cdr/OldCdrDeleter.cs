using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;

namespace TelcobrightMediation.Cdr
{
    public static class OldCdrDeleter
    {
        public static int DeleteOldCdrs(string tableName, List<KeyValuePair<long, DateTime>> rowIdWithDates,
            int segmentSizeForDbWrite,DbCommand dbCmd)
        {
            if (rowIdWithDates.Any() == false) return 0;
            int delCount = 0;
            int startAt = 0;
            CollectionSegmenter<KeyValuePair<long, DateTime>> collectionSegmenter =
                new CollectionSegmenter<KeyValuePair<long, DateTime>>
                    (rowIdWithDates, startAt);
            collectionSegmenter.ExecuteMethodInSegments(segmentSizeForDbWrite,
                segment =>
                {
                    dbCmd.CommandText = string.Join(";", segment
                        .Select(kv => $@"delete from {tableName} where IdCall={kv.Key} 
                                    and starttime={kv.Value.ToMySqlStyleDateTimeStrWithQuote()}"));
                    delCount += dbCmd.ExecuteNonQuery();
                });
            return delCount;
        }
    }
}
