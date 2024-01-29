using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using FlexValidation;
using LibraryExtensions;
using MediationModel;
using TelcobrightFileOperations;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;
using TelcobrightMediation.Mediation.Cdr;

namespace TelcobrightMediation
{
    public class NewCdrPreProcessor : AbstractCdrJobPreProcessor
    {
        public AbstractCdrDecoder Decoder { get; set; }
        private bool PartialCdrEnabled { get; }
        public List<string[]> TxtCdrRows { get; set; }= new List<string[]>();
        public List<string[]> DecodedCdrRowsBeforeDuplicateFiltering { get; set; } = new List<string[]>();
        public List<string[]> RowsToConsiderForAggregation { get; } = new List<string[]>();
        public List<string[]> NewRowsToBeDiscardedAfterAggregation { get; } = new List<string[]>();
        public List<string[]> OldRowsToBeDiscardedAfterAggregation { get; } = new List<string[]>();
        public List<string[]> FinalAggregatedInstances { get; set; } = new List<string[]>();
        public List<string[]> NewRowsCouldNotBeAggregated { get; set; } = new List<string[]>();
        public List<string[]> OldRowsCouldNotBeAggregated { get; set; } = new List<string[]>();
        public List<string[]> OldPartialInstancesFromDB { get; set; } = new List<string[]>();
        public Dictionary<string, string[]> FinalNonDuplicateEvents { get; set; }= new Dictionary<string, string[]>();
        public HashSet<string> ExistingUniqueEventInstancesFromDB { get; }= new HashSet<string>();
        public List<string[]> NewDuplicateEvents { get; set; }= new List<string[]>();
        public List<string[]> OriginalRowsBeforeMerge { get; }= new List<string[]>();
        public List<cdrinconsistent> OriginalCdrinconsistents { get; }= new List<cdrinconsistent>();
        public List<string[]> DebugCdrsForDump { get; set; }= new List<string[]>();
        public long OriginalCdrFileSize { get; set; }
        public NewCdrPreProcessor(List<string[]> txtCdrRows, List<cdrinconsistent> inconsistentCdrs,
            CdrCollectorInputData cdrCollectorInputData)
            : base(cdrCollectorInputData, txtCdrRows.Count + inconsistentCdrs.Count, inconsistentCdrs) //used after sql collection
        {
            this.TxtCdrRows = txtCdrRows;
            foreach (string[] row in this.TxtCdrRows)
            {
                this.OriginalRowsBeforeMerge.Add(row);
            }
            foreach (cdrinconsistent inconsistentCdr in inconsistentCdrs)
            {
                this.OriginalCdrinconsistents.Add(inconsistentCdr);
            }
            this.PartialCdrEnabled = base.CdrCollectorInputData.CdrSetting.PartialCdrEnabledNeIds
                .Contains(base.CdrCollectorInputData.CdrJobInputData.Ne.idSwitch);
        }
        public int NewAndInconsistentCount => this.TxtCdrRows.Count + base.InconsistentCdrs.Count;
        public void ValidateAggregation(job j)
        {
            Exception exception =
                new Exception($"Found Cdr count mismatch after aggregation. Job: {j.JobName}");
            if (this.DecodedCdrRowsBeforeDuplicateFiltering.Count +this.OldPartialInstancesFromDB.Count 
                != FinalAggregatedInstances.Count + NewRowsCouldNotBeAggregated.Count
                + OldRowsCouldNotBeAggregated.Count 
                + NewRowsToBeDiscardedAfterAggregation.Count + OldRowsToBeDiscardedAfterAggregation.Count + NewDuplicateEvents.Count)
            {
                Console.WriteLine(exception);
                throw exception;
            }
            if (this.FinalAggregatedInstances.Count != this.TxtCdrRows.Count)
            {
                Console.WriteLine(exception);
                throw exception;
            }
            if (this.TxtCdrRows.Count == 0)//no successful aggregation
            {
                if (this.NewRowsToBeDiscardedAfterAggregation.Count > 0
                    || this.OldRowsToBeDiscardedAfterAggregation.Count > 0 
                    || this.NewRowsCouldNotBeAggregated.Count < 0)
                {
                    Console.WriteLine(exception);
                    throw exception;
                }
            }
            else//at least one successful aggregation
            {
                if (this.NewRowsToBeDiscardedAfterAggregation.Count <= 0 && this.OldRowsToBeDiscardedAfterAggregation.Count <= 0)
                {
                    Console.WriteLine(exception);
                    throw exception;
                }
            }
        }

