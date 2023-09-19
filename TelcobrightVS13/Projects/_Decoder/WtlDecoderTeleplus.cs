using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using TelcobrightMediation;

namespace Decoders
{
    Export("Decoder", typeof(IFileDecoder))]
    public class WtlDecoderTeleplus : IFileDecoder
    {
        public  override string  ToString() => this.RuleName;
        public virtual string RuleName => GetType().Name;
        public int Id = 61;
        public virtual string Helptext => "Decode TelepluseNewyork WTL";
        public  CompressionType CompressionType { get; set; }
        protected  CdrCollectorInputData Input { get; set; }


        List<string[]> DecodeFile()
        {
            
        }





    }
}
