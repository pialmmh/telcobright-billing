using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using MySql.Data;
using MySql.Data.MySqlClient;
using MediationModel;

namespace TelcobrightMediation
{
    class RateList
    {
        public bool IsCachingForMediation { get; set; } = false;
        public static bool IsRatePlanWiseRateCacheInitialized { get; set; } = false;
        static Dictionary<long, List<Rateext>> RatePlanWiseCachedRatesForMediation { get; set; } =
            new Dictionary<long, List<Rateext>>();
        RateTuple _rTup = null;
        string _prefix = "-1";
        string _description = "";
        int _category = -1;
        int _subCategory = -1;
        RateChangeType _changeType = RateChangeType.All;
        private PartnerEntities Context { get; }
        RateContainerInMemoryLocal _rateContainer = null;

        public RateList(//constructor
            RateTuple pRtup,
            string pPrefix,
            string pDescription,
            RateChangeType pChangeType, int pServiceType, int pSubServiceType, PartnerEntities context,
            RateContainerInMemoryLocal rateContainer)
        {
            this._rTup = pRtup;
            this._prefix = pPrefix;
            this._description = pDescription;
            this._changeType = pChangeType;
            this._category = pServiceType;
            this._subCategory = pSubServiceType;
            this.Context = context;
            this._rateContainer = rateContainer;
        }


        public List<Rateext> GetAllRates(bool useInMemoryTable)
        {
            List<RateAssignWithTuple> lstRatePlans = new List<RateAssignWithTuple>();
            lstRatePlans = GetRatePlans();

            var prevLatencyMode = GCSettings.LatencyMode;
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            List<Rateext> lstRates = new List<Rateext>();
            foreach (RateAssignWithTuple ratePlan in lstRatePlans)
            {
                //for each rate plans, keep on adding list of rates
                List<Rateext> newrates = GetRatesByRatePlan(ratePlan, useInMemoryTable);
                lstRates.AddRange(newrates.Select(newRate => this._rateContainer.Append(newRate)));
            }
            GCSettings.LatencyMode = prevLatencyMode;
            //lstRates.OrderBy(c=>c.p)
            //obRates.obj = lstRates;


            return lstRates;
        }

        List<RateAssignWithTuple> GetRatePlans()
        {

            List<RateAssignWithTuple> lstRatePlans = new List<RateAssignWithTuple>();
            if (this._rTup.IdRatePlan > 0)//rateplan is mentioned directly, fetch rates for that rateplan only
            {
                RateAssignWithTuple rp = new RateAssignWithTuple();
                rp.IntersectingDateSpan = this._rTup.DRange;
                rp.RTup = this._rTup;
                lstRatePlans.Add(rp);
            }
            else//rate plan is not mentioned directly
            {
                //check assignment required part
                //if partnerassignment is not required but assign required e.g. intl in rateplans
                //partner and route will be set to -1
                lstRatePlans = GetRatePlanByAssignmentTuple();

            }

            return lstRatePlans;
        }


