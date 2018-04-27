using System;
using System.Globalization;
using LibraryExtensions;
using System.Collections.Generic;
using MediationModel;
namespace TelcobrightMediation
{
    public class A2ZRater
    {
        private ServiceContext ServiceContext { get; }
        PrefixMatcher PrefixMatcher { get; set; }
        cdr Cdr { get; set; }
        public A2ZRater(ServiceContext serviceContext,cdr cdr)//constructor
        {
            this.ServiceContext = serviceContext;
            this.Cdr = cdr;
            this.PrefixMatcher = new PrefixMatcher(this.ServiceContext);
            if (this.Cdr.ChargingStatus == 1)
            {
                if (cdr.AnswerTime == null)
                {
                    if (StartTimeIsInvalidToo(cdr))
                        throw new Exception("Answer time is null & starttime is invalid too.");
                    //if duration is present but there is no answertime, cdr cannot be ignored
                    this.Cdr.AnswerTime = this.Cdr.StartTime; //use answertime=starttime
                }
                else if (cdr.AnswerTime < this.ServiceContext.CdrProcessor.CdrJobContext.MediationContext.Tbc.CdrSetting
                             .NotAllowedCallDateTimeBefore)
                {
                    throw new Exception("Answer time is before allowed call datetime, probably invalid.");
                }
            }
            else //failed call, use start time as answertime, for prefix matching, duration and amount will be 0
            {
                cdr.AnswerTime = cdr.StartTime;
            }
        }

        private bool StartTimeIsInvalidToo(cdr cdr)
        {
            return cdr.StartTime <
                   this.ServiceContext.CdrProcessor.CdrJobContext.MediationContext.Tbc.CdrSetting.NotAllowedCallDateTimeBefore;
        }
        public Rateext ExecuteA2Z(out decimal finalDuration,out decimal finalAmount,bool flagLcr, bool useInMemoryTable)
        {
            //set out params defaults
            finalDuration = 0;
            finalAmount = 0;

            int tempCategory = Convert.ToInt32(this.Cdr.Category) ;
            int tempSubCategory = Convert.ToInt32(this.Cdr.SubCategory) ;
            int category = tempCategory>0?tempCategory: 1;//default 1=call
            int subCategory = tempSubCategory>0?tempSubCategory:1;//default 1=voice

            string phoneNumber = (this.ServiceContext.AssignDir == ServiceAssignmentDirection.Customer
                ? this.Cdr.OriginatingCalledNumber
                : this.Cdr.TerminatingCalledNumber);//change per service family#####
            //following will return tuples for the day of answer time. If rateplans are assigned to routes, then routetuples
            //in the rateplanassignment table will be fetched, otherwise partner tuples will be returned
            //TupleByPeriod=one rateplanassignmenttuple on the day of answertime
            List<TupleByPeriod> tups = GetAssignmentTuples(this.ServiceContext.ServiceGroupConfiguration.IdServiceGroup
                .ToString());
            if(tups==null) return null;
            Rateext thisRateWithAssigmentTupleId = this.PrefixMatcher.MatchPrefix(phoneNumber, category, subCategory,
                tups, Convert.ToDateTime(this.Cdr.AnswerTime), flagLcr,useInMemoryTable);
            if (thisRateWithAssigmentTupleId == null) return null;
            finalDuration = 0;
            finalDuration = this.PrefixMatcher.A2ZDuration(this.Cdr.DurationSec, thisRateWithAssigmentTupleId);
            finalAmount = this.PrefixMatcher.A2ZAmount(finalDuration, thisRateWithAssigmentTupleId, 0,this.ServiceContext.CdrProcessor);

            switch (this.ServiceContext.AssignDir)
            {
                case ServiceAssignmentDirection.Customer:
                    this.Cdr.matchedprefixcustomer  = thisRateWithAssigmentTupleId.Prefix;//remove - to avoid conflict in summary
                    this.Cdr.Duration1 = finalDuration;
                    this.Cdr.CustomerCost = Convert.ToDecimal(finalAmount);
                    this.Cdr.CustomerRate = thisRateWithAssigmentTupleId.rateamount;
                    break;
                case ServiceAssignmentDirection.Supplier:
                    this.Cdr.matchedprefixsupplier = thisRateWithAssigmentTupleId.Prefix;//remove - to avoid conflict in summary
                    this.Cdr.Duration2 = finalDuration;
                    this.Cdr.SupplierCost = Convert.ToDecimal(finalAmount);
                    this.Cdr.SupplierRate = thisRateWithAssigmentTupleId.rateamount;
                    break;
            }
            return thisRateWithAssigmentTupleId;
        }
        private List<TupleByPeriod> GetAssignmentTuples(string idServiceGroup)
        {
            //first try by route
            List<TupleByPeriod> newtup = GetRouteTuple(idServiceGroup);
            if (newtup != null)
            {
                return newtup;
            }
                
            //then by Partner
            newtup = GetPartnerTuple(idServiceGroup);
            if (newtup != null) return newtup;
            return null;
        }
        private List<TupleByPeriod> GetRouteTuple(string idServiceGroup)
        {
            rateplanassignmenttuple rpAssignTuple = new rateplanassignmenttuple() { idService = this.ServiceContext.ServiceFamily.Id,
                AssignDirection = Convert.ToInt32(this.ServiceContext.AssignDir) };
            route r = null;
            if (this.ServiceContext.AssignDir == ServiceAssignmentDirection.Customer)
            {
                this.ServiceContext.MefServiceFamilyContainer.DicRouteIncludingPartner.TryGetValue(new ValueTuple<int,string>(this.Cdr.SwitchId, this.Cdr.incomingroute), out r);
            }
            else if (this.ServiceContext.AssignDir == ServiceAssignmentDirection.Supplier)
            {
                this.ServiceContext.MefServiceFamilyContainer.DicRouteIncludingPartner.TryGetValue(new ValueTuple<int,string>(this.Cdr.SwitchId,this.Cdr.outgoingroute), out r);
            }
            if (r != null)
            {
                rpAssignTuple.route = r.idroute;
            }
            else
            {
                return null;
            }
            List<rateplanassignmenttuple> match = null;
            this.ServiceContext.MefServiceFamilyContainer.ServiceGroupWiseTupDefs[idServiceGroup]
                .DicRouteTuplesIncludingBillingRuleAssignment.TryGetValue(rpAssignTuple.GetTuple(), out match);

            if (match != null)
            {
                DateRange dRange = new DateRange();
                dRange.StartDate = Convert.ToDateTime(this.Cdr.AnswerTime).Date;
                dRange.EndDate = Convert.ToDateTime(this.Cdr.AnswerTime).Date.AddDays(1);
                //return new TupleByPeriod() { dRange = dRange, idAssignmentTuple = match.id };
                List<TupleByPeriod> lstTupleByPeriods = new List<TupleByPeriod>();
                foreach (rateplanassignmenttuple rTup in match)
                {
                    lstTupleByPeriods.Add(new TupleByPeriod() { DRange = dRange, IdAssignmentTuple = rTup.id,Priority=rTup.priority });
                }
                return lstTupleByPeriods;
            }
            else return null;
        }

