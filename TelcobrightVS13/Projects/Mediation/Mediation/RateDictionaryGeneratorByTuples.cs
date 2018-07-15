using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibraryExtensions;
using MediationModel;
using LibraryExtensions;
namespace TelcobrightMediation
{
    public class RateDictionaryGeneratorByTuples
    {

        public long IdService = -1;
        public DateRange DRange = new DateRange();
        public ServiceAssignmentDirection AssignDirection = ServiceAssignmentDirection.None;
        public int IdRatePlan = -1;
        public int IdPartner = -1;
        public int IdRoute = -1;

        public int Priority = 0;//0=all for priority
        public string Prefix = "-1";
        public string Description = "";
        public int Category = -1;
        public int SubCategory = -1;
        public RateChangeType ChangeType = RateChangeType.All;
        RateContainerInMemoryLocal _rateContainer = null;
        List<int> _lstPartner = new List<int>();
        List<int> _lstRoute = new List<int>();
        List<int> _lstRateplan = new List<int>();

        RateTuple _baseTup = null;
        List<RateTuple> _lstTuples = new List<RateTuple>();
        Dictionary<RateTuple, List<Rateext>> _dicTupRates = new Dictionary<RateTuple, List<Rateext>>();
        PartnerEntities Context { get; }
        public RateDictionaryGeneratorByTuples(//constructor
            long pIdService,
            DateRange pDRange,
            ServiceAssignmentDirection pAssignDirection,
            int pIdRatePlan,
            int pIdPartner,
            int pIdRoute,
            int pPriority,
            string pPrefix,
            string pDescription,
            int pServiceType,
            int pSubServiceType,
            RateChangeType pChangeType,
            PartnerEntities context,
            RateContainerInMemoryLocal rateContainer
        )
        {
            this.IdService = pIdService;
            this.DRange = pDRange;
            this.AssignDirection = pAssignDirection;
            this.IdRatePlan = pIdRatePlan;
            this.IdPartner = pIdPartner;
            this.IdRoute = pIdRoute;
            this.Priority = pPriority;
            this.Prefix = pPrefix;
            this.Description = pDescription;
            this.Category = pServiceType;
            this.SubCategory = pSubServiceType;
            this.ChangeType = pChangeType;
            this.Context=context;
            this._rateContainer = rateContainer;
        }
        public Dictionary<TupleByPeriod, List<Rateext>> GetRateDict(bool useInMemoryTable)
        {

            if (this.IdService < 1)//service must be selected
            {
                throw new Exception("No service selected!");
            }

            this._baseTup = new RateTuple(this.IdService, this.DRange, null, this.AssignDirection, null, null, null);

            if (this.IdRatePlan > 0)//get rates by a single rateplan
            {
                RateTuple newTup = this._baseTup.Copy();
                newTup.IdRatePlan = this.IdRatePlan;
                this._lstTuples.Add(newTup);
            }
            else//not by single rateplan, has to fetch tuple
            {
                bool partnerAssignmentRequired = false;
                int ca = Convert.ToInt32(this.Context.enumservicefamilies.Where(c => c.id == this.IdService).First()
                    .PartnerAssignNotNeeded);
                if (ca == 0)
                {
                    partnerAssignmentRequired = true; //opposite
                }
                else if (ca > 0)
                {
                    partnerAssignmentRequired = false;
                }
                string sqlAssign = " select * from rateplanassignmenttuple where idservice=" + this.IdService;
                sqlAssign += (this.Priority > 0 ? " and priority=" + this.Priority + " " : "");

                if (partnerAssignmentRequired == true)
                {
                    sqlAssign += " and idpartner is not null ";
                    //
                    sqlAssign += (this.AssignDirection > 0
                        ? " and assigndirection= " + Convert.ToInt32(this.AssignDirection)
                        : " ");
                    if (this.IdPartner > 0) //one partner
                    {
                        sqlAssign += " and idpartner= " + this.IdPartner;
                        if (this.IdRoute > 0) //one route
                        {
                            sqlAssign += " and route= " + this.IdRoute;
                        }
                    }
                }
                else //partnerassignment not required e.g. intl incoming and outgoing
                {
                    sqlAssign += " and idpartner is null ";
                }
                foreach (rateplanassignmenttuple rt in this.Context.rateplanassignmenttuples.SqlQuery
                    (sqlAssign, typeof(rateplanassignmenttuple)).ToList().OrderBy(c => c.priority).ToList())
                {
                    RateTuple newTuple = this._baseTup.Copy();
                    newTuple.IdRateplanAssignmenttuple = rt.id;
                    newTuple.IdPartner = rt.idpartner;
                    newTuple.IdRoute = rt.route;
                    newTuple.Priority = rt.priority;

                    this._lstTuples.Add(newTuple);
                }
            } //not by single rate plan

            //dic tup rates is now loaded with proper tuple with list<rate>=null, populate each list
            foreach (RateTuple rt in this._lstTuples)
            {
                RateList rateList = new RateList(rt, this.Prefix, this.Description, this.ChangeType, this.Category,
                    this.SubCategory,
                    this.Context, this._rateContainer);

                //order by prefix ascending and startdate descending
                List<Rateext> rates = new List<Rateext>();
                //todo: remove temp code
                //todo: uncomment original codes
                //lstRates = rateList.GetAllRates(useInMemoryTable).ToList()
                //    .OrderBy(c => c.Priority).ThenBy(c => c.Prefix).ThenByDescending(c => c.P_Startdate).ToList();
                rates = rateList.GetAllRates(useInMemoryTable).ToList();
                DictionaryBasedRateSorter rateSorter = new DictionaryBasedRateSorter(rates,
                    sortByPriorityDescending: false,
                    sortByPrefixDescending: false,
                    sortByStartDateDescending: true);
                rates = rateSorter.Sort();
                //end uncommend
                this._dicTupRates.Add(rt, rates);
            }

            //each tuple actually contains idrateplanassignment value
            //convert the dictionary to something simpler
            Dictionary<TupleByPeriod,List<Rateext>> dicReturn=new Dictionary<TupleByPeriod,List<Rateext>>();
            foreach (KeyValuePair<RateTuple, List<Rateext>> kv in this._dicTupRates)
            {
                TupleByPeriod shortTup=new TupleByPeriod{
                    IdAssignmentTuple=Convert.ToInt32(kv.Key.IdRateplanAssignmenttuple),
                    DRange=kv.Key.DRange};
                dicReturn.Add(shortTup, kv.Value);
            }
            return dicReturn;
        }//get ratedict

    }
}