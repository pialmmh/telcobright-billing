using MySql.Data.MySqlClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;

namespace TelcobrightMediation
{
    public class RateCache
    {
        public RateContainerInMemoryLocal RateContainer = null;
        public Dictionary<string, rateplan> DicRatePlan = new Dictionary<string, rateplan>();//key=id as string
        public long MaxRateInDic = 0;
        private PartnerEntities Context { get; }
        public static ConcurrentDictionary<ValueTuple<int, TupleByPeriod>, Dictionary<string, RatesWithAssignmentTuple>>
            PriorityAndTupleWisePrefixDicWithAssignmentTuples { get; private set; }
        public RateCache(long pMaxRateInDic,PartnerEntities context)
        {
            this.Context = context;
            this.MaxRateInDic = pMaxRateInDic;
            this.RateContainer = new RateContainerInMemoryLocal(this.Context);
            PriorityAndTupleWisePrefixDicWithAssignmentTuples =
                new ConcurrentDictionary<ValueTuple<int, TupleByPeriod>, Dictionary<string, RatesWithAssignmentTuple>>();
        }
        //MAIN RATE CACHE
        //ratedictionary loaded per date, all rates are loaded per day in the cache
        public Dictionary<DateRange, Dictionary<TupleByPeriod, Dictionary<string, List<Rateext>>>> DateRangeWiseRateDic
            = new Dictionary<DateRange, Dictionary<TupleByPeriod, Dictionary<string, List<Rateext>>>>(new DateRange.EqualityComparer());

        //add a clearratecache method public to allow resuming after memory exception at any stage
        public void ClearRateCache()
        {
            this.DateRangeWiseRateDic.Clear();
            PriorityAndTupleWisePrefixDicWithAssignmentTuples =
                new ConcurrentDictionary<ValueTuple<int, TupleByPeriod>, Dictionary<string, RatesWithAssignmentTuple>>();
            GC.Collect();
        }

        public Dictionary<TupleByPeriod, Dictionary<string, List<Rateext>>> GetRateDictsByDay(DateRange dRange,bool flagLcr,bool useInMemoryTable,bool isCachingForMediation)
        {
            Dictionary<TupleByPeriod, Dictionary<string, List<Rateext>>> todaysDict = null;

            this.DateRangeWiseRateDic.TryGetValue(dRange, out todaysDict);
            if (todaysDict == null)
            {
                PopulateDicByDay(dRange,flagLcr,useInMemoryTable,isCachingForMediation);
                this.DateRangeWiseRateDic.TryGetValue(dRange, out todaysDict);
            }
            
            return todaysDict;
        }
        private long GetCount()
        {
            long cnt = 0;
            foreach (KeyValuePair<DateRange, Dictionary<TupleByPeriod, Dictionary<string, List<Rateext>>>> kv in this.DateRangeWiseRateDic)
            {
                foreach (KeyValuePair<TupleByPeriod, Dictionary<string, List<Rateext>>> kvInner in kv.Value)
                {
                    foreach (KeyValuePair<string, List<Rateext>> kvRates in kvInner.Value)
                    {
                        cnt++;
                    }
                }
            }
            return cnt;
        }


        public void PopulateDicByDay(DateRange dRange,bool flagLcr, bool useInMemoryTable,bool isCachingForMediation)
        {
            RateList.IsRatePlanWiseRateCacheInitialized = false;
            using (DbCommand cmd = this.Context.Database.Connection.CreateCommand())
            {
                try
                {
                    InsertRatesToTempTable(dRange, cmd);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    bool cacheLimitExceeded = RateCacheCleaner.ClearTempRateTable(e, cmd);
                    if (cacheLimitExceeded == false) throw;
                    InsertRatesToTempTable(dRange,cmd);
                }
            }

            Dictionary<TupleByPeriod, Dictionary<string, List<Rateext>>> dicByDay =
                new Dictionary<TupleByPeriod, Dictionary<string, List<Rateext>>>(new TupleByPeriod.EqualityComparer());

            //for non assignable services
            Dictionary<TupleByPeriod, Dictionary<string, List<Rateext>>> tempDic =
                GetRateDicNonPartnerAssignable(dRange, flagLcr, useInMemoryTable, isCachingForMediation);
            foreach (KeyValuePair<TupleByPeriod, Dictionary<string, List<Rateext>>> kv in tempDic)
            {
                dicByDay.Add(kv.Key, kv.Value);
            }

            //assignable services, direction=customer
            tempDic = GetRateDicPartnerAssignable(dRange, ServiceAssignmentDirection.Customer, flagLcr,
                useInMemoryTable, isCachingForMediation);
            foreach (KeyValuePair<TupleByPeriod, Dictionary<string, List<Rateext>>> kv in tempDic)
            {
                dicByDay.Add(kv.Key, kv.Value);
            }

            //assignable services, direction=supplier
            tempDic = GetRateDicPartnerAssignable(dRange, ServiceAssignmentDirection.Supplier, flagLcr,
                useInMemoryTable, isCachingForMediation);
            foreach (KeyValuePair<TupleByPeriod, Dictionary<string, List<Rateext>>> kv in tempDic)
            {
                dicByDay.Add(kv.Key, kv.Value);
            }
            //clear cache if number of total entry exceeds the max value
            if (GetCount() > this.MaxRateInDic)
            {
                this.DateRangeWiseRateDic.Clear();
                GC.Collect();
            }
            this.DateRangeWiseRateDic.Add(dRange, dicByDay);
        }