        List<RateAssignWithTuple> GetRatePlanByAssignmentTuple()
        {
            List<RateAssignWithTuple> lstRatePlan = new List<RateAssignWithTuple>();
            long tupleId = Convert.ToInt64(this._rTup.IdRateplanAssignmenttuple);
            //rateassign is the actual rateplan assignment, field "prefix" is the tuple id
            string sql2 = " select * from rateassign where prefix =" + //list all the rateplans first
                          +tupleId; //, rate plans wouldn't be too many

            List<rateassign> lstPrefix = this.Context.Database.SqlQuery<rateassign>(sql2).ToList();
            foreach (rateassign ra in lstPrefix) //assigned rateplans
            {
                RateAssignWithTuple pSpan = new RateAssignWithTuple();
                //find intersecting daterange for each assinged rateplan
                DateRange assignedSpan = new DateRange();
                assignedSpan.StartDate = ra.startdate;
                if (ra.enddate == null)
                {
                    assignedSpan.EndDate = new DateTime(9999, 12, 31, 23, 59, 59); //default future date
                }
                else
                {
                    assignedSpan.EndDate = Convert.ToDateTime(ra.enddate);
                }
                pSpan.IntersectingDateSpan = Util.DateIntersection(this._rTup.DRange, assignedSpan);

                if (pSpan.IntersectingDateSpan != null) //add this rateassignment only if the period intersects
                {
                    pSpan.RTup = this._rTup;
                    pSpan.Rassign = ra;
                    lstRatePlan.Add(pSpan);
                }
            }


            return lstRatePlan;
        }
        private static int GetIdRatePlanFromAssignmentTuple(RateAssignWithTuple rateAssignWithTuple)
        {
            int idRatePlan = -1;
            if (rateAssignWithTuple.RTup.IdRatePlan >= 1) //by single rateplan
            {
                idRatePlan = Convert.ToInt32(rateAssignWithTuple.RTup.IdRatePlan);
            }
            else //by assignment tuple
            {
                idRatePlan = rateAssignWithTuple.Rassign.Inactive;
            }

            return idRatePlan;
        }
        List<Rateext> GetRatesByRatePlan(RateAssignWithTuple rateAssignWithTuple, bool useInMemoryTable)
        {
            if (IsRatePlanWiseRateCacheInitialized == true)
            {
                return GetRatesFromLocalCache(rateAssignWithTuple);
            }
            List<Rateext> lstRates = new List<Rateext>();
            StringBuilder sbRate = new StringBuilder();

            string strOpenRatePlan = "";
            if (rateAssignWithTuple.Rassign == null)
            {
                strOpenRatePlan = " (select -2) as OpenRateAssignment  "; //any value except 1, for rateplan based query
            }
            else
            {
                strOpenRatePlan = " (select " + (rateAssignWithTuple.Rassign.enddate == null ? "1" : "0") +
                                  ") as OpenRateAssignment  ";
            }

            string strEndDateByRatePlan = "";

            if (rateAssignWithTuple.Rassign == null || rateAssignWithTuple.Rassign.enddate == null)
            {
                strEndDateByRatePlan = " (select null) ";
            }
            else
            {
                strEndDateByRatePlan = " (select '" + Convert.ToDateTime(rateAssignWithTuple.Rassign.enddate)
                                           .ToString("yyyy-MM-dd HH:mm:ss") + "') ";
            }
            strEndDateByRatePlan += " as enddatebyrateplan, ";

            string strStartDateByRatePlan = "";

            if (rateAssignWithTuple.Rassign == null || rateAssignWithTuple.Rassign.startdate == null)
            {
                strStartDateByRatePlan = " (select null) ";
            }
            else
            {
                strStartDateByRatePlan = " (select '" + Convert.ToDateTime(rateAssignWithTuple.Rassign.startdate)
                                             .ToString("yyyy-MM-dd HH:mm:ss") + "') ";
            }
            strStartDateByRatePlan += " as startdatebyrateplan, ";

            //when using this method from portal, there are no in memory caching of rates.
            string rateTableNameBaseOnInMemoryFlag = useInMemoryTable == true ? "temp_rate" : "rate";

            sbRate.Append(" select r.*,(select " +
                          (rateAssignWithTuple.RTup.Priority == null ? " null " : rateAssignWithTuple.RTup.Priority.ToString()) +
                          ") as priority, " +
                          //" (select '" + RatePlan.rassign.startdate.ToString("yyyy-MM-dd HH:mm:ss") + "') as startdatebyrateplan, " +
                          strStartDateByRatePlan +
                          strEndDateByRatePlan +
                          " (select " + (rateAssignWithTuple.RTup.IdRatePlan > 0 ? "0" : "1") + ") as AssignmentFlag, " +
                          " (select " + (rateAssignWithTuple.RTup.IdPartner == null ? "-1" : rateAssignWithTuple.RTup.IdPartner.ToString()) +
                          ") as idPartner, " +
                          " (select " + (rateAssignWithTuple.RTup.IdRoute == null ? "-1" : rateAssignWithTuple.RTup.IdRoute.ToString()) +
                          ") as idRoute, " +
                          strOpenRatePlan +
                          $@" from {rateTableNameBaseOnInMemoryFlag} r where ")
                .Append(rateAssignWithTuple.IntersectingDateSpan.GetWhereExpressionRates("startdate", "enddate"));

            if (this.IsCachingForMediation == false)//e.g. from portal rate query
            {
                if (rateAssignWithTuple.RTup.IdRatePlan >= 1) //by single rateplan
                {
                    sbRate.Append(" and idrateplan= " + rateAssignWithTuple.RTup.IdRatePlan + " ");
                }
                else //by assignment tuple
                {
                    sbRate.Append(" and idrateplan= " + rateAssignWithTuple.Rassign.Inactive +
                                  " "); //inactive is the idrateplan in rateassigntable
                }
            }
            if (this._prefix.Trim() != "")
            {
                sbRate.Append(" and prefix like '" + this._prefix.Replace('*', '%') + "' ");
            }
            if (this._description.Trim() != "")
            {
                sbRate.Append(" and lower(description) like '%" + this._description.ToLower() + "%'");
            }
            if (Convert.ToInt32(this._changeType) >= 1)
            {
                sbRate.Append(" and status=" + Convert.ToInt32(this._changeType) + " ");
            }
            if (Convert.ToInt32(this._category) >= 1)
            {
                sbRate.Append(" and Category=" + Convert.ToInt32(this._category) + " ");
            }
            if (Convert.ToInt32(this._subCategory) >= 1)
            {
                sbRate.Append(" and SubCategory=" + Convert.ToInt32(this._subCategory) + " ");
            }
            string sql =
                " select case when crt.type is not null then crt.type else crp.type end as pcurrency, rp.field4 as TechPrefix,x.* from( " +
                sbRate.ToString() +
                " ) x left join rateplan rp on x.idrateplan=rp.id left join enumcurrency crt on x.currency=crt.id left join enumcurrency crp on rp.currency=crp.id";
            if (this.IsCachingForMediation == false)//from portal
            {
                lstRates = this.Context.Database.SqlQuery<Rateext>(sql, typeof(Rateext)).ToList();
                return lstRates;
            }
            else
            {
                if (IsRatePlanWiseRateCacheInitialized == false)
                {
                    RatePlanWiseCachedRatesForMediation = BuildRatePlanWiseLocalRateCache(sql, 500000);
                    IsRatePlanWiseRateCacheInitialized = true;
                }
                return GetRatesFromLocalCache(rateAssignWithTuple);
            }
        }

