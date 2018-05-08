using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexValidation;
using LibraryExtensions;
using MediationModel;
using Newtonsoft.Json;
using TelcobrightMediation;
using TelcobrightMediation.Cdr;

namespace TelcobrightMediation
{
    public class SegmentedCdrErrorProcessor : AbstractRowBasedSegmentedJobProcessor
    {
        private CdrCollectorInputData CdrCollectorInput { get; }

        public SegmentedCdrErrorProcessor(CdrCollectorInputData cdrCollectorInput,
            int batchSizeWhenPreparingLargeSqlJob,
            string indexedColumnName, string dateColumnName)
            : base(cdrCollectorInput.TelcobrightJob, cdrCollectorInput.Context, batchSizeWhenPreparingLargeSqlJob,
                indexedColumnName, dateColumnName)
        {
            this.CdrCollectorInput = cdrCollectorInput;
        }

        public override ISegmentedJob CreateJobSegmentInstance(jobsegment jobSegment)
        {
            DayWiseRowIdsCollection dayWiseRowsIdsCollection = base.DeserializeDayWiseRowIdsCollection(jobSegment);
            DbRowCollector<cdrerror> dbRowCollector =
                new DbRowCollector<cdrerror>(this.CdrCollectorInput.CdrJobInputData,
                    dayWiseRowsIdsCollection.GetSelectSql());
            List<string[]> txtRows = dbRowCollector.CollectAsTxtRows();
            NewCdrPreProcessor preProcessor =
                new NewCdrPreProcessor(txtRows, new List<cdrinconsistent>(), this.CdrCollectorInput);
            FlexValidator<string[]> inconsistentValidator = CreateValidatorInstance();
            Parallel.ForEach(txtRows, txtRow =>
            {
                preProcessor
                    .CheckAndConvertIfInconsistent(this.CdrCollectorInput.CdrJobInputData, inconsistentValidator, txtRow);
                cdrinconsistent cdrInconsistent = null;
                preProcessor.ConvertToCdr(txtRow,out cdrInconsistent);
                if (cdrInconsistent != null) preProcessor.InconsistentCdrs.Add(cdrInconsistent);
            });
            CdrCollectionResult newCollectionResult = null, oldCollectionResult = null;
            preProcessor.GetCollectionResults(out newCollectionResult, out oldCollectionResult);

            //PartialCdrTesterData partialCdrTesterData = OrganizeTestDataForPartialCdrs(preProcessor, newCollectionResult);
            CdrJobContext cdrJobContext = new CdrJobContext(this.CdrCollectorInput.CdrJobInputData,
                newCollectionResult.HoursInvolved);
            CdrProcessor cdrProcessor = new CdrProcessor(cdrJobContext, newCollectionResult);
            CdrEraser cdrEraser = !oldCollectionResult.IsEmpty ? new CdrEraser(cdrJobContext, oldCollectionResult) : null;
            int rawCount = preProcessor.TxtCdrRows.Count;
            CdrJob cdrJob = new CdrJob(cdrProcessor, cdrEraser, rawCount,partialCdrTesterData:null);
            return cdrJob;
        }

        private FlexValidator<string[]> CreateValidatorInstance()
        {
            Dictionary<string, string> rules = this.CdrCollectorInput.CdrJobInputData.MediationContext.Tbc.CdrSetting
                .ValidationRulesForInconsistentCdrs;
            rules = rules.Where(kv => kv.Value.StartsWith("SequenceNumber must be numeric") == false
                                      || kv.Value.StartsWith("StartTime must be a valid datetime") == false)
                .ToDictionary(kv => kv.Key,kv=>kv.Value);
            var flexValidator= new FlexValidator<string[]>(continueOnError: false,
                throwExceptionOnFirstError: false,
                validationExpressionsWithErrorMessage: 
                rules);
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
