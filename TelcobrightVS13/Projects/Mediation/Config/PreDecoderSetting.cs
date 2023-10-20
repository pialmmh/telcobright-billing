using System;
using TelcobrightMediation.Accounting;
using MediationModel;
using TelcobrightMediation.Config;
namespace TelcobrightMediation
{
    public class PreDecoderSetting
    {
        public bool PreDecodeAsTextFile { get; set; }
        public int MaxParallelDecoding { get; set; } = 1;
        public bool MergePreDecodedFiles { get; set; }
        public int MaxSizeMBAfterMerge { get; set; } = 20;
    }
}