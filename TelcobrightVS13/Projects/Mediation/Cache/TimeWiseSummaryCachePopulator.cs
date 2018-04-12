using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;

namespace TelcobrightMediation.Cache
{
    public class TimeWiseSummaryCachePopulator<TEntity, TKey> where TEntity:ICacheble<TEntity>,ISummary<TEntity,TKey>
    {
        private SummaryCache<TEntity,TKey> SummaryCache { get; }
        private PartnerEntities Context { get; }
        private string DateTimeColName { get; }
        private List<DateTime> DatesOrHoursInvolved { get; }

        public TimeWiseSummaryCachePopulator(SummaryCache<TEntity, TKey> summaryCache, PartnerEntities context,
            string dateTimeColName, List<DateTime> datesOrHoursInvolved)
        {
            this.SummaryCache = summaryCache;
            this.Context = context;
            this.DateTimeColName = dateTimeColName;
            this.DatesOrHoursInvolved = datesOrHoursInvolved;
        }

        public void Populate()
        {
            string sql = $@"select * from {this.SummaryCache.EntityOrTableName}
                                    where {this.DateTimeColName} in ({
                    string.Join(",", this.DatesOrHoursInvolved.Select(k => k.ToMySqlField()))
                })";
            List<TEntity> existingSummaries = new List<TEntity>();
            if (typeof(TEntity).Name == "AbstractCdrSummary")
            {
                //sql query result cannot be cast to Abstract Class (AbstractCdrSummary) in this case
                //temporarily cast all the existing summries as sum_voice_day_01, then up cast them to TEntity=AbstractCdrSummary
                existingSummaries = this.Context.Database.SqlQuery<sum_voice_day_01>(sql).AsEnumerable()
                    .Select(c => (TEntity) (ISummary) c).ToList();
            }
            else
            {
                existingSummaries = this.Context.Database.SqlQuery<TEntity>(sql).ToList();
            }
            this.SummaryCache.PopulateCache(
                () => existingSummaries.ToDictionary(this.SummaryCache.DictionaryKeyGenerator));
        }
    }
}
