using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using LibraryExtensions;
using System.Linq;
using System.Threading.Tasks;
using MediationModel;
namespace TelcobrightMediation
{
    public class PrefixMatcher
    {
        private Dictionary<string, rateplan> DicRatePlan { get; set; }
        private ServiceContext ServiceContext { get; }
        private int MaxDecimalPrecision { get; }
        private int category;
        private int subCategory;
        private DateTime answerTime;
        private string[] phoneNumbersAsArray;
        private static long nonParallelExecTime = 0;
        private static long parallelExecTime = 0;
        private Rateext[] prefixWiseMatchedRates = new Rateext[50];
        private List<Dictionary<string, RatesWithAssignmentTuple>> PriorityWisePrefixDicWithAssignTuple { get; }
            = new List<Dictionary<string, RatesWithAssignmentTuple>>();

        public PrefixMatcher(ServiceContext serviceContext, string phoneNumber, int category,
            int subCategory, List<TupleByPeriod> tups, DateTime answerTime, bool flagLcr,
            bool useInMemoryTable)
        {
            this.ServiceContext = serviceContext;
            this.category = category;
            this.subCategory = subCategory;
            this.answerTime = answerTime;
            this.MaxDecimalPrecision = this.ServiceContext.CdrSetting.MaxDecimalPrecision;
            if (this.MaxDecimalPrecision < 0 && this.MaxDecimalPrecision > 8)
                throw new Exception("Max decimal precision must be >=0 and <=8.");
            this.DicRatePlan = serviceContext.MefServiceFamilyContainer.RateCache.DicRatePlan;
            //TupleByPeriod=one rateplanassignmenttuple on the day of answertime
            foreach (TupleByPeriod tup in tups.OrderBy(c => c.Priority)) //ToList()
            {
                Dictionary<string, RatesWithAssignmentTuple>
                    prefixDicWithAssignmentTuples = null; //= new Dictionary<string, RatesWithAssignmentTuple>();
                var tupUniqueKeyWithPriority = new ValueTuple<int, TupleByPeriod>(tup.Priority, tup);
                RateCache.PriorityAndTupleWisePrefixDicWithAssignmentTuples.TryGetValue(
                    tupUniqueKeyWithPriority, out prefixDicWithAssignmentTuples);
                if (prefixDicWithAssignmentTuples != null)
                {
                    this.PriorityWisePrefixDicWithAssignTuple.Add(prefixDicWithAssignmentTuples);
                }
                else
                {
                    Dictionary<string, List<Rateext>> prefixDic = null;
                    prefixDic = GetPrefixWiseRateInstances(tup, flagLcr, useInMemoryTable);
                    prefixDicWithAssignmentTuples = new Dictionary<string, RatesWithAssignmentTuple>();
                    foreach (KeyValuePair<string, List<Rateext>> kv in prefixDic)
                    {
                        prefixDicWithAssignmentTuples.Add(kv.Key, new RatesWithAssignmentTuple(tup, kv.Value));
                    }
                    if (RateCache.PriorityAndTupleWisePrefixDicWithAssignmentTuples.TryAdd(tupUniqueKeyWithPriority,
                            prefixDicWithAssignmentTuples) == false)
                    {
                        if (RateCache.PriorityAndTupleWisePrefixDicWithAssignmentTuples
                                .ContainsKey(tupUniqueKeyWithPriority) == false)
                        {
                            throw new Exception($"Could not add item to RateCache.PriorityAndTupleWisePrefixDicWithAssignmentTuples, " +
                                                $"also item with the same key has not been added by a parallel process.");
                        }
                    }
                    ;
                    this.PriorityWisePrefixDicWithAssignTuple.Add(prefixDicWithAssignmentTuples);
                }
                //if (prefixDic != null) PriorityWisePrefixDicWithAssignTuple.Add(prefixDicWithAssignmentTuples);
            }
            var phCharArray = phoneNumber.ToCharArray();
            this.phoneNumbersAsArray = new string[phoneNumber.Length];
            for (int i = 0; i < phCharArray.Length; i++)
            {
                this.phoneNumbersAsArray[i] = new string(phCharArray, 0, phCharArray.Length - i);
            }
        }

