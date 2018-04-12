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
        public List<string[]> TxtCdrRows { get; set; }
        public NewCdrPreProcessor(List<string[]> txtCdrRows, List<cdrinconsistent> inconsistentCdrs,
            CdrCollectorInputData cdrCollectorInputData)
            : base(cdrCollectorInputData, txtCdrRows.Count + inconsistentCdrs.Count, inconsistentCdrs) //used after sql collection
        {
            this.TxtCdrRows = txtCdrRows;
        }

        public void CheckAndConvertIfInconsistent(CdrJobInputData input, FlexValidator<string[]> flexValidator,
            string[] txtRow)
        {
            ValidationResult validationResult = flexValidator.Validate(txtRow);
            if (validationResult.IsValid == false)
            {
                txtRow[Fn.Field4] = validationResult.FirstValidationFailureMessage;
                base.InconsistentCdrs.Add(CdrManipulatingUtil.ConvertTxtRowToCdrinconsistent(txtRow));
            }
        }

        public List<string[]> FilterCdrsWithDuplicateBillIdsAsInconsistent(List<string[]> txtRows)
        {
            List<string[]> dupRows = txtRows.GroupBy(c => c[Fn.Uniquebillid]).Where(g => g.Count() > 1)
                .Select(g => g.ToList())
                .SelectMany(c => c).ToList();
            txtRows = txtRows.Where(r => !dupRows.Select(dupRow => dupRow[Fn.Uniquebillid]).ToList()
                .Contains(r[Fn.Uniquebillid])).ToList();
            dupRows.ForEach(
                dupRow =>
                {
                    dupRow[Fn.Field4] = "Duplicate billids are not allowed when partial cdrs are disabled.";
                    base.InconsistentCdrs.Add(CdrManipulatingUtil.ConvertTxtRowToCdrinconsistent(dupRow));
                });
            return txtRows;
        }

        public void ConvertToCdrOrInconsistentOnFailure(string[] row)
        {
            cdr convertedCdr = null;
            cdrinconsistent cdrInconsistent = null;
            convertedCdr = CdrManipulatingUtil.ConvertTxtRowToCdrOrInconsistentOnFailure(row, out cdrInconsistent);
            if (convertedCdr == null && cdrInconsistent != null)
                base.InconsistentCdrs.Add(cdrInconsistent);
            else if (convertedCdr != null && cdrInconsistent == null)
            {
                if (convertedCdr.PartialFlag != 1)
                    base.NonPartialCdrs.Add(convertedCdr);
                else
                    base.RawPartialCdrInstances.Add(
                        new IcdrImplConverter<cdrpartialrawinstance>().Convert(convertedCdr));
            }
            else throw new Exception("Both converted & inconsistent cdrs cannot be null or not null at the same time.");
        }

        private static cdr ConvertTxtRowToCxxxxxdrOrInconistentOnFailure(BlockingCollection<cdrinconsistent> cdrInconsistentAggregator,
            string[] row)
        {
            cdrinconsistent cdrInconsistent = null;
            cdr convertedCdr = CdrManipulatingUtil.ConvertTxtRowToCdrOrInconsistentOnFailure(row, out cdrInconsistent);
            if (convertedCdr == null && cdrInconsistent != null)
            {
                cdrInconsistentAggregator.Add(cdrInconsistent);
                return null;
            }
            return convertedCdr;
        }

        public override void GetCollectionResults(out CdrCollectionResult newCollectionResult,
            out CdrCollectionResult oldCollectionResult)
        {
            if (base.CdrCollectorInputData.CdrSetting.PartialCdrEnabledNeIds
                .Contains(base.CdrCollectorInputData.CdrJobInputData.Ne.idSwitch))
            {
                if (base.RawPartialCdrInstances?.Any() == true)
                {
                    PartialCdrCollector partialCdrCollector = new PartialCdrCollector(
                        this.CdrCollectorInputData, base.RawPartialCdrInstances.ToList());
                    partialCdrCollector.CollectFullInfo();
                    base.PartialCdrContainers = partialCdrCollector.AggregateAll() ?? new BlockingCollection<PartialCdrContainer>();
                }
            }
            List<CdrExt> newCdrExts = this.CreateNewCdrExts();
            List<CdrExt> oldCdrExts = this.CreateOldCdrExts();

            base.PopulatePrevAccountingInfo(oldCdrExts);
            newCollectionResult = new CdrCollectionResult(base.CdrCollectorInputData.Ne, newCdrExts,
                base.InconsistentCdrs.ToList(), base.RawCount);
            oldCollectionResult = new CdrCollectionResult(base.CdrCollectorInputData.Ne, oldCdrExts,
                new List<cdrinconsistent>(), oldCdrExts.Count);
        }
        protected override List<CdrExt> CreateNewCdrExts()
        {
            var cdrExtsForNonPartials = this.NonPartialCdrs
                .Select(cdr => CdrExtFactory.CreateCdrExtWithNonPartialCdr(cdr, CdrNewOldType.NewCdr)).ToList();
            var cdrExtsForPartials = this.PartialCdrContainers
                .Select(partialContainer => CdrExtFactory.CreateCdrExtWithPartialCdr(partialContainer,
                    CdrNewOldType.NewCdr)).ToList();
            var concatedList = cdrExtsForPartials.Concat(cdrExtsForNonPartials).ToList();
            if (concatedList.GroupBy(c => c.UniqueBillId).Any(g => g.Count() > 1))
                throw new Exception("Duplicate billId for CdrExts in CdrJob");
            return concatedList;
        }

        protected override List<CdrExt> CreateOldCdrExts()
        {
            return this.PartialCdrContainers
                .Select(partialContainer => CdrExtFactory.CreateCdrExtWithPartialCdr(partialContainer,
                    CdrNewOldType.OldCdr)).ToList();
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

        public void MarkRowAsFinalRecordWhenPartialCdrsDisabled(string[] thisRow)
        {
            thisRow[Fn.Finalrecord] = "1";
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
            thisRow[1] = autoIncrementManager.GetNewCounter("cdr").ToString();
        }

        public void AdjustStartTimeBasedOnCdrSettingsForSummaryTimeField(SummaryTimeFieldEnum summaryTimeFieldEnum,
            string[] row)
        {
            row[Fn.Actualstarttime] = row[Fn.Starttime];
            if (summaryTimeFieldEnum == SummaryTimeFieldEnum.AnswerTime)
            {
                DateTime answerTime;
                if (row[Fn.Answertime].TryParseToDateTimeFromMySqlFormat(out answerTime) == true)
                {
                    row[Fn.Starttime] = row[Fn.Answertime];
                }
            }
        }
        public static FlexValidator<string[]> CreateValidatorForInconsistencyCheck(CdrCollectorInputData collectorinput)
        {
            FlexValidator<string[]> flexValidator = new FlexValidator<string[]>(
                continueOnError: false,
                throwExceptionOnFirstError: false,
                validationExpressionsWithErrorMessage: collectorinput.CdrJobInputData.MediationContext.Tbc.CdrSetting
                    .ValidationRulesForInconsistentCdrs);
            flexValidator.DateParsers.Add(
                "stringToDateConverterFromMySqlFormat", str => str.ConvertToDateTimeFromMySqlFormat());
            flexValidator.DateParsers.Add("strToMySqlDtConverter", str => str.ConvertToDateTimeFromMySqlFormat());
            flexValidator.DoubleParsers.Add("doubleConverterProxy", str => Convert.ToDouble(str));
            flexValidator.IntParsers.Add("intConverterProxy", str => Convert.ToInt32(str));
            flexValidator.BooleanParsers.Add("isDateTimeChecker", str => str.IsDateTime(StringExtensions.MySqlDateTimeFormat));
            flexValidator.BooleanParsers.Add("isNumericChecker", str => str.IsNumeric());
            return flexValidator;
        }
    }
}