        private List<TupleByPeriod> GetPartnerTuple(string idServiceGroup)
        {
            rateplanassignmenttuple rpAssignTuple = new rateplanassignmenttuple() { idService =this.ServiceContext.ServiceFamily.Id,
                AssignDirection = Convert.ToInt32(this.ServiceContext.AssignDir) };

            if (this.ServiceContext.AssignDir == ServiceAssignmentDirection.Customer)
            {
                int idpartner = Convert.ToInt32(this.Cdr.inPartnerId) ;
                if (idpartner > 0)
                {
                    rpAssignTuple.idpartner = idpartner;
                }
                else return null;
            }
            else if (this.ServiceContext.AssignDir == ServiceAssignmentDirection.Supplier)
            {
                int idpartner = Convert.ToInt32(this.Cdr.outPartnerId);
                if (idpartner > 0)
                {
                    rpAssignTuple.idpartner = idpartner;
                }
                else return null;
            }

            List<rateplanassignmenttuple> match = null;
            this.ServiceContext.MefServiceFamilyContainer.ServiceGroupWiseTupDefs[idServiceGroup]
                .DicPartnerTuplesIncludingBillingRuleAssignment.TryGetValue(rpAssignTuple.GetTuple(), out match);

            if (match != null)
            {
                DateRange dRange = new DateRange();
                dRange.StartDate = Convert.ToDateTime(this.Cdr.AnswerTime).Date;
                dRange.EndDate = Convert.ToDateTime(this.Cdr.AnswerTime).Date.AddDays(1);
                //return 
                List<TupleByPeriod> lstTupleByPeriods = new List<TupleByPeriod>();
                foreach(rateplanassignmenttuple rtup in match)
                {
                    lstTupleByPeriods.Add(new TupleByPeriod() { DRange = dRange, IdAssignmentTuple = rtup.id,
                    Priority=rtup.priority});   
                }
                return lstTupleByPeriods;
            }
            else return null;
        }

    }
}
