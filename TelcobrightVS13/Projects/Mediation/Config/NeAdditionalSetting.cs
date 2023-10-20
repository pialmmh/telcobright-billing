using System.Collections.Generic;
using LibraryExtensions;
using TelcobrightMediation.Config;

namespace TelcobrightMediation
{
    public class NeAdditionalSetting
    {
        public List<ILogPreprocessor> LogPreprocessors { get; set; }
        private List<CompressionType> SupportedCompressedLogTypes { get; set; }= new List<CompressionType>();
        public PreDecoderSetting PreDecoderSetting { get; set; }
        public Dictionary<string, string> OtherSettings { get; set; }
    }
}