using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using FlexValidation;
using LibraryExtensions;
using MediationModel;
using org.springframework.expression.spel.ast;
using TelcobrightFileOperations;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;
using TelcobrightMediation.Mediation.Cdr;

namespace TelcobrightMediation
{
    public class NewCdrPreProcessor : AbstractCdrJobPreProcessor
    {
        private bool PartialCdrEnabled { get; }
        public List<string[]> TxtCdrRows { get; set; }
        public Dictionary<string, string[]> FinalNonDuplicateEvents { get; set; }

        public NewCdrPreProcessor(List<string[]> txtCdrRows, List<cdrinconsistent> inconsistentCdrs,
            CdrCollectorInputData cdrCollectorInputData)
            : base(cdrCollectorInputData, txtCdrRows.Count + inconsistentCdrs.Count, inconsistentCdrs) //used after sql collection
        {
            this.TxtCdrRows = txtCdrRows;
            this.PartialCdrEnabled = base.CdrCollectorInputData.CdrSetting.PartialCdrEnabledNeIds
                .Contains(base.CdrCollectorInputData.CdrJobInputData.Ne.idSwitch);
        }

        public void CheckAndConvertIfInconsistent(CdrJobInputData input, MefValidator<string[]> mefValidator,
            string[] txtRow)
        {
            ValidationResult validationResult = mefValidator.Validate(txtRow);
            foreach (IValidationRule<string[]> rule in mefValidator.Rules)
            {
                if (rule.Validate(txtRow) == false)
                {
                    txtRow[Fn.ErrorCode] = rule.ValidationMessage;
                    base.InconsistentCdrs.Add(CdrConversionUtil.ConvertTxtRowToCdrinconsistent(txtRow));
                    return;
                }
            }
        }

        

        public List<string[]> FilterCdrsWithDuplicateBillIdsAsInconsistent(List<string[]> txtRows)
        {
            List<string[]> dupRows = txtRows.GroupBy(c => c[Fn.UniqueBillId]).Where(g => g.Count() > 1)
                .Select(g => g.ToList())
                .SelectMany(c => c).ToList();
            txtRows = txtRows.Where(r => !dupRows.Select(dupRow => dupRow[Fn.UniqueBillId]).ToList()
                .Contains(r[Fn.UniqueBillId])).ToList();
            dupRows.ForEach(
                dupRow =>
                {
                    dupRow[Fn.ErrorCode] = "Duplicate billids are not allowed when partial cdrs are disabled.";
                    base.InconsistentCdrs.Add(CdrConversionUtil.ConvertTxtRowToCdrinconsistent(dupRow));
                });
            return txtRows;
        }

        public void ConvertToCdr(string[] row,out cdrinconsistent cdrInconsistent)
        {
            cdr convertedCdr = null;
            cdrInconsistent = null;
            List<IExceptionalCdrPreProcessor> exceptionalCdrPreProcessor = 
                base.CdrCollectorInputData.MediationContext.MefExceptionalCdrPreProcessorContainer.DicExtensions.Values.ToList();
            convertedCdr = CdrConversionUtil.ConvertTxtRowToCdrOrInconsistentOnFailure(row, exceptionalCdrPreProcessor, out cdrInconsistent);
            if (convertedCdr == null && cdrInconsistent != null) return;
            if (convertedCdr != null && cdrInconsistent == null)
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
                base.InconsistentCdrs.ToList(), base.RawCount);
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
                new List<cdrinconsistent>(), oldCdrExts.Count);
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
            
            var allIdCalls = cdrExtsForNonPartials.Select(c => c.Cdr.IdCall)
                .Concat(cdrExtsForPartials
                .SelectMany(c => c.PartialCdrContainer.CombinedNewAndOldUnprocessedInstance).Select(c => c.IdCall))
                .Concat(errorCdrExts.Select(c=>c.CdrError.IdCall)).ToList();
            if (allIdCalls.GroupBy(i => i).Any(g => g.Count() > 1))
            {
                throw new Exception("Duplicate idcalls for CdrExts in CdrJob");
            }
            var rawPartialCount = this.PartialCdrContainers.SelectMany(p => p.NewRawInstances).Count();
            if (this.RawCount != cdrExtsForNonPartials.Count + cdrExtsForPartials.Count + errorCdrExts.Count+
                rawPartialCount - this.PartialCdrContainers.Count + base.InconsistentCdrs.Count)
                throw new Exception("Count of nonPartial and partial cdrs do not match expected with expected rawCount for this job.");
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
        
        public void SetJobNameWithFileName(string cdrFileName, string[] txtRow)
        {
            txtRow[3] = cdrFileName; //filename
        }

        public void SetIdCall(AutoIncrementManager autoIncrementManager, string[] thisRow)
        {
            thisRow[1] = autoIncrementManager.GetNewCounter(AutoIncrementCounterType.cdr).ToString();
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
