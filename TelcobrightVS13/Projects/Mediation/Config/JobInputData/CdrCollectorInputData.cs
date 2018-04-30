using LibraryExtensions.ConfigHelper;
using MediationModel;

namespace TelcobrightMediation
{
    public class CdrCollectorInputData
    {
        public CdrJobInputData CdrJobInputData { get; }
        public PartnerEntities Context => this.CdrJobInputData.Context;
        public MediationContext MediationContext => this.CdrJobInputData.MediationContext;
        public TelcobrightConfig Tbc => this.CdrJobInputData.Tbc;
        public ne Ne => this.CdrJobInputData.Ne;
        public CdrSetting CdrSetting => this.CdrJobInputData.Tbc.CdrSetting;
        public job TelcobrightJob => this.CdrJobInputData.TelcobrightJob;
        public MefDecoderContainer MefDecodersData => this.CdrJobInputData.MediationContext.MefDecoderContainer;
        public string FullPath { get; set; }
        public AutoIncrementManager AutoIncrementManager { get; }

        public CdrCollectorInputData(CdrJobInputData cdrJobInputData, string fullPath)
        {
            this.CdrJobInputData = cdrJobInputData;
            this.FullPath = fullPath;
            this.AutoIncrementManager = cdrJobInputData.AutoIncrementManager;
        }
    }
}

