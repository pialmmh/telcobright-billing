using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Configuration;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;

namespace TelcobrightMediation.Mediation.Cdr
{
    public class PartialCdrCollector
    {
        private PartnerEntities Context { get; }
        private CdrSetting CdrSetting { get; }
        private List<DateTime> DatesToScanForSafePartialCdrCollection { get;}
        private int IdSwitch { get; }
        private Dictionary<string, cdrpartialreference> BillIdWiseReferences { get; set; }
        private Dictionary<string,List<cdrpartialrawinstance>> BillIdWiseNewRawInstances { get; set; }
        private Dictionary<string, List<cdrpartialrawinstance>> BillIdWisePrevRawInstances { get; set; }
        private Dictionary<string, cdrpartiallastaggregatedrawinstance> BillIdWisePrevAggregatedRawInstances { get; set; }
        private Dictionary<string, cdr> BillIdWisePrevProcessedCdrInstances { get; set; }
        //public List<PartialCdrAggregatedInformation> AggregatedPartialCdrInfos { get; private set; }
        public PartialCdrCollector(CdrCollectorInputData cdrCollectorInputData, List<cdrpartialrawinstance> newPartialCdrInstances)
        {
            if (newPartialCdrInstances.Count == 0)
                throw new Exception(
                    "There are no partial cdr instances, PartialCdrCollector may have been used erronously.");
            this.Context = cdrCollectorInputData.Context;
            List<DateTime> datesInvolved = newPartialCdrInstances.Select(c => c.StartTime.Date).Distinct().ToList();
            this.BillIdWiseNewRawInstances = newPartialCdrInstances.GroupBy(c => c.UniqueBillId)
                .ToDictionary(g => g.Key, g => g.ToList());
            this.CdrSetting = cdrCollectorInputData.CdrSetting;
            this.IdSwitch = cdrCollectorInputData.Ne.idSwitch; //idSwitch;
            this.DatesToScanForSafePartialCdrCollection =
                AddDistinctDaysBeforeAndAfterUniqueDaysForSafePartialCollection(datesInvolved);
        }

