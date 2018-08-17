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
            int processedErrorCount = collectionResult.CdrExtErrors.Count;
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
            Assert.AreEqual(this.CdrWritingResult.TrueNonPartialCount, processedNonPartialCdrExts.Count);
            Assert.AreEqual(this.CdrWritingResult.NormalizedPartialCount, processedPartialCdrExts.Count);
            Assert.AreEqual(this.CdrWritingResult.CdrCount,
                (processedNonPartialCdrExts.Count + processedPartialCdrExts.Count));
            var processedPartialNewRawInstances = processedPartialCdrExts
                .SelectMany(c => c.PartialCdrContainer.NewRawInstances).Count();
            Assert.AreEqual(this.PartialCdrTesterData.RawPartialCount + this.PartialCdrTesterData.NonPartialCount
                            - inconsistentsCount,
                (processedNonPartialCdrExts.Count + processedPartialNewRawInstances + processedErrorCount
                 - inconsistentsCount));
            Assert.AreEqual(this.CdrWritingResult.PartialCdrWriter.WrittenCdrPartialReferences,
                processedPartialCdrExts.Count);
            Assert.AreEqual(this.CdrWritingResult.CdrCount, processedCdrExts.Count());
            Assert.AreEqual(collectionResult.RawCount,
                this.CdrWritingResult.CdrErrorCount + this.CdrWritingResult.CdrInconsistentCount
                + processedCdrExts.Select(c => c.NewRawCount).Sum());
            processedPartialCdrExts.ForEach(
                c => Assert.IsNotNull(c.PartialCdrContainer.NewAggregatedRawInstance));
            processedPartialCdrExts.ForEach(c => Assert.IsNotNull(c.PartialCdrContainer.NewCdrEquivalent));
            Assert.AreEqual(
                processedPartialCdrExts.Select(c => c.PartialCdrContainer.NewAggregatedRawInstance).Count(),
                processedPartialCdrExts.Select(c => c.PartialCdrContainer.NewCdrEquivalent).Count());

            decimal nonPartialDuration = processedNonPartialCdrExts.Sum(c => c.Cdr.DurationSec);
            decimal partialNewRawInstancesDuration = processedPartialCdrExts
                .SelectMany(c => c.PartialCdrContainer.NewRawInstances).Sum(c => c.DurationSec);
            decimal errorDuration = collectionResult.CdrExtErrors.Sum(c => Convert.ToDecimal(c.CdrError.DurationSec));
            Assert.AreEqual(this.PartialCdrTesterData.RawDurationWithoutInconsistents,
                nonPartialDuration + partialNewRawInstancesDuration + errorDuration);

            decimal partialNormalizedDuration = processedPartialCdrExts
                .Select(c => c.PartialCdrContainer.NewCdrEquivalent).Sum(c => c.DurationSec);
            Assert.AreEqual(collectionResult.ProcessedCdrExts.Sum(c => c.Cdr.DurationSec),
                (nonPartialDuration + partialNormalizedDuration));
        }
    }
}
