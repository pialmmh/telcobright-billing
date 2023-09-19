using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using TelcobrightMediation;

namespace Decoders
{

    public class WtlDecoderTeleplus 
    {
        public  override string  ToString() => this.RuleName;
        public virtual string RuleName => GetType().Name;
        public int Id = 54;
        public virtual string Helptext => "Decode TelepluseNewyork WTL";
        public  CompressionType CompressionType { get; set; }
        protected  CdrCollectorInputData Input { get; set; }

        public virtual List<string[]> DecodeFile()
        {
            return decodeLine();
        }


        public List<string[]> decodeLine()
        {
            List<string[]> decodeRows = new List<string[]>();


            return decodeRows;
        }

    }
}
