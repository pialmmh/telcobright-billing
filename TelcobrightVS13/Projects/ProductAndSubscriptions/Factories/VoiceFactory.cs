using System.ComponentModel.Composition;
using System;
namespace TelcobrightMediation.ProductAndContract
{
    [Export("VoiceFactory", typeof(IProductFactory))]
    public class DataVolumeFactory : ITelecomServiceProviderFactory
    {
        public string FactoryName { get { return "voiceFactory"; } }
        public void CreateProduct()
        {
            throw new NotImplementedException();
        }
        public IProduct CreateServiceCredit()
        {
            throw new NotImplementedException();
        }
        public IProduct CreateServiceCredit<T>()
        {
            throw new NotImplementedException();
        }
    }
}