        private List<Rateext> GetRatesFromLocalCache(RateAssignWithTuple rateAssignWithTuple)
        {
            List<Rateext> rates = null;
            int idRatePlan = GetIdRatePlanFromAssignmentTuple(rateAssignWithTuple);
            RatePlanWiseCachedRatesForMediation.TryGetValue(idRatePlan, out rates);
            if (rates == null)
            {
                return new List<Rateext>();
            }
            else
            {
                return rates;
            }
        }

        private Dictionary<long, List<Rateext>> BuildRatePlanWiseLocalRateCache(string sql, int segmentSize)
        {
            int startLimit = 0;
            List<Rateext> rates = new List<Rateext>();
            MySqlConnection connection = (MySqlConnection)this.Context.Database.Connection;
            MySqlCommand cmd = connection.CreateCommand();

            bool moreRecordMayExist = true;
            while (moreRecordMayExist)
            {
                var sqlWithOrderBy = sql.Replace(" ) x", $" order by r.id limit {startLimit},{segmentSize} ) x");
                cmd.CommandText = sqlWithOrderBy;
                List<Rateext> ratesForThisSegment = fetchRateExtSegment(cmd);
                if (ratesForThisSegment.Any())
                {
                    rates.AddRange(ratesForThisSegment);
                    startLimit += segmentSize;
                }
                else
                {
                    moreRecordMayExist = false;
                }
            }
            Dictionary<long, List<Rateext>> ratePlanWiseRates =
                rates.Where(r => r.idrateplan != null)
                    .GroupBy(r => (long)r.idrateplan).ToDictionary(g => g.Key, g => g.ToList());
            return ratePlanWiseRates;
        }