        public cdrinconsistent CheckAndConvertIfInconsistent(CdrJobInputData input, MefValidator<string[]> mefValidator,
            string[] txtRow)
        {
            ValidationResult validationResult = mefValidator.Validate(txtRow);
            foreach (IValidationRule<string[]> rule in mefValidator.Rules)
            {
                if (rule.Validate(txtRow) == false)
                {
                    txtRow[Fn.ErrorCode] = rule.ValidationMessage;
                   base.InconsistentCdrs.Add(CdrConversionUtil.ConvertTxtRowToCdrinconsistent(txtRow));
                    var inconsistentCdr = CdrConversionUtil.ConvertTxtRowToCdrinconsistent(txtRow);
                    return inconsistentCdr;
                }
            }
            return null;
        }

        public CdrAndInconsistentWrapper ConvertToCdr(string[] row)
        {
            cdr convertedCdr = null;
            cdrinconsistent cdrInconsistent = null;
            List<IExceptionalCdrPreProcessor> exceptionalCdrPreProcessor =
                base.CdrCollectorInputData.MediationContext.MefExceptionalCdrPreProcessorContainer.DicExtensions.Values.ToList();
            convertedCdr = CdrConversionUtil.ConvertTxtRowToCdrOrInconsistentOnFailure(row, exceptionalCdrPreProcessor, out cdrInconsistent);
            return new CdrAndInconsistentWrapper(convertedCdr,cdrInconsistent);
        }

        public void AddToBaseCollection(CdrAndInconsistentWrapper cdrAndInconsistent)
        {
            cdrinconsistent cdrInconsistent = cdrAndInconsistent.Cdrinconsistent;
            cdr convertedCdr = cdrAndInconsistent.Cdr;
            if (convertedCdr == null && cdrInconsistent != null) return;
            if (cdrInconsistent != null) this.InconsistentCdrs.Add(cdrInconsistent);
            if (convertedCdr != null)
            {
                if (convertedCdr.PartialFlag == 0)
                    base.NonPartialCdrs.Add(convertedCdr);
                else
                {
                    if (convertedCdr.PartialFlag > 0)
                        base.RawPartialCdrInstances.Add(
                            new IcdrImplConverter<cdrpartialrawinstance>().Convert(convertedCdr));
                    else
                        throw new Exception("Converted cdr from txtRow must have valid numeric partial flag.");
                }
            }
            else throw new Exception("Both converted & inconsistent cdrs cannot be null or not null at the same time.");
        }

        public override void GetCollectionResults(out CdrCollectionResult newCollectionResult,
            out CdrCollectionResult oldCollectionResult)
        {
            if (this.PartialCdrEnabled && base.RawPartialCdrInstances.Any())
            {
                BlockingCollection<PartialCdrContainer> partialCdrContainers = CollectPartialCdrs();
                AggregatePartialCdrs(partialCdrContainers.Where(c => c.IsErrorCdr == false));
                AggregatePartialCdrs(partialCdrContainers.Where(c => c.IsErrorCdr == true));
                base.PartialCdrContainers = partialCdrContainers;
            }
            newCollectionResult = CreateNewCollectionResult();
            oldCollectionResult = CreateOldCollectionResult();
        }
        protected  override CdrCollectionResult CreateNewCollectionResult()
        {
            List<CdrExt> newPartialNonPartialCdrExts = this.CreateNewCdrExts();
            List<CdrExt> newCdrExtErrors = this.CreateNewCdrExtErrorsForPreExistingPartialCdrInError();
            ValidateCdrExtCreation(newPartialNonPartialCdrExts, newCdrExtErrors);
            var collectionResult= new CdrCollectionResult(base.CdrCollectorInputData.Ne, newPartialNonPartialCdrExts,
                base.InconsistentCdrs.ToList(), base.RawCount, this.OriginalRowsBeforeMerge);
            newCdrExtErrors.ForEach(c => collectionResult.SendPreExistingPartialCdrToCdrErrors(c, c.CdrError.ErrorCode));
            return collectionResult;
        }
        protected override List<CdrExt> CreateNewCdrExts()
        {
            var cdrExtsForNonPartials = base.NonPartialCdrs
                .Select(cdr => CdrExtFactory.CreateCdrExtWithNonPartialOrFinalInstance(cdr, CdrNewOldType.NewCdr));
            var cdrExtsForPartials = base.PartialCdrContainers.Where(pc => pc.IsErrorCdr == false)
                .Select(partialContainer => CdrExtFactory.CreateCdrExtWithPartialCdrContainer(partialContainer,
                    CdrNewOldType.NewCdr));
            return cdrExtsForPartials.Concat(cdrExtsForNonPartials).ToList();
        }

