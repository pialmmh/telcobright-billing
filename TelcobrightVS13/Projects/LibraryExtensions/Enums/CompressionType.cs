using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryExtensions
{
    public enum CompressionType
    {
        None = 0,
        Zip = 1,
        Sevenzip = 2,
        Gzip = 3
    }

    public static class CompressionTypeHelper
    {
        public static Dictionary<string, CompressionType> ExtensionVsCompressionTypes =
            new Dictionary<string, CompressionType>()
            {
                { "none", CompressionType.None},
                { ".gz", CompressionType.Gzip},
                {".zip", CompressionType.Zip},
                { ".7z", CompressionType.Sevenzip}
            };
    }
}
