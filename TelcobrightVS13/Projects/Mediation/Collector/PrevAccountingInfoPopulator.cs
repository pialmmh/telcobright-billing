using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using javax.transaction;
using LibraryExtensions;
using MediationModel;

namespace TelcobrightMediation.Cdr.CollectionRelated.Collector
{
    public class PrevAccountingInfoPopulator
    {
        private PartnerEntities Context { get; }
        private List<CdrExt> SuccessfullOldCdrExts { get; }
        private Dictionary<string, CdrExt> BillIdWiseCdrExts { get; }

        public PrevAccountingInfoPopulator(PartnerEntities context, List<CdrExt> oldCdrExts)
        {
            this.Context = context;
            this.SuccessfullOldCdrExts = oldCdrExts.Where(c => c.Cdr.ChargingStatus == 1).ToList();
            this.BillIdWiseCdrExts = oldCdrExts.ToDictionary(c => c.UniqueBillId);
        }

        public void PopulatePreviousTransactions()
        {
            if (this.SuccessfullOldCdrExts.Any())
            {
                Dictionary<DateTime, List<CdrExt>> dayWiseOldCdrExts = this.SuccessfullOldCdrExts
                    .GroupBy(c => c.StartTime.Date).ToDictionary(g => g.Key, g => g.ToList());
                string sql = string.Join(" union all ", dayWiseOldCdrExts.Select(kv =>
                    $@"select * from acc_transaction where 
                           {kv.Key.ToMySqlWhereClauseForOneDay("transactionTime")} 
                           and uniquebillid in ({
                            string.Join(",", kv.Value.Select(c => c.UniqueBillId.EncloseWith("'")))
                        }) 
                           and cancelled is null"));
                Dictionary<string, List<acc_transaction>> billidWisePrevTransactions =
                    this.Context.Database.SqlQuery<acc_transaction>(sql)
                        .GroupBy(c => c.uniqueBillId).ToDictionary(g => g.Key, g => g.ToList());

                foreach (var kv in billidWisePrevTransactions)
                {
                    CdrExt targetCdrExt = this.BillIdWiseCdrExts[kv.Key];
                    foreach (var oldTransaction in kv.Value)
                    {
                        AccWiseTransactionContainer transactionContainer = null;
                        if (targetCdrExt.AccWiseTransactionContainers.TryGetValue(oldTransaction.glAccountId,
                                out transactionContainer) == false)
                        {
                            transactionContainer = new AccWiseTransactionContainer();
                            targetCdrExt.AccWiseTransactionContainers
                                .Add(oldTransaction.glAccountId, transactionContainer);
                            transactionContainer.OldTransactions.Add(oldTransaction);
                        }
                    }
                }
            }
        }

        public void PopulatePreviousChargeables()
        {
            if (this.SuccessfullOldCdrExts.Any())
            {
                Dictionary<DateTime, List<CdrExt>> dayWiseOldCdrExts =
                    this.SuccessfullOldCdrExts.GroupBy(c => c.StartTime.Date).ToDictionary(g => g.Key, g => g.ToList());
                string sql = string.Join(" union all ", dayWiseOldCdrExts.Select(kv =>
                    $@"select * from acc_chargeable where 
                           {kv.Key.ToMySqlWhereClauseForOneDay("transactionTime")} 
                           and uniquebillid in ({
                        string.Join(",", kv.Value.Select(c => c.UniqueBillId.EncloseWith("'")))})"));
                Dictionary<string, List<acc_chargeable>> billidWisePrevChargeables =
                                    this.Context.Database.SqlQuery<acc_chargeable>(sql).ToList()
                                    .GroupBy(c => c.uniqueBillId).ToDictionary(g => g.Key, g => g.ToList());
                foreach (var kv in billidWisePrevChargeables)
                {
                    List<acc_chargeable> chargeablesForOneCdrExt = kv.Value;
                    chargeablesForOneCdrExt.ForEach(oldChargeable => this.BillIdWiseCdrExts[kv.Key]
                                        .Chargeables.Add(oldChargeable.GetTuple(), oldChargeable));
                }
            }
        }
    }
}
