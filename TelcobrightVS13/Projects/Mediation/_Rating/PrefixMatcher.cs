using System;
using System.Collections.Generic;
using LibraryExtensions;
using System.Linq;
using MediationModel;
namespace TelcobrightMediation
{
    class RatesWithAssignmentTuple
    {
        public TupleByPeriod Tup { get; }
        public List<Rateext> Rates { get; }

        public RatesWithAssignmentTuple(TupleByPeriod tup, List<Rateext> rates)
        {
            this.Tup = tup;
            this.Rates = rates;
        }
    }
    public class PrefixMatcher
    {
        private Dictionary<string, rateplan> DicRatePlan { get; set; }
        private ServiceContext ServiceContext { get; }
        public PrefixMatcher(ServiceContext serviceContext)
        {
            this.ServiceContext = serviceContext;
            this.DicRatePlan =serviceContext.MefServiceFamilyContainer.RateCache.DicRatePlan;
        }
        public Rateext MatchPrefix(string phoneNumber, int category, int subCategory, List<TupleByPeriod> tups, DateTime answerTime,
            bool flagLcr)
        {
            //TupleByPeriod=one rateplanassignmenttuple on the day of answertime
            List<Dictionary<string, RatesWithAssignmentTuple>> priorityWisePrefixDicWithAssignTuple = new List<Dictionary<string, RatesWithAssignmentTuple>>();
            foreach (TupleByPeriod tup in tups.OrderBy(c => c.Priority).ToList())
            {
                Dictionary<string, List<Rateext>> prefixDic = null;
                prefixDic = GetPrefixWiseRateInstances(tup, flagLcr);
                Dictionary<string,RatesWithAssignmentTuple> prefixDicWithAssignmentTuples
                    =new Dictionary<string, RatesWithAssignmentTuple>();
                foreach (KeyValuePair<string, List<Rateext>> kv in prefixDic)
                {
                    prefixDicWithAssignmentTuples.Add(kv.Key,new RatesWithAssignmentTuple(tup,kv.Value));
                }
                if (prefixDic != null) priorityWisePrefixDicWithAssignTuple.Add(prefixDicWithAssignmentTuples);
            }

            //if (PrefixDic == null) return null;
            List<string> lstPhoneNumbers = new List<string>();
            for (int i = phoneNumber.Length; i > 0; i--)
            {
                lstPhoneNumbers.Add(phoneNumber.Substring(0, i));
            }
            foreach (Dictionary<string, RatesWithAssignmentTuple> prefixDic in priorityWisePrefixDicWithAssignTuple)
            {
                Rateext matchedRate = null;
                bool matchFound = false;
                foreach (string prefix in lstPhoneNumbers)
                {
                    if (matchFound == true) break;
                    RatesWithAssignmentTuple ratesWithAssignmentTuple = null;
                    prefixDic.TryGetValue(prefix, out ratesWithAssignmentTuple);
                    if (ratesWithAssignmentTuple == null)
                    {
                        continue;
                    }
                    List<Rateext> lstRates = ratesWithAssignmentTuple.Rates;
                    foreach (Rateext thisRate in lstRates)
                    {
                        if (thisRate.Category == category && thisRate.SubCategory == subCategory &&
                            answerTime >= thisRate.P_Startdate && answerTime < (thisRate.P_Enddate != null ? thisRate.P_Enddate : new DateTime(9999, 12, 31, 23, 59, 59)))
                        {
                            matchedRate = thisRate;
                            matchedRate.IdRatePlanAssignmentTuple =
                                Convert.ToInt64(ratesWithAssignmentTuple.Tup.IdAssignmentTuple);
                            matchFound = true;
                            break;//rates are sorted desc, starttime. latest match will be returned immediately
                        }
                    }
                }
                if (matchedRate != null) return matchedRate;
            }
            return null;

        }


        public Dictionary<string, List<Rateext>> GetPrefixWiseRateInstances(TupleByPeriod tup, bool flagLcr)
        {
            //TupleByPeriod=one rateplanassignmenttuple on the day of answertime
            Dictionary<TupleByPeriod, Dictionary<string, List<Rateext>>> dicRatesByDay
                = this.ServiceContext.MefServiceFamilyContainer.RateCache.GetRateDictsByDay(tup.DRange, flagLcr);
            Dictionary<string, List<Rateext>> dicRatesByPrefix = null;
            if (dicRatesByDay != null)
            {
                dicRatesByDay.TryGetValue(tup, out dicRatesByPrefix);
            }
            return dicRatesByPrefix;
        }

