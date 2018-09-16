using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TelcobrightMediation.Cdr
{
    public class PartialCdrTester
    {
        private CdrJob CdrJob { get; }
        private CdrWritingResult CdrWritingResult { get; }
        private PartialCdrTesterData PartialCdrTesterData { get; }

        public PartialCdrTester(CdrJob cdrJob, CdrWritingResult cdrWritingResult,
            PartialCdrTesterData partialCdrTesterData)
        {
            this.CdrJob = cdrJob;
            this.CdrWritingResult = cdrWritingResult;
            this.PartialCdrTesterData = partialCdrTesterData;
        }

        public void ValidatePartialCdrMediation()
        {
            //partial cdrs tests here...
            var collectionResult = this.CdrJob.CdrProcessor.CollectionResult;
            int processedNonPartialErrorCount = collectionResult.CdrExtErrors
                .Count(c => c.CdrError.PartialFlag == "0");
            var errorPartialCdrExts = collectionResult.CdrExtErrors
                .Where(c => c.IsPartial == true && c.CdrError.PartialFlag != "0").ToList();
            var errorCdrExts = collectionResult.CdrExtErrors;
            var errorNonPartialCdrExts = errorCdrExts
                .Where(c => c.IsPartial == false && c.CdrError.PartialFlag == "0").ToList();
            int inconsistentsCount = collectionResult.CdrInconsistents.Count;
            var processedCdrExts = collectionResult.ProcessedCdrExts;
            var processedNonPartialCdrExts = processedCdrExts.Where(c => c.Cdr.PartialFlag == 0).ToList();
            var processedPartialCdrExts = collectionResult.ProcessedCdrExts.Where(
                c => c.Cdr.PartialFlag > 0 && c.PartialCdrContainer != null).ToList();
            Assert.AreEqual(this.CdrWritingResult.CdrCount, collectionResult.ProcessedCdrExts.Count);
            Assert.AreEqual(this.CdrWritingResult.CdrCount,
                processedNonPartialCdrExts.Count + processedPartialCdrExts.Count);
            Assert.AreEqual(this.CdrWritingResult.CdrErrorCount, collectionResult.CdrExtErrors.Count);
            Assert.AreEqual(this.CdrWritingResult.CdrInconsistentCount, collectionResult.CdrInconsistents.Count);
            Assert.AreEqual(this.CdrWritingResult.NonPartialCdrCount, processedNonPartialCdrExts.Count);

            Assert.AreEqual(processedPartialCdrExts.Count + errorPartialCdrExts.Count,
                this.CdrWritingResult.NormalizedPartialCount);
            
            Assert.AreEqual(this.CdrWritingResult.CdrCount,
                (processedNonPartialCdrExts.Count + processedPartialCdrExts.Count));
            var processedPartialNewRawInstances = processedPartialCdrExts
                .SelectMany(c => c.PartialCdrContainer.NewRawInstances).Count();
            var errorPartialNewRawInstances = errorPartialCdrExts.SelectMany(c => c.PartialCdrContainer.NewRawInstances)
                .ToList();
            Assert.AreEqual(this.PartialCdrTesterData.RawPartialCount + this.PartialCdrTesterData.NonPartialCount
                            - inconsistentsCount,
                (processedNonPartialCdrExts.Count + processedNonPartialErrorCount + processedPartialNewRawInstances
                 + errorPartialNewRawInstances.Count- inconsistentsCount));
            Assert.AreEqual(this.CdrWritingResult.PartialCdrWriter.WrittenCdrPartialReferences,
                processedPartialCdrExts.Count+ errorPartialCdrExts.Count);
            Assert.AreEqual(this.CdrWritingResult.CdrCount, processedCdrExts.Count());

            Assert.AreEqual(CdrWritingResult.CdrCount+CdrWritingResult.CdrErrorCount+CdrWritingResult.CdrInconsistentCount,
                processedCdrExts.Count+errorPartialCdrExts.Count+errorNonPartialCdrExts.Count+inconsistentsCount);
            Assert.AreEqual(collectionResult.RawCount- inconsistentsCount-processedPartialCdrExts.SelectMany(c=>c.PartialCdrContainer.NewRawInstances).Count()
                -errorPartialCdrExts.SelectMany(c => c.PartialCdrContainer.NewRawInstances).Count(),
                this.CdrWritingResult.NonPartialCdrCount+this.CdrWritingResult.NonPartialErrorCount);
            processedPartialCdrExts.ForEach(
                c => Assert.IsNotNull(c.PartialCdrContainer.NewAggregatedRawInstance));
            processedPartialCdrExts.ForEach(c => Assert.IsNotNull(c.PartialCdrContainer.NewCdrEquivalent));
            Assert.AreEqual(
                processedPartialCdrExts.Select(c => c.PartialCdrContainer.NewAggregatedRawInstance).Count(),
                processedPartialCdrExts.Select(c => c.PartialCdrContainer.NewCdrEquivalent).Count());

            decimal nonPartialDuration = processedNonPartialCdrExts.Sum(c => c.Cdr.DurationSec);
            decimal partialNewRawInstancesDuration = processedPartialCdrExts
                .SelectMany(c => c.PartialCdrContainer.NewRawInstances).Sum(c => c.DurationSec);
            decimal nonPartialErrorDuration = collectionResult.CdrExtErrors
                .Where(c => c.IsPartial == false && c.CdrError.PartialFlag == "0")
                .Sum(c => Convert.ToDecimal(c.CdrError.DurationSec));
            decimal partialErrorDuration = collectionResult.CdrExtErrors
                .Where(c=>c.IsPartial==true && c.CdrError.PartialFlag!="0")
                .Sum(c => c.PartialCdrContainer.NewCdrEquivalent.DurationSec-
                Convert.ToDecimal(c.PartialCdrContainer.LastProcessedAggregatedRawInstance?.DurationSec));
            var errorDuration = nonPartialErrorDuration + partialErrorDuration;
            Assert.AreEqual(this.PartialCdrTesterData.RawDurationWithoutInconsistents,
                nonPartialDuration + partialNewRawInstancesDuration + errorDuration);

            decimal partialNormalizedDuration = processedPartialCdrExts
                .Select(c => c.PartialCdrContainer.NewCdrEquivalent).Sum(c => c.DurationSec);
            Assert.AreEqual(collectionResult.ProcessedCdrExts.Sum(c => c.Cdr.DurationSec),
                (nonPartialDuration + partialNormalizedDuration));
        }
    }
}
