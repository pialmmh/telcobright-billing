using LibraryExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TelcobrightMediation.Accounting;
using MediationModel;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web.ApplicationServices;
using FlexValidation;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;
using TelcobrightMediation.Mediation.Cdr;
using TransactionTuple = System.ValueTuple<int, int, long, int>;
using CdrSummaryTuple = System.ValueTuple<int, int, int, string, string, decimal, decimal, System.ValueTuple<string, string, string, string, string, string, string, System.ValueTuple<string, string, string, string, string, string>>>;

namespace TelcobrightMediation
{
    public class CdrProcessor
    {
        public CdrJobContext CdrJobContext { get; }
        public CdrCollectionResult CollectionResult { get; }
        private MediationContext MediationContext => this.CdrJobContext.MediationContext;
        private TelcobrightConfig Tbc => this.MediationContext.Tbc;
        private DbCommand DbCmd => this.CdrJobContext.DbCmd;
        public bool PartialProcessingEnabled => this.Tbc.CdrSetting.PartialCdrEnabledNeIds.Contains(this.CdrJobContext.Ne.idSwitch);
        private List<CdrExt> NewCdrExts => this.CollectionResult.ConcurrentCdrExts.Values.ToList();
        AccountingContext AccountingContext => this.CdrJobContext.AccountingContext;
        public CdrProcessor(CdrJobContext cdrJobContext, CdrCollectionResult newCollectionResult)
        {
            this.CdrJobContext = cdrJobContext;
            this.CollectionResult = newCollectionResult;
        }