        private static List<Rateext> fetchRateExtSegment(MySqlCommand cmd)
        {
            DataTable dt = new DataTable();
            dt.Load(cmd.ExecuteReader());
            IEnumerable<DataRow> readerRows = dt.Rows.OfType<DataRow>();
            var parallelIterator = new ParallelIterator<DataRow, Rateext>(readerRows);
            List<Rateext> ratesForThisSegment = parallelIterator
                .getOutput(readerRow =>
                {
                    Rateext rateExt = new Rateext();
                    rateExt.Pcurrency = Convert.ToString(readerRow["pcurrency"]);
                    rateExt.TechPrefix = Convert.ToString(readerRow["TechPrefix"]);
                    rateExt.id = Convert.ToInt32(readerRow["id"]);
                    rateExt.ProductId = Convert.ToInt32(readerRow["ProductId"]);
                    rateExt.Prefix = Convert.ToString(readerRow["Prefix"]);
                    rateExt.description = Convert.ToString(readerRow["description"]);
                    rateExt.rateamount = Convert.ToDecimal(readerRow["rateamount"]);
                    rateExt.WeekDayStart = Convert.ToInt32(readerRow["WeekDayStart"]);
                    rateExt.WeekDayEnd = Convert.ToInt32(readerRow["WeekDayEnd"]);
                    rateExt.starttime = Convert.ToString(readerRow["starttime"]);
                    rateExt.endtime = Convert.ToString(readerRow["endtime"]);
                    rateExt.Resolution = Convert.ToInt32(readerRow["Resolution"]);
                    rateExt.MinDurationSec = Convert.ToSingle(readerRow["MinDurationSec"]);
                    rateExt.SurchargeTime = Convert.ToInt32(readerRow["SurchargeTime"]);
                    rateExt.SurchargeAmount = Convert.ToDecimal(readerRow["SurchargeAmount"]);
                    rateExt.idrateplan = Convert.ToInt32(readerRow["idrateplan"]);
                    rateExt.CountryCode = Convert.ToString(readerRow["CountryCode"]);
                    rateExt.date1= readerRow["date1"]==DBNull.Value?(DateTime?)null: Convert.ToDateTime(readerRow["date1"]);
                    rateExt.field1 = readerRow["field1"]==DBNull.Value? (int?)null: Convert.ToInt32(readerRow["field1"]);
                    rateExt.field2 = readerRow["field2"]==DBNull.Value?(int?)null: Convert.ToInt32(readerRow["field2"]);
                    rateExt.field3 = Convert.ToInt32(readerRow["field3"]);
                    rateExt.field4 = Convert.ToString(readerRow["field4"]);
                    rateExt.field5 = Convert.ToString(readerRow["field5"]);
                    rateExt.startdate = Convert.ToDateTime(readerRow["startdate"]);
                    rateExt.enddate = readerRow["enddate"]==DBNull.Value? (DateTime?)null: Convert.ToDateTime(readerRow["enddate"]);
                    rateExt.Inactive = Convert.ToInt32(readerRow["Inactive"]);
                    rateExt.RouteDisabled = Convert.ToInt32(readerRow["RouteDisabled"]);
                    rateExt.Type = Convert.ToInt32(readerRow["Type"]);
                    rateExt.Currency = Convert.ToInt32(readerRow["Currency"]);
                    rateExt.OtherAmount1 = readerRow["OtherAmount1"]==DBNull.Value?(decimal?)null: Convert.ToDecimal(readerRow["OtherAmount1"]);
                    rateExt.OtherAmount2 = readerRow["OtherAmount2"]==DBNull.Value?(decimal?)null: Convert.ToDecimal(readerRow["OtherAmount2"]);
                    rateExt.OtherAmount3 = readerRow["OtherAmount3"]==DBNull.Value?(decimal?)null: Convert.ToDecimal(readerRow["OtherAmount3"]);
                    rateExt.OtherAmount4 = readerRow["OtherAmount4"]==DBNull.Value?(decimal?)null: Convert.ToDecimal(readerRow["OtherAmount4"]);
                    rateExt.OtherAmount5 = readerRow["OtherAmount5"]==DBNull.Value?(decimal?)null: Convert.ToDecimal(readerRow["OtherAmount5"]);
                    rateExt.OtherAmount6 = readerRow["OtherAmount6"]==DBNull.Value?(decimal?)null: Convert.ToDecimal(readerRow["OtherAmount6"]);
                    rateExt.OtherAmount7 = readerRow["OtherAmount7"]==DBNull.Value?(float?)null: Convert.ToSingle(readerRow["OtherAmount7"]);
                    rateExt.OtherAmount8 = readerRow["OtherAmount8"]==DBNull.Value?(float?)null: Convert.ToSingle(readerRow["OtherAmount8"]);
                    rateExt.OtherAmount9 = readerRow["OtherAmount9"] == DBNull.Value ? (float?)null : Convert.ToSingle(readerRow["OtherAmount9"]);
                    rateExt.OtherAmount10 = readerRow["OtherAmount10"] == DBNull.Value ? (float?)null : Convert.ToSingle(readerRow["OtherAmount10"]);
                    rateExt.TimeZoneOffsetSec = Convert.ToInt32(readerRow["TimeZoneOffsetSec"]);
                    rateExt.RatePosition = readerRow["RatePosition"]==DBNull.Value?(int?)null: Convert.ToInt32(readerRow["RatePosition"]);
                    rateExt.IgwPercentageIn = readerRow["IgwPercentageIn"] == DBNull.Value
                        ? (float?) null
                        : Convert.ToSingle(readerRow["IgwPercentageIn"]);
                    rateExt.ConflictingRateIds = Convert.ToString(readerRow["ConflictingRateIds"]);
                    rateExt.ChangedByTaskId = readerRow["ChangedByTaskId"]==DBNull.Value?(long?)null: Convert.ToInt64(readerRow["ChangedByTaskId"]);
                    rateExt.ChangedOn = readerRow["ChangedOn"]==DBNull.Value? (DateTime?)null: Convert.ToDateTime(readerRow["ChangedOn"]);
                    rateExt.Status = readerRow["Status"]==DBNull.Value? (int?)null: Convert.ToInt32(readerRow["Status"]);
                    rateExt.idPreviousRate = readerRow["idPreviousRate"]==DBNull.Value? (long?)null: Convert.ToInt64(readerRow["idPreviousRate"]);
                    rateExt.EndPreviousRate = readerRow["EndPreviousRate"]==DBNull.Value? (sbyte?)null: Convert.ToSByte(readerRow["EndPreviousRate"]);
                    rateExt.Category = readerRow["Category"]==DBNull.Value? (sbyte?)null:  Convert.ToSByte(readerRow["Category"]);
                    rateExt.SubCategory = readerRow["SubCategory"]==DBNull.Value? (sbyte?)null: Convert.ToSByte(readerRow["SubCategory"]);
                    rateExt.ChangeCommitted = readerRow["ChangeCommitted"]==DBNull.Value?(int?)null: Convert.ToInt32(readerRow["ChangeCommitted"]);
                    rateExt.ConflictingRates = Convert.ToString(readerRow["ConflictingRates"]);
                    rateExt.OverlappingRates = Convert.ToString(readerRow["OverlappingRates"]);
                    rateExt.Comment1 = Convert.ToString(readerRow["Comment1"]);
                    rateExt.Comment2 = Convert.ToString(readerRow["Comment2"]);
                    rateExt.billingspan = readerRow["billingspan"]==DBNull.Value? (int?)null: Convert.ToInt32(readerRow["billingspan"]);
                    rateExt.RateAmountRoundupDecimal = readerRow["RateAmountRoundupDecimal"]==DBNull.Value?(int?)null: Convert.ToInt32(readerRow["RateAmountRoundupDecimal"]);
                    rateExt.Priority = Convert.ToInt32(readerRow["priority"]);
                    rateExt.Startdatebyrateplan = readerRow["startdatebyrateplan"]==DBNull.Value?(DateTime?)null: Convert.ToDateTime(readerRow["startdatebyrateplan"]);
                    rateExt.Enddatebyrateplan = readerRow["enddatebyrateplan"]==DBNull.Value? (DateTime?)null: Convert.ToDateTime(readerRow["enddatebyrateplan"]);
                    rateExt.AssignmentFlag = Convert.ToInt32(readerRow["AssignmentFlag"]);
                    rateExt.IdPartner = Convert.ToInt32(readerRow["idPartner"]);
                    rateExt.IdRoute = Convert.ToInt32(readerRow["idRoute"]);
                    rateExt.OpenRateAssignment = Convert.ToInt32(readerRow["OpenRateAssignment"]);
                    return rateExt;
                });
            return ratesForThisSegment;
        }
    }
}
