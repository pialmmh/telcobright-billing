namespace TelcobrightMediation.ProductAndContract
{
    public interface IProductFactory
    {
        string FactoryName { get; }
    }
    public interface ITelecomServiceProviderFactory:IProductFactory
    {
        IProduct CreateServiceCredit();
        IProduct CreateServiceCredit<T>();//e.g. serviceCreditof<Product=A_Z>
    }
    public interface IDataFactory:ITelecomServiceProviderFactory
    {
        IProduct CreateDataVolume();
        IProduct CreateBandwidth();
    }
    public interface IVoiceFactory : ITelecomServiceProviderFactory
    {
        IProduct CreateServiceCredit();
        
        IProduct CreateBandwidth();
    }
}
