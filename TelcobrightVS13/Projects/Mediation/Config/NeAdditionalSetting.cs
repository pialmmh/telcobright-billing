using System.Collections.Generic;
using LibraryExtensions;
using TelcobrightMediation.Config;

namespace TelcobrightMediation
{
    public class NeAdditionalSetting
    {
        public bool PreDecodeAsTextFile { get; set; }
        public int MaxConcurrentFilesForParallelPreDecoding { get; set; } = 1;
        public bool ProcessMultipleCdrFilesInBatch { get; set; } = false;
        public int MinRowCountToStartBatchCdrProcessing { get; set; } = 90000;
        public int MaxNumberOfFilesInPreDecodedDirectory { get; set; } = 100;
        public int ExpectedNoOfCdrIn24Hour { get; set; }
        public string AggregationStyle { get; set; }
        public bool PerformPreaggregation { get; set; }
        public bool DumpAllInstancesToDebugCdrTable { get; set; }
        public bool CreateJobRecursively { get; set; }

        public List<EventPreprocessingRule> EventPreprocessingRules { get; set; }= new List<EventPreprocessingRule>();

        public List<CompressionType> SupportedCompressedLogTypes { get; set; } = new List<CompressionType>()
        {
            CompressionType.Gzip, CompressionType.Zip
        };
        public Dictionary<string, string> OtherSettings { get; set; }
    }
}