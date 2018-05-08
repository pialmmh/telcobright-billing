using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.Cdr
{
    public class CdrJobFactory
    {
        private CdrJobInputData Input { get; }
        private int RawCount { get; }

        public CdrJobFactory(CdrJobInputData input, int rawCount)
        {
            this.Input = input;
            this.RawCount = rawCount;
        }

        public CdrJob CreateCdrJob(NewCdrPreProcessor preProcessor, CdrCollectionResult newCollectionResult,
            CdrCollectionResult oldCollectionResult,PartialCdrTesterData partialCdrTesterData)
        {
            CdrJobContext cdrJobContext =
                new CdrJobContext(this.Input, newCollectionResult.HoursInvolved);
            CdrProcessor cdrProcessor = new CdrProcessor(cdrJobContext, newCollectionResult);
            if (cdrProcessor.CollectionResult.IsEmpty)
                throw new Exception("Newcdr collection in cdrProcessor cannot be empty.");
            CdrEraser cdrEraser = oldCollectionResult?.IsEmpty == false
                ? new CdrEraser(cdrJobContext, oldCollectionResult) : null;
            if (cdrEraser != null)
            {
                if (cdrEraser.CollectionResult.RawCount != cdrEraser.CollectionResult.ConcurrentCdrExts.Count)
                    throw new Exception("Raw count of cdrEraser does not match concurrentCdrExts total.");
                var newPartialCdrExtsWithOldInstance = cdrProcessor.CollectionResult.ConcurrentCdrExts.Values.Where(
                        c => c.Cdr.PartialFlag > 0 && c.PartialCdrContainer.LastProcessedAggregatedRawInstance != null)
                    .ToList();
                var cdrExtsAsOldCdr = cdrEraser.CollectionResult.ConcurrentCdrExts.Values;
                if (cdrExtsAsOldCdr.Count != newPartialCdrExtsWithOldInstance.Count)
                    throw new Exception("For newCdr job, number of old cdrExts must match equivalent partial cdrs " +
                                        "which have previous aggregated instance.");
                decimal sumDurationOfOldCdrsTobeDeleted = cdrExtsAsOldCdr.Sum(c => c.Cdr.DurationSec);
                decimal sumDurationOfOldPartialInstances = newPartialCdrExtsWithOldInstance
                    .Sum(c => c.PartialCdrContainer.LastProcessedAggregatedRawInstance.DurationSec);
                if (Math.Abs(sumDurationOfOldCdrsTobeDeleted -
                             sumDurationOfOldPartialInstances) > this.Input.CdrSetting.FractionalNumberComparisonTollerance)
                    throw new Exception("Duration sum of old cdrs in cdrEraser is not equal to " +
                                        "the same of cdrs in partialCdrExts with old Instances.");
            }
            CdrJob cdrJob = new CdrJob(cdrProcessor, cdrEraser, this.RawCount, partialCdrTesterData);
            return cdrJob;
        }

    }
}
