using LibraryExtensions;
using System;
using System.Collections;
using System.Collections.Concurrent;
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
using System.IO;
using System.Text.RegularExpressions;
using Itenso.TimePeriod;
using MediationModel.enums;
using TransactionTuple = System.ValueTuple<int, int, long, int>;
using CdrSummaryTuple = System.ValueTuple<int, int, int, string, string, decimal, decimal, System.ValueTuple<string, string, string, string, string, string, string, System.ValueTuple<string, string, string, string, string, string>>>;

namespace TelcobrightMediation
{
    public class CdrProcessor
    {
        public bool SkipServiceGroupRules { get; set; }
        public List<int> ServiceFamilyRulesToSkip { get; set; } = new List<int>();
        public List<int> PartnerRulesToSkip { get; set; } = new List<int>();
        public CdrJobContext CdrJobContext { get; }
        public CdrCollectionResult CollectionResult { get; }
        private MediationContext MediationContext => this.CdrJobContext.MediationContext;
        private TelcobrightConfig Tbc => this.MediationContext.Tbc;
        private DbCommand DbCmd => this.CdrJobContext.DbCmd;

        public bool PartialProcessingEnabled => this.Tbc.CdrSetting.PartialCdrEnabledNeIds.Contains(this.CdrJobContext
            .Ne.idSwitch);

        private List<CdrExt> NewCdrExts => this.CollectionResult.ConcurrentCdrExts.Values.ToList();
        AccountingContext AccountingContext => this.CdrJobContext.AccountingContext;
        private CdrReProcessingType CdrReProcessingType { get; }

        public CdrProcessor(CdrJobContext cdrJobContext, CdrCollectionResult newCollectionResult)
        {
            this.CdrJobContext = cdrJobContext;
            this.CollectionResult = newCollectionResult;
            var telcobrightJob = this.CdrJobContext.TelcobrightJob;
            switch (telcobrightJob.idjobdefinition)
            {
                case 1: //new cdr
                    this.CdrReProcessingType = CdrReProcessingType.NoneOrNew;
                    break;
                case 2: //error cdr
                    this.CdrReProcessingType = CdrReProcessingType.ErrorProcess;
                    break;
                case 3:
                    this.CdrReProcessingType = CdrReProcessingType.ReProcess;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unknown cdr job type.");
            }
        }

