using System.Collections.Generic;
using LibraryExtensions;
using TelcobrightMediation.Config;

namespace TelcobrightMediation
{
    public class NeAdditionalSetting
    {
        public bool PreDecodeAsTextFile { get; set; }
        public int MaxParallelPreDecoding { get; set; } = 1;
        public bool ProcessMultipleCdrFilesInBatch { get; set; } = false;
        public int MaxRowCountForBatchProcessing { get; set; } = 90000;
        public List<ILogPreprocessor> LogPreprocessors { get; set; }

        public List<CompressionType> SupportedCompressedLogTypes { get; set; } = new List<CompressionType>()
        {
            CompressionType.Gzip, CompressionType.Zip
        };
        public Dictionary<string, string> OtherSettings { get; set; }
    }
}