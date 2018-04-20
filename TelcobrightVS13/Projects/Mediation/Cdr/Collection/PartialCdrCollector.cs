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
        private Dictionary<string, cdrpartiallastaggregatedrawinstance> BillIdWiseLastAggregatedRawInstances { get; set; }
        private Dictionary<string, cdr> BillIdWiseLastProcessedCdrInstance { get; set; }
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

        public void CollectPartialCdrHistory()
        {
            List<string> uniqueBillIds = this.BillIdWiseNewRawInstances.Keys.ToList();
            string sql = CreateSqlToCollectPrevPartialCdrInfo(uniqueBillIds, "cdrpartialreference")
                .Replace("starttime", "calldate");
            this.BillIdWiseReferences = this.Context.Database.SqlQuery<cdrpartialreference>(sql)
                .ToDictionary(c => c.UniqueBillId);

            sql = CreateSqlToCollectPrevPartialCdrInfo(uniqueBillIds, "cdrpartialrawinstance");
            this.BillIdWisePrevRawInstances = this.Context.Database.SqlQuery<cdrpartialrawinstance>(sql)
                .GroupBy(c => c.UniqueBillId).ToDictionary(g => g.Key, g => g.ToList());

            sql = CreateSqlToCollectPrevPartialCdrInfo(uniqueBillIds, "cdrpartiallastaggregatedrawinstance");
            this.BillIdWiseLastAggregatedRawInstances = this.Context.Database
                .SqlQuery<cdrpartiallastaggregatedrawinstance>(sql)
                .ToDictionary(c => c.UniqueBillId);

            this.BillIdWiseLastProcessedCdrInstance = CollectLastProcessedCdrInstances();
        }

        Dictionary<string, cdr> CollectLastProcessedCdrInstances()
        {
            if(this.BillIdWiseReferences.Any()==false) return new Dictionary<string, cdr>();
            var dayWisePartialReferences = this.BillIdWiseReferences.Values
                .GroupBy(r => r.CallDate.Date).ToDictionary(g => g.Key, g => g.Select(r => r.lastIdcall).ToList());
            string sql = string.Join(" union all ",
                dayWisePartialReferences.Select(kv =>
                    $@" select * from cdr where switchid={this.IdSwitch} 
                    and idcall in ({string.Join(",", kv.Value)}) and {
                    kv.Key.ToMySqlWhereClauseForOneDay("starttime")}"));
            return this.Context.Database.SqlQuery<cdr>(sql).ToDictionary(c => c.UniqueBillId);
        }
        public BlockingCollection<PartialCdrContainer> AggregateAll()
        {
            BlockingCollection<PartialCdrContainer> partialCdrContainers = new BlockingCollection<PartialCdrContainer>();
            foreach (KeyValuePair<string, List<cdrpartialrawinstance>> kv in this.BillIdWiseNewRawInstances)
            {
                var partialCdrContainer = new PartialCdrContainer(
                    newRawInstances: kv.Value,
                    prevRawInstances: this.BillIdWisePrevRawInstances.ContainsKey(kv.Key)
                        ? this.BillIdWisePrevRawInstances[kv.Key]
                        : new List<cdrpartialrawinstance>(),
                    cdrPartialreference: this.BillIdWiseReferences.ContainsKey(kv.Key)
                        ? this.BillIdWiseReferences[kv.Key]
                        : null,
                    lastAggregatedRawInstance: this.BillIdWiseLastAggregatedRawInstances.ContainsKey(kv.Key) == true
                        ? this.BillIdWiseLastAggregatedRawInstances[kv.Key]
                        : null,
                    prevProcessedCdrInstance: this.BillIdWiseLastProcessedCdrInstance.ContainsKey(kv.Key) == true
                        ? this.BillIdWiseLastProcessedCdrInstance[kv.Key]
                        : null);
                partialCdrContainer.Aggregate();
                partialCdrContainers.Add(partialCdrContainer);
            }
            return partialCdrContainers;
        }

        private string CreateSqlToCollectPrevPartialCdrInfo(List<string> uniqueBillIds,string tableName)
        {
            var scanDates = this.DatesToScanForSafePartialCdrCollection;
            StringBuilder sb = new StringBuilder($@"select * from {tableName} where switchid={this.IdSwitch} and")
                .Append($@" uniquebillid in ({
                        string.Join(",", uniqueBillIds.Select(b => b.EncloseWithSingleQuotes()))})")
                .Append($@" and starttime >= {scanDates.Min().ToMySqlField()}")
                .Append($@" and starttime < {scanDates.Max().AddDays(1).ToMySqlField()}");
            return sb.ToString();
        }

        public void ValidatePartialCdrCollectionStatus()
        {
            long sumFromCdrPartialRef = this.BillIdWiseReferences.Values.SelectMany(r => r
                .commaSepIdcallsForAllInstances.Split(',').Select(strIdCall => Convert.ToInt64(strIdCall))).Sum();
            long sumFromPrevRawInstances = this.BillIdWisePrevRawInstances.Values
                .SelectMany(listOfInstances => listOfInstances).Sum(instance=>instance.idcall);
            if(sumFromCdrPartialRef!=sumFromPrevRawInstances) 
                throw new Exception("Sum of prev partial idCalls from partialCdrReference & prevRawinstances does not match.");

            decimal sumDurationFromLastAggregatedRawInstances = this.BillIdWiseLastAggregatedRawInstances.Values
                .Sum(lastAggInstance => lastAggInstance.DurationSec);
            decimal sumDurationFromPrevRawInstances = this.BillIdWisePrevRawInstances.Values
                                                          .SelectMany(r => r).Sum(r => r.DurationSec);
            if(sumDurationFromLastAggregatedRawInstances!= sumDurationFromPrevRawInstances)
                throw new Exception("Sum duration of prev partial cdrs from prevRawInstances & lastAggregatedInstances does not match.");
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