        private static void InsertRatesToTempTable(DateRange dRange, DbCommand cmd)
        {
            try
            {
                InsertRatesIntoTempInMemoryTable(dRange, cmd);
            }
            catch (Exception e)
            {
                bool cacheLimitExceeded = false;
                cacheLimitExceeded = RateCacheCleaner.ClearTempRateTable(e,cmd);
                if (cacheLimitExceeded == false) throw;
                InsertRatesIntoTempInMemoryTable(dRange, cmd);
            }
        }

        private static void InsertRatesIntoTempInMemoryTable(DateRange dRange, DbCommand cmd)
        {
            cmd.CommandText = $@"insert into temp_rate
                                     select * from rate
                                     where  (
                                     ( startdate <= {
                                                    dRange.StartDate.ToMySqlFormatWithQuote()
                                                } and ifnull(enddate,'9999-12-31 23:59:59') > {
                                                    dRange.StartDate.ToMySqlFormatWithQuote()
                                                })   
                                     or  ( startdate >= {dRange.StartDate.ToMySqlFormatWithQuote()} 
                                     and startdate < {dRange.EndDate.ToMySqlFormatWithQuote()}));";
            cmd.ExecuteNonQuery();
        }

        private Dictionary<TupleByPeriod, Dictionary<string, List<Rateext>>> GetRateDicNonPartnerAssignable(DateRange dRange,bool flagLcr,
            bool useInMemoryTable,bool isCachingForMediation)
        {
            Dictionary<TupleByPeriod, Dictionary<string, List<Rateext>>> dicRateDic =
                new Dictionary<TupleByPeriod, Dictionary<string, List<Rateext>>>(new TupleByPeriod.EqualityComparer());
            List<int> lstIdservices = new List<int>();
            lstIdservices = this.Context.enumservicefamilies.Where(c => c.PartnerAssignNotNeeded == 1).Select(c => c.id)
                .ToList();
            lstIdservices.ForEach(idService=>
            {
                    RateDictionaryGeneratorByTuples dicGenerator = new RateDictionaryGeneratorByTuples(idService,
                        dRange, ServiceAssignmentDirection.Customer,
                        -1, -1, -1, 0, "", "", -1, -1, RateChangeType.All,
                        this.Context, this.RateContainer);

                    //order by prefix ascending and startdate descending
                    Dictionary<TupleByPeriod, List<Rateext>> dicRateList = dicGenerator.GetRateDict(useInMemoryTable,isCachingForMediation); //Get the rate dictionary
                    foreach (KeyValuePair<TupleByPeriod, List<Rateext>> kv in dicRateList)
                    {
                        Dictionary<string, List<Rateext>> dicRates = RateListToDictionary(kv.Value, flagLcr);
                        dicRateDic.Add(kv.Key, dicRates);
                    }
                }
            );
            

            return dicRateDic;
        }

        private Dictionary<TupleByPeriod, Dictionary<string, List<Rateext>>> GetRateDicPartnerAssignable(DateRange dRange, ServiceAssignmentDirection assignDir,
            bool flagLcr,bool useInMemoryTable,bool isCachingForMediation)
        {
            Dictionary<TupleByPeriod, Dictionary<string, List<Rateext>>> dicRateDic =
                new Dictionary<TupleByPeriod, Dictionary<string, List<Rateext>>>(new TupleByPeriod.EqualityComparer());
            List<int> lstIdservices = new List<int>();
            lstIdservices = this.Context.enumservicefamilies.Where(c => c.PartnerAssignNotNeeded != 1).Select(c => c.id)
                .ToList();
            foreach (var item in lstIdservices)
            {
                RateDictionaryGeneratorByTuples dicGenerator = new RateDictionaryGeneratorByTuples(item,
                    dRange, assignDir,
                    -1, -1, -1, 0, "", "", -1, -1, RateChangeType.All,
                    this.Context, this.RateContainer);

                //order by prefix ascending and startdate descending
                Dictionary<TupleByPeriod, List<Rateext>> dicRateList = dicGenerator.GetRateDict(useInMemoryTable,isCachingForMediation); //Get the rate dictionary
                List<Rateext> combinedList = new List<Rateext>();
                foreach (KeyValuePair<TupleByPeriod, List<Rateext>> kv in dicRateList)
                {
                    Dictionary<string, List<Rateext>> dicRates = RateListToDictionary(kv.Value, flagLcr);
                    dicRateDic.Add(kv.Key, dicRates);
                }
            }

            return dicRateDic;
        }

        private Dictionary<string, List<Rateext>> RateListToDictionary(List<Rateext> lstRates,bool flagLcr)
        {
            Dictionary<string, List<Rateext>> dicRates = new Dictionary<string, List<Rateext>>();
            foreach (Rateext rate in lstRates)
            {
                List<Rateext> innerList = null;
                string techPrefix = "";
                techPrefix = this.DicRatePlan[rate.idrateplan.ToString()].field4;
                dicRates.TryGetValue(techPrefix + rate.Prefix, out innerList);
                if (innerList == null)
                {
                    innerList = new List<Rateext>();
                    if(flagLcr==false)
                    {
                        dicRates.Add(techPrefix + rate.Prefix, innerList);
                        dicRates.TryGetValue(techPrefix + rate.Prefix, out innerList);
                    }
                    else//for lcr don't add techprefix
                    {
                        dicRates.Add(rate.Prefix, innerList);
                        dicRates.TryGetValue(rate.Prefix, out innerList);
                    }
                    
                }
                innerList.Add(rate);
            }
            //sort
            foreach (KeyValuePair<string, List<Rateext>> kv in dicRates)
            {
                kv.Value.OrderBy(c => c.Priority).ThenByDescending(c => c.P_Startdate).ToList();
            }
            return dicRates;
        }
    }
}