        public void Mediate()
        {
            int maxDegreeOfParallelism = -1; //-1=no thread limitation
            if (this.CdrJobContext.MediationContext.CdrSetting.DisableParallelMediation == true)
            {
                maxDegreeOfParallelism = 1;
            }
            Parallel.ForEach(this.NewCdrExts, new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism },
                cdrExt =>
                {
                    try
                    {
                        cdr thisCdr = cdrExt.Cdr;
                        ResetMediationStatus(thisCdr);
                        foreach (var serviceGroupPreProcessor in
                            this.MediationContext.MefServiceGroupContainer.ServiceGroupPreProcessors.Values)
                        {
                            if (this.CdrJobContext.MediationContext.Tbc.CdrSetting
                                .ServiceGroupPreProcessingRules.Any())
                            {
                                serviceGroupPreProcessor.Execute(thisCdr, this);
                            }
                        }
                        IServiceGroup serviceGroup = null;
                        ServiceGroupConfiguration serviceGroupConfiguration = null;
                        if (this.SkipServiceGroupRules == true)
                        {
                            var lastMediatedIdServiceGroup = cdrExt.Cdr.ServiceGroup;
                            this.CdrJobContext.MediationContext.MefServiceGroupContainer.IdServiceGroupWiseServiceGroups
                                .TryGetValue(lastMediatedIdServiceGroup, out serviceGroup);
                            serviceGroupConfiguration = this.CdrJobContext.MediationContext.Tbc.CdrSetting
                                .ServiceGroupConfigurations[lastMediatedIdServiceGroup];
                        }
                        else serviceGroup = ExecuteServiceGroups(cdrExt, out serviceGroupConfiguration);

                        if (serviceGroup != null)
                        {
                            bool allPartnersFound = false;
                            if (serviceGroupConfiguration.PartnerRules.Any())
                            {
                                allPartnersFound = FindPartnersByPartnerRules(cdrExt, serviceGroupConfiguration);
                            }
                            else
                            {
                                if (serviceGroupConfiguration.InPartnerFirstMatchRules.Any()
                                    || serviceGroupConfiguration.OutPartnerFirstMatchRules.Any())
                                {
                                    allPartnersFound = FindPartnersByFirstMatchRules(cdrExt, serviceGroupConfiguration);
                                }
                            }
                            if (allPartnersFound)
                            {
                                ExecuteRating(serviceGroupConfiguration, serviceGroupConfiguration.Ratingtrules,
                                    cdrExt);
                                serviceGroup.ExecutePostRatingActions(cdrExt, this);
                                ExecuteNerRule(this, cdrExt);
                                if (this.CdrJobContext.MediationContext.Tbc.CdrSetting.CallConnectTimePresent == true &&
                                    cdrExt.Cdr.ServiceGroup > 0 && cdrExt.Cdr.ChargingStatus == 1)
                                {
                                    SetPdd(cdrExt.Cdr);
                                }
                            }
                        }
                        string validationErorMsg = MediationValidator.ExecuteRules(cdrExt, this.CdrJobContext);
                        if (!string.IsNullOrEmpty(validationErorMsg))
                            SendToCdrError(cdrExt, "Mediation error: " + validationErorMsg);
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

        private bool FindPartnersByPartnerRules(CdrExt cdrExt, ServiceGroupConfiguration serviceGroupConfiguration)
        {
            bool allPartnersFound = true;
            foreach (var idPartnerRule in serviceGroupConfiguration.PartnerRules
                .Where(r => !this.PartnerRulesToSkip.Contains(r)))
            {
                var partnerRule = GefMefPartnerRule(idPartnerRule);
                if (partnerRule.Execute(cdrExt.Cdr,
                        this.CdrJobContext.MediationContext.MefPartnerRuleContainer) <= 0)
                {
                    return false;
                }
            }
            return allPartnersFound;
        }
        private bool FindPartnersByFirstMatchRules(CdrExt cdrExt, ServiceGroupConfiguration serviceGroupConfiguration)
        {
            bool allPartnersFound = true;
            Func<List<int>, bool> firstMatchRulesMatcher = rules =>
            {
                bool match = false;
                foreach (var idPartnerRule in rules)
                {
                    var partnerRule = GefMefPartnerRule(idPartnerRule);
                    if (partnerRule.Execute(cdrExt.Cdr,
                            this.CdrJobContext.MediationContext.MefPartnerRuleContainer) > 0)
                    {
                        match = true;
                        break;
                    }
                    else//todo: remote
                    {
                        var x = false;
                    }
                }
                return match;
            };
            if (serviceGroupConfiguration.InPartnerFirstMatchRules.Any())
            {
                allPartnersFound = firstMatchRulesMatcher(serviceGroupConfiguration.InPartnerFirstMatchRules);
                if (allPartnersFound == false) return false;
            }

            if (serviceGroupConfiguration.OutPartnerFirstMatchRules.Any())
            {
                allPartnersFound = firstMatchRulesMatcher(serviceGroupConfiguration.OutPartnerFirstMatchRules);
                if (allPartnersFound == false) return false;
            }
            return allPartnersFound;
        }
        IPartnerRule GefMefPartnerRule(int idRule)
        {
            IPartnerRule rule = null;
            this.CdrJobContext.MediationContext.MefPartnerRuleContainer.DicExtensions.TryGetValue(idRule,
                out rule);
            if (rule == null)
                throw new Exception("Partnerrule with id=" + idRule + "not found.");
            return rule;
        }
        public ParallelQuery<CdrExt> GenerateSummaries()
        {
            ParallelQuery<CdrExt> parallelCdrs = this.CollectionResult.ProcessedCdrExts.AsParallel();
            parallelCdrs.ForAll(processedCdrExt =>
            {
                this.CdrJobContext.CdrSummaryContext.GenerateSummary(processedCdrExt);
                var cdr = processedCdrExt.Cdr;
                if (Convert.ToDecimal(cdr.SummaryMetaTotal) != 0)
                    throw new Exception("Summmary meta data of new cdr must be zero.");
                cdr.SummaryMetaTotal = processedCdrExt.TableWiseSummaries.Values.Sum(s => s.actualduration);
            });
            return parallelCdrs;
        }

        public void MergeNewSummariesIntoCache(ParallelQuery<CdrExt> parallelCdrExts)
        {
            parallelCdrExts.ForAll(processedCdrExt =>
            {
                foreach (var kv in processedCdrExt.TableWiseSummaries)
                {
                    CdrSummaryType summaryTargetTable = kv.Key;
                    this.CdrJobContext.CdrSummaryContext.MergeAddSummary(summaryTargetTable, kv.Value);
                }
            });
        }

        public void ProcessChargeables(ParallelQuery<CdrExt> parallelCdrExts)
        {
            parallelCdrExts.Where(c => c.Cdr.ChargingStatus == 1).ForAll(processedCdrExt =>
            {
                if (Convert.ToDecimal(processedCdrExt.Cdr.ChargeableMetaTotal) > 0)
                    throw new Exception("Chargeable meta total cannot be > 0 for new cdr.");
                processedCdrExt.Cdr.ChargeableMetaTotal =
                    processedCdrExt.Chargeables.Values.Sum(c => c.BilledAmount);
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
            else if (this.CdrReProcessingType == CdrReProcessingType.NoneOrNew == false) //error or reProcess
                this.CollectionResult.AddPartialCdrAsNonPartialCdrErrorDuringReProcess(cdrExt, errorMessage);
            else if (cdrExt.PartialCdrContainer != null && cdrExt.Cdr.PartialFlag != 0)
            {
                this.CollectionResult.AddPartialCdrToCdrErrors(cdrExt, errorMessage);
            }
            else throw new InvalidDataContractException("Cdr must be either partial or non-partial.");
        }

        private IServiceGroup ExecuteServiceGroups(CdrExt newCdrExt,
            out ServiceGroupConfiguration serviceGroupConfiguration)
        {
            IServiceGroup serviceGroup = null;
            serviceGroupConfiguration = null;
            Dictionary<int, ServiceGroupConfiguration> serviceGroupConfigurations =
                this.CdrJobContext.MediationContext.Tbc.CdrSetting.ServiceGroupConfigurations;
            foreach (KeyValuePair<int, ServiceGroupConfiguration> kv in serviceGroupConfigurations)
            {
                if (kv.Value.Disabled) continue;
                this.CdrJobContext.MediationContext.MefServiceGroupContainer.IdServiceGroupWiseServiceGroups
                    .TryGetValue(kv.Key, out serviceGroup);
                if (serviceGroup != null)
                {
                    serviceGroupConfiguration = kv.Value;
                    newCdrExt.Cdr.ServiceGroup = 0; //unset sg first if already set, e.g. during re-processing
                    serviceGroup.Execute(newCdrExt.Cdr, this);
                    if (newCdrExt.Cdr.ServiceGroup > 0) return serviceGroup;
                }
            }
            return serviceGroup;
        }

        private void ResetMediationStatus(cdr cdr)
        {
            cdr.ErrorCode = ""; //set error flag empty by default to prevent calls going to cdr table...
            cdr.MediationComplete = 0;
            cdr.AdditionalMetaData = null;
            cdr.ChargeableMetaTotal = null;
            cdr.SummaryMetaTotal = null;
            cdr.TransactionMetaTotal = null;
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


        private void ExecuteRating(ServiceGroupConfiguration serviceGroupConfiguration,
            List<RatingRule> ratingRules, CdrExt cdrExt)
        {
            var cdr = cdrExt.Cdr;
            foreach (RatingRule rule in ratingRules.Where(
                r => !this.ServiceFamilyRulesToSkip.Contains(r.IdServiceFamily)))
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
                        , idProductForIndividualAccount, rule.DigitRulesData);
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

        public CdrWritingResult WriteCdrs(ParallelQuery<CdrExt> processedCdrExts)
        {
            int numberOfProcessedCdrs = processedCdrExts.Count();
            Action<List<KeyValuePair<long, DateTime>>, int> cdrErrorDeleter = (idCallsToDelete, expectedDelCount) =>
            {
                int delCount = OldCdrDeleter.DeleteOldCdrs("cdrerror", idCallsToDelete,
                    this.CdrJobContext.SegmentSizeForDbWrite, this.CdrJobContext.DbCmd);
                if (delCount != expectedDelCount)
                    throw new Exception("Number of deleted cdrerrors does not match count in collectionResult.");
            };
            if (this.CdrJobContext.TelcobrightJob.idjobdefinition == 2) //cdrError
            {
                var idCallsOfProcessedCdrs = processedCdrExts
                    .Select(c => new KeyValuePair<long, DateTime>(c.Cdr.IdCall, c.StartTime)).ToList();
                var idCallsOfCdrErrors = this.CollectionResult.CdrExtErrors
                    .Select(c => new KeyValuePair<long, DateTime>(c.CdrError.IdCall, c.CdrError.StartTime)).ToList();
                var idCallsInCdrErrorToDelete = idCallsOfProcessedCdrs.Concat(idCallsOfCdrErrors).ToList();
                cdrErrorDeleter.Invoke(idCallsInCdrErrorToDelete,
                    numberOfProcessedCdrs + this.CollectionResult.CdrExtErrors.Count);
            }
            if (this.CdrJobContext.TelcobrightJob.idjobdefinition == 1) //newCdr
            {
                var idCallsOfCdrErrorsForPreExistingErrorPartial = this.CollectionResult.CdrExtErrors
                    .Where(c => c.IsPartial == true && c.PartialCdrContainer.LastProcessedAggregatedRawInstance != null
                                && c.HasPreExistingCdrError == true).Select(c => new KeyValuePair<long, DateTime>
                        (c.PartialCdrContainer.LastProcessedAggregatedRawInstance.IdCall, c.CdrError.StartTime))
                    .ToList();
                if (idCallsOfCdrErrorsForPreExistingErrorPartial.Any())
                    cdrErrorDeleter.Invoke(idCallsOfCdrErrorsForPreExistingErrorPartial,
                        idCallsOfCdrErrorsForPreExistingErrorPartial.Count);
            }
            long writtenInconsistentCount = 0,
                writtenErrorCount = 0,
                writtenNonPartialCdrCount = 0,
                writtenNormalizedPartialCdrCount = 0,
                writtenCdrCount = 0;

            if (this.CollectionResult.CdrInconsistents.Any())
                writtenInconsistentCount = WriteCdrInconsistent();//code changed, from now on inconsistent will be written to cdrError too.

            if (this.CollectionResult.CdrExtErrors.Any())
                writtenErrorCount = WriteCdrError();

            var nonPartialCdrs = processedCdrExts
                .Where(c => c.Cdr.MediationComplete == 1 && c.Cdr.PartialFlag == 0).Select(c => c.Cdr).ToList();
            var normalizedPartialCdrs = processedCdrExts
                .Where(c => c.Cdr.MediationComplete == 1 && c.Cdr.PartialFlag > 0).Select(c => c.Cdr).ToList();
            var partialFlagIndicators = this.CdrJobContext.MediationContext.CdrSetting.PartialCdrFlagIndicators;
            var errorCdrExts = this.CollectionResult.CdrExtErrors;
            var nonPartialCdrErrors = errorCdrExts
                .Where(c => c.CdrError.PartialFlag == "0").Select(c => c.CdrError).ToList();
            var normalizedPartialCdrErrors = errorCdrExts
                .Where(c => partialFlagIndicators.Contains(c.CdrError.PartialFlag)).Select(c => c.CdrError).ToList();
            var nonPartialIdCalls = nonPartialCdrs.Select(c => c.IdCall);
            var partialNewAndPrevIdCalls = normalizedPartialCdrs.Select(c => c.IdCall)
                .Concat(normalizedPartialCdrErrors.Select(c => c.IdCall));
            var allSupposedTobeUniqueIdCalls = nonPartialIdCalls.Concat(partialNewAndPrevIdCalls).ToList();
            if (allSupposedTobeUniqueIdCalls.GroupBy(c => c)
                .Any(g => g.Count() > 1))
            {
                throw new Exception("Idcalls must be unique.");
            }

            var skipSettingsForSummaryOnly = this.Tbc.CdrSetting.SkipSettingsForSummaryOnly;

            if (skipSettingsForSummaryOnly.SkipCdr == false)
            {
                if (nonPartialCdrs.Any())
                {
                    writtenNonPartialCdrCount = WriteCdr(nonPartialCdrs);
                }
                if (normalizedPartialCdrs.Any())
                {
                    writtenNormalizedPartialCdrCount = WriteCdr(normalizedPartialCdrs);
                }
                writtenCdrCount = writtenNonPartialCdrCount + writtenNormalizedPartialCdrCount;
            }
            else//summary mode only
            {
                writtenNonPartialCdrCount = nonPartialCdrs.Count;
                writtenNormalizedPartialCdrCount = normalizedPartialCdrs.Count;
                writtenCdrCount = writtenNonPartialCdrCount + writtenNormalizedPartialCdrCount;
            }

            if (writtenCdrCount != numberOfProcessedCdrs)
            {
                throw new Exception("Written number of cdrs does not match processed cdrs count.");
            }
            int uniqueEventCount = 0;
            if (this.CdrJobContext.CdrjobInputData.Ne.FilterDuplicateCdr == 1)
            {
                uniqueEventCount = WriteUniqueEventsHistory(this.CollectionResult.FinalNonDuplicateEvents);
            }
            if (this.CollectionResult.FinalNonDuplicateEvents != null &&
                uniqueEventCount != this.CollectionResult.FinalNonDuplicateEvents.Count)
            {
                throw new Exception("Written number of unique event count does not match non-duplicate event count.");
            }

            List<PartialCdrContainer> partialCdrContainers = new List<PartialCdrContainer>();
            PartialCdrWriter partialCdrWriter = null;
            if (this.PartialProcessingEnabled)
            {
                List<PartialCdrContainer> processedPartialCdrs = processedCdrExts
                    .Where(e => e.Cdr.PartialFlag != 0 && e.PartialCdrContainer != null)
                    .Select(e => e.PartialCdrContainer).ToList();
                List<PartialCdrContainer> errorPartialCdrs = this.CollectionResult.CdrExtErrors
                    .Where(e => e.CdrError.PartialFlag != "0" && e.PartialCdrContainer != null)
                    .Select(e => e.PartialCdrContainer).ToList();
                partialCdrWriter = new PartialCdrWriter(
                    processedPartialCdrs.Concat(errorPartialCdrs).ToList(), this.CdrJobContext);
                if (skipSettingsForSummaryOnly.SkipCdr == false)
                {
                    partialCdrWriter.Write();
                }
                partialCdrContainers = partialCdrWriter.PartialCdrContainers;
            }
            int rawPartialActualCount = partialCdrContainers.Sum(c => c.NewRawInstances.Count);
            int writtenNewRawInstances = partialCdrWriter?.WrittenNewRawInstances ?? 0;

            if (skipSettingsForSummaryOnly.SkipCdr == false)
            {
                if (writtenNewRawInstances != rawPartialActualCount)
                {
                    throw new Exception("Written number of new partial raw instances does not match actual count.");
                }
            }

            if (this.CdrReProcessingType == CdrReProcessingType.NoneOrNew) //newCdrFile
            {
                if (this.CollectionResult.RawCount !=
                    writtenNonPartialCdrCount + writtenNewRawInstances + writtenInconsistentCount
                    + nonPartialCdrErrors.Count)
                    throw new Exception(
                        "RawCount in collection result must equal (nonPartialCount+ newRawPartialInstances+inconsistentCount.");

                if (nonPartialCdrs.Count + normalizedPartialCdrs.Count + normalizedPartialCdrErrors.Count
                    + nonPartialCdrErrors.Count != writtenErrorCount + writtenCdrCount)
                    throw new Exception(
                        "RawCount in collection result must equal (inconsistentCount+ errorCount + cdrCount+rawPartialActualCount-distintPartialCount");
            }
            else //error or re_process
            {
                if (processedCdrExts.Count() + errorCdrExts.Count != writtenErrorCount + writtenCdrCount)
                    throw new Exception(
                        "RawCount in collection result must equal (inconsistentCount+ errorCount + cdrCount+rawPartialActualCount-distintPartialCount");
            }
            return new CdrWritingResult()
            {
                CdrCount = writtenCdrCount,
                CdrErrorCount = writtenErrorCount,
                CdrInconsistentCount = writtenInconsistentCount,
                PartialCdrWriter = partialCdrWriter,
                NonPartialCdrCount = writtenNonPartialCdrCount,
                NonPartialErrorCount = this.CollectionResult.CdrExtErrors.Count(c => c.IsPartial == false),
                NormalizedPartialCount = writtenNormalizedPartialCdrCount +
                                         this.CollectionResult.CdrExtErrors.Count(c => c.IsPartial),
            };
        }

        public long WriteCdrInconsistent()
        {
            long inconsistentCount = 0;
            int startAt = 0;
            CollectionSegmenter<StringBuilder> segmenter
                = new CollectionSegmenter<StringBuilder>(
                    this.CollectionResult.CdrInconsistents.Select(c => c.GetExtInsertValues()), startAt);
            segmenter.ExecuteMethodInSegments(this.CdrJobContext.SegmentSizeForDbWrite,
                segment =>
                {
                    var stringBuilders = segment.ToList();
                    int segmentCount = stringBuilders.Count;
                    int affectedRecordCount = DbWriterWithAccurateCount.ExecSingleStatementThroughStoredProc(
                        dbCmd: this.DbCmd,
                        //command: "insert into cdrinconsistent values " + string.Join(",", stringBuilders),
                        command: "insert into cdrerror values " + string.Join(",", stringBuilders),
                        expectedRecCount: segmentCount);
                    if (affectedRecordCount != segmentCount)
                        throw new Exception(
                            "Affected record count does not match segment count while writing cdr inconsistent.");
                    inconsistentCount += affectedRecordCount; //write cdr loaded
                });
            return inconsistentCount;
        }

        long WriteCdrError()
        {
            List<CdrExt> cdrErrors = this.CollectionResult.CdrExtErrors.ToList();
            long errorInsertedCount = 0;
            int startAt = 0;
            CollectionSegmenter<CdrExt> collectionSegmenter =
                new CollectionSegmenter<CdrExt>(cdrErrors, startAt);
            collectionSegmenter.ExecuteMethodInSegments(this.CdrJobContext.SegmentSizeForDbWrite,
                segment =>
                {
                    int segmentCount = segment.Count();
                    int affectedRecordCount = DbWriterWithAccurateCount.ExecSingleStatementThroughStoredProc(
                        dbCmd: this.DbCmd,
                        command: new StringBuilder(StaticExtInsertColumnHeaders.cdrerror)
                            .Append(string.Join(",", segment.AsParallel().Select(c => c.CdrError.GetExtInsertValues())))
                            .ToString(),
                        expectedRecCount: segmentCount);
                    if (affectedRecordCount != segmentCount)
                        throw new Exception(
                            "Affected record count does not match segment count while writing cdr errors.");
                    errorInsertedCount += affectedRecordCount;
                });
            return errorInsertedCount;
        }

        int WriteCdr(List<cdr> cdrs)
        {
            decimal durationSumNonSegmented = cdrs.Sum(c => c.DurationSec);
            decimal durationSumSegmented = 0;
            int insertCount = 0;
            int startAt = 0;
            CollectionSegmenter<cdr> collectionSegmenter = new CollectionSegmenter<cdr>(cdrs, startAt);
            collectionSegmenter.ExecuteMethodInSegments(this.CdrJobContext.SegmentSizeForDbWrite,
                segment =>
                {
                    int segmentCount = segment.Count();
                    var segmentAsParallel = segment.AsParallel();
                    ParallelQuery<StringBuilder> sbs = segmentAsParallel.Select(c => c.GetExtInsertValues());
                    durationSumSegmented += segmentAsParallel.AsParallel().Sum(c => c.DurationSec);

                    StringBuilder extInsertCommandsForthisSegment = StringBuilderJoiner.Join(",", sbs.ToList());
                    this.DbCmd.CommandType = CommandType.StoredProcedure;
                    this.DbCmd.CommandText = "sp_extInsert";

                    int affectedRecordCount = DbWriterWithAccurateCount.ExecSingleStatementThroughStoredProc(
                        dbCmd: this.DbCmd,
                        command: new StringBuilder(StaticExtInsertColumnHeaders.cdr)
                            .Append(extInsertCommandsForthisSegment).ToString(),
                        expectedRecCount: segmentCount);
                    if (affectedRecordCount != segmentCount)
                        throw new Exception("Affected record count does not match segment count while writing cdrs.");
                    insertCount += affectedRecordCount;
                });
            if (durationSumNonSegmented != durationSumSegmented)
                throw new Exception("Duration mismatch between segmented & non segmented cdrs.");
            return insertCount;
        }

        int WriteUniqueEventsHistory(Dictionary<string, string[]> finalNonDuplicateEvents)//<tuple, cdr row as text[] as decoded initially>
        {
            int insertCount = 0;
            int startAt = 0;
            List<KeyValuePair<string, string[]>> tupleVsTextCdrs = finalNonDuplicateEvents
                .Select(kv => new KeyValuePair<string, string[]>(kv.Key, kv.Value)).ToList();

            Dictionary<DateTime, List<KeyValuePair<string, string[]>>> dateWiseTupleVsTextCdrs =
                tupleVsTextCdrs.GroupBy(kv => kv.Value[Fn.StartTime].ConvertToDateTimeFromMySqlFormat().Date)
                    .ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());

            foreach (var kvDateWise in dateWiseTupleVsTextCdrs)
            {
                DateTime date = kvDateWise.Key;
                string tableName = $"uniqueevent{date:yyyyMMdd}";
                List<KeyValuePair<string, string[]>> tupleVsCdrRow = kvDateWise.Value;

                CollectionSegmenter<KeyValuePair<string, string[]>> collectionSegmenter =
                    new CollectionSegmenter<KeyValuePair<string, string[]>>(tupleVsCdrRow, startAt);
                collectionSegmenter.ExecuteMethodInSegments(this.CdrJobContext.SegmentSizeForDbWrite,
                    segment =>
                    {
                        var segmentAsList = segment.ToList();
                        int segmentCount = segmentAsList.Count;

                        ParallelIterator<KeyValuePair<string, string[]>, StringBuilder> iterator =
                            new ParallelIterator<KeyValuePair<string, string[]>, StringBuilder>(segmentAsList);
                        List<StringBuilder> sbs =
                            iterator.getOutput(kv => new StringBuilder($"('{kv.Key}','{kv.Value[Fn.StartTime]}')"));

                        string sql = $" insert into {tableName}(tuple, starttime) values " +
                                     $"{StringBuilderJoiner.Join(",", sbs).ToString()}";
                        this.DbCmd.CommandType = CommandType.StoredProcedure;
                        this.DbCmd.CommandText = "sp_extInsert";

                        int affectedRecordCount = DbWriterWithAccurateCount.ExecSingleStatementThroughStoredProc(
                            dbCmd: this.DbCmd,
                            command: sql,
                            expectedRecCount: segmentCount);
                        if (affectedRecordCount != segmentCount)
                            throw new Exception("Affected record count does not match segment count while writing cdrs.");
                        insertCount += affectedRecordCount;
                    });
            }
            return insertCount;
        }

    }
}

