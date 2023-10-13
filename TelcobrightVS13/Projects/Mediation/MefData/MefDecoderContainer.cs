using System.Collections.Generic;
using System.Linq;
using MediationModel;

namespace TelcobrightMediation
{
    public class MefDecoderContainer
    {
        public DecoderComposer CmpDecoder = new DecoderComposer();
        public IDictionary<int, AbstractCdrDecoder> DicExtensions = new Dictionary<int, AbstractCdrDecoder>();
        public Dictionary<int, List<cdrfieldmappingbyswitchtype>> DicFieldMapping = new Dictionary<int, List<cdrfieldmappingbyswitchtype>>();
        public Dictionary<int, enumcdrformat> DicCdrFormatsByid = new Dictionary<int, enumcdrformat>();
        public PartnerEntities Context { get; }
        public int Totalfieldtelcobright = 0;
        public MefDecoderContainer(PartnerEntities context)
        {
            this.Context = context;
            foreach (ne thisne in context.nes.ToList())
            {
                //skip decoding disabled, including dummy switch
                if (thisne.SkipCdrDecoded == 1) continue;
                if (!this.DicFieldMapping.ContainsKey(thisne.idcdrformat))
                {
                    this.DicFieldMapping.Add(thisne.idcdrformat, context.cdrfieldmappingbyswitchtypes.Where(c => c.idCdrFormat == thisne.idcdrformat).ToList());
                }
            }
            foreach (enumcdrformat cf in context.enumcdrformats.ToList())
            {
                this.DicCdrFormatsByid.Add(cf.id, cf);
            }
            //dicFieldMapping = Context.cdrfieldmappingbyswitchtypes.Where(c=>c.SwitchType == 2).OrderBy(c=>c.FieldNumber).ToList();
            this.Totalfieldtelcobright = context.cdrfieldlists.Count();

        }
    }
}