        public Rateext MatchPrefix()
        {
            Rateext rateext = null;
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            //rateext= MatchPrefixNonParallel();
            //stopwatch.Stop();
            //nonParallelExecTime += stopwatch.ElapsedTicks;
            //Console.WriteLine("Non parallel EXEC TIME {0}", nonParallelExecTime);

            //rateext = null;
            //stopwatch.Reset();
            //stopwatch.Start();
            rateext = MatchPrefixParallel();
            //stopwatch.Stop();
            //parallelExecTime += stopwatch.ElapsedTicks;
            //Console.WriteLine("Parallel EXEC TIME {0}", parallelExecTime);
            return rateext;
        }
        public Rateext MatchPrefixNonParallel()
        {
            foreach (Dictionary<string, RatesWithAssignmentTuple> prefixDic in
                this.PriorityWisePrefixDicWithAssignTuple)
            {
                Rateext matchedRate = null;
                bool matchFound = false;
                foreach (string prefix in this.phoneNumbersAsArray)
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
                        if (thisRate.Category == this.category && thisRate.SubCategory == this.subCategory
                            && this.answerTime >= thisRate.P_Startdate && this.answerTime <
                            (thisRate.P_Enddate != null ? thisRate.P_Enddate : new DateTime(9999, 12, 31, 23, 59, 59)))
                        {
                            matchedRate = thisRate;
                            matchedRate.IdRatePlanAssignmentTuple =
                                Convert.ToInt32(ratesWithAssignmentTuple.Tup.IdAssignmentTuple);
                            matchFound = true;
                            break; //rates are sorted desc, starttime. latest match will be returned immediately
                        }
                    }
                }
                if (matchedRate != null) return matchedRate;
            }
            return null;
        }

        public Rateext MatchPrefixParallel()
        {
            foreach (Dictionary<string, RatesWithAssignmentTuple> prefixDic in
                this.PriorityWisePrefixDicWithAssignTuple)
            {
                bool matchFound = false;
                Parallel.For((int)0, this.phoneNumbersAsArray.Length, i =>
                {
                    var prefix =
                        this.phoneNumbersAsArray[i]; //max length of prefix is processed first i.e. the whole number
                    RatesWithAssignmentTuple ratesWithAssignmentTuple = null;
                    prefixDic.TryGetValue(prefix, out ratesWithAssignmentTuple);
                    if (ratesWithAssignmentTuple != null)
                    {
                        List<Rateext> lstRates = ratesWithAssignmentTuple.Rates;
                        foreach (Rateext thisRate in lstRates)
                        {
                            if (thisRate.Category == this.category && thisRate.SubCategory == this.subCategory
                                && this.answerTime >= thisRate.P_Startdate && this.answerTime <
                                (thisRate.P_Enddate != null
                                    ? thisRate.P_Enddate
                                    : new DateTime(9999, 12, 31, 23, 59, 59)))
                            {
                                var matchedRateForthisPrefix = thisRate;
                                matchedRateForthisPrefix.IdRatePlanAssignmentTuple =
                                    Convert.ToInt32(ratesWithAssignmentTuple.Tup.IdAssignmentTuple);
                                this.prefixWiseMatchedRates[i] = matchedRateForthisPrefix;
                                matchFound = true;
                            }
                        }
                    }
                });
                //if (matchFound == false) return null; 
                if (matchFound == false) continue;//mustafa changes, sr telecom ip integration July 2023
                foreach (Rateext t in this.prefixWiseMatchedRates) //first matched is the longest match
                {
                    if (t != null) return t;
                }
            }
            return null;
        }


        public Dictionary<string, List<Rateext>> GetPrefixWiseRateInstances(TupleByPeriod tup, bool flagLcr, bool useInMemoryTable)
        {
            //TupleByPeriod=one rateplanassignmenttuple on the day of answertime
            Dictionary<TupleByPeriod, Dictionary<string, List<Rateext>>> dicRatesByDay
                = this.ServiceContext.MefServiceFamilyContainer.RateCache.GetRateDictsByDay(tup.DRange, flagLcr,
                    useInMemoryTable, isCachingForMediation: true);
            Dictionary<string, List<Rateext>> dicRatesByPrefix = null;
            if (dicRatesByDay != null)
            {
                dicRatesByDay.TryGetValue(tup, out dicRatesByPrefix);
            }
            return dicRatesByPrefix;
        }

        public decimal GetA2ZDuration(decimal actualDurationSec, Rateext thisRate)
        {
            if (actualDurationSec == 0) return 0;

            decimal minDurationSec = (decimal)thisRate.MinDurationSec;

            if (minDurationSec < 0M)//no rounding, use actual duration
            {
                return actualDurationSec;
            }
            else if (minDurationSec > 0M)//e.g. minimum .1 sec (100 ms) required for rounding up
            {
                //the code below works upto 11 digits e.g. 3538.099999999994 if the last digit >4 or there is more decimal then only rounds up
                decimal floorDuration = Math.Floor(actualDurationSec);
                decimal miliSecPart = actualDurationSec - floorDuration;
                if (miliSecPart >= minDurationSec)
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


        public decimal GetA2ZAmountWithOutSurCharge(decimal finalDurationSec, Rateext thisRate, int rateFieldNumber,
            CdrProcessor cdrProcessor)
        {
            int maxDecimalPrecision = cdrProcessor.CdrJobContext.CdrjobInputData.CdrSetting.MaxDecimalPrecision;
            decimal thisRateAmount = 0;
            if (finalDurationSec == 0) return 0;
            switch (rateFieldNumber)
            {
                case 0: thisRateAmount = thisRate.rateamount; break;
                case 1:
                    thisRateAmount = Convert.ToDecimal(thisRate.OtherAmount1).RoundFractionsUpTo(maxDecimalPrecision);
                    break;
                case 2:
                    thisRateAmount = Convert.ToDecimal(thisRate.OtherAmount2).RoundFractionsUpTo(maxDecimalPrecision);
                    break;
                case 3:
                    thisRateAmount = Convert.ToDecimal(thisRate.OtherAmount3).RoundFractionsUpTo(maxDecimalPrecision);
                    break;
                case 4:
                    thisRateAmount = Convert.ToDecimal(thisRate.OtherAmount4).RoundFractionsUpTo(maxDecimalPrecision);
                    break;
                case 5:
                    thisRateAmount = Convert.ToDecimal(thisRate.OtherAmount5).RoundFractionsUpTo(maxDecimalPrecision);
                    break;
                case 6:
                    thisRateAmount = Convert.ToDecimal(thisRate.OtherAmount6).RoundFractionsUpTo(maxDecimalPrecision);
                    break;
                case 7:
                    thisRateAmount = Convert.ToDecimal(thisRate.OtherAmount7).RoundFractionsUpTo(maxDecimalPrecision);
                    break;
                case 8:
                    thisRateAmount = Convert.ToDecimal(thisRate.OtherAmount8).RoundFractionsUpTo(maxDecimalPrecision);
                    break;
                case 9:
                    thisRateAmount = Convert.ToDecimal(thisRate.OtherAmount9).RoundFractionsUpTo(maxDecimalPrecision);
                    break;
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
            if (this.MaxDecimalPrecision > 0) finalAmount = decimal.Round(finalAmount, this.MaxDecimalPrecision);
            return finalAmount;
        }
        public decimal GetA2ZAmountWithSurCharge(decimal finalDurationSec, Rateext thisRate, int rateFieldNumber,
            CdrProcessor cdrProcessor)
        {
            int maxDecimalPrecision = cdrProcessor.CdrJobContext.CdrjobInputData.CdrSetting.MaxDecimalPrecision;
            decimal thisRateAmount = 0;
            if (finalDurationSec == 0) return 0;
            switch (rateFieldNumber)
            {
                case 0: thisRateAmount = thisRate.rateamount; break;
                case 1:
                    thisRateAmount = Convert.ToDecimal(thisRate.OtherAmount1).RoundFractionsUpTo(maxDecimalPrecision);
                    break;
                case 2:
                    thisRateAmount = Convert.ToDecimal(thisRate.OtherAmount2).RoundFractionsUpTo(maxDecimalPrecision);
                    break;
                case 3:
                    thisRateAmount = Convert.ToDecimal(thisRate.OtherAmount3).RoundFractionsUpTo(maxDecimalPrecision);
                    break;
                case 4:
                    thisRateAmount = Convert.ToDecimal(thisRate.OtherAmount4).RoundFractionsUpTo(maxDecimalPrecision);
                    break;
                case 5:
                    thisRateAmount = Convert.ToDecimal(thisRate.OtherAmount5).RoundFractionsUpTo(maxDecimalPrecision);
                    break;
                case 6:
                    thisRateAmount = Convert.ToDecimal(thisRate.OtherAmount6).RoundFractionsUpTo(maxDecimalPrecision);
                    break;
                case 7:
                    thisRateAmount = Convert.ToDecimal(thisRate.OtherAmount7).RoundFractionsUpTo(maxDecimalPrecision);
                    break;
                case 8:
                    thisRateAmount = Convert.ToDecimal(thisRate.OtherAmount8).RoundFractionsUpTo(maxDecimalPrecision);
                    break;
                case 9:
                    thisRateAmount = Convert.ToDecimal(thisRate.OtherAmount9).RoundFractionsUpTo(maxDecimalPrecision);
                    break;
            }
            decimal finalAmount = 0;
            decimal durationToBeChargeble = 0;
            decimal surchareAmount = 0;
            decimal durationExcludingSurcharge = finalDurationSec;
            decimal billedAmountExcludingSurcharge = 0;
            long bspanSec = GetBillingSpanByRateOrIfMissingByRatePlan(thisRate, cdrProcessor);

            if (thisRate.SurchargeTime > 0)//surcharge applicable
            {
                if (finalDurationSec >= thisRate.SurchargeTime)
                {
                    durationToBeChargeble = thisRate.SurchargeTime;
                }
                else
                {
                    durationToBeChargeble = finalDurationSec;
                }
                durationExcludingSurcharge = finalDurationSec - durationToBeChargeble;
                surchareAmount = Convert.ToDecimal(durationToBeChargeble * (thisRateAmount / bspanSec));
            }

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
            if (this.MaxDecimalPrecision > 0) finalAmount = decimal.Round(finalAmount, this.MaxDecimalPrecision);
            return finalAmount;
        }
        long GetBillingSpanByRateOrIfMissingByRatePlan(Rateext rate, CdrProcessor cdrProcessor)
        {
            long bspanSec = Convert.ToInt64(rate.billingspan);
            if (bspanSec > 0) return bspanSec;
            string strTimeFreqUom = this.DicRatePlan[rate.idrateplan.ToString()].BillingSpan;
            bspanSec = cdrProcessor.CdrJobContext.MediationContext.BillingSpans[strTimeFreqUom].value;
            if (bspanSec <= 0) throw new Exception("Billing Span Value Must be > 0");
            return bspanSec;
        }

    }
}