        protected List<CdrExt> CreateNewCdrExtErrorsForPreExistingPartialCdrInError()
        {
            var cdrExtErrors= base.PartialCdrContainers.Where(pc => pc.IsErrorCdr == true)
                .Select(partialContainer => CdrExtFactory.CreateCdrExtWithPartialCdrContainer(partialContainer,
                    CdrNewOldType.NewCdr)).ToList();
            cdrExtErrors.ForEach(c=>c.CdrError=CdrConversionUtil.ConvertCdrToCdrError(c.Cdr));
            return cdrExtErrors;
        }
        private BlockingCollection<PartialCdrContainer> CollectPartialCdrs()
        {
            PartialCdrCollector partialCdrCollector = new PartialCdrCollector(
                this.CdrCollectorInputData, base.RawPartialCdrInstances.ToList());
            partialCdrCollector.CollectPartialCdrHistory();
            partialCdrCollector.ValidateCollectionStatus();
            return partialCdrCollector.CreatePartialCdrContainers() ??
                   new BlockingCollection<PartialCdrContainer>();
        }
        private void AggregatePartialCdrs(IEnumerable<PartialCdrContainer> partialCdrContainers)
        {
            foreach (var partialCdrContainer in partialCdrContainers)
            {
                partialCdrContainer.Aggregate();
                partialCdrContainer.ValidateAggregation();
            }
        }
        

        protected override CdrCollectionResult CreateOldCollectionResult()
        {
            List<CdrExt> oldCdrExts = new List<CdrExt>();
            if (this.PartialCdrEnabled)
            {
                if (base.RawPartialCdrInstances?.Any() == true)
                {
                    Func<List<CdrExt>> oldCdrExtCreatorByLastPartialEqCloning = () => this.CreateOldCdrExts();
                    oldCdrExts = oldCdrExtCreatorByLastPartialEqCloning.Invoke();
                    var allIdCalls = oldCdrExts.Select(c => c.Cdr.IdCall).ToList();
                    if (allIdCalls.GroupBy(i => i).Any(g => g.Count() > 1))
                    {
                        throw new Exception("Duplicate idcalls for CdrExts in CdrJob");
                    }
                    List<CdrExt> successfulOldCdrExts = oldCdrExts.Where(c => c.Cdr.ChargingStatus == 1).ToList();
                    if (successfulOldCdrExts.Any())
                    {
                        base.LoadPrevAccountingInfoForSuccessfulCdrExts(successfulOldCdrExts);
                        base.VerifyPrevAccountingInfoCollection(successfulOldCdrExts,
                            base.CdrSetting.FractionalNumberComparisonTollerance);
                    }
                }
            }
            var oldCollectionResult = new CdrCollectionResult(base.CdrCollectorInputData.Ne, oldCdrExts,
                new List<cdrinconsistent>(), oldCdrExts.Count, this.OriginalRowsBeforeMerge);
            return oldCollectionResult;
        }

        
        protected override List<CdrExt> CreateOldCdrExts()
        {
            var partialCdrContainers = this.PartialCdrContainers
                .Where(c => c.IsErrorCdr==false&&c.LastProcessedAggregatedRawInstance != null).ToList();
            return partialCdrContainers
                .Select(partialContainer => CdrExtFactory.CreateCdrExtWithPartialCdrContainer(partialContainer,
                    CdrNewOldType.OldCdr)).ToList();
        }