        public void Mediate()
        {
            //todo: change to parallel
            this.NewCdrExts.ForEach(cdrExt =>
                //Parallel.ForEach(newCdrExts, cdrExt =>
            {
                try
                {
                    ResetMediationStatus(cdrExt.Cdr);
                    ServiceGroupConfiguration serviceGroupConfiguration = null;
                    IServiceGroup serviceGroup = ExecuteServiceGroups(cdrExt, out serviceGroupConfiguration);
                    serviceGroupConfiguration.ServiceGroup = serviceGroup;
                    if (serviceGroup != null)
                    {
                        ExecutePartnerRules(serviceGroupConfiguration.PartnerRules, cdrExt);
                        ExecuteRating(serviceGroupConfiguration, serviceGroupConfiguration.Ratingtrules, cdrExt);

                        serviceGroup.ExecutePostRatingActions(cdrExt, this);
                        ExecuteNerRule(this, cdrExt);
                        if (this.CdrJobContext.MediationContext.Tbc.CdrSetting.CallConnectTimePresent == true &&
                            cdrExt.Cdr.ServiceGroup > 0 && cdrExt.Cdr.ChargingStatus == 1)
                        {
                            SetPdd(cdrExt.Cdr);
                        }
                    }
                    ValidationResult mediationResult =
                        MediationErrorChecker.ExecuteValidationRules(cdrExt, this.CdrJobContext);
                    if (mediationResult.IsValid == false)
                        SendToCdrError(cdrExt, "Mediation error: " + mediationResult.FirstValidationFailureMessage);
                    else
                    {
                        SetMediationStatusToSuccess(cdrExt.Cdr);
                        this.CollectionResult.ProcessedCdrExts.Add(cdrExt);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    string errorMessage = new StringBuilder("Exception: ").Append(e.Message).ToString();
                    this.SendToCdrError(cdrExt, errorMessage);
                }
            });
            this.CollectionResult.CollectionResultProcessingState = CollectionResultProcessingState.AfterMediation;
        }
        public void GenerateSummaries()
        {
            //todo: change to parallel
            //Parallel.ForEach(this.CollectionResult.ProcessedCdrExts, processedCdrExt =>
            this.CollectionResult.ProcessedCdrExts.ToList().ForEach(processedCdrExt =>
            {
                this.CdrJobContext.CdrSummaryContext.GenerateSummary(processedCdrExt);
                var cdr = processedCdrExt.Cdr;
                if (Convert.ToDecimal(cdr.SummaryMetaTotal)!=0)
                    throw new Exception("Summmary meta data of new cdr must be zero.");
                cdr.SummaryMetaTotal = processedCdrExt.TableWiseSummaries.Values.Sum(s => s.actualduration);
            });
        }
        public void MergeNewSummariesIntoCache()
        {
            //todo: change to parallel
            //Parallel.ForEach(this.CollectionResult.ProcessedCdrExts, processedCdrExt =>
            this.CollectionResult.ProcessedCdrExts.ToList().ForEach(processedCdrExt =>
            {
                foreach (var kv in processedCdrExt.TableWiseSummaries)
                {
                    string summaryTargetTable = kv.Key;
                    this.CdrJobContext.CdrSummaryContext.MergeAddSummary(summaryTargetTable, kv.Value);
                }
            });
        }
        public void InsertChargeablesIntoAccountingContext()
        {
            //todo: change to parallel
            //Parallel.ForEach(this.CollectionResult.ProcessedCdrExts, processedCdrExt =>
            this.CollectionResult.ProcessedCdrExts.ToList().ForEach(processedCdrExt =>
            {
                foreach (var chargeable in processedCdrExt.Chargeables.Values.AsEnumerable())
                {
                    this.AccountingContext.ChargeableCache.Insert(chargeable, ch => ch.id > 0);
                }
            });
        }
        private void SendToCdrError(CdrExt cdrExt, string errorMessage)
        {
            if (cdrExt.Cdr.PartialFlag == 0)
                this.CollectionResult.AddNonPartialCdrExtToCdrErrors(cdrExt, errorMessage);
            else if (cdrExt.PartialCdrContainer != null && cdrExt.Cdr.PartialFlag != 0)
            {
                this.CollectionResult.AddNewRawPartialCdrsToCdrErrors(cdrExt, errorMessage);
            }
            else throw new InvalidDataContractException("Cdr must be either partial or non-partial.");
        }

        private IServiceGroup ExecuteServiceGroups(CdrExt newCdrExt,
            out ServiceGroupConfiguration serviceGroupConfiguration)
        {
            serviceGroupConfiguration = null;
            Dictionary<int  , ServiceGroupConfiguration> serviceGroupConfigurations =
                this.CdrJobContext.MediationContext.Tbc.CdrSetting.ServiceGroupConfigurations;
            foreach (KeyValuePair<int, ServiceGroupConfiguration> kv in serviceGroupConfigurations)
            {
                IServiceGroup serviceGroup = null;
                this.CdrJobContext.MediationContext.MefServiceGroupContainer.IdServiceGroupWiseServiceGroups.TryGetValue(kv.Key,
                    out serviceGroup);
                if (serviceGroup != null)
                {
                    serviceGroupConfiguration = kv.Value;
                    newCdrExt.Cdr.ServiceGroup =
                        0; //unset ServiceGroup first if already set e.g. during re-processing
                    serviceGroup.Execute(newCdrExt.Cdr, this);
                    if (newCdrExt.Cdr.ServiceGroup > 0) return serviceGroup;
                }
            }
            return null;
        }

        private void ResetMediationStatus(cdr cdr)
        {
            cdr.ErrorCode = ""; //set error flag empty by default to prevent calls going to cdrloaded table...
            cdr.MediationComplete = 0;
        }

        private void SetMediationStatusToSuccess(cdr cdr)
        {
            cdr.MediationComplete = 1;
            cdr.ErrorCode = ""; //clear error flag/description from previous mediation attempt
        }

        private static void ExecuteNerRule(CdrProcessor cdrProcessor, CdrExt cdrExt)
        {
            string ruleName = cdrProcessor.CdrJobContext.MediationContext.Tbc.CdrSetting.NerCalculationRule;
            INerCalculationRule nerCalculationRule = null;
            cdrProcessor.CdrJobContext.MediationContext.MefNerRulesContainer.DicExtensions.TryGetValue(ruleName,
                out nerCalculationRule);
            nerCalculationRule?.ExecuteNerRule(cdrProcessor, cdrExt);
        }

        private static void SetPdd(cdr thisCdr)
        {
            var diffInSeconds = thisCdr.ConnectTime != null
                ? (Convert.ToDateTime(thisCdr.ConnectTime) - thisCdr.SignalingStartTime).TotalSeconds
                : (Convert.ToDateTime(thisCdr.AnswerTime) - thisCdr.SignalingStartTime).TotalSeconds;
            thisCdr.PDD = (float)diffInSeconds;
        }

        private void ExecutePartnerRules(List<int> partnerRules, CdrExt cdrExt)
        {
            foreach (var rule in partnerRules)
            {
                IPartnerRule partnerRule = null;
                this.CdrJobContext.MediationContext.MefPartnerRuleContainer.DicExtensions.TryGetValue(rule,
                    out partnerRule);
                if (partnerRule != null)
                {
                    partnerRule.Execute(cdrExt.Cdr, this.CdrJobContext.MediationContext.MefPartnerRuleContainer);
                }
            }
        }

        private void ExecuteRating(ServiceGroupConfiguration serviceGroupConfiguration,
            List<RatingRule> ratingRules, CdrExt cdrExt)
        {
            var cdr = cdrExt.Cdr;
            foreach (RatingRule rule in ratingRules)
            {
                IServiceFamily sf = null;
                this.CdrJobContext.MediationContext.MefServiceFamilyContainer.DicExtensions.TryGetValue(
                    rule.IdServiceFamily, out sf);
                if (sf != null)
                {
                    int idProductForIndividualAccount = 0; //find out a way to implement this later
                    if (string.IsNullOrEmpty(cdr.UniqueBillId))
                        throw new Exception("Unique BillId not found!");
                    ServiceContext serviceContext = new ServiceContext(this, serviceGroupConfiguration, sf,
                        rule.AssignDirection == 1
                            ? ServiceAssignmentDirection.Customer
                            : rule.AssignDirection == 2
                                ? ServiceAssignmentDirection.Supplier
                                : ServiceAssignmentDirection.None
                        , idProductForIndividualAccount);
                    //execute rating & accounting
                    AccChargeableExt chargeableExt = sf.Execute(cdrExt, serviceContext, false);
                    if (chargeableExt != null)
                    {
                        cdrExt.Chargeables.Add(chargeableExt.AccChargeable.GetTuple(), chargeableExt.AccChargeable);
                        acc_transaction transaction = sf.GetTransaction(chargeableExt, serviceContext);
                        AccWiseTransactionContainer transactionContainer = null;
                        if (cdrExt.AccWiseTransactionContainers.TryGetValue(transaction.glAccountId,
                                out transactionContainer) == false)
                        {
                            transactionContainer = new AccWiseTransactionContainer();
                        }
                        cdrExt.AccWiseTransactionContainers.Add(transaction.glAccountId, transactionContainer);
                        transactionContainer.NewTransaction = transaction;
                    }
                }

            }
        }

        public CdrWritingResult WriteCdrs()
        {
            if (this.CdrJobContext.TelcobrightJob.idjobdefinition == 2) //cdrError
            {
                var idCallsOfProcessedCdrs = this.CollectionResult.ProcessedCdrExts
                    .Select(c => new KeyValuePair<long, DateTime>(c.Cdr.IdCall, c.StartTime)).ToList();
                var idCallsOfCdrErrors = this.CollectionResult.CdrErrors
                    .Select(c => new KeyValuePair<long, DateTime>(c.IdCall, c.StartTime)).ToList();

                OldCdrDeleter.DeleteOldCdrs("cdrerror", idCallsOfProcessedCdrs.Concat(idCallsOfCdrErrors).ToList(),
                    this.CdrJobContext.SegmentSizeForDbWrite, this.CdrJobContext.DbCmd);
            }

            long inconsistentCount = WriteCdrInconsistent();
            long errorCount = WriteCdrError();

            var nonPartialCdrs = this.CollectionResult.ProcessedCdrExts
                .Where(c => c.Cdr.MediationComplete == 1 && c.Cdr.PartialFlag==0).Select(c => c.Cdr).ToList();
            long nonPartialCdrCount = WriteCdr(nonPartialCdrs);
            var normalizedPartialCdrs = this.CollectionResult.ProcessedCdrExts
                .Where(c => c.Cdr.MediationComplete == 1 && c.Cdr.PartialFlag > 0).Select(c => c.Cdr).ToList();
            long normalizedPartialCdrCount = 0;
            if (normalizedPartialCdrs.Any())
            {
                normalizedPartialCdrCount= WriteCdr(normalizedPartialCdrs);
            }
            long cdrCount = nonPartialCdrCount + normalizedPartialCdrCount;

            List<PartialCdrContainer> partialCdrContainers = new List<PartialCdrContainer>();
            PartialCdrWriter partialCdrWriter = null;
            if (this.PartialProcessingEnabled)
            {
                partialCdrWriter = new PartialCdrWriter(
                    this.CollectionResult.ProcessedCdrExts.
                    Where(e => e.Cdr.PartialFlag !=0&&e.PartialCdrContainer!=null)
                        .Select(e => e.PartialCdrContainer).ToList(), this.CdrJobContext);
                partialCdrWriter.Write();
                partialCdrContainers = partialCdrWriter.PartialCdrContainers;
            }
            int rawPartialActualCount = partialCdrContainers.Sum(c => c.NewRawInstances.Count);
            int distinctPartialCount = partialCdrContainers.Count;

            if (this.CollectionResult.RawCount !=
                (inconsistentCount + errorCount + cdrCount + rawPartialActualCount - distinctPartialCount))
                throw new Exception(
                    "RawCount in collection result must equal (inconsistentCount+ errorCount + cdrCount+rawPartialActualCount-distintPartialCount");
            return new CdrWritingResult()
            {
                CdrCount = cdrCount,
                CdrErrorCount = errorCount,
                CdrInconsistentCount = inconsistentCount,
                PartialCdrWriter = partialCdrWriter,
                TrueNonPartialCount = nonPartialCdrCount,
                NormalizedPartialCount = normalizedPartialCdrCount
            };
        }

        public long WriteCdrInconsistent()
        {
            long inconsistentCount = 0;
            if (this.CollectionResult.CdrInconsistents.Any())
            {
                int startAt = 0;
                CollectionSegmenter<string> segmenter
                    = new CollectionSegmenter<string>(
                        this.CollectionResult.CdrInconsistents.Select(c => c.GetExtInsertValues()), startAt);
                segmenter.ExecuteMethodInSegments(this.CdrJobContext.SegmentSizeForDbWrite,
                    segment =>
                    {
                        this.DbCmd.CommandText = "insert into cdrinconsistent values " + string.Join(",", segment);
                        inconsistentCount += this.DbCmd.ExecuteNonQuery(); //write cdr loaded
                    });
            }
            return inconsistentCount;
        }

        long WriteCdrError()
        {
            List<cdrerror> cdrErrors = this.CollectionResult.CdrErrors.ToList();
            long errorInsertedCount = 0;
            if (cdrErrors.Any())
            {
                int startAt = 0;
                CollectionSegmenter<cdrerror> methodEnumerator =
                    new CollectionSegmenter<cdrerror>(cdrErrors, startAt);
                this.DbCmd.CommandType = CommandType.Text;
                methodEnumerator.ExecuteMethodInSegments(this.CdrJobContext.SegmentSizeForDbWrite,
                    segment =>
                    {
                        this.DbCmd.CommandText =
                            new StringBuilder(StaticExtInsertColumnHeaders.cdrerror)
                                .Append(string.Join(",", segment.Select(c => c.GetExtInsertValues()))).ToString();
                        errorInsertedCount += this.DbCmd.ExecuteNonQuery(); //write cdr loaded
                    });
            }
            return errorInsertedCount;
        }

        int WriteCdr(List<cdr> cdrs)
        {
            int insertCount = 0;
            if (cdrs.Any())
            {
                int startAt = 0;
                CollectionSegmenter<cdr> methodEnumerator = new CollectionSegmenter<cdr>
                    (cdrs, startAt);
                methodEnumerator.ExecuteMethodInSegments(this.CdrJobContext.SegmentSizeForDbWrite,
                    segment =>
                    {
                        this.DbCmd.CommandText = new StringBuilder(StaticExtInsertColumnHeaders.cdr)
                            .Append(string.Join(",",
                                segment.Select(c => c.GetExtInsertValues()).ToList())).ToString();
                        insertCount += this.DbCmd.ExecuteNonQuery();
                    });
                return insertCount;
            }
            return insertCount;
        }
    }
}
