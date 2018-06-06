using System;
using System.Globalization;
using LibraryExtensions;
using System.Collections.Generic;
using MediationModel;
using TelcobrightMediation.Accounting;

namespace TelcobrightMediation
{
    public class A2ZRater
    {
        private ServiceContext ServiceContext { get; }
        cdr Cdr { get; set; }
        public A2ZRater(ServiceContext serviceContext,cdr cdr)//constructor
        {
            this.ServiceContext = serviceContext;
            this.Cdr = cdr;
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
                   this.ServiceContext.CdrProcessor.CdrJobContext.MediationContext.Tbc.CdrSetting
                       .NotAllowedCallDateTimeBefore;
        }
        public Rateext ExecuteA2ZRating(out decimal finalDuration,out decimal finalAmount,bool flagLcr, bool useInMemoryTable)
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
            List<TupleByPeriod> tups = GetAssignmentTuples(this.ServiceContext.ServiceGroupConfiguration.IdServiceGroup);
            if(tups==null) return null;
            var prefixMatcher=new PrefixMatcher(this.ServiceContext, phoneNumber, category, subCategory,
                tups, Convert.ToDateTime(this.Cdr.AnswerTime), flagLcr, useInMemoryTable);
            Rateext rateWithAssigmentTupleId = prefixMatcher.MatchPrefix();
            if (rateWithAssigmentTupleId == null) return null;
            finalDuration = 0;
            finalDuration = prefixMatcher.GetA2ZDuration(this.Cdr.DurationSec, rateWithAssigmentTupleId);
            finalAmount = prefixMatcher.GetA2ZAmount(finalDuration, rateWithAssigmentTupleId, 0,this.ServiceContext.CdrProcessor);
            
            int ceilingUpPositionAfterDecimal = Convert.ToInt32(rateWithAssigmentTupleId.OtherAmount9);
            if (ceilingUpPositionAfterDecimal>0 && ceilingUpPositionAfterDecimal<=7)
            {
                FractionCeilingHelper ceilingHelper =
                    new FractionCeilingHelper(finalAmount, ceilingUpPositionAfterDecimal);
                finalAmount = ceilingHelper.GetPreciseDecimal();
            }
            switch (this.ServiceContext.AssignDir)
            {
                case ServiceAssignmentDirection.Customer:
                    this.Cdr.MatchedPrefixCustomer  = rateWithAssigmentTupleId.Prefix;//remove - to avoid conflict in summary
                    this.Cdr.Duration1 = finalDuration;
                    this.Cdr.InPartnerCost = Convert.ToDecimal(finalAmount);
                    this.Cdr.CustomerRate = rateWithAssigmentTupleId.rateamount;
                    this.Cdr.CountryCode = rateWithAssigmentTupleId.CountryCode;
                    break;
                case ServiceAssignmentDirection.Supplier:
                    this.Cdr.MatchedPrefixSupplier = rateWithAssigmentTupleId.Prefix;//remove - to avoid conflict in summary
                    this.Cdr.Duration2 = finalDuration;
                    this.Cdr.OutPartnerCost = Convert.ToDecimal(finalAmount);
                    this.Cdr.SupplierRate = rateWithAssigmentTupleId.rateamount;
                    this.Cdr.CountryCode = rateWithAssigmentTupleId.CountryCode;
                    break;
            }
            return rateWithAssigmentTupleId;
        }
        private List<TupleByPeriod> GetAssignmentTuples(int idServiceGroup)
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
        private List<TupleByPeriod> GetRouteTuple(int idServiceGroup)
        {
            rateplanassignmenttuple rpAssignTuple = new rateplanassignmenttuple() { idService = this.ServiceContext.ServiceFamily.Id,
                AssignDirection = Convert.ToInt32(this.ServiceContext.AssignDir) };
            route r = null;
            if (this.ServiceContext.AssignDir == ServiceAssignmentDirection.Customer)
            {
                this.ServiceContext.MefServiceFamilyContainer.DicRouteIncludingPartner.TryGetValue(new ValueTuple<int,string>(this.Cdr.SwitchId, this.Cdr.IncomingRoute), out r);
            }
            else if (this.ServiceContext.AssignDir == ServiceAssignmentDirection.Supplier)
            {
                this.ServiceContext.MefServiceFamilyContainer.DicRouteIncludingPartner.TryGetValue(new ValueTuple<int,string>(this.Cdr.SwitchId,this.Cdr.OutgoingRoute), out r);
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
            TupleDefinitions serviceGroupWiseTupDef = null;
            this.ServiceContext.MefServiceFamilyContainer.ServiceGroupWiseTupDefs.TryGetValue(idServiceGroup,
                out serviceGroupWiseTupDef);
            if (serviceGroupWiseTupDef == null)
                throw new Exception(
                    "Could not find serviceGroupWisetupDef in ServiceContext.MefServiceFamilyContainer.ServiceGroupWiseTupDefs");
                serviceGroupWiseTupDef
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

        private List<TupleByPeriod> GetPartnerTuple(int idServiceGroup)
        {
            rateplanassignmenttuple rpAssignTuple = new rateplanassignmenttuple() { idService =this.ServiceContext.ServiceFamily.Id,
                AssignDirection = Convert.ToInt32(this.ServiceContext.AssignDir) };

            if (this.ServiceContext.AssignDir == ServiceAssignmentDirection.Customer)
            {
                int idpartner = Convert.ToInt32(this.Cdr.InPartnerId) ;
                if (idpartner > 0)
                {
                    rpAssignTuple.idpartner = idpartner;
                }
                else return null;
            }
            else if (this.ServiceContext.AssignDir == ServiceAssignmentDirection.Supplier)
            {
                int idpartner = Convert.ToInt32(this.Cdr.OutPartnerId);
                if (idpartner > 0)
                {
                    rpAssignTuple.idpartner = idpartner;
                }
                else return null;
            }

            List<rateplanassignmenttuple> match = null;
            TupleDefinitions serviceGroupWiseTupDef = null;
            this.ServiceContext.MefServiceFamilyContainer.ServiceGroupWiseTupDefs.TryGetValue(idServiceGroup,
                out serviceGroupWiseTupDef);
            if (serviceGroupWiseTupDef == null)
                throw new Exception(
                    "Could not find serviceGroupWisetupDef in ServiceContext.MefServiceFamilyContainer.ServiceGroupWiseTupDefs");
            serviceGroupWiseTupDef
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
