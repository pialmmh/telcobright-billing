using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;
using TelcobrightMediation;

namespace Decoders
{

    public class WtlDecoderTeleplus:AbstractCdrDecoder
    {
        public  override string  ToString() => this.RuleName;
        public override string RuleName => GetType().Name;
        public override int Id => 54;
        public override string HelpText => "Decode TelepluseNewyork WTL";
        public override CompressionType CompressionType { get; set; }
        public override string UniqueEventTablePrefix { get; }
        public override string PartialTableStorageEngine { get; }
        public override string partialTablePartitionColName { get; }
        protected CdrCollectorInputData Input { get; set; }

        public override List<string[]> DecodeFile(CdrCollectorInputData decoderInputData, out List<cdrinconsistent> inconsistentCdrs)
        {
            inconsistentCdrs= new List<cdrinconsistent>();
            return decodeLine();
        }

        public override string getTupleExpression(Object data)
        {
            throw new NotImplementedException();
        }

        public override string getSelectExpressionForPartialCollection(object data)
        {
            throw new NotImplementedException();
        }


        public List<string[]> decodeLine()
        {
            List<string[]> decodeRows = new List<string[]>();


            return decodeRows;
        }

    }
}