        private void ValidateCdrExtCreation(List<CdrExt> newCdrExts, List<CdrExt> errorCdrExts)
        {
            var cdrExtsForNonPartials = newCdrExts.Where(c => c.Cdr.PartialFlag == 0).ToList();
            var cdrExtsForPartials = newCdrExts.Where(c => c.Cdr.PartialFlag != 0).ToList();
            if (newCdrExts.GroupBy(c => c.UniqueBillId).Any(g => g.Count() > 1))
                throw new Exception("Duplicate billId for CdrExts in CdrJob");

            List<long> allIdCalls = new List<long>();
            allIdCalls = cdrExtsForNonPartials.Select(c => c.Cdr.IdCall)
                .Concat(cdrExtsForPartials
                    .SelectMany(c => c.PartialCdrContainer.CombinedNewAndOldUnprocessedInstance).Select(c => c.IdCall))
                .Concat(errorCdrExts.Select(c => c.CdrError.IdCall)).ToList();
            if (allIdCalls.GroupBy(i => i).Any(g => g.Count() > 1))
            {
                throw new Exception("Duplicate idcalls for CdrExts in CdrJob");
            }
            var rawPartialCount = this.PartialCdrContainers.SelectMany(p => p.NewRawInstances).Count();
            var cdrJobInputData = this.CdrCollectorInputData.CdrJobInputData;
            int originalRawCount = cdrJobInputData.MergedJobsDic.Any()==false?this.RawCount
                :cdrJobInputData.MergedJobsDic.Values.Sum(wrappedJobs => wrappedJobs.NewAndInconsistentCount);
            if (originalRawCount+OldPartialInstancesFromDB.Count!= 
                cdrExtsForNonPartials.Count + cdrExtsForPartials.Count + errorCdrExts.Count 
                +NewRowsCouldNotBeAggregated.Count+ OldRowsCouldNotBeAggregated.Count +
                NewRowsToBeDiscardedAfterAggregation.Count + OldRowsToBeDiscardedAfterAggregation.Count + this.NewDuplicateEvents.Count +
                rawPartialCount - this.PartialCdrContainers.Count + base.InconsistentCdrs.Count)
                throw new Exception(
                    "Count of nonPartial and partial cdrs do not match expected with expected rawCount for this job.");
        }

        public void SetAllBlankFieldsToZerolengthString(string[] thisRow)
        {
            int colcount = this.TxtCdrRows[0].GetLength(0);
            int c = 0;
            for (c = 0; c < colcount; c++)
            {
                if (string.IsNullOrEmpty(thisRow[c]))
                {
                    thisRow[c] = "";
                }
                else if (string.IsNullOrWhiteSpace(thisRow[c]))
                {
                    thisRow[c] = (thisRow[c] == null ? string.Empty : thisRow[c].Trim());
                }
            }
        }

        public void RemoveIllegalCharacters(List<string> illegalStrToRemoveFromFields, string[] thisRow)
        {
            int colcount = this.TxtCdrRows[0].GetLength(0);
            int col = 0;
            for (col = 0; col < colcount; col++)
            {
                if (thisRow[col] != "")
                {
                    foreach (string illegalStr in illegalStrToRemoveFromFields)
                    {
                        thisRow[col] = thisRow[col].Replace(illegalStr, "");
                    }
                }
            }
            ;
        }

        public void SetSwitchid(string[] txtRow)
        {
            txtRow[0] = this.CdrCollectorInputData.Ne.idSwitch.ToString();
        }

        public void SetFileNameWithJobName(string cdrFileName, string[] txtRow)
        {
            txtRow[Fn.Filename] = cdrFileName; //filename
        }

        public void SetIdCall(AutoIncrementManager autoIncrementManager, string[] thisRow)
        {
            thisRow[Fn.IdCall] = autoIncrementManager.GetNewCounter(AutoIncrementCounterType.cdr).ToString();
        }

        public void AdjustStartTimeBasedOnCdrSettingsForSummaryTimeField(SummaryTimeFieldEnum summaryTimeFieldEnum,
            string[] row)
        {
            row[Fn.SignalingStartTime] = row[Fn.StartTime];
            if (summaryTimeFieldEnum == SummaryTimeFieldEnum.AnswerTime)
            {
                DateTime answerTime;
                if (row[Fn.AnswerTime].TryParseToDateTimeFromMySqlFormat(out answerTime) == true)
                {
                    row[Fn.StartTime] = row[Fn.AnswerTime];
                }
            }
        }
        public static MefValidator<string[]> CreateValidatorForInconsistencyCheck(CdrCollectorInputData collectorinput)
        {
            MefValidator<string[]> mefValidator = new MefValidator<string[]>(
                continueOnError: false,
                throwExceptionOnFirstError: false,
                rules:collectorinput.CdrSetting.ValidationRulesForInconsistentCdrs);
            return mefValidator;
        }
    }
}