        public void CollectFullInfo()
        {
            List<string> uniqueBillIds = this.BillIdWiseNewRawInstances.Keys.ToList();
            this.BillIdWiseReferences = CollectBillIdWiseCdrPartialReferences(this.Context, uniqueBillIds);
            this.BillIdWisePrevRawInstances =
                CollectBillIdWiseRawPartialInstances(uniqueBillIds);
            if (ValidatePartialCdrCollectionByIdCallsSum(this.BillIdWisePrevRawInstances) == false)
            {
                throw new Exception($@"Sum of idCalls from cdrpartialreference & cdrpartialrawinstance
                                       does not match, partial cdr collection result is not correct.");
            }
            this.BillIdWisePrevAggregatedRawInstances=CollectLastAggregatedRawInstances(uniqueBillIds);
            this.BillIdWisePrevProcessedCdrInstances = CollectLastProcessedCdrInstances(uniqueBillIds);
        }

        public BlockingCollection<PartialCdrContainer> AggregateAll()
        {
            BlockingCollection<PartialCdrContainer> partialCdrFullInfos = new BlockingCollection<PartialCdrContainer>();
            foreach (KeyValuePair<string, List<cdrpartialrawinstance>> kv in this.BillIdWiseNewRawInstances)
            {
                var partialCdrContainer = new PartialCdrContainer(
                    newRawInstances:this.BillIdWiseNewRawInstances[kv.Key],
                    prevRawInstances:this.BillIdWisePrevRawInstances.ContainsKey(kv.Key)
                                                    ?this.BillIdWisePrevRawInstances[kv.Key]:null,
                    cdrPartialreference: this.BillIdWiseReferences.ContainsKey(kv.Key)
                                        ? this.BillIdWiseReferences[kv.Key]:null,
                    prevAggregatedRawInstance: this.BillIdWisePrevAggregatedRawInstances.ContainsKey(kv.Key)==true
                                           ? this.BillIdWisePrevAggregatedRawInstances[kv.Key]:null,
                    prevProcessedCdrInstance: this.BillIdWisePrevProcessedCdrInstances.ContainsKey(kv.Key) == true
                        ? this.BillIdWisePrevProcessedCdrInstances[kv.Key] : null);
                partialCdrContainer.Aggregate();
                partialCdrFullInfos.Add(partialCdrContainer);
            }
            return partialCdrFullInfos;
        }

        


        private Dictionary<string, cdrpartialreference> CollectBillIdWiseCdrPartialReferences(PartnerEntities context,
            List<string> uniqueBillIds)
        {
            string sql =
                GetDateWiseWhereClauseToSelectPrevInstancesBasedOnUniqueBillId(
                    uniqueBillIds, "cdrpartialreference", "calldate");
            return context.Database.SqlQuery<cdrpartialreference>(sql)
                    .ToDictionary(c => c.UniqueBillId);
            
        }
        
        private Dictionary<string, List<cdrpartialrawinstance>>
            CollectBillIdWiseRawPartialInstances(List<string> uniqueBillIds)
        {
            string sql = GetDateWiseWhereClauseToSelectPrevInstancesBasedOnUniqueBillId(
                            uniqueBillIds, "cdrpartialrawinstance", "starttime");
            Dictionary<string, List<cdrpartialrawinstance>>
                billIdWisePrevInstances = this.Context.Database.
                SqlQuery<cdrpartialrawinstance>(sql)
                .GroupBy(c=>c.UniqueBillId)
                .ToDictionary(g=>g.Key,g=>g.ToList());
            return billIdWisePrevInstances;
        }

        private Dictionary<string, cdrpartiallastaggregatedrawinstance>
            CollectLastAggregatedRawInstances(List<string> uniqueBillIds)
        {
            string sql = GetDateWiseWhereClauseToSelectPrevInstancesBasedOnUniqueBillId(
                uniqueBillIds, "cdrpartiallastaggregatedrawinstance", "starttime");
            return this.Context.Database.SqlQuery<cdrpartiallastaggregatedrawinstance>(sql)
                .ToDictionary(c => c.UniqueBillId);
        }
        private Dictionary<string, cdr>
            CollectLastProcessedCdrInstances(List<string> uniqueBillIds)
        {
            Dictionary<DateTime, List<long>> dayWiseIdCalls
                = this.BillIdWiseReferences.Values.GroupBy(c => c.CallDate.Date)
                    .ToDictionary(g => g.Key, g => g.Select(c => c.lastIdcall).ToList());
            string sql = 
                string.Join(" union all ",
                dayWiseIdCalls.Select(kv => $@" select * from cdr 
                                              where {kv.Key.ToMySqlWhereClauseForOneDay("starttime")}
                                              and idcall in ({string.Join(",", kv.Value)})"));
            Dictionary<string, cdr> lastProcessedCdrInstances =
                this.Context.Database.SqlQuery<cdr>(sql).ToDictionary(c => c.UniqueBillId);
            Dictionary<string, string> uniqueBillIdsAsDic = uniqueBillIds.ToDictionary(c => c);
            foreach (var kv in lastProcessedCdrInstances)
            {
                if(uniqueBillIdsAsDic.ContainsKey(kv.Key)==false)
                    throw new Exception("At least one billId for last processed partial cdr is not in current jobs partial cdr list.");
            }
            return lastProcessedCdrInstances;
        }

        private string GetDateWiseWhereClauseToSelectPrevInstancesBasedOnUniqueBillId(List<string> uniqueBillIds,
            string tableName,string columnNameForDateWisePartition)
        {
            List<string> dateWisewhereClauses = new List<string>();
            foreach (var singleDate in this.DatesToScanForSafePartialCdrCollection)
            {
                var sb = new StringBuilder("select * from ").Append(tableName).Append(" where switchid=")
                    .Append(this.IdSwitch).Append(" and ");
                sb.Append(singleDate.ToMySqlWhereClauseForOneDay(columnNameForDateWisePartition))
                    .Append(" and uniqueBillId in ("+string.Join(",",uniqueBillIds.Select(bId=>bId.EncloseWith("'")))+")");
                dateWisewhereClauses.Add(sb.ToString());
            }
            return string.Join(" union all ", dateWisewhereClauses);
        }

        private bool ValidatePartialCdrCollectionByIdCallsSum(Dictionary<string, List<cdrpartialrawinstance>>
            billIdWisePrevUnProcessedPartialInstancesInstances)
        {
            Dictionary<string, List<long>> billIdWiseIdCallsFromCdrPartialReference =
                PrepareBillIdWiseIdCallsFromCdrPartialReferences();
            long sumFromCdrPartialRef = billIdWiseIdCallsFromCdrPartialReference.Values.Select(c => c.Sum()).Sum();
            long sumFromPrevPartialUnprocessedInstances = billIdWisePrevUnProcessedPartialInstancesInstances.Values
                .Select(listOfInstances => listOfInstances.Sum(c => c.idcall)).Sum();
            return sumFromCdrPartialRef==sumFromPrevPartialUnprocessedInstances;
        }

        private Dictionary<string, List<long>> PrepareBillIdWiseIdCallsFromCdrPartialReferences()
        {
            Dictionary<string, List<long>> billIdWiseIdCalls = new Dictionary<string, List<long>>();
            foreach (KeyValuePair<string, cdrpartialreference> kv in this.BillIdWiseReferences)
            {
                billIdWiseIdCalls.Add(kv.Key,
                    kv.Value.commaSepIdcallsForAllInstances.Split(',')
                        .Select(idCallAsString => Convert.ToInt64(idCallAsString)).ToList());
            }
            return billIdWiseIdCalls;
        }
        private List<DateTime> AddDistinctDaysBeforeAndAfterUniqueDaysForSafePartialCollection(
            List<DateTime> datesToScanForPrevInstances)
        {
            int configuredValue = this.CdrSetting
                .DaysToAddBeforeAndAfterUniqueDaysForSafePartialCollection;
            int daysToAddBeforeAndAfterUniqueDays = configuredValue <= 0 ? 3 : configuredValue;
            foreach (DateTime dateTime in datesToScanForPrevInstances.ToList())//use new tolist() to avoid 
            {                                                           //modifying collection during enumeration
                for (int i = 1; i <= daysToAddBeforeAndAfterUniqueDays; i++)
                {
                    datesToScanForPrevInstances.Add(dateTime.AddDays(i));
                    datesToScanForPrevInstances.Add(dateTime.AddDays((-1) * i));
                }
            }
            //now filter unique days again, because there will be repeated dates
            datesToScanForPrevInstances = datesToScanForPrevInstances.Distinct().OrderBy(c => c).ToList();
            return datesToScanForPrevInstances;
        }
    }
}
