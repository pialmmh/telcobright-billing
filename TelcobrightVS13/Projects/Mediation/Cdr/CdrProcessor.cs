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
using System.Threading.Tasks;
using System.Web.ApplicationServices;
using FlexValidation;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;
using TelcobrightMediation.EntityHelpers;
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

        public CdrProcessor(CdrJobContext cdrJobContext, CdrCollectionResult newCollectionResult)
        {
            this.CdrJobContext = cdrJobContext;
            this.CollectionResult = newCollectionResult;
        }

        public void Process()
        {
            var newCdrExts = this.CollectionResult.ConcurrentCdrExts.Values.ToList();
            var accountingContext = this.CdrJobContext.AccountingContext;
            //newCdrExts.ForEach(cdrExt=>
            Parallel.ForEach(newCdrExts, cdrExt =>
            {
                try
                {
                    ResetMediationStatus(cdrExt.Cdr);
                    ServiceGroupConfiguration serviceGroupConfiguration = null;
                    IServiceGroup serviceGroup = null;
                    ExecuteServiceGroups(cdrExt, out serviceGroupConfiguration, out serviceGroup);
                    if (serviceGroup != null)
                    {
                        ExecutePartnerRules(serviceGroupConfiguration.PartnerRules, cdrExt);
                        ExecuteServiceFamilyRules(serviceGroup, serviceGroupConfiguration.Ratingtrules, cdrExt);
                        serviceGroup.ExecutePostRatingActions(cdrExt, this);
                        ExecuteNerRule(this, cdrExt);
                        if (this.CdrJobContext.MediationContext.Tbc.CdrSetting.CallConnectTimePresent == true &&
                            cdrExt.Cdr.CallDirection > 0 && cdrExt.Cdr.ChargingStatus == 1)
                        {
                            SetPdd(cdrExt.Cdr);
                        }
                    }
                    SeparateErrorAndProcessedCdrsBasedOnMediationStatus(cdrExt);
                }
                catch (Exception e)
                {
                    CdrExt removedCdrExt = null;
                    this.CollectionResult.ConcurrentCdrExts.TryRemove(cdrExt.UniqueBillId, out removedCdrExt);
                    removedCdrExt.Cdr.mediationcomplete = 0;
                    removedCdrExt.Cdr.errorCode = new StringBuilder("Exception: ").Append(e.Message).ToString();
                    this.CollectionResult.CdrErrors.Add(ConvertCdrToCdrError(removedCdrExt.Cdr.errorCode, removedCdrExt));
                }
            });
            //todo: change to parallel
            //Parallel.ForEach(this.CollectionResult.ProcessedCdrExts, processedCdrExt =>
            this.CollectionResult.ProcessedCdrExts.ToList().ForEach(processedCdrExt =>
            {
                IServiceGroup serviceGroup = null;
                this.MediationContext.MefServiceGroupContainer.IdServiceGroupWiseServiceGroups.TryGetValue(
                    processedCdrExt.Cdr.CallDirection, out serviceGroup);
                if (serviceGroup == null) throw new Exception("Servicegroup not found before generating summary.");
                foreach (string targetTableName in serviceGroup.GetSummaryTargetTables().Keys)
                {
                    AbstractCdrSummary newSummary = (AbstractCdrSummary) this.CdrJobContext.CdrSummaryContext
                        .TargetTableWiseSummaryFactory[targetTableName].CreateNewInstance(processedCdrExt);
                    processedCdrExt.TableWiseSummaries.Add(targetTableName, newSummary);
                    this.CdrJobContext.CdrSummaryContext.MergeAddSummary(targetTableName, newSummary);
                }
                foreach (var chargeable in processedCdrExt.Chargeables.Values.AsEnumerable())
                {
                    accountingContext.ChargeableCache.Insert(chargeable, ch => ch.id > 0);
                }
            });
        }


        private void SeparateErrorAndProcessedCdrsBasedOnMediationStatus(CdrExt cdrExt)
        {
            ValidationResult validationResult =
                MediationErrorChecker.ExecuteValidationRules(cdrExt, this.CdrJobContext);
            if (validationResult.IsValid == false)
            {
                CdrExt removedCdrExt = null;
                this.CollectionResult.ConcurrentCdrExts.TryRemove(cdrExt.UniqueBillId, out removedCdrExt);
                this.CollectionResult.CdrErrors.Add(
                    ConvertCdrToCdrError(validationResult.FirstValidationFailureMessage, removedCdrExt));
            }
            else
            {
                SetMediationStatusToSuccess(cdrExt.Cdr);
                this.CollectionResult.ProcessedCdrExts.Add(cdrExt);
            }
        }

        private void ExecuteServiceGroups(CdrExt newCdrExt,
            out ServiceGroupConfiguration serviceGroupConfiguration, out IServiceGroup serviceGroup)
        {
            serviceGroupConfiguration = null;
            serviceGroup = null;
            Dictionary<int  , ServiceGroupConfiguration> serviceGroupConfigurations =
                this.CdrJobContext.MediationContext.Tbc.CdrSetting.ServiceGroupConfigurations;
            foreach (KeyValuePair<int, ServiceGroupConfiguration> kv in serviceGroupConfigurations)
            {
                serviceGroup = null;
                this.CdrJobContext.MediationContext.MefServiceGroupContainer.IdServiceGroupWiseServiceGroups.TryGetValue(kv.Key,
                    out serviceGroup);
                if (serviceGroup != null)
                {
                    serviceGroupConfiguration = kv.Value;
                    newCdrExt.Cdr.CallDirection =
                        0; //unset calldirection first if already set e.g. during re-processing
                    serviceGroup.Execute(newCdrExt.Cdr, this);
                    if (newCdrExt.Cdr.CallDirection > 0) break;
                }
            }
        }

        private void ResetMediationStatus(cdr cdr)
        {
            cdr.errorCode = ""; //set error flag empty by default to prevent calls going to cdrloaded table...
            cdr.mediationcomplete = 0;
        }

        private void SetMediationStatusToSuccess(cdr cdr)
        {
            cdr.mediationcomplete = 1;
            cdr.errorCode = ""; //clear error flag/description from previous mediation attempt
        }

        public void WriteChangesExceptContext()
        {
            this.WriteCdrs();
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
                ? (Convert.ToDateTime(thisCdr.ConnectTime) - thisCdr.ActualStartTime).TotalSeconds
                : (Convert.ToDateTime(thisCdr.AnswerTime) - thisCdr.ActualStartTime).TotalSeconds;
            thisCdr.PDD = (float)diffInSeconds;
        }

        private cdrerror ConvertCdrToCdrError(string validationMsg, CdrExt cdrExt)
        {
            cdrExt.Cdr.mediationcomplete = 0;
            cdrExt.Cdr.errorCode = new StringBuilder("Error: ") //field4 in cdrerror to keep the error expression
                .Append(validationMsg).ToString();
            return CdrToCdrErrorConverter.Convert(cdrExt.Cdr);
        }


        private CollectionResultProcessingSummary GetCdrJobSegmentSummary()
        {
            //https://stackoverflow.com/questions/24776710/lambda-expression-select-min-and-max-at-the-same-time
            CollectionResultProcessingSummary jobSummary =
                (CollectionResultProcessingSummary) this.CollectionResult.ConcurrentCdrExts.Values
                    .Select(e => e.Cdr)
                    .GroupBy(anyConstant => 1)
                    .Select(g => new CollectionResultProcessingSummary()
                    {
                        StartSequenceNumber = g.Min(c => c.SequenceNumber),
                        EndSequenceNumber = g.Max(c => c.SequenceNumber),
                        MinIdCall = g.Min(c => c.idcall),
                        MaxIdCall = g.Max(c => c.idcall),
                        MinCallStartTime = g.Min(c => c.StartTime),
                        MaxCallStartTime = g.Max(c => c.StartTime),
                        PartialDuration = g.Sum(c => Convert.ToDouble(c.PartialDuration)),
                        TotalDuration = g.Sum(c => Convert.ToDouble(c.DurationSec)),
                        SuccessfulCount = g.Sum(c => Convert.ToInt32(c.ChargingStatus)),
                    }).First();
            jobSummary.FailedCount = this.CollectionResult.ConcurrentCdrExts.Count - jobSummary.SuccessfulCount;
            return jobSummary;
        }

        private void ExecutePartnerRules(List<string> partnerRules, CdrExt cdrExt)
        {
            foreach (var c in partnerRules)
            {
                IPartnerRule partnerRule = null;
                this.CdrJobContext.MediationContext.MefPartnerRuleContainer.DicExtensions.TryGetValue(c,
                    out partnerRule);
                if (partnerRule != null)
                {
                    partnerRule.Execute(cdrExt.Cdr, this.CdrJobContext.MediationContext.MefPartnerRuleContainer);
                }
            }
        }

        private void ExecuteServiceFamilyRules(IServiceGroup serviceGroup, List<RatingRule> ratingRules, CdrExt cdrExt)
        {
            foreach (RatingRule rule in ratingRules)
            {
                IServiceFamily sf = null;
                this.CdrJobContext.MediationContext.MefServiceFamilyContainer.DicExtensions.TryGetValue(
                    rule.IdServiceFamily, out sf);
                if (sf != null)
                {
                    int idProductForIndividualAccount = 0; //find out a way to implement this later
                    if (string.IsNullOrEmpty(cdrExt.Cdr.UniqueBillId))
                        throw new Exception("Unique BillId not found!");
                    ServiceContext serviceContext = new ServiceContext(this, serviceGroup, sf,
                        rule.AssignDirection == 1
                            ? ServiceAssignmentDirection.Customer
                            : rule.AssignDirection == 2
                                ? ServiceAssignmentDirection.Supplier
                                : ServiceAssignmentDirection.None, idProductForIndividualAccount);
                    //execute rating & accounting
                    AccChargeableExt chargeableExt = sf.Execute(cdrExt, serviceContext, false);
                    if (chargeableExt != null)
                    {
                        cdrExt.Chargeables.Add(chargeableExt.AccChargeable.GetTuple(), chargeableExt.AccChargeable);
                        acc_transaction transaction = sf.GetTransaction(chargeableExt, serviceContext);
                        cdrExt.Transactions.Add(transaction);
                    }
                }
            }
        }

        void WriteCdrs()
        {
            if (this.CdrJobContext.TelcobrightJob.idjobdefinition == 2) //cdrError
            {
                OldCdrDeleter.DeleteOldCdrs("cdrerror", this.CollectionResult.ConcurrentCdrExts.Values
                        .Select(c => new KeyValuePair<long, DateTime>(c.Cdr.idcall, c.StartTime)).ToList(),
                    this.CdrJobContext.SegmentSizeForDbWrite, this.CdrJobContext.DbCmd);
            }

            long inconsistentCount = WriteCdrInconsistent();
            long errorCount = WriteCdrError();
            long cdrCount = WriteCdr();

            List<PartialCdrContainer> partialCdrContainers = new List<PartialCdrContainer>();
            if (this.Tbc.CdrSetting.PartialCdrEnabledNeIds.Contains(this.CdrJobContext.Ne.idSwitch))
            {
                var partialCdrWriter = new PartialCdrWriter(
                    this.CollectionResult.ConcurrentCdrExts.Values.Where(e => e.Cdr.PartialFlag == 1)
                        .Select(e => e.PartialCdrContainer).ToList(), this.CdrJobContext);
                partialCdrWriter.Write();
                partialCdrContainers = partialCdrWriter.PartialCdrContainers;
            }
            int cdrExtCount = this.CollectionResult.ConcurrentCdrExts.Count;
            int rawPartialActualCount = partialCdrContainers.Sum(c => c.NewRawInstances.Count);
            int distintPartialCount = partialCdrContainers.Count;

            if (this.CollectionResult.RawCount !=
                (inconsistentCount + errorCount + cdrCount + rawPartialActualCount - distintPartialCount))
                throw new Exception(
                    "RawCount in collection result must equal (inconsistentCount+ errorCount + cdrCount+rawPartialActualCount-distintPartialCount");
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

        int WriteCdr()
        {
            var cdrs = this.CollectionResult.ConcurrentCdrExts.Values
                .Where(c => c.Cdr != null && c.Cdr.mediationcomplete == 1).Select(c => c.Cdr).ToList();
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