        public decimal A2ZDuration(decimal actualDurationSec, Rateext thisRate)
        {
            if (actualDurationSec == 0) return 0;

            float minDurationSec = thisRate.MinDurationSec;

            if (minDurationSec < 0)//no rounding, use actual duration
            {
                return actualDurationSec;
            }
            else if (minDurationSec > 0)//e.g. minimum .1 sec (100 ms) required for rounding up
            {
                //the code below works upto 11 digits e.g. 3538.099999999994 if the last digit >4 or there is more decimal then only rounds up
                decimal floorDuration = Convert.ToDecimal(Math.Floor(actualDurationSec));
                decimal miliSecPart = Convert.ToDecimal(actualDurationSec) - floorDuration;
                if (miliSecPart >= Convert.ToDecimal(minDurationSec))//holly sheet! 3000.1-3000 is not .1, 0.1999999999998181
                {
                    actualDurationSec = Math.Ceiling(actualDurationSec);
                }
                else
                {
                    actualDurationSec = Math.Floor(actualDurationSec);
                }
            }
            else//always round up
            {
                actualDurationSec = Math.Ceiling(actualDurationSec);
            }
            long lngDuration = Convert.ToInt64(actualDurationSec);
            if (thisRate.Resolution > 0)
            {
                long lngResolution = Convert.ToInt64(thisRate.Resolution);
                if (lngDuration % lngResolution > 0)
                {
                    lngDuration = ((lngDuration / lngResolution) + 1) * lngResolution;
                }
                else
                {
                    lngDuration = ((lngDuration / lngResolution)) * lngResolution;
                }
            }
            return lngDuration;
        }

        public decimal HundredMsDuration(decimal actualDurationSec)
        {
            if (actualDurationSec == 0) return 0;
            decimal minDurationSec = .1M;
            if (minDurationSec > 0) //e.g. minimum .1 sec (100 ms) required for rounding up
            {
                //the code below works upto 11 digits e.g. 3538.099999999994 if the last digit >4 or there is more decimal then only rounds up
                decimal floorDuration = Math.Floor(actualDurationSec);
                decimal miliSecPart = actualDurationSec - floorDuration;

                if (miliSecPart >= minDurationSec) //holly sheet! for double, 3000.1-3000 is not .1, 0.1999999999998181
                {
                    actualDurationSec = Math.Ceiling(actualDurationSec);
                }
                else
                {
                    actualDurationSec = Math.Floor(actualDurationSec);
                }
            }
            return actualDurationSec;
        }


        public decimal A2ZAmount(decimal finalDurationSec, Rateext thisRate, int indexForRateAmount,
            CdrProcessor cdrProcessor)
        {
            decimal thisRateAmount = 0;
            if (finalDurationSec == 0) return 0;
            switch (indexForRateAmount)
            {
                case 0: thisRateAmount = thisRate.rateamount; break;
                case 1: thisRateAmount = Convert.ToDecimal(thisRate.OtherAmount1); break;
                case 2: thisRateAmount = Convert.ToDecimal(thisRate.OtherAmount2); break;
                case 3: thisRateAmount = Convert.ToDecimal(thisRate.OtherAmount3); break;
                case 4: thisRateAmount = Convert.ToDecimal(thisRate.OtherAmount4); break;
                case 5: thisRateAmount = Convert.ToDecimal(thisRate.OtherAmount5); break;
                case 6: thisRateAmount = Convert.ToDecimal(thisRate.OtherAmount6); break;
                case 7: thisRateAmount = Convert.ToDecimal(thisRate.OtherAmount7); break;
                case 8: thisRateAmount = Convert.ToDecimal(thisRate.OtherAmount8); break;
                case 9: thisRateAmount = Convert.ToDecimal(thisRate.OtherAmount9); break;
            }
            decimal finalAmount = 0;
            decimal surchargeDuration = 0;
            decimal surchareAmount = 0;
            decimal durationExcludingSurcharge = finalDurationSec;
            decimal billedAmountExcludingSurcharge = 0;


            if (thisRate.SurchargeTime > 0)//surcharge applicable
            {
                if (finalDurationSec >= thisRate.SurchargeTime)
                {
                    surchargeDuration = thisRate.SurchargeTime;
                }
                else
                {
                    surchargeDuration = finalDurationSec;
                }
                durationExcludingSurcharge = finalDurationSec - surchargeDuration;
                surchareAmount = Convert.ToDecimal(thisRate.SurchargeAmount);
            }

            long bspanSec = GetBillingSpanByRateOrIfMissingByRatePlan(thisRate, cdrProcessor);

            int rateAmountRoundUpTo = 0;
            if (thisRate.RateAmountRoundupDecimal != null && thisRate.RateAmountRoundupDecimal > 0)
            {
                rateAmountRoundUpTo = Convert.ToInt32(thisRate.RateAmountRoundupDecimal);
            }
            else
            {
                rateAmountRoundUpTo = Convert.ToInt32(this.DicRatePlan[thisRate.idrateplan.ToString()].RateAmountRoundupDecimal);
            }

            if (rateAmountRoundUpTo > 0)
            {
                thisRateAmount = Math.Round(thisRateAmount, rateAmountRoundUpTo);
            }

            billedAmountExcludingSurcharge = durationExcludingSurcharge * (thisRateAmount / bspanSec);
            finalAmount = billedAmountExcludingSurcharge + surchareAmount;
            return finalAmount;
        }

        long GetBillingSpanByRateOrIfMissingByRatePlan(Rateext rate,CdrProcessor cdrProcessor)
        {
            long bspanSec= Convert.ToInt64(rate.billingspan);
            if (bspanSec > 0) return bspanSec;
            string strTimeFreqUom = this.DicRatePlan[rate.idrateplan.ToString()].BillingSpan;
            bspanSec = cdrProcessor.CdrJobContext.MediationContext.BillingSpans[strTimeFreqUom].value;
            if (bspanSec <= 0) throw new Exception("Billing Span Value Must be > 0");
            return bspanSec;
        }
        
    }
}
