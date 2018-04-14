using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibraryExtensions;
using MySql.Data.MySqlClient;
using MediationModel;
namespace TelcobrightMediation
{

    public class RateAssignWithTuple
    {
        public DateRange IntersectingDateSpan = null;
        public rateassign Rassign = null;
        public RateTuple RTup = null;
    }

    //public class idVsDateRange : idVsDateRange
    //{
    //    public int Priority = 0;
    //    public int PartnerAssignable = 0;
    //    public long idInRateAssignTbl = 0;
    //}


    class RateList
    {
        RateTuple _rTup = null;
        string _prefix = "-1";
        string _description = "";
        int _category = -1;
        int _subCategory = -1;
        RateChangeType _changeType = RateChangeType.All;
        private PartnerEntities  Context { get; }
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
            this.Context=context;
            this._rateContainer = rateContainer;
        }


        public List<Rateext> GetAllRates(bool useInMemoryTable)
        {
            List<RateAssignWithTuple> lstRatePlans = new List<RateAssignWithTuple>();

            lstRatePlans = GetRatePlans();

            List<Rateext> lstRates = new List<Rateext>();
            foreach (RateAssignWithTuple ratePlan in lstRatePlans)
            {
                //for each rate plans, keep on adding list of rates
                List<Rateext> newrates = GetRatesByRatePlan(ratePlan,useInMemoryTable);
                lstRates.AddRange(newrates.Select(newRate => this._rateContainer.Append(newRate)));
            }
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

        List<Rateext> GetRatesByRatePlan(RateAssignWithTuple ratePlan,bool useInMemoryTable)
        {
            List<Rateext> lstRates = new List<Rateext>();
            StringBuilder sbRate = new StringBuilder();

            string strOpenRatePlan = "";
            if (ratePlan.Rassign == null)
            {
                strOpenRatePlan = " (select -2) as OpenRateAssignment  "; //any value except 1, for rateplan based query
            }
            else
            {
                strOpenRatePlan = " (select " + (ratePlan.Rassign.enddate == null ? "1" : "0") +
                                  ") as OpenRateAssignment  ";
            }

            string strEndDateByRatePlan = "";

            if (ratePlan.Rassign == null || ratePlan.Rassign.enddate == null)
            {
                strEndDateByRatePlan = " (select null) ";
            }
            else
            {
                strEndDateByRatePlan = " (select '" + Convert.ToDateTime(ratePlan.Rassign.enddate)
                                           .ToString("yyyy-MM-dd HH:mm:ss") + "') ";
            }
            strEndDateByRatePlan += " as enddatebyrateplan, ";

            string strStartDateByRatePlan = "";

            if (ratePlan.Rassign == null || ratePlan.Rassign.startdate == null)
            {
                strStartDateByRatePlan = " (select null) ";
            }
            else
            {
                strStartDateByRatePlan = " (select '" + Convert.ToDateTime(ratePlan.Rassign.startdate)
                                             .ToString("yyyy-MM-dd HH:mm:ss") + "') ";
            }
            strStartDateByRatePlan += " as startdatebyrateplan, ";

            //when using this method from portal, there are no in memory caching of rates.
            string rateTableNameBaseOnInMemoryFlag = useInMemoryTable == true ? "temp_rate" : "rate";

            sbRate.Append(" select r.*,(select " +
                          (ratePlan.RTup.Priority == null ? " null " : ratePlan.RTup.Priority.ToString()) +
                          ") as priority, " +
                          //" (select '" + RatePlan.rassign.startdate.ToString("yyyy-MM-dd HH:mm:ss") + "') as startdatebyrateplan, " +
                          strStartDateByRatePlan +
                          strEndDateByRatePlan +
                          " (select " + (ratePlan.RTup.IdRatePlan > 0 ? "0" : "1") + ") as AssignmentFlag, " +
                          " (select " + (ratePlan.RTup.IdPartner == null ? "-1" : ratePlan.RTup.IdPartner.ToString()) +
                          ") as idPartner, " +
                          " (select " + (ratePlan.RTup.IdRoute == null ? "-1" : ratePlan.RTup.IdRoute.ToString()) +
                          ") as idRoute, " +
                          strOpenRatePlan +
                          $@" from {rateTableNameBaseOnInMemoryFlag} r where ")
                .Append(ratePlan.IntersectingDateSpan.GetWhereExpressionRates("startdate", "enddate"));

            if (ratePlan.RTup.IdRatePlan >= 1) //by single rateplan
            {
                sbRate.Append(" and idrateplan= " + ratePlan.RTup.IdRatePlan + " ");
            }
            else //by assignment tuple
            {
                sbRate.Append(" and idrateplan= " + ratePlan.Rassign.Inactive +
                              " "); //inactive is the idrateplan in rateassigntable
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
            lstRates = this.Context.Database.SqlQuery<Rateext>(sql,typeof(Rateext)).ToList();
            return lstRates;
        }
    }
}
